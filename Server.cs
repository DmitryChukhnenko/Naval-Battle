using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Numerics;
using System.Security.Cryptography;
using System.Windows;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Threading;
using System.Collections.Frozen;
using GameData;

namespace Client {   
public class Server {
        private IList<TcpClient> clients = new List<TcpClient>();
        bool runServer = true;
        private CreateGameModel createGameModel;

        public async Task HostServer(string ip, int port) {
            TcpListener listener =
            new TcpListener(IPAddress.Parse(ip), port);
            listener.Start();
            while (runServer) {
                TcpClient client = await listener.AcceptTcpClientAsync();                
                lock (clients) clients.Add(client);
                ListenToClient(client); 
            }
        }

        private List<string> names = new List<string>();
        private List<Player> players = new List<Player>();
        private bool run = true;
        private bool isEx = false;

        public async void ListenToClient(TcpClient from) {
            IReadOnlyList<TcpClient> copy;
            List<Player> playersCopy;

            // Получение данных об игре от хоста и пересылка всем подключившимся
            if (clients[0] == from) createGameModel = (await from.ReceiveMessage())
                    .ExpectType(MessageType.Lobby_ClientToServer_CreateGameModel)
                    .Deserialize1Arg<CreateGameModel>();

            while (run) {
                try {
                    Message message = await from.ReceiveMessage();
                    if (message.Type == MessageType.Lobby_ClientToServer_Close) {
                        runServer = false;
                        run = false;
                        // Пересылка данных об игре
                        lock (clients) copy = clients.ToList();
                        foreach (TcpClient to in copy) {
                            await to.SendMessage(MessageType.Lobby_ServerToClient_Close);
                            await to.SendMessage(MessageType.Lobby_ServerToClient_CreateGameModel, createGameModel!);                            
                        }
                    }
                    else if (message.Type == MessageType.Lobby_ClientToServer_MyName) {
                        message.ExpectType(MessageType.Lobby_ClientToServer_MyName);
                        string name = message.Deserialize1Arg<string>();
                        names.Add(name);

                        // Пересылка имен игроков всем
                        lock (clients) copy = clients.ToList();
                        foreach (TcpClient to in copy) {
                            await to.SendMessage(MessageType.Lobby_ServerToClient_Name, names!);
                        }
                    }
                }
                // Код на исключения
                catch (Exception ex) {
                    run = false;
                    isEx = true;
                    lock (clients) clients.Remove(from);
                    from.Dispose();
                    MessageBox.Show(ex.ToString());
                    return;
                }                
            }

            // Расстановка
            // Получение данных об игроке
            Player me = (await from.ReceiveMessage())
                    .ExpectType(MessageType.Arrangement_ClientToServer_Player)
                    .Deserialize1Arg<Player>();             
            lock (players) players.Add(me);
            foreach (OneCell cel in me.Cells) cel.AddNeighboors(me.Cells);

            // Ожидание закрытия расстановки
            run = !isEx;
            while (run) {
                try {
                    Message message = await from.ReceiveMessage();
                    if (message.Type == MessageType.Arrangement_ClientToServer_Close) {
                        if (players.Count != clients.Count) await from.SendMessage(MessageType.Arrangement_ServerToClient_Wait);
                        else {
                            run = false;
                            runServer = false;

                            lock (clients) copy = clients.ToList();
                            foreach (TcpClient to in copy)
                                await to.SendMessage(MessageType.Arrangement_ServerToClient_Close);
                        }
                    }
                    else if (message.Type == MessageType.Arrangement_ClientToServer_Exit) {
                        lock (clients) clients.Remove(from);
                        lock (players) players.Remove(me);
                        from.Dispose();
                        return;
                    }
                    else if (message.Type == MessageType.Lobby_ClientToServer_Exit) break;
                }
                // Код на исключения
                catch (Exception ex) {
                    run = false;
                    isEx = true;
                    lock (clients) clients.Remove(from);
                    lock (players) players.Remove(me);
                    from.Dispose();
                    MessageBox.Show(ex.ToString());
                    return;
                }
            }

            // Сортируем игроков, чтобы каждый из них соответсвовал порядку подключения
            playersCopy = new List<Player>();
            lock (players) {
                foreach (string name in names) {
                    playersCopy.Add(players.Find(plr => plr.Nickname == name)!);
                }
                players = playersCopy;
            }            
            string staticPlayers = JsonSerializer.Serialize<List<Player>>(playersCopy);
            List<string> moves = new List<string>();

            // Ход игры
            Player winner = null!;
            run = !isEx;
            while (run) {
                try {
                    // Получение данных о ходе и их извлечение
                    Message message = await from.ReceiveMessage();
                    if (message.Type == MessageType.Game_ClientToServer_NicknameCell) {
                        message.ExpectType(MessageType.Game_ClientToServer_NicknameCell);

                        moves.Add(message.Args[0]);
                        NicknamePoint nicknamePoint = message.Deserialize1Arg<NicknamePoint>();

                        Player player;
                        lock (players) player = players.FirstOrDefault(plr => plr.Nickname == nicknamePoint.Nickname)!;

                        OneCell cell = OneCell.FindCell(player.Cells, nicknamePoint.Point)!;

                        // Логика хода
                        string result;

                        // Если попал
                        if (cell.IsShipHere) {
                            result = "Strike!";
                            cell.IsDamagedShipHere = true;
                            player.FleetSize--;

                            // Если кораблей не осталось
                            if (player.FleetSize == 0) {
                                player.HasLost = true;
                                result += $" Player {nicknamePoint.Nickname} has lost!";

                                int notLost = 0;
                                Player last = new Player();
                                lock (players) playersCopy = players.ToList();
                                foreach (Player p in playersCopy) {
                                    if (!p.HasLost) { notLost++; last = p; }
                                }
                                run = notLost > 1;

                                // Отправка победителя (и данных об игре) - добавить
                                if (notLost == 1) {
                                    winner = last;
                                    lock (clients) copy = clients.ToList();
                                    foreach (TcpClient to in copy) {
                                        await to.SendMessage(MessageType.Game_ServerToClient_Winner, winner);
                                    }

                                    // Отправка в бд игр
                                    using GameDataContext db = new();
                                    db.Saves.Add(new(staticPlayers, moves));
                                    db.SaveChanges();

                                    lock (players) players.Remove(me);
                                    lock (clients) clients.Remove(from);
                                    from.Dispose();
                                }
                            }

                            // Если корабль уничтожен
                            else if (!OneCell.IsAlive(cell, new())) {
                                List<OneCell> changedList = OneCell.AfterSunk(new(), cell, new());
                                int width = (int)Math.Sqrt(changedList.Count);
                                ObservableCollection<OneCell> changed = new(changedList.Distinct().OrderBy(cel => cel.Point.X + cel.Point.Y * width));

                                lock (clients) copy = clients.ToList();
                                foreach (TcpClient to in copy) {
                                    await to.SendMessage(MessageType.Game_ServerToClient_ChangedCells, new PlayerList(nicknamePoint.Nickname, changed));
                                }
                            }
                        }
                        else result = "Miss!";

                        // Отправка результата хода
                        // Если игрок проиграл
                        if (player.HasLost) {
                            result += $" Player {player.Nickname} has lost!";
                            await clients[players.IndexOf(player)].SendMessage(MessageType.Game_ServerToClient_YouLost);
                        }

                        lock (clients) copy = clients.ToList();
                        foreach (TcpClient to in copy) {
                            await to.SendMessage(MessageType.Game_ServerToClient_Move, new Move(result, nicknamePoint.Nickname, cell));
                        }
                    }
                }
                // Код на исключения
                catch (Exception ex) {
                    run = false;
                    isEx = true;

                    lock (players) players.Remove(me);
                    lock (clients) clients.Remove(from);
                    from.Dispose();

                    //MessageBox.Show(ex.ToString());
                    return;
                }
            }                       
        }
    }
}

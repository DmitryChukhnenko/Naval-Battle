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
            IReadOnlyList<Player> playersCopy;

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
                    MessageBox.Show(ex.Message);
                    return;
                }                
            }

            // Расстановка
            // Получение данных об игроке
            Player newOne = (await from.ReceiveMessage())
                    .ExpectType(MessageType.Arrangement_ClientToServer_Player)
                    .Deserialize1Arg<Player>(); 
            lock (players) players.Add(newOne);

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
                        lock (players) players.Remove(newOne);
                        from.Dispose();
                        return;
                    }
                }
                // Код на исключения
                catch (Exception ex) {
                    run = false;
                    isEx = true;
                    lock (clients) clients.Remove(from);
                    lock (players) players.Remove(newOne);
                    from.Dispose();
                    MessageBox.Show(ex.Message);
                    return;
                }
            }

            // Ход игры
            Player winner = new();
            run = !isEx;
            while (run) {
                try {
                    // Получение данных о ходе и их извлечение
                    NicknameCell nicknameCell = (await from.ReceiveMessage())
                    .ExpectType(MessageType.Game_ClientToServer_NicknameCell)
                    .Deserialize1Arg<NicknameCell>();

                    Player player = new();
                    lock (players) playersCopy = players.ToList();
                    foreach (Player plr in playersCopy) {
                        if (plr.Nickname == nicknameCell.Nickname) player = plr;
                    }

                    OneCell cell = new OneCell();
                    foreach (OneCell cel in player.Cells) {
                        if (cel.Point.Equals (nicknameCell.Cell.Point)) cell = cel;
                    }

                    // Логика хода
                    string message;

                    cell.IsFogHere = false;
                    // Если попал
                    if (cell.IsShipHere) {
                        message = "Strike!";
                        cell.IsDamagedShipHere = true;
                        player.FleetSize--;

                        // Если кораблей не осталось
                        if (player.FleetSize == 0) {
                            player.HasLost = true;
                            message += $" Player {nicknameCell.Nickname} has lost!";

                            int notLost = 0;
                            Player last = new Player();
                            lock (players) playersCopy = players.ToList();
                            foreach (Player p in playersCopy) {
                                if (!p.HasLost) { notLost++; last = p; }
                            }
                            if (notLost == 1) winner = last;
                            run = notLost > 1;
                        }

                        // Если корабль уничтожен
                        if (!OneCell.IsAlive(cell, new())) {
                            List<OneCell> changed = new();
                            OneCell.AfterSunk(changed, cell, new());

                            lock (clients) copy = clients.ToList();
                            foreach (TcpClient to in copy) {
                                await to.SendMessage(MessageType.Game_ServerToClient_ChangedCells, new PlayerList(nicknameCell.Nickname, changed));
                            }
                        }
                    }
                    else message = "Miss!";

                    // Отправка результата хода
                    // Если игрок проиграл
                    if (player.HasLost) {
                        message += $" Player {player.Nickname} has lost!";
                        await clients[players.IndexOf(player)].SendMessage(MessageType.Game_ServerToClient_YouLost);
                    }

                    lock (clients) copy = clients.ToList();
                    foreach (TcpClient to in copy) {
                        await to.SendMessage(MessageType.Game_ServerToClient_Move, new Move(message, nicknameCell.Nickname, cell));
                    }
                }
                // Код на исключения
                catch (Exception ex) {
                    run = false;
                    isEx = true;
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            // Отправка победителя
            if (!isEx) {
                lock (clients) copy = clients.ToList();
                foreach (TcpClient to in copy) {
                    await to.SendMessage(MessageType.Game_ServerToClient_Winner, winner);
                }
            }
            lock (clients) clients.Remove(from);
            from.Dispose();

            // Отправка в бд игр
        }
    }
}

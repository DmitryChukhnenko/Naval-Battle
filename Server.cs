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

namespace Client {   
public class Server {
        private IList<TcpClient> clients = new List<TcpClient>();
        bool runServer = true;
        private byte[] bytes;

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
        private ObservableCollection<Player> players = new ObservableCollection<Player>();

        public async void ListenToClient(TcpClient from) {
            bool run = true;
            bool ex = false;
            players.CollectionChanged += Test;

            // Получение данных об игре от хоста и пересылка всем подключившимся
            if (clients[0] == from) bytes = await TCP.ReceiveVariable(from);

            IReadOnlyList<TcpClient> copy;
            IReadOnlyList<Player> playersCopy;

            while (run) {
                try {
                    string name = await TCP.ReceiveString(from);
                    if (name == "cmd:Close") {
                        runServer = false;
                        run = false;
                        // Вот здесь пересылка
                        lock (clients) copy = clients.ToList();
                        foreach (TcpClient to in copy) {
                            // Данных об игре
                            await TCP.SendVariable(from, bytes);
                            // Имени игрока
                            await TCP.SendString(to, name);
                        }
                    }
                    else names.Add(name);

                    // А также здесь
                    // Данных об игре
                    await TCP.SendVariable(from, bytes);

                    // Имён игроков
                    lock (clients) copy = clients.ToList();
                    foreach (TcpClient to in copy) {
                        await TCP.SendString(to, string.Join('\n', names));
                    }
                }
                // Код на исключения
                catch (Exception) {
                    runServer = false;
                    run = false;
                    ex = true;
                }                
            }
            if (ex) {
                lock (clients) clients.Remove(from);
                from.Dispose();
            }

            // Расстановка
            // Получение данных об игроке
            Player newOne = JsonSerializer.Deserialize<Player>(await TCP.ReceiveVariable(from), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            lock (players) players.Add(newOne);

            // Ожидание закрытия расстановки
            run = true;
            while (run) {
                string name;
                try {
                    name = await TCP.ReceiveString(from);
                    if (name == "cmd:Close") {
                        run = false;
                        runServer = false;
                    }
                    else if (name == "cmd:Exit") break;

                    lock (clients) copy = clients.ToList();
                    foreach (TcpClient to in copy) {
                        await TCP.SendString(to, name);
                    }
                }
                // Код на исключения
                catch (Exception) {
                    runServer = false;
                    run = false;
                    ex = true;
                }
            }
            if (ex) {
                lock (clients) clients.Remove(from);
                from.Dispose();
            }

            // Ход игры
            Player winner = new();
            run = true;
            while (run) {
                try {
                    // Получение данных о ходе и их извлечение
                    NicknameCell nicknameCell = JsonSerializer.Deserialize<NicknameCell>(await TCP.ReceiveVariable(from), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

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
                    if (cell.IsShipHere) {
                        message = "Strike!";
                        cell.IsDamagedShipHere = true;
                        player.FleetSize--;
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
                    }
                    else message = "Miss!";

                    // Отправка результата хода
                    lock (clients) copy = clients.ToList();
                    foreach (TcpClient to in copy) {
                        await TCP.SendVariable(to, JsonSerializer.SerializeToUtf8Bytes<NicknameCell>(new NicknameCell(message, cell)));
                    }
                    if (player.HasLost) {
                        foreach (TcpClient to in copy) {
                            await TCP.SendVariable(to, JsonSerializer.SerializeToUtf8Bytes<NicknameCell>(new NicknameCell("cmd:Close", cell)));
                        }
                    }
                }
                // Код на исключения
                catch (Exception) {
                    runServer = false;
                    run = false;
                    ex = true;
                }
            }
            // Отправка победителя
            if (!ex) {
                lock (clients) copy = clients.ToList();
                foreach (TcpClient to in copy) {
                    await TCP.SendVariable(to, JsonSerializer.SerializeToUtf8Bytes<Player>(winner));
                }
            }
            lock (clients) clients.Remove(from);
            from.Dispose();

            // Отправка в бд игр
        }

        private void Test(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            var x = e.NewItems;
        }
    }
}

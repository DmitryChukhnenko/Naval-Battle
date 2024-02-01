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

namespace Client {
    public class Server {
        private IList<TcpClient> clients = new List<TcpClient>();
        bool runServer = true;
        private byte[] bytes;

        public async Task HostServer(string ip, Action<TcpClient> action) {
            TcpListener listener =
            new TcpListener(IPAddress.Parse(ip), 2024);
            listener.Start();
            while (runServer) {
                TcpClient client = await listener.AcceptTcpClientAsync();                
                lock (clients) clients.Add(client);
                action.Invoke(client); 
            }
        }

        private List<string> names = new List<string>();
        public async void LobbyListenToClient(TcpClient from) {
            bool run = true;

            if (clients.Count == 1) bytes = await TCP.ReceiveVariable(from);

            IReadOnlyList<TcpClient> copy;            

            while (run) {
                try {
                    string name = await TCP.ReceiveString(from);
                    if (name == "Close") {
                        runServer = false;
                        run = false;
                        lock (clients) copy = clients.ToList();
                        foreach (TcpClient to in copy)
                        {
                            await TCP.SendString(to, name);
                        }
                    }
                    else names.Add(name);
                    await TCP.SendVariable(from, bytes);
                }
                catch (Exception) {
                    break;
                }
                
                lock (clients) copy = clients.ToList();
                
                foreach (TcpClient to in copy) {
                    await TCP.SendString(to, string.Join('\n', names));
                }
            }            
            lock (clients) clients.Remove(from);
            from.Dispose();
        }

        private List<Player> players = new List<Player>();
        public async void ArrangementListenToClient(TcpClient from) {
            if (clients.Count == 1) bytes = await TCP.ReceiveVariable(from);
            else await TCP.SendVariable(from, bytes);
            players.Add((Player)JsonSerializer.Deserialize(await TCP.ReceiveVariable(from), typeof(Player))!);

            bool run = true;
            IReadOnlyList<TcpClient> copy;

            while (run) {
                string name;
                try {                   
                    name = await TCP.ReceiveString(from);
                    if (name == "Close" || name == "Exit") {
                        run = false;
                        if (name == "Close") {
                            runServer = false;
                            lock (clients) copy = clients.ToList();
                            foreach (TcpClient to in copy) {
                                if (to != from) 
                                    await TCP.SendString(to, name);
                                await TCP.SendVariable(to, JsonSerializer.SerializeToUtf8Bytes(new PlayerList(players), typeof(PlayerList)));
                            }
                        }
                    }                    
                }
                catch (Exception) {
                    break;
                }
            }            
            lock (clients) clients.Remove(from);
            from.Dispose();
        }

        public async void GameListenToClient(TcpClient from)
        {
            bool run = true;
            while (run)
            {
                byte[] bytes;
                try
                {
                    bytes = await TCP.ReceiveVariable(from);
                }
                catch (Exception)
                {
                    break;
                }

                IReadOnlyList<TcpClient> copy;
                lock (clients) copy = clients.ToList();

                foreach (TcpClient to in copy)
                {
                    if (to != from)
                    {
                        await TCP.SendVariable(to, bytes);
                    }
                }
            }
            lock (clients) clients.Remove(from);
            from.Dispose();
        }
    }
}

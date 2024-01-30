using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public class Server {
        private IList<TcpClient> clients = new List<TcpClient>();
        bool runServer = true;

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

        private byte[] bytes;
        public async void LobbyListenToClient(TcpClient from) {
            bool run = true;

            if (clients.Count == 0) bytes = await TCP.ReceiveVariable(from);

            while (run) {
                string name;
                try {
                    name = await TCP.ReceiveString(from);
                    if (name == "Close") { runServer = false; run = false; }
                    await TCP.SendWithLength(from, bytes);
                }
                catch (Exception) {
                    break;
                }

                IReadOnlyList<TcpClient> copy;
                lock (clients) copy = clients.ToList();
                
                foreach (TcpClient to in copy) {
                    await TCP.SendString(to, name);
                }
            }            
            lock (clients) clients.Remove(from);
            from.Dispose();
        }
        
        public async void ArrangementListenToClient(TcpClient from) {
            bool run = true;

            byte[] bytes = await TCP.ReceiveVariable(from);

            IReadOnlyList<TcpClient> copy;
            lock (clients) copy = clients.ToList();

            foreach (TcpClient to in copy) {
                if (to != from) 
                    await TCP.SendWithLength(to, bytes);
            }

            while (run) {
                string name;
                try {                   
                    name = await TCP.ReceiveString(from);
                    if (name == "Close") { runServer = false; run = false; }
                    else if (name == "Confirm") run = false;
                }
                catch (Exception) {
                    break;
                }
            }            
            lock (clients) clients.Remove(from);
            from.Dispose();
        }
    }
}

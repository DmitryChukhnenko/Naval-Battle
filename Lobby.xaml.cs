using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.Json;
using System.Runtime.ConstrainedExecution;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для Lobby.xaml
    /// </summary>
    public partial class Lobby : Window {
        bool isHost;
        int port = 2024;
        Server server;
        Task runServer;
        LobbyModel lobbyModel;
        MainWindow mainWindow;
        CreateGameModel? createGameModel;
        TcpClient serverTcp;

        public Lobby(string nickname, CreateGameModel createGameModel, MainWindow mainWindow) {
            InitializeComponent();
            isHost = true;
            lobbyModel = new();
            this.mainWindow = mainWindow;
            this.createGameModel = createGameModel;

            // с помощью танцев с бубном получаем свой ip,
            string ip;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                ip = endPoint.Address.ToString();
            }


            lobbyModel.GameId = ip;
            DataContext = lobbyModel;

            // на нём создаём сервер, отвечающий за список игроков, на отдельном потоке
            server = new Server();
            runServer = Task.Run(() => server.HostServer(ip, port));
            LobbyClient(ip, nickname);
        }

        public Lobby(string nickname, string gameId, MainWindow mainWindow) {
            InitializeComponent();
            isHost = false;
            lobbyModel = new();
            this.mainWindow = mainWindow;

            lobbyModel.GameId = gameId;
            DataContext = lobbyModel;

            // обращаемся к серверу за списком игроков (по gameId) по кд на отдельном потоке
            LobbyClient(gameId, nickname);
        }

        public async void LobbyClient(string gameId, string nickname) {
            serverTcp = new TcpClient(gameId, port);

            if (isHost) {
                await serverTcp.SendMessage(MessageType.Lobby_ClientToServer_CreateGameModel, createGameModel!);
            }            

            await serverTcp.SendMessage(MessageType.Lobby_ClientToServer_MyName, nickname);

            while (true) {
                try {
                    Message namesMessage = await serverTcp.ReceiveMessage();
                    if (namesMessage.Type == MessageType.Lobby_ServerToClient_Close)
                        break;

                    namesMessage.ExpectType(MessageType.Lobby_ServerToClient_Name);
                    lobbyModel.Players = namesMessage.Deserialize1Arg<string[]>();
                }
                catch (Exception) {
                    serverTcp.Dispose();
                    break;
                }
            }

            // длинный способ:
            //Message createGameModelMessage = await serverTcp.ReceiveMessage ();
            //if (createGameModelMessage.Type != MessageType.ClientToServer_CreateGameModel)
            //    throw new InvalidOperationException ("Expected CreateGameModel");
            //createGameModel = createGameModelMessage.Deserialize<CreateGameModel> (0);

            // короткий способ:
            createGameModel = (await serverTcp.ReceiveMessage())
                    .ExpectType(MessageType.Lobby_ServerToClient_CreateGameModel)
                    .Deserialize1Arg<CreateGameModel>();

            List<OneCell> cells = OneCell.CreateEmptyList(createGameModel.FieldSize);

            Player player = new (); 
            List<Player> players = new List<Player>();
            foreach (string name in lobbyModel.Players) {
               Player tmp = new Player(name, cells, createGameModel.FleetSize);
               if (name == nickname) player = tmp;
            }

            foreach (Player plr in players) {
                if (plr != player) {
                    foreach (OneCell cell in plr.Cells) {
                        cell.isFogHere = true;
                    }
                }
            }

            Arrangement arrangement = new Arrangement(isHost, lobbyModel.GameId, player, players, createGameModel, mainWindow, runServer, serverTcp);
            this.Visibility = Visibility.Hidden;
            arrangement.ShowDialog();
        }

        private async void Continue(object sender, RoutedEventArgs e) {
            if (isHost) {
                await serverTcp.SendMessage(MessageType.Lobby_ClientToServer_Close);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}

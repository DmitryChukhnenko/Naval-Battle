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
            string ip = Dns.GetHostAddresses(Dns.GetHostName())
           .Where(adress => adress.AddressFamily == AddressFamily.InterNetwork)
           .Where(adress => adress.ToString().StartsWith("192.168"))
           .First().ToString();

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
                await TCP.SendVariable(serverTcp, JsonSerializer.SerializeToUtf8Bytes<CreateGameModel>(createGameModel!));                
            }

            await TCP.SendString(serverTcp, nickname);
            createGameModel = (CreateGameModel) JsonSerializer.Deserialize<CreateGameModel>(await TCP.ReceiveVariable(serverTcp))!;

            while (true) {
                try {
                    string result = await TCP.ReceiveString(serverTcp);
                    if (result == "cmd:Close") break;
                    lobbyModel.Players = result.Split('\n');
                }
                catch (Exception) {
                    serverTcp.Dispose();
                    break;
                }
            }

            List<OneCell> cells = OneCell.CreateEmptyList(createGameModel.FieldSize);
            
            Arrangement arrangement = new Arrangement(isHost, lobbyModel.GameId, new Player(nickname, cells, createGameModel.FleetSize), createGameModel, mainWindow, runServer, serverTcp);
            this.Visibility = Visibility.Hidden;
            arrangement.ShowDialog();
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            if (isHost) {
                await TCP.SendString(serverTcp, "cmd:Close");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}

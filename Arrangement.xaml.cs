using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для Arrangement.xaml
    /// </summary>
    public partial class Arrangement : Window {
        bool isHost;
        bool hasSent;
        string gameId;
        Task runServer;
        Player player;
        List<Player> players = new List<Player>();
        CreateGameModel createGameModel;
        MainWindow mainWindow;
        int shipsCounter;
        List<int> shipsCounters;

        public Arrangement(bool isHost, string gameId, Player player, CreateGameModel createGameModel, MainWindow mainWindow) {
            InitializeComponent();

            this.isHost = isHost;
            hasSent = false;
            this.gameId = gameId;
            this.player = player;
            players.Add(player);
            this.createGameModel = createGameModel;
            this.mainWindow = mainWindow;

            shipsCounter = createGameModel.FleetSize;
            shipsCounters = createGameModel.Ships[createGameModel.FleetSizes.IndexOf(shipsCounter)];

            DataContext = player;
                        
            if (isHost) {
                Server server = new Server();
                runServer = Task.Run(() => server.HostServer(gameId, server.ArrangementListenToClient));
                serverTcp = new TcpClient(gameId, 2024);
            }            
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (hasSent) { MessageBox.Show("You have already sent your arrangement!"); return; }

            Button button = (Button)sender;
            OneCell cell = (OneCell)button.DataContext;

            if (cell.IsShipBowHere && cell.IsShipHere) {
                foreach (OneCell cel in cell.Neighboors) {
                    if (cel.IsShipHere && (!cel.IsShipBowHere)) 
                        cel.IsShipBowHere = true;
                }
                cell.IsShipBowHere = false;
                cell.IsShipHere = false;
                shipsCounter++;
                shipsCounters[OneCell.CountLength(0, cell)]++;
            }

            else if (!cell.IsShipHere && shipsCounter != 0) {
                foreach (OneCell cel in cell.Neighboors) {
                    if (cel.IsShipHere && (!cel.IsShipBowHere)) return;
                }
                if (cell.Neighboors[0, 0].IsShipHere || cell.Neighboors[0, 2].IsShipHere || cell.Neighboors[2, 0].IsShipHere || cell.Neighboors[2, 2].IsShipHere) return;
                if ((cell.Neighboors[0, 1].IsShipBowHere && cell.Neighboors[2, 1].IsShipBowHere) || (cell.Neighboors[1, 0].IsShipBowHere && cell.Neighboors[1, 2].IsShipBowHere)) return;

                foreach (OneCell cel in cell.Neighboors) {
                    if (cel.IsShipHere && cel.IsShipBowHere) 
                        cel.IsShipBowHere = false;
                }
                cell.IsShipHere = true;
                cell.IsShipBowHere = true;
                shipsCounter--;
                shipsCounters[OneCell.CountLength(0, cell)]--;
            }
        }

        TcpClient serverTcp;
        private async void ArrangementClient(string gameId) {
            await TCP.SendVariable(serverTcp, JsonSerializer.SerializeToUtf8Bytes(player, typeof(Player)));

            string result;
            while (true) {
                try {
                    result = await TCP.ReceiveString(serverTcp);
                    if (result == "Close") break; 
                }
                catch (Exception) {
                    break;
                }
            }

            List<Player> tmp = ((PlayerList)JsonSerializer.Deserialize(await TCP.ReceiveVariable(serverTcp), typeof(PlayerList))!).Players;
            foreach (Player player in tmp) {
                foreach (OneCell cell in player.Cells) {
                    cell.AddNeighboors(player.Cells);
                }
            }
            players = tmp;

            serverTcp.Dispose();

            Game game = new Game(players, gameId, players.IndexOf(player), mainWindow);
            this.Visibility = Visibility.Hidden;
            game.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            if (shipsCounter != 0) { MessageBox.Show($"Use all ships! Rest is {shipsCounter}"); return; }
            ArrangementClient(gameId);
            hasSent = true;
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e) {
            if (isHost) {
                await TCP.SendString(serverTcp, "Close");
            }
            else {
                await TCP.SendString(serverTcp, "Exit");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
        

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            if (isHost) await TCP.SendVariable(serverTcp, JsonSerializer.SerializeToUtf8Bytes(createGameModel, typeof(CreateGameModel)));
            else {
                await Task.Delay(50);
                serverTcp = new TcpClient(gameId, 2024);
                createGameModel = (CreateGameModel)JsonSerializer.Deserialize(await TCP.ReceiveVariable(serverTcp), typeof(CreateGameModel))!;
            }
        }
    }
}

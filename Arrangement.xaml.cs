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
using static System.Collections.Specialized.BitVector32;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для Arrangement.xaml
    /// </summary>
    public partial class Arrangement : Window {
        bool isHost;
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
            }

            // Начать писать саму игру
            // Найти картинки
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Grid grid = (Grid)sender;
            OneCell cell = (OneCell)grid.DataContext;

            if (cell.IsShipBowHere && cell.IsShipHere) {
                foreach (OneCell cel in cell.Neighboors) {
                    if (cel.IsShipHere && (!cel.IsShipBowHere)) {
                        cell.IsShipBowHere = false;
                        cell.IsShipHere = false;
                        cel.IsShipBowHere = true;
                        shipsCounter++;
                        shipsCounters[OneCell.CountLength(-1, cell)]++;
                    }
                }
            }

            else if (!cell.IsShipHere && shipsCounter != 0) {
                foreach (OneCell cel in cell.Neighboors) {
                    if (cel.IsShipHere && (!cel.IsShipBowHere)) return;
                }
                if (cell.Neighboors[0, 0].IsShipHere || cell.Neighboors[0, 2].IsShipHere || cell.Neighboors[2, 0].IsShipHere || cell.Neighboors[2, 2].IsShipHere) return;
                if ((cell.Neighboors[0, 1].IsShipBowHere && cell.Neighboors[2, 1].IsShipBowHere) || (cell.Neighboors[1, 0].IsShipBowHere && cell.Neighboors[1, 2].IsShipBowHere)) return;

                foreach (OneCell cel in cell.Neighboors) {
                    if (cel.IsShipHere && cel.IsShipBowHere) {
                        cel.IsShipBowHere = false;
                        cell.IsShipHere = true;
                        cell.IsShipBowHere = true;
                        shipsCounter--;
                        shipsCounters[OneCell.CountLength(-1, cell)]--;
                    }
                }
            }
        }

        TcpClient serverTcp;
        private async void ArrangementClient(string gameId) {
            serverTcp = new TcpClient(gameId, 2024);

            if (isHost) await TCP.SendWithLength(serverTcp, JsonSerializer.SerializeToUtf8Bytes(createGameModel, typeof(CreateGameModel)));

            while (true) {
                try {
                    Player tmp = (Player)JsonSerializer.Deserialize(await TCP.ReceiveVariable(serverTcp), typeof(Player))!;
                    tmp.IsEnemy = true;
                    players.Add(tmp);
                }
                catch (Exception) {
                    break;
                }
            }
            serverTcp.Dispose();


        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            ArrangementClient(gameId);
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e) {
            if (isHost) {
                await TCP.SendString(serverTcp, "Close");
            }
            else {
                await TCP.SendString(serverTcp, "Confirm");
            }
        }
    }
}

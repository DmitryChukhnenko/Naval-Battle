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
        List<Player> players;
        CreateGameModel createGameModel;
        MainWindow mainWindow;
        int shipsCounter;
        List<int> shipsCounters;
        TcpClient serverTcp;

        public Arrangement(bool isHost, string gameId, Player player, List<Player> players, CreateGameModel createGameModel, MainWindow mainWindow, Task runServer, TcpClient serverTcp) {
            InitializeComponent();

            this.isHost = isHost;
            hasSent = false;
            this.gameId = gameId;
            this.player = player;
            this.players = players;
            this.createGameModel = createGameModel;
            this.mainWindow = mainWindow;
            this.serverTcp = serverTcp;

            shipsCounter = createGameModel.FleetSize;
            shipsCounters = createGameModel.Ships[createGameModel.FleetSizes.IndexOf(shipsCounter)];

            DataContext = player;            
                        
            if (isHost) {
                this.runServer = runServer;
            }            
        }

        private void SelectCell(object sender, RoutedEventArgs e) {
            if (hasSent) { MessageBox.Show("You have already sent your arrangement!"); return; }

            Button button = (Button)sender;
            OneCell cell = (OneCell)button.DataContext;

            if (cell.IsShipHere) {
                cell.IsShipHere = false;
                shipsCounter++;

                int length = OneCell.CountLength(0, cell, new OneCell(new XY()), player.Cells);
                shipsCounters[length]++;
                shipsCounters[length + 1]--;
            }

            else if (!cell.IsShipHere && shipsCounter != 0) {
                int count = 0;
                foreach (OneCell? cel in cell.Neighboors) {
                    if (cel is null) continue;
                    if (cel.IsShipHere) count++;
                    if (count > 1) return;
                }

                int length = OneCell.CountLength(0, cell, new OneCell(new XY()), player.Cells);
                if (shipsCounters[length] != 0) {
                    cell.IsShipHere = true;
                    shipsCounter--;
                    shipsCounters[length]--;
                    if (length - 1 >= 0)
                        shipsCounters[length - 1]++;
                }
            }
        }

        private async void ArrangementClient(string gameId) {
            await serverTcp.SendMessage(MessageType.Arrangement_ClientToServer_Player, player!);

            while (true) {
                try {
                    Message namesMessage = await serverTcp.ReceiveMessage();
                    if (namesMessage.Type == MessageType.Arrangement_ServerToClient_Close)
                        break;
                }
                catch (Exception) {
                    serverTcp.Dispose();
                    break;
                }
            }

            Game game = new Game(players, gameId, players.IndexOf(player), mainWindow, runServer, serverTcp);
            this.Visibility = Visibility.Hidden;
            game.ShowDialog();
        }

        private async void Continue(object sender, RoutedEventArgs e) {
            if (shipsCounter != 0) { MessageBox.Show($"Use all ships! Rest is {shipsCounter}"); return; }
            ArrangementClient(gameId);
            hasSent = true;
            if (isHost) {
                await serverTcp.SendMessage(MessageType.Arrangement_ClientToServer_Close);
            }
        }

        private async void Exit(object sender, RoutedEventArgs e) {
            if (isHost) {
                await serverTcp.SendMessage(MessageType.Arrangement_ClientToServer_Exit);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}

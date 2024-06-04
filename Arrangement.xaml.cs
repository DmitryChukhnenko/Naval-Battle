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
        MainWindow mainWindow;
        ArrangementModel arrangementModel;

        public Arrangement(bool isHost, string gameId, Player player, List<Player> players, CreateGameModel createGameModel, MainWindow mainWindow, Task runServer, TcpClient serverTcp) {
            InitializeComponent();

            arrangementModel = new ArrangementModel(isHost, false, gameId, player, players, createGameModel, serverTcp);

            DataContext = arrangementModel;            
                        
            if (isHost) {
                arrangementModel.RunServer = runServer;
            }            
        }

        private void SelectCell(object sender, RoutedEventArgs e) {
            if (arrangementModel.HasSent) { MessageBox.Show("You have already sent your arrangement!"); return; }

            Button button = (Button)sender;
            OneCell cell = (OneCell)button.DataContext;

            if (cell.IsShipHere) {
                cell.IsShipHere = false;
                arrangementModel.ShipsCounter++;

                int length = OneCell.CountLength(0, cell, new OneCell(new XY()));
                arrangementModel.ShipsCounters[length]++;
                arrangementModel.ShipsCounters[length + 1]--;
            }

            else if (!cell.IsShipHere && arrangementModel.ShipsCounter != 0) {
                int count = 0;
                foreach (OneCell? cel in cell.Neighboors) {
                    if (cel is null) continue;
                    if (cel.IsShipHere) count++;
                    if (count > 1) return;
                }

                int length = OneCell.CountLength(0, cell, new OneCell(new XY()));
                if (arrangementModel.ShipsCounters[length] != 0) {
                    cell.IsShipHere = true;
                    arrangementModel.ShipsCounter--;
                    arrangementModel.ShipsCounters[length]--;
                    if (length - 1 >= 0)
                        arrangementModel.ShipsCounters[length - 1]++;
                }
            }
        }

        private async Task ArrangementClient() {
            await arrangementModel.ServerTcp.SendMessage(MessageType.Arrangement_ClientToServer_Player, arrangementModel.Player!);
            arrangementModel.HasSent = true;

            while (true) {
                try {
                    Message namesMessage = await arrangementModel.ServerTcp.ReceiveMessage();
                    if (namesMessage.Type == MessageType.Arrangement_ServerToClient_Close) {
                        if (!arrangementModel.IsHost) await arrangementModel.ServerTcp.SendMessage(MessageType.Lobby_ClientToServer_Exit);
                        break;
                    }
                    else if (namesMessage.Type == MessageType.Arrangement_ServerToClient_Wait) {
                        MessageBox.Show("Wait for all players!");
                    }
                }
                catch (Exception ex) {
                    arrangementModel.ServerTcp.Dispose();
                    MessageBox.Show(ex.Message);
                    break;
                }
            }

            Game game = new Game(mainWindow, arrangementModel);
            this.Visibility = Visibility.Hidden;
            game.ShowDialog();
        }

        private async void Continue(object sender, RoutedEventArgs e) {            
            if (arrangementModel.IsHost) {
                await arrangementModel.ServerTcp.SendMessage(MessageType.Arrangement_ClientToServer_Close);
            }
        }

        private async void Exit(object sender, RoutedEventArgs e) {
            await arrangementModel.ServerTcp.SendMessage(MessageType.Arrangement_ClientToServer_Exit);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);

        private async void Send(object sender, RoutedEventArgs e) {
            if (arrangementModel.ShipsCounter != 0) {
                MessageBox.Show($"Use all ships! Rest is {arrangementModel.ShipsCounter}");
                return;
            }
            await ArrangementClient();
        }
    }
}

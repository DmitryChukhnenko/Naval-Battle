using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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

namespace Client {
    public static class GTVisualTreeHelper
    {
        //Finds the visual parent.
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null)
            {
                return (null);
            }

            //get parent item
            DependencyObject parentObj = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObj == null) return null;

            // check if the parent matches the type we are requested
            T parent = parentObj as T;

            if (parent != null)
            {
                return parent;
            }
            else
            {
                // here, To find the next parent in the tree. we are using recursion until we found the requested type or reached to the end of tree.
                return FindVisualParent<T>(parentObj);
            }
        }
    }

    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Window {
        GameModel gameModel;
        int index;
        Task runServer;
        Task runClient;
        MainWindow mainWindow;
        TcpClient serverTcp;

        public Game(List<Player> players, string gameId, int index, MainWindow mainWindow) {
            InitializeComponent();
            gameModel = new GameModel(players, gameId);
            this.index = index;
            this.mainWindow = mainWindow;
            serverTcp = new TcpClient(gameId, 2024);

            Server server = new Server();
            runServer = Task.Run(() => server.HostServer(gameId, server.GameListenToClient));

            runClient = Task.Run(() => ArrangementClient());

            DataContext = gameModel;
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;
            OneCell cell = (OneCell) button.DataContext;
            GroupBox groupBox = GTVisualTreeHelper.FindVisualParent<GroupBox>(button);
            Player player = (Player)groupBox.DataContext;

            if (gameModel.Turn != index) return;

            cell.IsFogHere = false;
            if (cell.IsShipHere)
            {
                cell.IsDamagedShipHere = true;
                player.FleetSize--;
                if (player.FleetSize == 0) {
                    player.HasLost = true;

                    int notLost = 0;
                    Player last = new Player();
                    foreach (Player p in gameModel.Players)
                    {
                        if (!p.HasLost) { notLost++; last = p; }
                    }
                    if (notLost == 1) gameModel.Winner = last;
                    gameModel.IsRunning = notLost>1;
                }
            }
            
            await TCP.SendVariable(serverTcp, JsonSerializer.SerializeToUtf8Bytes(gameModel, typeof(GameModel)));
        }

        
        private async Task ArrangementClient()
        {
            while (gameModel.IsRunning)
            {
                try
                {
                    gameModel = (GameModel)JsonSerializer.Deserialize(await TCP.ReceiveVariable(serverTcp), typeof(GameModel))!;
                    gameModel.Turn = (gameModel.Turn+1) % gameModel.Players.Count;
                    if (gameModel.Winner is not null) MessageBox.Show($"Game ended! Winner is {gameModel.Winner.Nickname}.");
                    foreach (Player p in gameModel.Players) {
                        if (!p.HasLost) {
                            foreach (OneCell cell in p.Cells) {
                                if (index != gameModel.Players.IndexOf(p)) cell.IsFogHere = true;
                                cell.AddNeighboors(p.Cells);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
            serverTcp.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}

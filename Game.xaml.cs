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

        public Game(List<Player> players, string gameId, int index, MainWindow mainWindow, Task runServer, TcpClient serverTcp) {
            InitializeComponent();
            gameModel = new GameModel(players, gameId);
            this.index = index;
            this.mainWindow = mainWindow;            

            this.runServer = runServer;

            this.serverTcp = serverTcp;

            runClient = Task.Run(() => ArrangementClient());

            DataContext = gameModel;
        }

        private async void Move(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;
            OneCell cell = (OneCell) button.DataContext;
            GroupBox groupBox = GTVisualTreeHelper.FindVisualParent<GroupBox>(button);
            Player player = (Player)groupBox.DataContext;

            if (cell is null || gameModel.Turn != index || !cell.IsFogHere || player.Cells.Contains(cell)) return;

            await serverTcp.SendMessage(MessageType.Game_ClientToServer_NicknameCell, new NicknameCell(player.Nickname, cell));
        }

        
        private async Task ArrangementClient() {
            while (gameModel.IsRunning) {
                try {
                    Message message = await serverTcp.ReceiveMessage();

                    if (message.Type == MessageType.Game_ServerToClient_YouLost) {
                        gameModel.Players[index].HasLost = true;
                    }

                    else if (message.Type == MessageType.Game_ServerToClient_Winner) {
                        gameModel.IsRunning = false;

                        message.ExpectType(MessageType.Game_ServerToClient_Winner);
                        gameModel.Winner = message.Deserialize1Arg<Player>();
                        if (gameModel.Winner is not null) MessageBox.Show($"Game ended! Winner is {gameModel.Winner.Nickname}.");
                    }

                    else {
                        message.ExpectType(MessageType.Game_ServerToClient_NicknameCell);
                        NicknameCell nicknameCell = message.Deserialize1Arg<NicknameCell>();
                        gameModel.Turn++;
                        MessageBox.Show(nicknameCell.Nickname);
                    }
                }
                catch (Exception) {
                    gameModel.IsRunning = false;
                }
            }
            serverTcp.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}

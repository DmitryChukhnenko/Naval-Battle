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
        MainWindow mainWindow;

        public Game(MainWindow mainWindow, ArrangementModel arrangementModel) {
            InitializeComponent();

            gameModel = new GameModel(arrangementModel, Task.Run(() => GameClient()));
            this.mainWindow = mainWindow;     

            DataContext = gameModel;
        }

        private async void Move(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;
            OneCell cell = (OneCell) button.DataContext;
            GroupBox groupBox = GTVisualTreeHelper.FindVisualParent<GroupBox>(button);
            Player player = (Player)groupBox.DataContext;

            if (cell is not null && gameModel.Turn == gameModel.Index && cell.IsFogHere && player.Cells.Contains(cell)) 
                await gameModel.ServerTcp.SendMessage(MessageType.Game_ClientToServer_NicknameCell, new NicknameCell(player.Nickname, cell));
        }

        
        private async Task GameClient() {
            while (gameModel.IsRunning) {
                try {
                    Message message = await gameModel.ServerTcp.ReceiveMessage();

                    switch (message.Type) {                        
                        case MessageType.Game_ServerToClient_YouLost: {
                                gameModel.Players[gameModel.Index].HasLost = true;
                                break;
                            }                            
                        case MessageType.Game_ServerToClient_Move: {
                                message.ExpectType(MessageType.Game_ServerToClient_Move);
                                Move move = message.Deserialize1Arg<Move>();
                                gameModel.Turn++;

                                Player player = new();
                                foreach (Player plr in gameModel.Players) {
                                    if (plr.Nickname == move.Nickname) player = plr;
                                }
                                OneCell cell = new OneCell();
                                foreach (OneCell cel in player.Cells) {
                                    if (cel.Point.Equals(move.Cell.Point)) cell = cel;
                                }
                                cell = move.Cell;

                                MessageBox.Show(move.Message);

                                break;
                            }
                        case MessageType.Game_ServerToClient_ChangedCells: {
                                message.ExpectType(MessageType.Game_ServerToClient_ChangedCells);
                                PlayerList playerList = message.Deserialize1Arg<PlayerList>();

                                Player player = new();
                                foreach (Player plr in gameModel.Players) {
                                    if (plr.Nickname == playerList.Nickname) player = plr;
                                }

                                List<OneCell> changed = playerList.Changed;
                                XY startPoint = changed[0].Point;
                                XY endPoint = changed[^1].Point;
                                int rowBig = (int) Math.Sqrt(player.Cells.Count);
                                int rowSml = endPoint.X-startPoint.X;

                                for (int y = startPoint.Y; y < endPoint.Y; y++) {
                                    for (int x = startPoint.X; x < endPoint.X; x++) {
                                        player.Cells[y*rowBig + x] = changed[y*rowSml + x];
                                    }
                                }

                                break;
                            }
                        case MessageType.Game_ServerToClient_Winner: {
                                gameModel.IsRunning = false;

                                message.ExpectType(MessageType.Game_ServerToClient_Winner);
                                gameModel.Winner = message.Deserialize1Arg<Player>();
                                if (gameModel.Winner is not null) MessageBox.Show($"Game ended! Winner is {gameModel.Winner.Nickname}.");

                                break;
                            }
                        default: break;
                    }
                }
                catch (Exception ex) {
                    gameModel.IsRunning = false;
                    MessageBox.Show(ex.Message);
                }
            }
            gameModel.ServerTcp.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}

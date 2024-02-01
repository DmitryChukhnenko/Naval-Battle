using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Client {
    /// <summary>
    /// Логика взаимодействия для CreateGame.xaml
    /// </summary>
    public partial class CreateGame : Window {
        MainWindow mainWindow;
        string nickname;
        CreateGameModel createGameModel;
        public CreateGame(MainWindow mainWindow, string nickname) {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.nickname = nickname;
            createGameModel = new CreateGameModel();
            DataContext = createGameModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Lobby lobby = new Lobby(nickname, createGameModel, mainWindow);
            this.Visibility = Visibility.Hidden;
            lobby.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        MainWindowModel mainMenuModel;
        public CreateGame create;
        public Lobby lobby;

        public MainWindow() {
            InitializeComponent();
            mainMenuModel = new MainWindowModel();
            DataContext = mainMenuModel;
        }

        private void PlayWithId(object sender, RoutedEventArgs e) {
            if (mainMenuModel.GameId == "") {
                MessageBox.Show("Input game id");
                return;
            }
            if (mainMenuModel.Nickname == "") {
                MessageBox.Show("Input your nickname");
                return;
            }
            if (mainMenuModel.Nickname == "Close") {
                MessageBox.Show("Input another nickname");
                return;
            }

            lobby = new Lobby(mainMenuModel.Nickname, mainMenuModel.GameId, this);
            this.Visibility = Visibility.Hidden;
            lobby.ShowDialog();            
        }

        private void HostGame(object sender, RoutedEventArgs e) {
            if (mainMenuModel.Nickname == "") {
                MessageBox.Show("Input your nickname");
                return;
            }
            if (mainMenuModel.Nickname == "Close") {
                MessageBox.Show("Input another nickname");
                return;
            }

            create = new CreateGame(this, mainMenuModel.Nickname);
            this.Visibility = Visibility.Hidden;
            create.ShowDialog();
        }

        public void GoBack(Window window) {
            Visibility = Visibility.Visible;
            if (create is not null && create != window) create.Close();
            else if (lobby is not null && create != window) lobby.Close();
        }
    }
}

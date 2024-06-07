using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Client {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
public partial class MainWindow : System.Windows.Window {
        MainWindowModel mainMenuModel;
        public CreateGame create;
        public Lobby lobby;
        public Saves saves;

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
            if (Check()) return;

            lobby = new Lobby(mainMenuModel.Nickname, mainMenuModel.GameId, this);
            this.Visibility = Visibility.Hidden;
            lobby.ShowDialog();            
        }

        private void HostGame(object sender, RoutedEventArgs e) {
            if (Check()) return;

            create = new CreateGame(this, mainMenuModel.Nickname);
            this.Visibility = Visibility.Hidden;
            create.ShowDialog();
        }

        public void GoBack(System.Windows.Window window) {
            Visibility = Visibility.Visible;
            if (create is not null && create != window) create.Close();
            else if (lobby is not null && lobby != window) lobby.Close();
            else if (saves is not null && saves != window) saves.Close();
        }

        private bool Check() {
            if (mainMenuModel.Nickname == "") {
                MessageBox.Show("Input your nickname");
                return true;
            }
            return false;
        }

        private void Saves(object sender, RoutedEventArgs e) {
            saves = new Saves(this);
            this.Visibility = Visibility.Hidden;
            saves.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (create is not null) create.Close();
            else if (lobby is not null) lobby.Close();
            else if (saves is not null) saves.Close();
        }
    }
}

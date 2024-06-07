using GameData;
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

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для Saves.xaml
    /// </summary>
    public partial class Saves : Window
    {
        MainWindow mainWindow;
        public Saves(MainWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            using GameDataContext db = new();
            DataContext = db.Saves.ToArray();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => mainWindow.GoBack(this);
    }
}

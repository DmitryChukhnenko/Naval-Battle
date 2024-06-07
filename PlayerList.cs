using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    internal class PlayerList{
        public string Nickname { get; set; }
        public ObservableCollection<OneCell> Changed { get; set; }

        public PlayerList(string nickname, ObservableCollection<OneCell> changed) {
            Nickname = nickname;
            Changed = changed;
        }

        public PlayerList() { }
    }
}

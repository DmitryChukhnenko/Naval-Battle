using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    internal class PlayerList{
        public List<Player> Players { get; set; }

        public PlayerList(List<Player> players) {
            Players = players;
        }

        public PlayerList() { }
    }
}

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    internal class PlayerList{
        public string Nickname { get; set; }
        public List<OneCell> Changed { get; set; }

        public PlayerList(string nickname, List<OneCell> changed) {
            Nickname = nickname;
            Changed = changed;
        }

        public PlayerList() { }
    }
}

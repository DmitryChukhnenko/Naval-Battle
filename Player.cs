using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public class Player :BindableBase {
        private string nickname;
        public string Nickname {
            get => nickname;
            set => SetProperty(ref nickname, value);
        }

        private bool isEnemy;
        public bool IsEnemy {
            get => isEnemy;
            set => SetProperty(ref isEnemy, value);
        }

        private List<OneCell> cells = new List<OneCell> { };
        public List<OneCell> Cells {
            get => cells;
            set => SetProperty(ref cells, value);
        }

        public void Add(OneCell cell) {
            cells.Add(cell);
            Cells = cells;
        }
        public void Remove(OneCell cell) {
            cells.Remove(cell);
            Cells = cells;
        }

        public Player(string nickname, List<OneCell> cells) {
            Nickname = nickname;
            IsEnemy = false;
            Cells = cells;
        }
    }
}

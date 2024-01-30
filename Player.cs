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

        private bool hasLost;
        public bool HasLost
        {
            get => hasLost;
            set => SetProperty(ref hasLost, value);
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

        private int fieldSize;
        public int FieldSize
        {
            get => fieldSize;
            set => SetProperty(ref fieldSize, value);
        }

        private int fleetSize;
        public int FleetSize
        {
            get => fleetSize;
            set => SetProperty(ref fleetSize, value);
        }

        public Player(string nickname, List<OneCell> cells, int fleetSize) {
            Nickname = nickname;
            IsEnemy = false;
            HasLost = false;
            Cells = cells;
            FieldSize = (int) Math.Sqrt(cells.Count);
            FleetSize = fleetSize;
        }

        public Player(){}
    }
}

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
            HasLost = false;
            Cells = cells;
            FieldSize = (int) Math.Sqrt(cells.Count);
            FleetSize = fleetSize;
        }

        public Player(){}

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (!(obj is Player)) return false;
            Player other = obj as Player;

            if (other!.Nickname != Nickname) return false;
            if (other.HasLost != HasLost) return false;
            if (other.Cells.Count != Cells.Count) return false;
            if (other.FieldSize != FieldSize) return false;
            if (other.FleetSize != FleetSize) return false;
            return true;
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }
    }
}

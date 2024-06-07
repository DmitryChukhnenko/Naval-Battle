using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ObservableCollection<OneCell> cells = new ObservableCollection<OneCell> { };
        public ObservableCollection<OneCell> Cells {
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

        public Player(string nickname, ObservableCollection<OneCell> cells, int fleetSize) {
            Nickname = nickname;
            HasLost = false;
            Cells = cells;
            FieldSize = Squares.QuadNums[cells.Count];
            FleetSize = fleetSize;
        }

        public Player(){}

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (!(obj is Player)) return false;
            Player other = obj as Player;

            if (other!.Nickname == Nickname && other.HasLost == HasLost && other.Cells.Count == Cells.Count && other.FieldSize == FieldSize && other.FleetSize == FleetSize) return true;
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}

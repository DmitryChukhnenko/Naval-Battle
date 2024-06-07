using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace Client {
    public class OneCell : BindableBase {
        public XY Point { get; set; }

        public bool isShipHere = false;
        public bool IsShipHere {
            get => isShipHere;
            set => SetProperty(ref isShipHere, value);
        }

        public bool isDamagedShipHere = false;
        public bool IsDamagedShipHere {
            get => isDamagedShipHere;
            set => SetProperty(ref isDamagedShipHere, value);
        }

        public bool isFogHere = false;
        public bool IsFogHere
        {
            get => isFogHere;
            set => SetProperty(ref isFogHere, value);
        }

        
        public OneCell(XY point) {
            Point = point;
        }

        public OneCell(XY point, bool isShipHere, bool isDamagedShipHere, bool isFogHere) : this(point) {
            IsShipHere = isShipHere;
            IsDamagedShipHere = isDamagedShipHere;
            IsFogHere = isFogHere;
        }

        public OneCell() { }

        [JsonIgnore] public List<OneCell?> Neighboors { get; set; } = new List<OneCell?>();
        public void AddNeighboors(ObservableCollection<OneCell> cells) {
            for (int i = 0; i <= 8; i++) {
                if (i == 4) Neighboors.Add(null);
                else Neighboors.Add(FindCell(cells, new XY(Point.X + (i%3)-1, Point.Y + (i / 3) - 1)));
            }
        }

        public static OneCell? FindCell(ObservableCollection<OneCell> cells, XY coords) {
            int row = Squares.QuadNums[cells.Count];
            return (coords.X >= 0 && coords.Y >= 0 && coords.X < row && coords.Y < row) ? cells[coords.X + coords.Y*row] : null;
        }
         
        public static int CountLength(int length, OneCell cell, OneCell previous) {
            OneCell? left = cell.Neighboors[3];
            OneCell? up = cell.Neighboors[1];
            OneCell? right = cell.Neighboors[5];
            OneCell? down = cell.Neighboors[7];

            if (left is not null && left.IsShipHere && !previous.Equals(left)) return CountLength(++length, left, cell);
            else if (up is not null && up.IsShipHere && !previous.Equals(up)) return CountLength(++length, up, cell);
            else if (right is not null && right.IsShipHere && !previous.Equals(right)) return CountLength(++length, right, cell);
            else if (down is not null && down.IsShipHere && !previous.Equals(down)) return CountLength(++length, down, cell);
            return length;
        }

        public static bool IsAlive(OneCell cell, OneCell previous) {
            OneCell? left = cell.Neighboors[3];
            OneCell? up = cell.Neighboors[1];
            OneCell? right = cell.Neighboors[5];
            OneCell? down = cell.Neighboors[7];

            if (left is not null && left.IsShipHere && !previous.Equals(left)) {
                if (left.IsDamagedShipHere) return IsAlive(left, cell);
                else return true;
            }
            if (up is not null && up.IsShipHere && !previous.Equals(up)) {
                if (up.IsDamagedShipHere) return IsAlive(up, cell);
                else return true;
            }
            if (right is not null && right.IsShipHere && !previous.Equals(right)) {
                if (right.IsDamagedShipHere) return IsAlive(right, cell);
                else return true;
            }
            if (down is not null && down.IsShipHere && !previous.Equals(down)) {
                if (down.IsDamagedShipHere) return IsAlive(down, cell);
                else return true;
            }
            if (cell.IsShipHere && cell.IsDamagedShipHere) return false;
            else return true;
        }

        public static List<OneCell> AfterSunk (List<OneCell> result, OneCell cell, OneCell previous) {
            OneCell? left = cell.Neighboors[3];
            OneCell? up = cell.Neighboors[1];
            OneCell? right = cell.Neighboors[5];
            OneCell? down = cell.Neighboors[7];

            result.AddRange(cell.Neighboors.Where(cel => cel is not null)!);

            if (left is not null && left.IsShipHere && left.IsDamagedShipHere && !previous.Equals(left)) AfterSunk(result, left, cell);
            if (up is not null && up.IsShipHere && up.IsDamagedShipHere && !previous.Equals(up)) AfterSunk(result, up, cell);            
            if (right is not null && right.IsShipHere && right.IsDamagedShipHere && !previous.Equals(right)) AfterSunk(result, right, cell);
            if (down is not null && down.IsShipHere && down.IsDamagedShipHere && !previous.Equals(down)) AfterSunk(result, down, cell);

            if (cell.IsShipHere && cell.IsDamagedShipHere) result.Add(cell);
            return result;
        }

        public static ObservableCollection<OneCell> CreateEmptyList(int fieldSize) {
            ObservableCollection<OneCell> cells = new ObservableCollection<OneCell> { };
            for (int y = 0; y < Squares.QuadNums[fieldSize]; y++) {
                for (int x = 0; x < Squares.QuadNums[fieldSize]; x++) {
                    OneCell cell = new OneCell(new XY (x, y));
                    cells.Add(cell);
                }
            }
            foreach (OneCell cell in cells) {
                cell.AddNeighboors(cells);
            }
            return cells;
        }

        public void CopyFrom(OneCell cell) {
            Point = cell.Point;
            IsShipHere = cell.IsShipHere;
            IsFogHere = cell.IsFogHere;
            IsDamagedShipHere = cell.IsDamagedShipHere;
        }

        public override bool Equals(object obj) {
            OneCell? cell = obj as OneCell;
            if (cell is null)
                return false;

            if (cell.Point.Equals(Point) && cell.IsShipHere == IsShipHere && cell.IsFogHere == IsFogHere && cell.IsDamagedShipHere == IsDamagedShipHere)
                return true;
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}

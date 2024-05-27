using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace Client {
    public class OneCell : BindableBase {
        public XY Point;

        public bool isShipHere;
        public bool IsShipHere {
            get => isShipHere;
            set => SetProperty(ref isShipHere, value);
        }

        public bool isDamagedShipHere;
        public bool IsDamagedShipHere {
            get => isShipHere;
            set => SetProperty(ref isDamagedShipHere, value);
        }

        public bool isFogHere;
        public bool IsFogHere
        {
            get => isFogHere;
            set => SetProperty(ref isFogHere, value);
        }

        
        public OneCell(XY point) {
            Point = point;
            IsShipHere = false;
            IsDamagedShipHere = false;
            IsFogHere = false;
        }

        public OneCell(XY point, bool isShipHere, bool isDamagedShipHere, bool isFogHere) : this(point) {
            IsShipHere = isShipHere;
            IsDamagedShipHere = isDamagedShipHere;
            IsFogHere = isFogHere;
        }

        public OneCell() { }

        [JsonIgnore] public OneCell?[,] Neighboors { get; set; } = new OneCell[3, 3];
        public void AddNeighboors(List<OneCell> cells) {
            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <=1; x++) {
                    if (x == 0 && y == 0) Neighboors[1 + y, 1 + x] = null;
                    else Neighboors[1 + y, 1 + x] = FindCell(cells, new XY(Point.X + x, Point.Y + y));
                }
            }
        }

        private static OneCell? FindCell(List<OneCell> cells, XY coords) {
            int row = (int)Math.Sqrt(cells.Count);
            return (coords.X >= 0 && coords.Y >= 0 && coords.X < row && coords.Y < row) ? cells[coords.X + coords.Y*row] : null;
        }
         
        public static int CountLength(int length, OneCell cell, OneCell previous, List<OneCell> cells) {
            OneCell? left = cell.Neighboors[1, 0];
            OneCell? up = cell.Neighboors[0, 1];
            OneCell? right = cell.Neighboors[1, 2];
            OneCell? down = cell.Neighboors[2, 1];

            if (left is not null && left.IsShipHere && !previous.Equals(left)) return CountLength(++length, left, cell, cells);
            else if (up is not null && up.IsShipHere && !previous.Equals(up)) return CountLength(++length, up, cell, cells);
            else if (right is not null && right.IsShipHere && !previous.Equals(right)) return CountLength(++length, right, cell, cells);
            else if (down is not null && down.IsShipHere && !previous.Equals(down)) return CountLength(++length, down, cell, cells);
            return length;
        }

        public static List<OneCell> CreateEmptyList(int fieldSize) {
            List<OneCell> cells = new List<OneCell> { };
            for (int y = 0; y < Math.Sqrt(fieldSize); y++) {
                for (int x = 0; x < Math.Sqrt(fieldSize); x++) {
                    OneCell cell = new OneCell(new XY (x, y));
                    cells.Add(cell);
                }
            }
            foreach (OneCell cell in cells) {
                cell.AddNeighboors(cells);
            }
            return cells;
        }

        public override bool Equals(object obj) {
            OneCell cell = new();
            if (obj is OneCell) {
                cell = obj as OneCell;
            }
            else return false;

            if (cell.Point.Equals(Point) && cell.IsShipHere == IsShipHere && cell.IsFogHere == IsFogHere && cell.IsDamagedShipHere == IsDamagedShipHere)
                return true;
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}

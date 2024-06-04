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

        public bool isShipHere = false;
        public bool IsShipHere {
            get => isShipHere;
            set => SetProperty(ref isShipHere, value);
        }

        public bool isDamagedShipHere = false;
        public bool IsDamagedShipHere {
            get => isShipHere;
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
        public void AddNeighboors(List<OneCell> cells) {
            for (int i = 0; i <= 8; i++) {
                if (i == 4) Neighboors.Add(null);
                else Neighboors.Add(FindCell(cells, new XY(Point.X + (i%3)-1, Point.Y + (i / 3) - 1)));
            }
        }

        private static OneCell? FindCell(List<OneCell> cells, XY coords) {
            int row = (int)Math.Sqrt(cells.Count);
            return (coords.X >= 0 && coords.Y >= 0 && coords.X < row && coords.Y < row) ? cells[coords.X + coords.Y*row] : null;
        }
         
        public static int CountLength(int length, OneCell cell, OneCell previous) {
            OneCell? left = cell.Neighboors[3];
            OneCell? up = cell.Neighboors[1];
            OneCell? right = cell.Neighboors[5];
            OneCell? down = cell.Neighboors[7];

            if (left is not null && left.IsShipHere) return CountLength(++length, left, cell);
            else if (up is not null && up.IsShipHere) return CountLength(++length, up, cell);
            else if (right is not null && right.IsShipHere) return CountLength(++length, right, cell);
            else if (down is not null && down.IsShipHere) return CountLength(++length, down, cell);
            return length;
        }

        public static bool IsAlive(OneCell cell, OneCell previous) {
            OneCell? left = cell.Neighboors[3];
            OneCell? up = cell.Neighboors[1];
            OneCell? right = cell.Neighboors[5];
            OneCell? down = cell.Neighboors[7];

            if (left is not null && left.IsShipHere) {
                if (left.IsDamagedShipHere) return IsAlive(left, cell);
                else return true;
            }
            if (up is not null && up.IsShipHere) {
                if (up.IsDamagedShipHere) return IsAlive(up, cell);
                else return true;
            }
            if (right is not null && right.IsShipHere) {
                if (right.IsDamagedShipHere) return IsAlive(right, cell);
                else return true;
            }
            if (down is not null && down.IsShipHere) {
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

            if (left is not null && left.IsShipHere && left.IsDamagedShipHere) AfterSunk(result, left, cell);
            if (up is not null && up.IsShipHere && up.IsDamagedShipHere) AfterSunk(result, up, cell);            
            if (right is not null && right.IsShipHere && right.IsDamagedShipHere) AfterSunk(result, right, cell);
            if (down is not null && down.IsShipHere && down.IsDamagedShipHere) AfterSunk(result, down, cell);

            if (cell.IsShipHere && cell.IsDamagedShipHere) result.Add(cell);
            return result.Distinct().OrderBy(cel => cel.Point.X*cel.Point.Y).ToList();
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

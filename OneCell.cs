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
        public Point Point;

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

        
        public OneCell(Point point) {
            Point = point;
            IsShipHere = false;
            IsDamagedShipHere = false;
            IsFogHere = false;
        }

        public OneCell(Point point, bool isShipHere, bool isDamagedShipHere, bool isFogHere) : this(point) {
            IsShipHere = isShipHere;
            IsDamagedShipHere = isDamagedShipHere;
            IsFogHere = isFogHere;
        }

        public OneCell() { }

        [JsonIgnore] public OneCell[,] Neighboors { get; set; } = new OneCell[3, 3];

        public void AddNeighboors(List<OneCell> cells) {
            for (int y = -1; y < 2; y++) {
                for (int x = -1; x < 2; x++) {
                    if (x == 0 && y == 0) Neighboors[1 + y, 1 + x] = new OneCell(new Point(Point.X + x, Point.Y + y));
                    else {
                        OneCell? result = cells.Find((OneCell cell) => cell.Point == new Point(Point.X + x, Point.Y + y));
                        if (result is null) result = new OneCell(new Point(Point.X + x, Point.Y + y));
                        Neighboors[1 + y, 1 + x] = result;
                    }
                }
            }
        }

        public static int CountLength(int length, OneCell cell, OneCell previous) {
            if (cell.Neighboors[0, 1].IsShipHere && previous != cell.Neighboors[0, 1]) return CountLength(++length, cell.Neighboors[0, 1], cell);
            else if (cell.Neighboors[2, 1].IsShipHere && previous != cell.Neighboors[2, 1]) return CountLength(++length, cell.Neighboors[2, 1], cell);
            else if (cell.Neighboors[1, 0].IsShipHere && previous != cell.Neighboors[1, 0]) return CountLength(++length, cell.Neighboors[1, 0], cell);
            else if (cell.Neighboors[1, 2].IsShipHere && previous != cell.Neighboors[1, 2]) return CountLength(++length, cell.Neighboors[1, 2], cell);
            return length;
        }
    }
}

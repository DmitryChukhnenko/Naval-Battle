using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public class XY {
        public int X { get; set; }
        public int Y { get; set; }
        public XY() { X = 0; Y = 0; }
        public XY(int x, int y) { X = x; Y = y; }

        public override bool Equals(object obj) {
            XY xy = new();
            if (obj is XY) {
                xy = obj as XY;
            }
            else return false;

            if (xy.X == X && xy.Y == Y)
                return true;
            return false;           
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}

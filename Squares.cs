using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public static class Squares {
        public static Dictionary<int, int> QuadNums { get; } = new Dictionary<int, int> { 
            {1, 1},
            {4, 2},
            {9, 3},
            {16, 4},
            {25, 5},
            {36, 6},
            {49, 7},
            {64, 8},
            {81, 9},
            {100, 10},
            {121, 11},
            {144, 12},
            {169, 13},
            {196, 14},
            {225, 15},
            {256, 16},
            {289, 17},
            {324, 18},
            {361, 19},
            {400, 20},
        };
        public static Dictionary<int, int> NumQuads { get; } = new Dictionary<int, int> {
            {1, 1},
            {2, 4},
            {3, 9},
            {4, 16},
            {5, 25},
            {6, 36},
            {7, 49},
            {8, 64},
            {9, 81},
            {10, 100},
            {11, 121},
            {12, 144},
            {13, 169},
            {14, 196},
            {15, 225},
            {16, 256},
            {17, 289},
            {18, 324},
            {19, 361},
            {20, 400},
        };
    }
}

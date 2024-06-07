using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public class NicknamePoint {
        public string Nickname { get; set; } = string.Empty;
        public XY Point { get; set; }
        public NicknamePoint() { }
        public NicknamePoint(string nickname, XY point) {
            Nickname = nickname;
            Point = point;
        }
    }
}

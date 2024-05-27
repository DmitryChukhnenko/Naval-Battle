using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public class NicknameCell {
        public string Nickname { get; set; }
        public OneCell Cell { get; set; }
        public NicknameCell() { }
        public NicknameCell(string nickname, OneCell cell) {
            Nickname = nickname;
            Cell = cell;
        }
    }
}

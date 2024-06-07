using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public class Move {
        public string Message { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public OneCell Cell { get; set; }
        public Move() { }
        public Move(string message, string nickname, OneCell cell){
            Message = message;
            Nickname = nickname;
            Cell = cell;
        }
    }
}

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    internal class MainWindowModel : BindableBase {
        private string gameId;
        public string GameId {
            get => gameId;
            set => SetProperty(ref gameId, value);
        }

        private string nickname;
        public string Nickname {
            get => nickname;
            set => SetProperty(ref nickname, value);
        }

        public MainWindowModel() {
            gameId = string.Empty;
            nickname = string.Empty;
        }
    }
}

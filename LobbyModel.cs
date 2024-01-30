using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    internal class LobbyModel : BindableBase {
        private string gameId = "";
        public string GameId {
            get => gameId;
            set => SetProperty(ref gameId, value);
        }

        private IList<string> players = new List<string> {};
        public IList<string> Players {
            get => players;
            set => SetProperty(ref players, value);
        }

        public void Add(string player) {
            players.Add(player);
            Players = players;
        }
        public void Remove(string player) {
            players.Remove(player);
            Players = players;
        }

        public LobbyModel() {}
    }
}

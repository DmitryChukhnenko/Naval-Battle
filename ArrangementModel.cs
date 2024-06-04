using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client {
    public class ArrangementModel : BindableBase {
        public bool IsHost {get; set;}
        public bool HasSent { get; set; }
        public string GameId { get; set; }
        public Task RunServer { get; set; }
        public List<Player> Players { get; set; }
        public CreateGameModel CreateGameModel { get; set; }
        public int ShipsCounter { get; set; }
        public List<int> ShipsCounters { get; set; }
        public TcpClient ServerTcp { get; set; }

        private Visibility isHostVisibility;
        public Visibility IsHostVisibility {
            get => isHostVisibility;
            set => SetProperty(ref isHostVisibility, value);
        }

        private Player player;
        public Player Player {
            get => player;
            set => SetProperty(ref player, value);
        }

        private int fieldSize;
        public int FieldSize {
            get => fieldSize;
            set => SetProperty(ref fieldSize, value);
        }

        public ArrangementModel(bool isHost, bool hasSent, string gameId, Player player, List<Player> players, CreateGameModel createGameModel, TcpClient serverTcp) {
            IsHost = isHost;
            IsHostVisibility = isHost ? Visibility.Visible : Visibility.Collapsed;
            HasSent = hasSent;
            GameId = gameId;
            Player = player;
            Players = players;
            CreateGameModel = createGameModel;

            FieldSize = (int) Math.Sqrt(createGameModel.FieldSize);
            ShipsCounter = createGameModel.FleetSize;
            ShipsCounters = createGameModel.Ships[createGameModel.FleetSizes.IndexOf(ShipsCounter)];

            ServerTcp = serverTcp;
        }
    }
}

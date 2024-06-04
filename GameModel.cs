using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class GameModel : BindableBase
    {         
        public int Index {get; set;}
        public Task RunServer { get; set; }
        public Task RunClient { get; set; }
        public TcpClient ServerTcp { get; set; }

        private List<Player> players;
        public List<Player> Players {
            get => players;
            set => SetProperty(ref players, value);
        }

        private Player winner;
        public Player Winner
        {
            get => winner;
            set => SetProperty(ref winner, value);
        }

        private int turn;
        public int Turn
        {
            get => turn;
            set => SetProperty(ref turn, value);
        }

        private string gameId;
        public string GameId
        {
            get => gameId;
            set => SetProperty(ref gameId, value);
        }

        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            set => SetProperty(ref isRunning, value);
        }

        private int fieldSize;
        public int FieldSize {
            get => fieldSize;
            set => SetProperty(ref fieldSize, value);
        }

        public GameModel(ArrangementModel arrangementModel, Task runClient)
        {
            Index = arrangementModel.Players.IndexOf(arrangementModel.Player);
            RunServer = arrangementModel.RunServer;
            RunClient = runClient;
            ServerTcp = arrangementModel.ServerTcp;
            Players = arrangementModel.Players;
            GameId = arrangementModel.GameId;
            FieldSize = arrangementModel.FieldSize;
            Turn = 0;
            IsRunning = true;
        }
    }
}

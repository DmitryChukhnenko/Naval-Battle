using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class GameModel : BindableBase
    {
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

        public GameModel(List<Player> players, string gameId)
        {
            Players = players;
            Turn = 0;
            GameId = gameId;
            IsRunning = true;
        }
    }
}

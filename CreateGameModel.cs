using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public enum Regime {
        Classic = 0
    }

    public class CreateGameModel : BindableBase {
        private Dictionary<int, int> Fleet_Field { get; } = new Dictionary<int, int> {};
        private Dictionary<int, int> Field_Fleet { get; }

        public IList<int> FleetSizes { get; }
        public IList<int> FieldSizes { get; }
        public IList<Regime> Regimes { get; }

        public List<List<int>> Ships { get; } = new List<List<int>>{
            new List<int> { 0, 0, 0, 0, 0, 0 },
            new List<int> { 0, 0, 0, 0, 0, 0 },
            new List<int> { 2, 0, 0, 0, 0, 0 },
            new List<int> { 3, 0, 0, 0, 0, 0 },
            new List<int> { 3, 1, 0, 0, 0, 0 },
            new List<int> { 3, 2, 0, 0, 0, 0 },
            new List<int> { 3, 2, 1, 0, 0, 0 },
            new List<int> { 4, 3, 1, 0, 0, 0 },
            new List<int> { 4, 3, 2, 0, 0, 0 },
            new List<int> { 4, 3, 2, 1, 0, 0 },
            new List<int> { 6, 4, 2, 1, 0, 0 },
            new List<int> { 6, 5, 3, 1, 0, 0 },
            new List<int> { 7, 5, 3, 2, 0, 0 },
            new List<int> { 7, 5, 3, 2, 1, 0 },
            new List<int> { 8, 6, 4, 2, 1, 0 },
            new List<int> { 8, 7, 4, 3, 1, 1 },
            new List<int> { 9, 7, 4, 3, 2, 1 },
            new List<int> { 9, 8, 4, 3, 2, 1 },
            new List<int> { 9, 8, 5, 4, 2, 1 },
            new List<int> { 9, 8, 6, 4, 3, 1 }
        };

        private int fleetSize;
        public int FleetSize {
            get => fleetSize;
            set {
                SetProperty(ref fleetSize, value);
                FieldSize = Field_Fleet[value];
            }
        }

        private int fieldSize;
        public int FieldSize {
            get => fieldSize;
            set {
                SetProperty(ref fieldSize, value);
                FleetSize = Fleet_Field[value];
            }
        }

        private Regime regime;
        private Regime Regime {
            get => regime;
            set => SetProperty(ref regime, value);
        }

        public CreateGameModel() {         
            for (int i = 3; i<=20; i++) Fleet_Field.Add(i * i / 5, i * i);
            Field_Fleet = Field_Fleet!.ToDictionary(ff => ff.Value, ff => ff.Key);

            FleetSizes = Fleet_Field.Select(ff => ff.Key).ToList();
            FieldSizes = Fleet_Field.Select(ff => ff.Value).ToList();

            Regimes = (Regime[]) Enum.GetValues(typeof(Client.Regime));

            FleetSize = FleetSizes[0];
            FieldSize = FieldSizes[0];
            Regime = Regimes[0];
        }
    }
}

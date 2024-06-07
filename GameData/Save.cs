using System.ComponentModel.DataAnnotations;

namespace GameData {
    public class Save {
        [Key] public int Id { get; set; }
        public string StartingField { get; set; } = string.Empty;
        public List<string> Moves { get; set; } = new List<string>();

        public Save() { }

        public Save(string startingField, List<string> moves) {
            StartingField = startingField;
            Moves = moves;
        }
    }
}

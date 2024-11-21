using System.Text.Json.Serialization;

namespace BlackjackBackend.Models
{

    public class Player
    {
        [JsonInclude]
        private string _id;
        [JsonInclude]
        private string _name;
        [JsonInclude]

        private int _money { get; set; }
        [JsonInclude]

        private bool _ready { get; set; }

        public Player(string id, string name) 
        { 
            _id = id;
            _name = name;
            _money = 100;
            _ready = false;
        }

        public void ToggleReady()
        {
            _ready = !_ready;
        }

    }
}

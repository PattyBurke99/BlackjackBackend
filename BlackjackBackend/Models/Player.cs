using System.Text.Json.Serialization;

namespace BlackjackBackend.Models
{

    public class Player
    {
        [JsonInclude]
        public string Id { get; set; }
        [JsonInclude]
        public string Name { get; set; }
        [JsonInclude]
        public int Money { get; set; }

        public Player(string id, string name) 
        { 
            Id = id;
            Name = name;
            Money = 100;
        }
    }
}
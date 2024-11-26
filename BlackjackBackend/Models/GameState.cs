using System.Text.Json.Serialization;

namespace BlackjackBackend.Models
{
    public class GameState
    {
        [JsonInclude]
        public string?[] Seats = new string[6];
        public void UpdateState(string?[] seats)
        {
            Seats = seats;
        }
    }
}

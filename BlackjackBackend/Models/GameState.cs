using System.Text.Json.Serialization;

namespace BlackjackBackend.Models
{

    public enum GameAction
    {
        Standby,
        Betting
    }

    public class SeatData
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public SeatData(string id, string name) 
        {
            Id = id;
            Name = name;
        }

        //Required to assess object equality during "TryUpdate" function
        public override bool Equals(object? obj)
        {
            if (obj is SeatData other)
            {
                return Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class GameState
    {
        public SeatData?[] Seats { get; set; } = new SeatData?[6];
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameAction CurrentState { get; set; } = GameAction.Standby;
        public DateTime? NextActionTime { get; set; }
        public string? Info { get; set; }

        public async Task UpdateStateAsync(SeatData?[] seatData, GameAction currentState)
        {
            await Task.Run(() =>
            {
                Seats = seatData;

                if (currentState != CurrentState)
                {
                    switch (currentState)
                    {
                        case GameAction.Standby:
                            Info = "Waiting for players to start game...";
                            NextActionTime = null;
                            break;
                        case GameAction.Betting:
                            Info = "Place your bets!";
                            NextActionTime = DateTime.UtcNow.AddSeconds(20);
                            break;
                        default:
                            break;
                    }
                }

                CurrentState = currentState;
            });
        }
    }
}

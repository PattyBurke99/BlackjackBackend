namespace BlackjackBackend.Models
{
    public class SeatData
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public SeatData(string id, string name) 
        {
            Id = id;
            Name = name;
        }
    }

    public class GameState
    {
        public SeatData?[] Seats { get; set; } = new SeatData?[6];
    public void UpdateState(SeatData?[] seatData)
        {
            Seats = seatData;
        }
    }
}

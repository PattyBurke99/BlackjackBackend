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
    public void UpdateState(SeatData?[] seatData)
        {
            Seats = seatData;
        }
    }
}

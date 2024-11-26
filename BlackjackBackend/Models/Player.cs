namespace BlackjackBackend.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Money { get; set; }

        public Player(string id, string name) 
        { 
            Id = id;
            Name = name;
            Money = 100;
        }
    }
}
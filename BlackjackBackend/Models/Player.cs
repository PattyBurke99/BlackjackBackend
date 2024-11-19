namespace BlackjackBackend.Models
{
    public class Player
    {
        private string _id;
        private int _money { get; set; }

        public Player(string id) 
        { 
            _id = id;
            _money = 100;
        }
    }
}

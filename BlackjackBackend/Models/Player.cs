namespace BlackjackBackend.Models
{
    public class Player
    {
        private string _name;
        private int _money { get; set; }
        private bool _ready { get; set; }

        public Player(string name) 
        { 
            _name = name;
            _money = 100;
            _ready = false;
        }
    }
}

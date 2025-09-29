namespace CardsDBZ_Server
{
    public class Player
    {
        private string _connectionId;
        private string _playerName;
        public Player(string connectionId, string playerName)
        {
            _connectionId = connectionId;
            _playerName = playerName;
        }
        public string ConnectionId { get => _connectionId; set => _connectionId = value; }
        public string PlayerName { get => _playerName; set => _playerName = value; }
    }
}

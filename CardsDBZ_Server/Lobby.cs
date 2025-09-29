
namespace CardsDBZ_Server
{
    public class Lobby
    {
        private List<Player> _players = new List<Player>();
        public void Start()
        {
            Console.WriteLine("Game Lobby starting");
        }
        public void AddPlayer(string connectionId, string playerName)
        {
            Player player = new Player(connectionId, playerName);
            _players.Add(player);
        }
        public Player? RemovePlayer(string connectionId)
        {
            Player removedPlayer = _players.Find(p => p.ConnectionId == connectionId);
            _players.Remove(removedPlayer);
            return removedPlayer;
        }
        public string GetPlayerList()
        {
            string playerList = "";
            foreach (Player player in _players) {
                playerList += player.PlayerName + Environment.NewLine;
            }
            return playerList.Trim();
        }
    }
}

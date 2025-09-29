using Microsoft.AspNetCore.SignalR;

namespace CardsDBZ_Server.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly Lobby _lobby;
        public LobbyHub(Lobby lobby)
        {
            _lobby = lobby;
        }
        public void JoinServer(string player)
        {
            Console.WriteLine("Player " + player + " has joined.");
        }
    }
}

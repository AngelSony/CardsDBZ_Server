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

        public async Task JoinServer(string playerName)
        {
            string connectionId = Context.ConnectionId;
            Console.WriteLine($"Player {playerName} joined.");

            _lobby.AddPlayer(connectionId, playerName);

            await Clients.Caller.SendAsync("PlayerListUpdate", _lobby.GetPlayerList());
        }
        public async Task PlayerListUpdate()
        {
            await Clients.Caller.SendAsync("PlayerListUpdate", _lobby.GetPlayerList());
        }

        public async Task SendGameMessage(string message)
        {
            /*var group = _lobby.GetGroupFor(Context.ConnectionId);
            if (group is null)
            {
                return;
            }

            await Clients.Group(group).SendAsync("GameMessage", new
            {
                from = Context.ConnectionId,
                message
            });*/
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Player removedPlayer = _lobby.RemovePlayer(Context.ConnectionId) ?? new Player("dummy", "dummy");
            Console.WriteLine($"Player {removedPlayer.PlayerName} disconnected.");

            await base.OnDisconnectedAsync(exception);
        }
    }
}

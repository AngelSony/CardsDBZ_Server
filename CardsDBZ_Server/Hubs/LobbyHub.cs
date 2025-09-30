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
        public async Task TableListUpdate()
        {
            await Clients.Caller.SendAsync("TableListUpdate", _lobby.GetTableList());
        }
        public async Task PlayerListUpdate()
        {
            await Clients.Caller.SendAsync("PlayerListUpdate", _lobby.GetPlayerList());
        }
        public async Task JoinServer(string playerName)
        {
            string connectionId = Context.ConnectionId;
            Console.WriteLine($"{DateTime.Now} Player {playerName} joined.");

            _lobby.AddPlayer(connectionId, playerName);

            await Clients.Caller.SendAsync("PlayerListUpdate", _lobby.GetPlayerList());
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Player removedPlayer = _lobby.RemovePlayer(Context.ConnectionId) ?? new Player("error", "error");
            Console.WriteLine($"{DateTime.Now} Player {removedPlayer.PlayerName} disconnected.");

            await base.OnDisconnectedAsync(exception);
        }
    }
}

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
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"Player {playerName} joined with connection {connectionId}.");

            var result = _lobby.AddPlayer(connectionId, playerName);

            if (result.IsWaiting)
            {
                await Clients.Caller.SendAsync("WaitingForOpponent", new
                {
                    you = result.Player.Name
                });
                return;
            }

            if (!result.HasMatch || result.Opponent is null || result.GroupName is null)
            {
                return;
            }

            await Groups.AddToGroupAsync(connectionId, result.GroupName);
            await Groups.AddToGroupAsync(result.Opponent.ConnectionId, result.GroupName);

            await Clients.Client(connectionId).SendAsync("MatchFound", new
            {
                you = result.Player.Name,
                opponent = result.Opponent.Name,
                group = result.GroupName
            });

            await Clients.Client(result.Opponent.ConnectionId).SendAsync("MatchFound", new
            {
                you = result.Opponent.Name,
                opponent = result.Player.Name,
                group = result.GroupName
            });

            await Clients.Group(result.GroupName).SendAsync("GameStarted", new
            {
                group = result.GroupName,
                players = new[] { result.Player.Name, result.Opponent.Name }
            });
        }

        public async Task SendGameMessage(string message)
        {
            var group = _lobby.GetGroupFor(Context.ConnectionId);
            if (group is null)
            {
                return;
            }

            await Clients.Group(group).SendAsync("GameMessage", new
            {
                from = Context.ConnectionId,
                message
            });
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var result = _lobby.RemovePlayer(Context.ConnectionId);

            if (result.PreviousGroup is not null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, result.PreviousGroup);
            }

            if (result.Opponent is not null)
            {
                await Clients.Client(result.Opponent.ConnectionId).SendAsync("OpponentDisconnected", new
                {
                    opponent = Context.ConnectionId
                });

                if (result.PreviousGroup is not null)
                {
                    await Groups.RemoveFromGroupAsync(result.Opponent.ConnectionId, result.PreviousGroup);
                }

                if (result.RequeueResult is { } requeue)
                {
                    if (requeue.IsWaiting)
                    {
                        await Clients.Client(result.Opponent.ConnectionId).SendAsync("WaitingForOpponent", new
                        {
                            you = result.Opponent.Name
                        });
                    }
                    else if (requeue.HasMatch && requeue.GroupName is not null && requeue.Opponent is not null)
                    {
                        await Groups.AddToGroupAsync(result.Opponent.ConnectionId, requeue.GroupName);
                        await Groups.AddToGroupAsync(requeue.Opponent.ConnectionId, requeue.GroupName);

                        await Clients.Client(result.Opponent.ConnectionId).SendAsync("MatchFound", new
                        {
                            you = result.Opponent.Name,
                            opponent = requeue.Opponent.Name,
                            group = requeue.GroupName
                        });

                        await Clients.Client(requeue.Opponent.ConnectionId).SendAsync("MatchFound", new
                        {
                            you = requeue.Opponent.Name,
                            opponent = result.Opponent.Name,
                            group = requeue.GroupName
                        });

                        await Clients.Group(requeue.GroupName).SendAsync("GameStarted", new
                        {
                            group = requeue.GroupName,
                            players = new[] { result.Opponent.Name, requeue.Opponent.Name }
                        });
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

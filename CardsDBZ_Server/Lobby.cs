namespace CardsDBZ_Server
{
    public class Lobby
    {
        private readonly object _syncLock = new();
        private readonly Dictionary<string, Player> _players = new();
        private readonly Dictionary<string, MatchMembership> _memberships = new();
        private Player? _waitingPlayer;

        public void Start()
        {
            Console.WriteLine("Game Lobby starting");
        }

        public JoinResult AddPlayer(string connectionId, string name)
        {
            lock (_syncLock)
            {
                var player = new Player(connectionId, name);
                _players[connectionId] = player;
                return QueuePlayer(player);
            }
        }

        public DisconnectResult RemovePlayer(string connectionId)
        {
            lock (_syncLock)
            {
                if (!_players.Remove(connectionId, out _))
                {
                    return new DisconnectResult(null, null, null);
                }

                if (_waitingPlayer?.ConnectionId == connectionId)
                {
                    _waitingPlayer = null;
                    return new DisconnectResult(null, null, null);
                }

                if (_memberships.Remove(connectionId, out var membership))
                {
                    if (_players.TryGetValue(membership.OpponentConnectionId, out var opponent))
                    {
                        _memberships.Remove(opponent.ConnectionId);
                        var requeueResult = QueuePlayer(opponent);
                        return new DisconnectResult(opponent, membership.GroupName, requeueResult);
                    }

                    return new DisconnectResult(null, membership.GroupName, null);
                }

                return new DisconnectResult(null, null, null);
            }
        }

        public string? GetGroupFor(string connectionId)
        {
            lock (_syncLock)
            {
                return _memberships.TryGetValue(connectionId, out var membership)
                    ? membership.GroupName
                    : null;
            }
        }

        private JoinResult QueuePlayer(Player player)
        {
            if (_waitingPlayer is null)
            {
                _waitingPlayer = player;
                return new JoinResult(player, null, null);
            }

            if (_waitingPlayer.ConnectionId == player.ConnectionId)
            {
                return new JoinResult(player, null, null);
            }

            var opponent = _waitingPlayer;
            _waitingPlayer = null;

            var groupName = $"match-{Guid.NewGuid():N}";
            _memberships[player.ConnectionId] = new MatchMembership(groupName, opponent.ConnectionId);
            _memberships[opponent.ConnectionId] = new MatchMembership(groupName, player.ConnectionId);

            return new JoinResult(player, opponent, groupName);
        }

        public record Player(string ConnectionId, string Name);

        public record JoinResult(Player Player, Player? Opponent, string? GroupName)
        {
            public bool IsWaiting => Opponent is null;
            public bool HasMatch => Opponent is not null && GroupName is not null;
        }

        public record DisconnectResult(Player? Opponent, string? PreviousGroup, JoinResult? RequeueResult);

        private record MatchMembership(string GroupName, string OpponentConnectionId);
    }
}

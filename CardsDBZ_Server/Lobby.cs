namespace CardsDBZ_Server
{
    public class Lobby
    {
        private readonly object _syncLock = new();
        private readonly Dictionary<string, Player> _players = new();
        private readonly Dictionary<string, RoomState> _rooms = new();

        public void Start()
        {
            Console.WriteLine("Game Lobby starting");
        }

        public JoinResult AddPlayer(string roomId, string connectionId, string name)
        {
            lock (_syncLock)
            {
                var player = new Player(connectionId, name, roomId);
                _players[connectionId] = player;
                var room = GetOrCreateRoom(roomId);
                return room.AddPlayer(player);
            }
        }

        public DisconnectResult RemovePlayer(string connectionId)
        {
            lock (_syncLock)
            {
                if (!_players.Remove(connectionId, out var player))
                {
                    return new DisconnectResult(null, null, null, null);
                }

                if (!_rooms.TryGetValue(player.RoomId, out var room))
                {
                    return new DisconnectResult(null, null, null, player.RoomId);
                }

                Player? ResolvePlayer(string id) => _players.TryGetValue(id, out var p) ? p : null;

                var result = room.RemovePlayer(player, ResolvePlayer);

                if (room.IsEmpty)
                {
                    _rooms.Remove(player.RoomId);
                }

                return result with { RoomId = player.RoomId };
            }
        }

        public string? GetGroupFor(string connectionId)
        {
            lock (_syncLock)
            {
                if (!_players.TryGetValue(connectionId, out var player))
                {
                    return null;
                }

                return _rooms.TryGetValue(player.RoomId, out var room)
                    ? room.GetGroupFor(connectionId)
                    : null;
            }
        }

        public string? GetRoomFor(string connectionId)
        {
            lock (_syncLock)
            {
                return _players.TryGetValue(connectionId, out var player)
                    ? player.RoomId
                    : null;
            }
        }

        private RoomState GetOrCreateRoom(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                return room;
            }

            room = new RoomState(roomId);
            _rooms.Add(roomId, room);
            return room;
        }

        public record Player(string ConnectionId, string Name, string RoomId);

        public record JoinResult(Player Player, Player? Opponent, string? GroupName, string RoomId)
        {
            public bool IsWaiting => Opponent is null;
            public bool HasMatch => Opponent is not null && GroupName is not null;
        }

        public record DisconnectResult(Player? Opponent, string? PreviousGroup, JoinResult? RequeueResult, string? RoomId);

        private class RoomState
        {
            private readonly string _roomId;
            private readonly Dictionary<string, MatchMembership> _memberships = new();
            private Player? _waitingPlayer;

            public RoomState(string roomId)
            {
                _roomId = roomId;
            }

            public JoinResult AddPlayer(Player player)
            {
                return QueuePlayer(player);
            }

            public DisconnectResult RemovePlayer(Player leavingPlayer, Func<string, Player?> playerResolver)
            {
                if (_waitingPlayer?.ConnectionId == leavingPlayer.ConnectionId)
                {
                    _waitingPlayer = null;
                    return new DisconnectResult(null, null, null, _roomId);
                }

                if (_memberships.Remove(leavingPlayer.ConnectionId, out var membership))
                {
                    if (playerResolver(membership.OpponentConnectionId) is { } opponent)
                    {
                        _memberships.Remove(opponent.ConnectionId);
                        var requeueResult = QueuePlayer(opponent);
                        return new DisconnectResult(opponent, membership.GroupName, requeueResult, _roomId);
                    }

                    return new DisconnectResult(null, membership.GroupName, null, _roomId);
                }

                return new DisconnectResult(null, null, null, _roomId);
            }

            public string? GetGroupFor(string connectionId)
            {
                return _memberships.TryGetValue(connectionId, out var membership)
                    ? membership.GroupName
                    : null;
            }

            public bool IsEmpty => _waitingPlayer is null && _memberships.Count == 0;

            private JoinResult QueuePlayer(Player player)
            {
                if (_waitingPlayer is null)
                {
                    _waitingPlayer = player;
                    return new JoinResult(player, null, null, _roomId);
                }

                if (_waitingPlayer.ConnectionId == player.ConnectionId)
                {
                    return new JoinResult(player, null, null, _roomId);
                }

                var opponent = _waitingPlayer;
                _waitingPlayer = null;

                var groupName = $"match-{Guid.NewGuid():N}";
                _memberships[player.ConnectionId] = new MatchMembership(groupName, opponent.ConnectionId);
                _memberships[opponent.ConnectionId] = new MatchMembership(groupName, player.ConnectionId);

                return new JoinResult(player, opponent, groupName, _roomId);
            }

            private record MatchMembership(string GroupName, string OpponentConnectionId);
        }
    }
}

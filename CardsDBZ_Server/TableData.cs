namespace CardsDBZ_Server
{
    public class TableData
    {
        private readonly int _tableId;
        private readonly int _playerCount;
        private readonly TableStates _tableState;
        public TableData(int tableId, int playerCount, TableStates tableState)
        {
            _tableId = tableId;
            _playerCount = playerCount;
            _tableState = tableState;
        }
        public int TableId => _tableId;
        public int PlayerCount => _playerCount;
        public TableStates TableState => _tableState;
        public enum TableStates
        {
            OnGame,
            Idle
        }
    }
}

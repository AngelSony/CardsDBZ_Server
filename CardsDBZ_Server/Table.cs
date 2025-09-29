namespace CardsDBZ_Server
{
    public class Table
    {
        private readonly Player? _player1;
        private readonly Player? _player2;
        private int _tableId;
        private TableState _tableState;
        public Table(int tableId)
        {
            _tableId = tableId;
            _tableState = TableState.Idle;
        }
        public enum TableState { 
            OnGame,
            Idle
        }
        public void StartGame()
        {
            Console.WriteLine("Game starting at " + _tableId);
        }
        public int TableId { get => _tableId; set => _tableId = value; }
        public TableState TableState1 { get => _tableState; set => _tableState = value; }

    }
}

using static CardsDBZ_Server.TableData;

namespace CardsDBZ_Server
{
    public class Table
    {
        private readonly Player? _player1;
        private readonly Player? _player2;
        private int _tableId;
        private TableStates _tableState;
        public Table(int tableId)
        {
            _tableId = tableId;
            _tableState = TableStates.Idle;
        }
        public void StartGame()
        {
            Console.WriteLine(DateTime.Now + "Game starting at " + _tableId);
        }
        public TableData GetTableData()
        {
            int playerCount = 0;
            if (_player1 != null) playerCount++;
            if (_player2 != null) playerCount++;

            return new TableData(_tableId, playerCount, _tableState);
        }
        public int TableId { get => _tableId; set => _tableId = value; }
        public TableStates TableState1 { get => _tableState; set => _tableState = value; }
    }
}

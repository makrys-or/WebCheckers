using System.Net.WebSockets;

namespace Checkers0._1.Components.Pages
{
    public partial class Home
    {
        private Cell? selectedCell;
        private readonly Logic logic = new();
        string act = "";
        public PieceColor Turn => logic.Turn;

        private void HandleClick(Cell cell)
        {
            var Col = cell.Col;
            var Row = cell.Row;

            // Если в клетке есть шашка — выбираем её
            if (cell.Checker != null && cell.Checker.Colour == logic.Turn) 
            {
                selectedCell = cell;
                act = $"{cell.Row}{cell.Col}";
            }
            // Если в клетке нет шашки и есть выделенная шашка — делаем ход
            else if (selectedCell != null && cell.Checker == null)
            {
                string fullAct = $"{act} {cell.Row}{cell.Col}";
                bool success = logic.Action(board, fullAct);
                selectedCell = null;
                act = "";
            }
            // Если повторно клик, то снять выделение
            else if (selectedCell != null)
            {
                selectedCell = null;
                act = "";
            }
            winner = logic.CheckWinner(board);
        }
    }
}

using static Checkers0._1.Components.Pages.Home;

namespace Checkers0._1
{
    public class Board
    {
        public Cell[,] Cells { get; } = new Cell[9, 9];
        public int countWhite = 0;
        public int countBlack = 0;

        public Board()
        {
            // Инициализируем все клетки
            for (int row = 8; row > 0; row--)
            {
                for (int col = 1; col < 9; col++)
                {
                    Cells[row, col] = new Cell { Row = row, Col = col };
                }
            }

            // Расставляем начальные шашки
            Utils.InitializeCheckers(this);
        }
    }
    public class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public Checker? Checker { get; set; }
        public bool IsPlayable => (Row + Col) % 2 == 0;  // только тёмные клетки "игровые"
    }
    public class Checker
    {
        public PieceColor Colour { get; set; }
        public bool IsKing { get; set; }

    }
    public enum PieceColor
    {
        White,
        Black
    }
}

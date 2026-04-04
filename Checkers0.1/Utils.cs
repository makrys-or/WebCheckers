using static Checkers0._1.Components.Pages.Home;

namespace Checkers0._1
{
    class Utils
{
    public static void InitializeCheckers(Board board)
    {
        var Cells = board.Cells;

        for (int row = 6; row < 9; row++)
        {
            for (int col = 1; col < 9; col++)
            {
                if (Cells[row, col].IsPlayable)//if ((row + col) % 2 == 1) // только на тёмных клетках
                {
                    Cells[row, col].Checker = new Checker { Colour = PieceColor.Black, IsKing = false };
                    board.countWhite++;
                }
            }
        }

        for (int row = 1; row < 4; row++)
        {
            for (int col = 1; col < 9; col++)
            {
                if (Cells[row, col].IsPlayable)//if ((row + col) % 2 == 1) // только на тёмных клетках
                {
                    Cells[row, col].Checker = new Checker { Colour = PieceColor.White, IsKing = false };
                    board.countBlack++;
                }
            }
        }

    }

    public static int LetterToColumn(char letter)
    {
        letter = char.ToLower(letter); // на случай заглавной буквы            
        return letter - 'a' + 1;
    }
}
}

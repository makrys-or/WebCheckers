using Checkers0._1;

namespace Checkers0._1
{
    class Logic
    {
        public PieceColor Turn { get; set; } = PieceColor.White;
        public void SwapTurn()
        {
            if (Turn == PieceColor.White)
            {
                Turn = PieceColor.Black;
            }
            else if (Turn == PieceColor.Black)
            {
                Turn = PieceColor.White;
            }
        }
        public PieceColor? CheckWinner(Board board)
        {
            int whiteCount = 0;
            int blackCount = 0;

            foreach (var cell in board.Cells)
            {
                if (cell?.Checker != null)
                {
                    if (cell.Checker.Colour == PieceColor.White) whiteCount++;
                    else blackCount++;
                }
            }

            if (whiteCount == 0) return PieceColor.Black;
            if (blackCount == 0) return PieceColor.White;

            return null; // Игра продолжается
        }


        public bool Action(Board board, string Act)
        {
            string[] actions = Act.Split(" "); // Вход 22 33 => Выход [22; 33] 

            int fromRow = Convert.ToInt32(actions[0][0] - '0');
            int fromCol = Convert.ToInt32(actions[0][1] - '0');
            int toRow = Convert.ToInt32(actions[1][0] - '0');
            int toCol = Convert.ToInt32(actions[1][1] - '0');

            var from = board.Cells[fromRow, fromCol];
            var to = board.Cells[toRow, toCol];

            var (ok, VictimCells) = CheckActMove(board, actions);
            if (!ok) return false;

            var (mustkill, list) = HasAnyCapture(board);

            // Если нет рубки, то просто ход
            if (!mustkill)
            {
                to.Checker = from.Checker;
                from.Checker = null;
            }
            // Если есть рубка, но не рубит
            if (mustkill && VictimCells.Count == 0)
            {
                return false;
            }
            // Выполняем рубку
            if (VictimCells.Count > 0)
            {
                var (vRow, vCol) = VictimCells[0];
                board.Cells[vRow, vCol].Checker = null;
                to.Checker = from.Checker;
                from.Checker = null;

                // Проверяем, есть ли еще обязательная рубка для этой шашки
                var fromChecker = new List<(int, int)> { (to.Row, to.Col) };
                var (mustContinue, nextTargets) = HasAnyCaptureSolo(board, fromChecker);

                if (mustContinue)
                {
                    // НЕ меняем Turn
                    // Blazor/UI должен запросить новый ход для той же шашки
                    return true;
                }
            }

            // Если цепной рубки нет, меняем ход
            SwapTurn();

            if (!to.Checker!.IsKing) // Проверка на дамку
            {
                if ((to.Checker!.Colour == PieceColor.White && toRow == 8) || (to.Checker.Colour == PieceColor.Black && toRow == 1))//проверка на становление дамкой
                {
                    to.Checker.IsKing = true;
                }
            }

            return true;
        }

        // Проверка на возможность перемещения
        public (bool, List<(int row, int col)>) CheckActMove(Board board, string[] actions)
        {
            int fromRow = Convert.ToInt32(actions[0][0] - '0');
            int fromCol = Convert.ToInt32(actions[0][1] - '0');
            int toRow = Convert.ToInt32(actions[1][0] - '0');
            int toCol = Convert.ToInt32(actions[1][1] - '0');

            var from = board.Cells[fromRow, fromCol];
            var to = board.Cells[toRow, toCol];

            var victims = new List<(int row, int col)>();

            //  Базовые проверки
            if (!from.IsPlayable || !to.IsPlayable)
                return (false, victims);

            if (to.Checker != null)
                return (false, victims);

            if (from.Checker == null)
                return (false, victims);

            if (from.Checker.Colour != Turn)
                return (false, victims);

            // Обычная шашка
            if (!from.Checker.IsKing)
            {
                if (Math.Abs(fromRow - toRow) != 1 || Math.Abs(fromCol - toCol) != 1)
                {
                    if (Math.Abs(fromRow - toRow) != 2 || Math.Abs(fromCol - toCol) != 2)
                        return (false, victims);
                    else
                    {
                        int VictimFirstCoord = (fromRow + toRow) / 2;
                        int VictimSecondCoord = (fromCol + toCol) / 2;
                        var VictimCell = board.Cells[VictimFirstCoord, VictimSecondCoord];

                        if (VictimCell.Checker == null || VictimCell.Checker.Colour == from.Checker.Colour)
                            return (false, victims);

                        victims.Add((VictimFirstCoord, VictimSecondCoord));
                        return (true, victims);
                    }
                }

                if (Turn == PieceColor.White && toRow <= fromRow)
                    return (false, victims);

                if (Turn == PieceColor.Black && toRow >= fromRow)
                    return (false, victims);

                return (true, victims);
            }
            // Дамка
            else if (from.Checker.IsKing)
            {
                if (Math.Abs(fromRow - toRow) != Math.Abs(fromCol - toCol))
                    return (false, victims);

                var (ok, list) = KingCheck(board, actions);
                if (!ok)
                    return (false, victims);

                return (true, list);
            }
            // Если ошибка
            return (false, victims); 
        }

        // Проверка, есть ли на пути у дамки другие шашки
        public static (bool, List<(int row, int col)>) KingCheck(Board board, string[] actions) // Возвращает жертвы для дамки, если есть
        {
            bool Out = true;

            int fromRow = Convert.ToInt32(actions[0][0] - '0');
            int fromCol = Convert.ToInt32(actions[0][1] - '0');
            int toRow = Convert.ToInt32(actions[1][0] - '0');
            int toCol = Convert.ToInt32(actions[1][1] - '0');

            var from = board.Cells[fromRow, fromCol];

            // d8 h4 (f6 стоит враг)   8,4 4,8 (6,6 враг)

            // Начинаем подозрительную клетку от положения дамки
            int SusFC = fromRow;
            int SusSC = fromCol;
            var VictimCells = new List<(int row, int col)>(); // Список найденных на пути шашек

            for (int i = SusFC; i != toRow;) // Перечисляем подозрительные клетки
            {

                if (fromRow > toRow) // Если дамка выше
                {
                    i -= 1;
                    if (fromCol > toCol) // Если дамка правее 
                    {
                        SusSC -= 1;
                    }
                    else { SusSC += 1; } // Если дамка левее
                }

                if (fromRow < toRow) // Если дамка ниже
                {
                    i += 1;
                    if (fromCol > toCol) // Если дамка правее 
                    {
                        SusSC -= 1;
                    }
                    else { SusSC += 1; } // Если дамка левее
                }

                if (board.Cells[i, SusSC].Checker != null) // Если подозрительная клетка непустая 
                {
                    VictimCells.Add((i, SusSC));
                    //System.Console.WriteLine("DEBAG:На пути дамки обнаружена шашка");

                    if (board.Cells[i, SusSC].Checker!.Colour == from.Checker!.Colour) // Через свою шашку ходить нельзя
                    {
                        Out = false;
                        break;
                    }
                }

                // Дамка не может ходить, если на пути 2 шашки или больше
                if (VictimCells.Count == 2)
                {
                    Out = false;
                    break;
                }
            }

            return (Out, VictimCells);
        }

        public (bool, List<(int row, int col)>) HasAnyCapture(Board board) // Чек на обязательную рубку (Выводит есть или нет рубка и координаты этих шашек)
        {
            var Captures = new List<(int row, int col)>(); // Список шашек на обяз рубку
            for (int row = 8; row >= 1; row--)
            {
                for (int col = 1; col <= 8; col++)
                {
                    var cell = board.Cells[row, col];

                    if (!cell.IsPlayable)
                        continue;

                    if (cell.Checker == null)
                        continue;

                    if (cell.Checker.Colour == Turn)
                    {
                        if (!cell.Checker.IsKing) // Если обычная шашка
                        {
 
                            if (row + 1 < 8 && col + 1 < 8)
                            {
                                if (board.Cells[row + 1, col + 1].Checker != null && board.Cells[row + 1, col + 1].Checker?.Colour != Turn) // Справа сверху от шашки
                                {
                                    if (board.Cells[row + 2, col + 2].Checker == null)
                                    {
                                        Captures.Add((row + 1, col + 1));
                                    }
                                }
                            }

                            if (row + 1 < 8 && col - 1 > 1)
                            {
                                if (board.Cells[row + 1, col - 1].Checker != null && board.Cells[row + 1, col - 1].Checker?.Colour != Turn) // Слева сверху от шашки
                                {
                                    if (board.Cells[row + 2, col - 2].Checker == null)
                                    {
                                        Captures.Add((row + 1, col - 1));
                                    }
                                }
                            }

                            if (row - 1 > 1 && col + 1 < 8)
                            {
                                if (board.Cells[row - 1, col + 1].Checker != null && board.Cells[row - 1, col + 1].Checker?.Colour != Turn) // Справа снизу от шашки
                                {
                                    if (board.Cells[row - 2, col + 2].Checker == null)
                                    {
                                        Captures.Add((row - 1, col + 1));
                                    }
                                }
                            }

                            if (row - 1 > 1 && col - 1 > 1)
                            {
                                if (board.Cells[row - 1, col - 1].Checker != null && board.Cells[row - 1, col - 1].Checker?.Colour != Turn) // Слева снизу от шашки
                                {
                                    if (board.Cells[row - 2, col - 2].Checker == null)
                                    {
                                        Captures.Add((row - 1, col - 1));
                                    }
                                }
                            }
                                
                        }

                        if (cell.Checker.IsKing) // Если дамка
                        {
                            var SusCell = board.Cells[row, col];

                            var Srow = row;
                            var Scol = col;
                            while (Srow + 1 <= 8 && Scol + 1 <= 8) // Вправо вверх 
                            {
                                SusCell = board.Cells[Srow, Scol];
                                Srow += 1;
                                Scol += 1;
                            

                                if (SusCell.Checker != null && board.Cells[Srow, Scol].IsPlayable && board.Cells[Srow, Scol].Checker == null)
                                {
                                    if (SusCell.Checker?.Colour != Turn)
                                    {
                                        Captures.Add((Srow, Scol));
                                        break;
                                    }
                                }
                            }

                            Srow = row;
                            Scol = col;
                            while (Srow + 1 <= 8 && Scol - 1 >= 1) // Вправо вниз
                            {
                                SusCell = board.Cells[Srow, Scol];
                                Srow += 1;
                                Scol -= 1;

                                if (SusCell.Checker != null && board.Cells[Srow, Scol].IsPlayable && board.Cells[Srow, Scol].Checker == null)
                                {
                                    if (SusCell.Checker?.Colour != Turn)
                                    {
                                        Captures.Add((Srow, Scol));
                                        break;
                                    }
                                }
                            }

                            Srow = row;
                            Scol = col;
                            while (Srow - 1 >= 1 && Scol + 1 <= 8) // Влево вверх
                            {
                                SusCell = board.Cells[Srow, Scol];
                                Srow -= 1;
                                Scol += 1;

                                if (SusCell.Checker != null && board.Cells[Srow, Scol].IsPlayable && board.Cells[Srow, Scol].Checker == null)
                                {
                                    if (SusCell.Checker?.Colour != Turn)
                                    {
                                        Captures.Add((Srow, Scol));
                                        break;
                                    }
                                }
                            }

                            SusCell = board.Cells[row, col];
                            Srow = row;
                            Scol = col;
                            while (Srow - 1 >= 1 && Scol - 1 >= 1) // Влево вниз
                            {
                                SusCell = board.Cells[Srow, Scol];
                                Srow -= 1;
                                Scol -= 1;

                                if (SusCell.Checker != null && board.Cells[Srow, Scol].IsPlayable && board.Cells[Srow, Scol].Checker == null)
                                {
                                    if (SusCell.Checker?.Colour != Turn)
                                    {
                                        Captures.Add((Srow, Scol));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Captures.Count != 0)
                return (true, Captures);
            else return (false, Captures);
        }


        public (bool, List<(int OutRow, int OutCol)>) HasAnyCaptureSolo(Board board, List<(int InputRow, int InputCol)> values) //МЕТОД ДЛЯ ПРОВЕРКИ ОБЯЗАТЕЛЬНОЙ(ЦЕПНОЙ) РУБКИ ДЛЯ ОТДЕЛЬНОЙ ШАШКИ
        {
            var Captures = new List<(int row, int col)>(); // Список шашек на обяз рубку

            var (row, col) = values[0]; // В списке один элемент всегда
            var cell = board.Cells[row, col];


            if (!cell.Checker!.IsKing) // Если обычная шашка
            {
                if (row + 1 < 8 && col + 1 < 8)
                {
                    if (board.Cells[row + 1, col + 1].Checker != null && board.Cells[row + 1, col + 1].Checker?.Colour != Turn) // Справа сверху от шашки
                    {
                        if (board.Cells[row + 2, col + 2].Checker == null)
                        {
                            Captures.Add((row + 1, col + 1));
                        }
                    }
                }

                if (row + 1 < 8 && col - 1 > 1)
                    {
                    if (board.Cells[row + 1, col - 1].Checker != null && board.Cells[row + 1, col - 1].Checker?.Colour != Turn) // Слева сверху от шашки
                    {
                        if (board.Cells[row + 2, col - 2].Checker == null)
                        {
                            Captures.Add((row + 1, col - 1));
                        }
                    }
                }

                if (row - 1 > 1 && col + 1 < 8)
                {
                    if (board.Cells[row - 1, col + 1].Checker != null && board.Cells[row - 1, col + 1].Checker?.Colour != Turn) // Справа снизу от шашки
                    {
                        if (board.Cells[row - 2, col + 2].Checker == null)
                        {
                            Captures.Add((row - 1, col + 1));
                        }
                    }
                }

                if (row - 1 > 1 && col - 1 > 1)
                {
                    if (board.Cells[row - 1, col - 1].Checker != null && board.Cells[row - 1, col - 1].Checker?.Colour != Turn) // Слева снизу от шашки
                    {
                        if (board.Cells[row - 2, col - 2].Checker == null)
                        {
                            Captures.Add((row - 1, col - 1));
                        }
                    }
                }
            }

            if (cell.Checker.IsKing) // Если дамка
            {
                var SusCell = board.Cells[row, col];

                var Srow = row;
                var Scol = col;
                while (Srow + 1 <= 8 && Scol + 1 <= 8) // Вправо вверх 
                {
                    SusCell = board.Cells[Srow, Scol];
                    Srow += 1;
                    Scol += 1;

                    if (SusCell.Checker != null && board.Cells[Srow, Scol].IsPlayable && board.Cells[Srow, Scol].Checker == null)
                    {
                        if (SusCell.Checker?.Colour != Turn)
                        {
                            Captures.Add((Srow, Scol));
                            break;
                        }
                    }
                }

                SusCell = board.Cells[row, col];
                Srow = row;
                Scol = col;
                while (Srow + 1 <= 8 && Scol - 1 >= 1) // Вправо вниз
                {
                    SusCell = board.Cells[Srow, Scol];
                    Srow += 1;
                    Scol -= 1;

                    if (SusCell.Checker != null && board.Cells[Srow, Scol].IsPlayable && board.Cells[Srow, Scol].Checker == null)
                    {
                        if (SusCell.Checker?.Colour != Turn)
                        {
                            Captures.Add((Srow, Scol));
                            break;
                        }
                    }
                }

                SusCell = board.Cells[row, col];
                Srow = row;
                Scol = col;
                while (Srow - 1 >= 1 && Scol + 1 <= 8) // Влево вверх
                {
                    SusCell = board.Cells[Srow, Scol];
                    Srow -= 1;
                    Scol += 1;

                    if (SusCell.Checker != null && board.Cells[Srow, Scol].IsPlayable && board.Cells[Srow, Scol].Checker == null)
                    {
                        if (SusCell.Checker?.Colour != Turn)
                        {
                            Captures.Add((Srow, Scol));
                            break;
                        }
                    }
                }

                SusCell = board.Cells[row, col];
                Srow = row;
                Scol = col;
                while (Srow - 1 >= 1 && Scol - 1 >= 1) // Влево вниз
                {
                    SusCell = board.Cells[Srow, Scol];
                    Srow -= 1;
                    Scol -= 1;

                    if (SusCell.Checker != null && board.Cells[Srow, Scol].IsPlayable && board.Cells[Srow, Scol].Checker == null)
                    {
                        if (SusCell.Checker?.Colour != Turn)
                        {
                            Captures.Add((Srow, Scol));
                            break;
                        }
                    }
                }
            }
            if (Captures.Count != 0)
                return (true, Captures);
            else return (false, Captures);
        }
    }
}

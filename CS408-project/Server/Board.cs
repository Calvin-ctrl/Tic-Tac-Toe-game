using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe_Server
{
    public class Board
    {

        private char[] cells = new char[9];

        public Board()
        {
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = (char)(i + '1');
            }
        }

        public void MakeMove(PlayerSymbol symbol, int cell)
        {
            if (cell >= 1 && cell <= 9 && cells[cell - 1] != 'X' && cells[cell - 1] != 'O')
            {
                cells[cell - 1] = symbol == PlayerSymbol.X ? 'X' : 'O';
            }
        }

        public string GetBoardString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cells.Length; i++)
            {
                sb.Append(" " + cells[i] + " ");
                if (i % 3 == 2)
                {
                    sb.Append("\n");
                }
                else
                {
                    sb.Append("|");
                }
            }
            return sb.ToString();
        }

        public string check_winneer()
        {
            //Checking the horizontal, vertical, and diagonal lines for winners
            int[][] winningConditions = new int[][]
             {
                    new int[]{ 0, 1, 2 },
                    new int[]{ 3, 4, 5 },
                    new int[]{ 6, 7, 8 },
                    new int[]{ 0, 3, 6 },
                    new int[]{ 1, 4, 7 },
                    new int[]{ 2, 5, 8 },
                    new int[]{ 0, 4, 8 },
                    new int[]{ 2, 4, 6 },
            };

            foreach (var condition in winningConditions)
            {

                if (cells[condition[0]] == cells[condition[1]] && cells[condition[1]] == cells[condition[2]])
                {
                    return "winner";
                }
            }
            // Checkfor a draw if there is no winner
            bool isDraw = true;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != 'X' && cells[i] != 'O')
                {
                    isDraw = false;
                    break;
                }
            }

            if (isDraw)
            {
                return "draw";
            }

            return "no winner";
        }

        public bool IsCellFree(int cell)
        {
            return cell >= 1 && cell <= 9 && cells[cell - 1] != 'X' && cells[cell - 1] != 'O';
        }
    }
 }


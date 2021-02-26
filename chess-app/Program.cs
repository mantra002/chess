using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    using Chess.Management;
    using Chess.Game;
    class Program
    {
        static void Main(string[] args)
        {
            GameManager gm = new GameManager("3k4/3Q4/8/8/8/8/3R4/3K4 b - - 0 1");
            //gm.PlayMove(new Move("b1a3", gm.Board));
            gm.PerftDivided(1);
            Console.ReadKey();
            /*for(int i = 0; i < 80; i++)
            {
                gm.GenerateAndPlayRandomMove();
                gm.PrintBoard();
                Console.Write("Move? : ");
                textMove = Console.ReadLine();
                if (textMove.ToUpper() == "UNDO") gm.UnplayMoves(2);
                else gm.PlayMove(textMove);
                gm.PrintBoard();
            }*/

        }
    }
}

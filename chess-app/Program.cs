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
            GameManager gm = new GameManager("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1");

            //gm.RunPerft(1);
            gm.PerftDivided(3);
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

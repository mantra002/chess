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
            GameManager gm = new GameManager("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 2");

            //gm.RunPerft(1);
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

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
            GameManager gm = new GameManager("rnR2k1r/pp2bppp/1qpQ4/8/2B5/8/PPP1NnPP/RNB1K2R b KQ - 0 9");

            //gm.RunPerft(5);
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

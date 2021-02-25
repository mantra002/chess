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
            string textMove;
            GameManager gm = new GameManager();
            //gm.RunPerft(4);
            for(int i = 0; i < 80; i++)
            {
                gm.GenerateAndPlayRandomMove();
                gm.PrintBoard();
                Console.Write("Move? : ");
                textMove = Console.ReadLine();
                if (textMove.ToUpper() == "UNDO") gm.UnplayMoves(2);
                else gm.PlayMove(textMove);
                gm.PrintBoard();
            }
            
        }
    }
}

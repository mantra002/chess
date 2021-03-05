using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    using Chess.Management;
    using Chess.Game;
    using Chess.Engine;
    class Program
    {
        static void Main(string[] args)

        {

            Board b = new Board("2kr3r/pppq1p2/2np1n1p/2b3p1/2BpP1b1/2P2NB1/PP1N1PPP/R2Q1RK1 w - - 0 12");
            Search s = new Search(b);
            s.StartSearch(7);
            //GameManager gm = new GameManager();
            //gm.RunPerft(5);
            Console.ReadKey();
            //string textMove;
            //for(int i = 0; i < 80; i++)
            //{
            //    Console.WriteLine("Z Hash: " + gm.Board.ZobristHash);
            //    Console.WriteLine("Eval: " + Engine.Evaluation.Evaluate(gm.Board));
            //    gm.PrintBoard();
            //    Console.Write("Move? : ");
            //    textMove = Console.ReadLine();
            //    if (textMove.ToUpper() == "UNDO") gm.UnplayMoves(1);
            //    else if (textMove.ToUpper() == "AUTO") gm.GenerateAndPlayRandomMove();
            //    else gm.PlayMove(textMove);
            //}

        }
    }
}

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
            Board b = new Board("5k2/8/8/2r5/8/8/8/3KQ3 w - - 0 1");
            Search s = new Search(b);
            s.StartSearch();
            //GameManager gm = new GameManager();
            //gm.RunPerft(5);
            //Console.ReadKey();
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

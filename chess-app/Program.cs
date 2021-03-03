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
            Board b = new Board("k7/8/8/8/8/8/2Q5/1Q5K w - - 0 1");
            GameManager gameManager = new GameManager(b);
            gameManager.RunPerft(5);
            Search s = new Search(b);
            s.StartSearch();

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

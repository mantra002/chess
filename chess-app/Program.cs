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
            GameManager gm = new GameManager("r3kb1r/ppp1ppp1/2n2n1p/qB4B1/3P2b1/2N2N2/PPP2PPP/R2Q1RK1 b kq - 1 8");
            Search s = new Search(gm.Board);
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

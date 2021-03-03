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
            GameManager gm = new GameManager("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            string textMove;
            for(int i = 0; i < 80; i++)
            {
                Console.WriteLine("Z Hash: " + gm.Board.ZobristHash);
                Console.WriteLine("Eval: " + Engine.Evaluation.Evaluate(gm.Board));
                gm.PrintBoard();
                Console.Write("Move? : ");
                textMove = Console.ReadLine();
                if (textMove.ToUpper() == "UNDO") gm.UnplayMoves(1);
                else if (textMove.ToUpper() == "AUTO") gm.GenerateAndPlayRandomMove();
                else gm.PlayMove(textMove);
            }

        }
    }
}

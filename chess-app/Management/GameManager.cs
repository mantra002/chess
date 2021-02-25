using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Management
{
    using Chess.Game;
    public class GameManager
    {
        public Board board;
        public Random r;

        public GameManager(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            board = new Board(fen);
            r = new Random();
        }
        public void GenerateAndPlayRandomMove()
        {
            List<Move> cms = MoveGeneration.GenerateLegalMoves(board);
            Move randomMove = cms[r.Next(0, cms.Count())];
            this.PlayMove(randomMove);
        }
        public void PlayMove(string m)
        {
            Move move = new Move(m, this.board);
            board.PlayMove(move);
        }
        public void PlayMove(Move m)
        {
            board.PlayMove(m);
        }
        public void PrintBoard()
        {
            Console.WriteLine("".PadRight(8, '='));
            Console.WriteLine(board.ToString());
            Console.WriteLine("".PadRight(8, '='));
        }
        public void UnplayMoves(int movesToUndo)
        {
            for (int i = 0; i < movesToUndo; i++)
            {
                board.UndoMove(board.GameHistory.Peek());
            }
        }
        public void RunPerft(int depth)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            GameManager gm = new GameManager();
            long count;
            sw.Start();
            for (int i = 0; i < depth; i++)
            {
                count = gm.Perft(i);
                Console.WriteLine("Perft result depth: " + i + " Result: " + count + " Time: " + sw.ElapsedMilliseconds + "ms kN/S: " + Math.Round((double)count/(sw.ElapsedMilliseconds)).ToString());
            }
            sw.Stop();
        }
        public long Perft(int depth)
        {
            long nodes = 0;

            if (depth == 0) return 1;

            List<Move> moves = MoveGeneration.GenerateLegalMoves(this.board);
            int numberOfMoves = moves.Count();

            for (int i = 0; i < numberOfMoves; i++)
            {
                //Console.WriteLine(moves[i].ToString().PadLeft(10 - depth));
                this.board.PlayMove(moves[i]);
                nodes += Perft(depth - 1);
                this.board.UndoMove(moves[i]);
            }
            return nodes;
        }
    }
}

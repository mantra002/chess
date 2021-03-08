using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Chess.Management
{
    using Chess.Game;
    using Chess.Engine;
    public class GameManager
    {
        public Board Board;
        public Search AbSearch;
        public OpeningBook<string> OpeningBk;
        public Random r;
        public SearchSettings SearchSet = new SearchSettings();
        Thread search;

        public GameManager(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") : this(new Board(fen))
        {
        }

        public void PerformSearch(short depth, bool startingFromStartpos = false)
        {
            this.AbortSearch();
            search = new Thread(() => AbSearch.StartSearch(depth, this.Board, startingFromStartpos));
            search.IsBackground = true;
            search.Start();
        }
        public void AbortSearch()
        {
            AbSearch.AbortSearch = true;
            if (search != null) search.Join();
        }
        public GameManager(Board b)
        {
            Board = b;
            AbSearch = new Search(Board, SearchSet);
            r = new Random();
        }
        public void GenerateAndPlayRandomMove()
        {
            List<Move> cms = MoveGeneration.GenerateLegalMoves(Board);
            Move randomMove = cms[r.Next(0, cms.Count())];
            this.PlayMove(randomMove);
        }
        public void PlayMove(string m)
        {
            Move move = new Move(m, this.Board);
            Board.PlayMove(move);
        }
        public void PlayMove(Move m)
        {
            Board.PlayMove(m);
        }
        public void PrintBoard()
        {
            Console.WriteLine("".PadRight(8, '='));
            Console.WriteLine(Board.ToString());
            Console.WriteLine("".PadRight(8, '='));
        }
        public void UnplayMoves(int movesToUndo)
        {
            for (int i = 0; i < movesToUndo; i++)
            {
                Board.UndoMove(Board.GameHistory.Peek().PlayedMove);
            }
        }
        public void RunPerft(int depth)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            GameManager gm = new GameManager();
            long count;
            sw.Start();
            for (int i = 0; i < depth + 1; i++)
            {
                count = gm.Perft(i);
                Console.WriteLine("Perft result depth: " + i + " Result: " + count + " Time: " + sw.ElapsedMilliseconds + "ms kN/S: " + Math.Round((double)count / (sw.ElapsedMilliseconds)).ToString());
            }
            sw.Stop();
        }
        public long Perft(int depth)
        {
            long nodes = 0;

            if (depth == 0)
            {
                return 1;
            }
            Move m;

            List<Move> moves = MoveGeneration.GenerateLegalMoves(this.Board);
            int numberOfMoves = moves.Count();

            for (int i = 0; i < numberOfMoves; i++)
            {
                m = moves[i];
                //Console.WriteLine(m.ToString().PadLeft(10 - depth));
                this.Board.PlayMove(m);
                nodes += Perft(depth - 1);
                this.Board.UndoMove(moves[i]);
            }


            return nodes;
        }

        public long PerftDivided(int depth)
        {
            long nodes = 0;
            long totalNodes = 0;
            string moveStr;
#if DEBUG
            Enums.Colors ColorToMove = Board.ColorToMove;
            byte CastleMask = Board.CastleMask; //White Short - White Long - Black Short - Black Long
            Enums.Squares EnPassantTarget = Board.EnPassantTarget;
            bool InCheck = Board.InCheck;
            bool CheckMate = Board.CheckMate;
            byte[] KingSquares = Board.KingSquares;


            byte[] originalBoard = new byte[64];
            List<ushort> originalPieceList = this.Board.PieceList.ConvertAll(x => x);
            for (int i = 0; i < 64; i++)
            {
                originalBoard[i] = this.Board.GameBoard[i];
            }
            Console.WriteLine("".PadLeft(10, '='));
            this.PrintBoard();
            Console.WriteLine("".PadLeft(10, '='));
#endif
            List<Move> moves = MoveGeneration.GenerateLegalMoves(this.Board);
            moves.Sort();


            foreach (Move m in moves)
            {
                moveStr = m.ToString();
                nodes = 0;
                //Console.WriteLine("Parent: " + moveStr);
                this.Board.PlayMove(m);
                nodes += Perft(depth - 1);
                totalNodes += nodes;
                this.Board.UndoMove(m);
#if DEBUG
                for (int i = 0; i < 64; i++)
                {
                    if (this.Board.GameBoard[i] != originalBoard[i])
                    {
                        Console.WriteLine("".PadLeft(10, '='));
                        Console.WriteLine("Game board is mismatched after play/unplay at " + (Enums.Squares)i);
                        Console.WriteLine("Original Board has " + originalBoard[i]);
                        Console.WriteLine("Current board has " + this.Board.GameBoard[i]);
                        this.PrintBoard();
                        Console.WriteLine("".PadLeft(10, '='));
                    }
                }

                if (originalPieceList.Count() != this.Board.PieceList.Count()) Console.WriteLine("Piecelist has extra/missing pieces");
                foreach (ushort og in originalPieceList)
                {
                    if (!this.Board.PieceList.Contains(og)) Console.WriteLine("Piecelists are not identical");
                }
                if (ColorToMove != Board.ColorToMove) Console.WriteLine("Color to play desynced!");
                if (CastleMask != Board.CastleMask) Console.WriteLine("Castle mask desynced!"); //White Short - White Long - Black Short - Black Long
                if (EnPassantTarget != Board.EnPassantTarget) Console.WriteLine("EP target desynced!");
                if (InCheck != Board.InCheck) Console.WriteLine("In check desynced!");
                if (CheckMate != Board.CheckMate) Console.WriteLine("Checkmate desynced!");
                if (KingSquares != Board.KingSquares) Console.WriteLine("King squares desynced!");
#endif
                Console.WriteLine(m.ToString() + ": " + nodes);
            }

            Console.WriteLine("Depth " + depth + "    Count: " + totalNodes);
            return nodes;
        }
    }
}

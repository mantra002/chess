using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace Chess.Engine
{
    using Chess.Game;

    // This implementation is HEAVILY leaveraged from: https://github.com/SebLague/Chess-AI

    public class Search
    {
        public short TargetDepth = 6;
        const short MaximumQuiescenceSearchDepth = 10;
        const bool IterativeDeepeningEnable = true;
        const bool QuiescenceSearchEnable = true;
        const bool UseMoveOrdering = true;
        Board board;
        int BestEval;
        Move BestMove;

        TranspositionTable tt;

        const int MateScore = 100000;
        const int PositiveInfinity = 9999999;
        const int NegativeInfinity = -PositiveInfinity;

        List<Move> PrincipalVariation;

        Move BestMoveSoFar;
        int BestEvalSoFar;

        int numNodes;
        int numDeltaCutoffs;
        int numCutoffCount;
        int numTTHit;
        int numSeeCutoff;
        int qDepth;


        public Search(Board b)
        {
            this.board = b;
            tt = new TranspositionTable();
        }

        public void StartSearch(short depth)
        {
            this.TargetDepth = depth;
            PrincipalVariation = new List<Move>();
            numTTHit = numCutoffCount = numNodes = 0;
  
            BestEval = BestEvalSoFar = qDepth = 0;
            BestMove = BestMoveSoFar = null;

            if(IterativeDeepeningEnable)
            {
                for(int i = 1; i <= TargetDepth; i++)
                {
                    DoSearch(i, 0, NegativeInfinity, PositiveInfinity);
                    PrintSearchStats(i);
                    BestMove = BestMoveSoFar;
                    BestEval = BestEvalSoFar;
                }
            }
            else
            {
                DoSearch (TargetDepth, 0, NegativeInfinity, PositiveInfinity);
                BestMove = BestMoveSoFar;
                BestEval = BestEvalSoFar;
                PrintSearchStats(TargetDepth);
            }
        }
        public void PrintSearchStats(int depth)
        {
            Console.WriteLine("".PadLeft(8, '='));
            Console.WriteLine("Depth: " + depth + "/" + qDepth);
            Console.WriteLine("Nodes: " + numNodes);
            Console.WriteLine("TT Hits: " + numTTHit);
            Console.WriteLine("Cutoffs: ");
            Console.WriteLine("  Beta: "+ numCutoffCount);
            Console.WriteLine("  Delta: " + numDeltaCutoffs);
            Console.WriteLine("  SEE: " + numSeeCutoff);
            Console.WriteLine("Move: " + BestMoveSoFar.ToString());
            Console.WriteLine("Score: " + BestEvalSoFar);
            Console.WriteLine("".PadLeft(8, '='));
        }
        private int DoSearch(int depth, int plyFromRoot, int alpha, int beta, Board b = null)
        {
            if (b == null) b = this.board;
            if (plyFromRoot > 0)
            {
                alpha = Max(alpha, -MateScore + plyFromRoot);
                beta = Min(beta, MateScore - plyFromRoot);
                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            int ttLookupScore = tt.LookupScore(board.ZobristHash, depth, plyFromRoot, alpha, beta);
            if(ttLookupScore != int.MinValue)
            {
                numTTHit++;
                if(plyFromRoot == 0)
                {
                    TranspositionTable.Position p = tt.LookupPosition(board.ZobristHash);
                    BestMoveSoFar = p.MovePlayed;
                    BestEvalSoFar = p.Score;
                }
                return ttLookupScore;
            }

            if (depth == 0)
            {
                if(QuiescenceSearchEnable) return QuiescenceSearch(alpha, beta, plyFromRoot+1);
                else return Evaluation.Evaluate(board);
            }

            List<Move> moves = MoveGeneration.GenerateLegalMoves(board);
            if(UseMoveOrdering) MoveOrdering.OrderMoves(board, tt, moves);
            // Detect checkmate and stalemate when no legal moves are available
            if (moves.Count == 0)
            {
                if (board.InCheck)
                {
                    int mateScore = MateScore - plyFromRoot;
                    return -mateScore;
                }
                else
                {
                    return 0;
                }
            }

            TranspositionTable.NodeType nodeType = TranspositionTable.NodeType.UpperBound;

            Move bestMoveInThisPosition = null;

            for (int i = 0; i < moves.Count; i++)
            {
                board.PlayMove(moves[i]);
                int eval = -DoSearch(depth - 1, plyFromRoot + 1, -beta, -alpha);
                board.UndoMove(moves[i]);
                numNodes++;

                // Move was *too* good, so opponent won't allow this position to be reached
                // (by choosing a different move earlier on). Skip remaining moves.
                if (eval >= beta)
                {
                    tt.AddPosition(board.ZobristHash, beta, moves[i], (byte) depth, TranspositionTable.NodeType.LowerBound);
                    numCutoffCount++;
                    return beta;
                }

                // Found a new best move in this position
                if (eval > alpha)
                {
                    bestMoveInThisPosition = moves[i];
                    nodeType = TranspositionTable.NodeType.Exact;
                    alpha = eval;
                    if (plyFromRoot == 0)
                    {
                        BestMoveSoFar = moves[i];
                        BestEvalSoFar = eval;
                    }
                }
            }
            tt.AddPosition(board.ZobristHash, beta, BestMoveSoFar, (byte)depth, nodeType);
            return alpha;

        }
     
        public (int score, Move m, int MateInPly) CurrentSearchResult()
        {
            int mateInPly = -1;
            if (Math.Abs(BestEval) > Evaluation.MateValue - 1000)
            {
                mateInPly = Evaluation.MateValue - Math.Abs(BestEval);
            }
            return (BestEval, BestMove, mateInPly);
        }

        private int QuiescenceSearch(int alpha, int beta, int plyFromRoot)
        {
            qDepth = Max(plyFromRoot, qDepth);
            int eval = Evaluation.Evaluate(board);
            //return eval;
            if (eval >= beta)
            {
                return beta;
            }
            if(eval < alpha - Evaluation.QueenValue)
            {
                numDeltaCutoffs++;
                return alpha;
            }
            if (eval > alpha)
            {
                alpha = eval;
            }

            List<Move> moves = MoveGeneration.GenerateLegalMoves(board, includeQuietMoves: false);
            if (UseMoveOrdering) MoveOrdering.OrderMoves(board, tt, moves, UseSEE: true);
            //Order moves
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].MoveScore < Evaluation.SeeCutoff)
                {
                    board.PlayMove(moves[i]);
                    eval = -QuiescenceSearch(-beta, -alpha, plyFromRoot + 1);
                    board.UndoMove(moves[i]);
                    this.numNodes++;

                    if (eval >= beta)
                    {
                        this.numCutoffCount++;
                        return beta;
                    }
                    if (eval > alpha)
                    {
                        alpha = eval;
                    }
                }
                else
                {
                    numSeeCutoff++;
                    return alpha;
                }
            }

            return alpha;
        }
    }
}

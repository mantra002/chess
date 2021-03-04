using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace Chess.Engine
{
    using Chess.Game;

    // This implementation is HEAVILY leaveraged from: https://github.com/SebLague/Chess-AI

    public class Search
    {
        const short TargetDepth = 8;
        const bool IterativeDeepening = true;
        Board board;
        int BestEval;
        Move BestMove;
        Move bestIterativeMove;
        int bestIterativeScore;
        long ttHits = 0;
        TranspositionTable tt;

        const int MateScore = 100000;
        const int PositiveInfinity = 9999999;
        const int NegativeInfinity = -PositiveInfinity;

        Move BestMoveSoFar;
        int BestEvalSoFar;

        int numNodes;
        int numCutoffCount;
        int numTTHit;


        public Search(Board b)
        {
            this.board = b;
            tt = new TranspositionTable();
        }

        public void StartSearch()
        {
            numTTHit = numCutoffCount = numNodes =0;
  
            BestEval = bestIterativeScore = 0;
            BestMove = bestIterativeMove = null;

            if(IterativeDeepening)
            {
                for(int i = 1; i <= TargetDepth; i++)
                {
                    DoSearch(i, 0, NegativeInfinity, PositiveInfinity);

                    Console.WriteLine("Depth: " + i + " Nodes: " + numNodes + " TT Hits: " + numTTHit + " Cutoffs: " + numCutoffCount + " Move: " + BestMoveSoFar.ToString() + " Score: " + BestEvalSoFar);
                }
                BestMove = BestMoveSoFar;
                BestEval = BestEvalSoFar;

            }
            else
            {
                DoSearch (TargetDepth, 0, NegativeInfinity, PositiveInfinity);
                Console.WriteLine("Depth: " + TargetDepth + " Nodes: " + numNodes + " TT Hits: " + numTTHit + " Cutoffs: " + numCutoffCount+ " Move: " + BestMoveSoFar.ToString() + " Score: " + BestEvalSoFar);
                BestMove = BestMoveSoFar;
                BestEval = BestEvalSoFar;
            }
        }
        int DoSearch(int depth, int plyFromRoot, int alpha, int beta)
        {
 

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
                int evaluation = Evaluation.Evaluate(board);
                return evaluation;
            }

            List<Move> moves = MoveGeneration.GenerateLegalMoves(board);
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
    }
}

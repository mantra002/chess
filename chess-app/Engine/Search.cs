using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Chess.Engine
{
    using Chess.Game;

    public class Search
    {
        public short TargetDepth;

        Board board;
        public int BestEval;
        public Move BestMove;

        TranspositionTable tt;

        const int MateScore = 100000;
        const int PositiveInfinity = 9999999;
        const int NegativeInfinity = -PositiveInfinity;

        public Move[] PrincipalVariation;
        public List<Move> BookMoves;

        bool inBook = true;
        Random r;

        Move BestMoveSoFar;
        int BestEvalSoFar;

        public int numNodes;
        public int numDeltaCutoffs;
        public int numCutoffCount;
        public int numTTHit;
        public int numSeeCutoff;
        public int qDepth;
        public int nodesPerSecond;

        private Stopwatch sw = new Stopwatch();
        SearchSettings SearchSetting;
        OpeningBook<string> openingBook;

        public Search(Board b, SearchSettings ss)
        {
            this.SearchSetting = ss;
            r = new Random();
            if(this.SearchSetting.UseOpeningBook) openingBook = OpeningBook<string>.InitializeOpeningBook();
            tt = new TranspositionTable(ss.TranspositionTableSizeMb);
        }

        public void StartSearch(short depth, Board b)
        {
            this.board = b;
            this.TargetDepth = depth;
            numTTHit = numCutoffCount = numNodes = 0;

            BestEval = BestEvalSoFar = qDepth = 0;
            BestMove = BestMoveSoFar = null;

            if (SearchSetting.UseOpeningBook)
            {
                BookMoves = new List<Move>();
                if (this.board.ZobristHash == 9293682485474089328)
                {
                    List<string> options = openingBook.ListAllChildren();
                    BookMoves.Add(new Move(options[r.Next(0, options.Count)], board));
                    PrintSearchStats(1);
                    board.PlayMove(BookMoves[0]);
                    depth--;
                }
                else
                {
                    //for(int i)
                }


            }

            sw.Start();
            if (SearchSetting.IterativeDeepeningEnable)
            {
                for (int i = 1; i <= depth; i++)
                {
                    PrincipalVariation = new Move[i];
                    DoSearch(i, 0, NegativeInfinity, PositiveInfinity);
                    nodesPerSecond = (int)(numNodes / (sw.ElapsedMilliseconds / 1000.0));
                    BestMove = BestMoveSoFar;
                    BestEval = BestEvalSoFar;
                    PrintSearchStats(i);
                }
            }
            else
            {
                DoSearch(depth, 0, NegativeInfinity, PositiveInfinity);
                BestMove = BestMoveSoFar;
                BestEval = BestEvalSoFar;
                nodesPerSecond = (int)(numNodes / (sw.ElapsedMilliseconds / 1000.0));
                PrintSearchStats(TargetDepth);
            }
            sw.Reset();
            if (SearchSetting.UseOpeningBook && BookMoves.Count > 0)
            {
                for (int i = BookMoves.Count - 1; i < 0; i--)
                {
                    board.UndoMove(BookMoves[i]);
                }
            }
        }

        public void PrintSearchStats(int depth)
        {
            int realDepth = depth;
            if (SearchSetting.UseOpeningBook && BookMoves.Count > 0)
            {
                realDepth = BookMoves.Count + depth;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("info depth ");
            sb.Append(realDepth);
            sb.Append(" seldepth ");
            sb.Append(qDepth);
            sb.Append(" nodes ");
            sb.Append(numNodes);
            sb.Append(" nps ");
            sb.Append(nodesPerSecond);
            sb.Append(" score cp ");
            sb.Append(BestEval);
            sb.Append(" pv ");
            if (SearchSetting.UseOpeningBook && BookMoves != null)
            {
                for (int i = 0; i < BookMoves.Count; i++)
                {
                    sb.Append(BookMoves[i].ToString() + " ");
                }
            }
            if (PrincipalVariation != null)
            {
                for (int i = 0; i < depth; i++)
                {
                    if (PrincipalVariation[i] != null) sb.Append(PrincipalVariation[i].ToString() + " ");
                }
            }
            Console.WriteLine(sb.ToString());
        }
        private int DoSearch(int depth, int plyFromRoot, int alpha, int beta)
        {
            int eval;
         
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
            if (ttLookupScore != int.MinValue)
            {
                numTTHit++;
                if (plyFromRoot == 0)
                {
                    TranspositionTable.Position p = tt.LookupPosition(board.ZobristHash);
                    BestMoveSoFar = p.MovePlayed;
                    BestEvalSoFar = p.Score;
                    PrincipalVariation[plyFromRoot] = p.MovePlayed;
                }
                return ttLookupScore;
            }

            if (depth == 0)
            {
                if (SearchSetting.QuiescenceSearchEnable) return QuiescenceSearch(alpha, beta, plyFromRoot + 1, SearchSetting);
                else return Evaluation.Evaluate(board);
            }

            List<Move> moves = MoveGeneration.GenerateLegalMoves(board);
            if (SearchSetting.UseMoveOrdering) MoveOrdering.OrderMoves(board, tt, moves);
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
                eval = -DoSearch(depth - 1, plyFromRoot + 1, -(alpha + 1), -alpha);
                if (alpha < eval && eval < beta) eval = -DoSearch(depth - 1, plyFromRoot + 1, -beta, -alpha);
                board.UndoMove(moves[i]);
                numNodes++;

                // Move was *too* good, so opponent won't allow this position to be reached
                // (by choosing a different move earlier on). Skip remaining moves.
                if (eval >= beta)
                {
                    tt.AddPosition(board.ZobristHash, beta, moves[i], (byte)depth, TranspositionTable.NodeType.LowerBound);
                    numCutoffCount++;
                    return beta;
                }

                // Found a new best move in this position
                if (eval > alpha)
                {
                    bestMoveInThisPosition = moves[i];
                    PrincipalVariation[plyFromRoot] = moves[i];
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

        private int QuiescenceSearch(int alpha, int beta, int plyFromRoot, SearchSettings ss)
        {
            qDepth = Max(plyFromRoot, qDepth);
            int eval = Evaluation.Evaluate(board);
            //return eval;
            if (eval >= beta)
            {
                return beta;
            }
            if (eval < alpha - Evaluation.QueenValue)
            {
                numDeltaCutoffs++;
                return alpha;
            }
            if (eval > alpha)
            {
                alpha = eval;
            }

            List<Move> moves = MoveGeneration.GenerateLegalMoves(board, includeQuietMoves: false);
            if (ss.UseMoveOrdering) MoveOrdering.OrderMoves(board, tt, moves, UseSEE: true);
            //Order moves
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].MoveScore < Evaluation.SeeCutoff)
                {
                    board.PlayMove(moves[i]);
                    eval = -QuiescenceSearch(-(alpha + 1), -alpha, plyFromRoot + 1, ss);
                    if (alpha < eval && eval < beta) eval = -QuiescenceSearch(-beta, -alpha, plyFromRoot + 1, ss);
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

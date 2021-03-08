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

// Based on a combination of this https://github.com/SebLague/Chess-AI and https://en.wikipedia.org/wiki/Principal_variation_search 

namespace Chess.Engine
{
    using Chess.Game;

    public class Search
    {
        public short TargetDepth;

        private Board board;
        public int BestEval;
        public Move BestMove;

        public bool AbortSearch = false;

        private TranspositionTable tt;

        private const int PositiveInfinity = 9999999;
        private const int NegativeInfinity = -PositiveInfinity;

        public Move[] PrincipalVariation;
        public Move BookMove;

        private bool inBook = true;
        private Random r;

        private Move BestMoveSoFar;
        private int BestEvalSoFar;
        private int MateInPly;

        public int numNodes;
        public int numDeltaCutoffs;
        public int numCutoffCount;
        public int numTTHit;
        public int numSeeCutoff;
        public int qDepth;
        public int nodesPerSecond;

        private Stopwatch sw = new Stopwatch();
        public SearchSettings SearchSetting;
        private OpeningBook<string> openingBook;

        public Search(Board b, SearchSettings ss)
        {
            this.SearchSetting = ss;
            r = new Random();
            if(this.SearchSetting.UseOpeningBook) openingBook = OpeningBook<string>.InitializeOpeningBook();
            tt = new TranspositionTable(ss.TranspositionTableSizeMb);
        }

        public void StartSearch(short depth, Board b, bool startingFromStartPos = false)
        {
            AbortSearch = false;
            this.board = b;
            this.TargetDepth = depth;
            numTTHit = numCutoffCount = numNodes = 0;

            BestEval = BestEvalSoFar = qDepth = 0;
            BestMove = BestMoveSoFar = null;

            inBook = startingFromStartPos;
            BookMove = null;

            if (SearchSetting.UseOpeningBook && inBook)
            {
                OpeningBook<string> childBook = openingBook;
                List<Move> playedMoves = b.GetPlayedMoves();
                for(int i = playedMoves.Count - 1; i >= 0; i--)
                {
                    childBook = childBook.GetChildList(playedMoves[i].ToString());
                    if (childBook == null)
                    {
                        inBook = false;
                        break;
                    }
                }
                if (inBook)
                {
                    List<string> options = childBook.ListAllChildren();
                    string lastMove = options[r.Next(0, options.Count)];
                    BookMove = new Move(lastMove, board);
                    PrintSearchStats(0); //Passing a depth of zero because the book depth is handled in the print search method
                    board.PlayMove(BookMove);
                    depth--;
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
                    MateInPly = GetMateInNMoves();
                    PrintSearchStats(i);

                    if (MateInPly != -1 && i > 10) break;
                }
            }
            else
            {
                DoSearch(depth, 0, NegativeInfinity, PositiveInfinity);
                BestMove = BestMoveSoFar;
                BestEval = BestEvalSoFar;
                nodesPerSecond = (int)(numNodes / (sw.ElapsedMilliseconds / 1000.0));
                MateInPly = GetMateInNMoves();
                PrintSearchStats(TargetDepth);
            }
            sw.Reset();
            if(SearchSetting.UseOpeningBook && inBook) board.UndoMove(BookMove);

        }

        public void PrintSearchStats(int depth)
        {
            int realDepth = depth;
            if (AbortSearch) return;
            if (SearchSetting.UseOpeningBook && BookMove != null)
            {
                realDepth = 1 + depth;
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
            if (MateInPly == -1)
            {
                sb.Append(" score cp ");
                sb.Append(BestEval);
            }
            else
            {
                sb.Append(" score mate ");
                sb.Append(MateInPly * Math.Sign(BestEval));
            }
            sb.Append(" pv ");
            if (SearchSetting.UseOpeningBook && BookMove != null)
            {
                    sb.Append(BookMove.ToString() + " ");
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
            if (AbortSearch) return 0;
            if (plyFromRoot > 0)
            {
                alpha = Max(alpha, -Evaluation.MateValue + plyFromRoot);
                beta = Min(beta, Evaluation.MateValue - plyFromRoot);
                if (alpha >= beta)
                {
                    //Mate has been found
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
            // Detect checkmate and stalemate, could use the board states for this.
            if (moves.Count == 0)
            {
                if (board.InCheck)
                {
                    //Checkmate
                    int mateScore = Evaluation.MateValue - plyFromRoot;
                    return -mateScore;
                }
                else
                {
                    return 0; //Stalemate
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

                    // Beta cutoff
                    if (eval >= beta)
                    {
                        tt.AddPosition(board.ZobristHash, beta, moves[i], (byte)depth, TranspositionTable.NodeType.LowerBound);
                        numCutoffCount++;
                        return beta;
                    }

                    // New best move
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

        public int GetMateInNMoves()
        {
            int mateInPly = -1;
            if (Math.Abs(BestEval) > Evaluation.MateValue - 1000)
            {
                mateInPly = Evaluation.MateValue - Math.Abs(BestEval);
            }
            return mateInPly;
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

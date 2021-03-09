using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private Board _board;
        public int BestEval;
        public Move BestMove;

        public bool AbortSearch = false;

        private TranspositionTable _tt;

        private const int _positiveInfinity = 9999999;
        private const int _negativeInfinity = -_positiveInfinity;

        public Move[] PrincipalVariation;
        public Move BookMove;

        private bool _inBook = true;
        private bool _usingTimeControl = false;
        private Random _random;
        private Thread timeKeeper;

        private Move _bestMoveSoFar;
        private int _bestEvalSoFar;
        private int _mateInPly;
        private int _moveOutOfBook;
        private long _predictedTime;
        private long _timeBank;

        public int numNodes;
        public int numDeltaCutoffs;
        public int numCutoffCount;
        public int numTTHit;
        public int numSeeCutoff;
        public int qDepth;
        public int nodesPerSecond;

        private Stopwatch _stopWatch = new Stopwatch();
        public SearchSettings SearchSetting;
        private OpeningBook<string> _openingBook;

        public Search(Board b, SearchSettings SearchSetting)
        { 
            this.SearchSetting = SearchSetting;
            _random = new Random();
            if(SearchSetting.UseOpeningBook) _openingBook = OpeningBook<string>.InitializeOpeningBook();
            _tt = new TranspositionTable(SearchSetting.TranspositionTableSizeMb);
            _timeBank = 0;
        }

        public void StartSearch(Board b, SearchSettings SearchSetting,  bool startingFromStartPos = false)
        {
            this.SearchSetting = SearchSetting;
            AbortSearch = false;
            this._board = b;
            numTTHit = numCutoffCount = numNodes = 0;

            BestEval = _bestEvalSoFar = qDepth = 0;
            BestMove = _bestMoveSoFar = null;

            _inBook = startingFromStartPos;
            BookMove = null;

            _stopWatch.Restart();
            if ((SearchSetting.WhiteIncrementInMs != 0 || SearchSetting.BlackIncrementInMs != 0 || SearchSetting.WhiteTimeInMs != 0 || SearchSetting.BlackTimeInMs != 0 || SearchSetting.TimeLimitInMs != 0) && SearchSetting.Depth == 0)
            {
                _predictedTime = GetTimeAllowedForSearch();
                timeKeeper = new Thread(() => this.TimeWatchdog(_predictedTime));
                timeKeeper.IsBackground = true;
                timeKeeper.Start();
                Console.WriteLine("info string estimated time to search: " + _predictedTime + "ms");
                SearchSetting.InfiniteSearch = true;
                _usingTimeControl = true;

            }

            int depth = SearchSetting.Depth;
            if (SearchSetting.InfiniteSearch) depth = 100;
            
            if (SearchSetting.UseOpeningBook && _inBook)
            {
                OpeningBook<string> childBook = _openingBook;
                List<Move> playedMoves = b.GetPlayedMoves();
                for(int i = playedMoves.Count - 1; i >= 0; i--)
                {
                    childBook = childBook.GetChildList(playedMoves[i].ToString());
                    if (childBook == null)
                    {
                        _inBook = false;
                        _moveOutOfBook = b.MoveCounter;
                        break;
                    }
                }
                if (_inBook)
                {
                    List<string> options = childBook.ListAllChildren();
                    string lastMove = options[_random.Next(0, options.Count)];
                    BookMove = new Move(lastMove, _board);
                    PrintSearchStats(0); //Passing a depth of zero because the book depth is handled in the print search method
                    _board.PlayMove(BookMove);
                    depth--;
                }
            }

            
            if (SearchSetting.IterativeDeepeningEnable)
            {
                for (int i = 1; i <= depth; i++)
                {
                    PrincipalVariation = new Move[i];
                    DoSearch(i, 0, _negativeInfinity, _positiveInfinity);
                    nodesPerSecond = (int)(numNodes / (_stopWatch.ElapsedMilliseconds / 1000.0));
                    if (!AbortSearch)
                    {
                        BestMove = _bestMoveSoFar;
                        BestEval = _bestEvalSoFar;
                    }
                    _mateInPly = GetMateInNMoves();
                    PrintSearchStats(i);

                    if (_mateInPly != -1 && i > 10) break;
                }
                if(timeKeeper != null) timeKeeper.Abort();
            }
            else
            {
                DoSearch(depth, 0, _negativeInfinity, _positiveInfinity);
                BestMove = _bestMoveSoFar;
                BestEval = _bestEvalSoFar;
                nodesPerSecond = (int)(numNodes / (_stopWatch.ElapsedMilliseconds / 1000.0));
                _mateInPly = GetMateInNMoves();
                PrintSearchStats(SearchSetting.Depth);
            }
            _stopWatch.Stop();
            if (_usingTimeControl)
            {
                _timeBank += (int)(_predictedTime - _stopWatch.ElapsedMilliseconds);
                Console.WriteLine("info string banking: " + _timeBank + "ms");
            }
            if(SearchSetting.UseOpeningBook && _inBook) _board.UndoMove(BookMove);
            if (BookMove != null) BestMove = BookMove;
            if(BestMove != null) Console.Write("bestmove " + BestMove.ToString());
            if (PrincipalVariation != null)
            {
                if (BookMove != null && PrincipalVariation[0] != null) Console.Write(" ponder " + PrincipalVariation[0]);
                else if (PrincipalVariation.Count() >= 2 && PrincipalVariation[1] != null)
                {
                    Console.Write(" ponder " + PrincipalVariation[1]);
                }
            }

            Console.WriteLine();
        }
        private void TimeWatchdog(long timeAllowed)
        {
            while(timeAllowed > _stopWatch.ElapsedMilliseconds)
            {
                Thread.Sleep(200);
            }
            this.AbortSearch = true;
            Console.WriteLine("info string search aborted for time " + timeAllowed + " / " + _stopWatch.ElapsedMilliseconds);
        }
        private long GetTimeAllowedForSearch()
        {
            int movesRemaining = (SearchSetting.MovesToGoUntilAdditionalTime != 0) ? SearchSetting.MovesToGoUntilAdditionalTime : SearchSetting.AssumedGameLength - _board.MoveCounter; 
            if (SearchSetting.TimeLimitInMs != 0) return SearchSetting.TimeLimitInMs;
            if(_board.ColorToMove == Enums.Colors.White)
            {
                return Math.Max((long)(SearchSetting.WhiteIncrementInMs * 0.8 + SearchSetting.WhiteTimeInMs / (double)movesRemaining) + _timeBank, 250); 
            }
            else return Math.Max((long)(SearchSetting.BlackIncrementInMs * 0.8 + SearchSetting.BlackTimeInMs / (double)movesRemaining) + _timeBank, 250);
        }

        private int EstimateDepthFromTime(long timeInMs)
        {
            double depth = 0.8426 * Math.Log(timeInMs) - 0.6893;
            return Math.Max(4,(int)Math.Floor(depth));
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
            sb.Append(" time ");
            sb.Append(_stopWatch.ElapsedMilliseconds);
            sb.Append(" hashfull ");
            sb.Append((int)(_tt.PercentFull*1000));
            if (_mateInPly == -1)
            {
                sb.Append(" score cp ");
                sb.Append(BestEval);
            }
            else
            {
                sb.Append(" score mate ");
                sb.Append(_mateInPly * Math.Sign(BestEval));
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

            int ttLookupScore = _tt.LookupScore(_board.ZobristHash, depth, plyFromRoot, alpha, beta);
            if (ttLookupScore != int.MinValue)
            {
                numTTHit++;
                if (plyFromRoot == 0)
                {
                    TranspositionTable.Position p = _tt.LookupPosition(_board.ZobristHash);
                    _bestMoveSoFar = p.MovePlayed;
                    _bestEvalSoFar = p.Score;
                    PrincipalVariation[plyFromRoot] = p.MovePlayed;
                }
                return ttLookupScore;
            }

            if (depth == 0)
            {
                if (SearchSetting.QuiescenceSearchEnable) return QuiescenceSearch(alpha, beta, plyFromRoot + 1, SearchSetting);
                else return Evaluation.Evaluate(_board);
            }

            List<Move> moves = MoveGeneration.GenerateLegalMoves(_board);
            if (SearchSetting.UseMoveOrdering) MoveOrdering.OrderMoves(_board, _tt, moves);
            // Detect checkmate and stalemate, could use the board states for this.
            if (moves.Count == 0)
            {
                if (_board.InCheck)
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
                    _board.PlayMove(moves[i]);
                    eval = -DoSearch(depth - 1, plyFromRoot + 1, -(alpha + 1), -alpha);
                    if (alpha < eval && eval < beta) eval = -DoSearch(depth - 1, plyFromRoot + 1, -beta, -alpha);
                    _board.UndoMove(moves[i]);
                    numNodes++;

                    // Beta cutoff
                    if (eval >= beta)
                    {
                        _tt.AddPosition(_board.ZobristHash, beta, moves[i], (byte)depth, TranspositionTable.NodeType.LowerBound);
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
                            _bestMoveSoFar = moves[i];
                            _bestEvalSoFar = eval;
                        }
                    }
                }
            _tt.AddPosition(_board.ZobristHash, beta, _bestMoveSoFar, (byte)depth, nodeType);
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
            if (AbortSearch) return 0;
            qDepth = Max(plyFromRoot, qDepth);
            int eval = Evaluation.Evaluate(_board);
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

            List<Move> moves = MoveGeneration.GenerateLegalMoves(_board, includeQuietMoves: false);
            if (ss.UseMoveOrdering) MoveOrdering.OrderMoves(_board, _tt, moves, UseSEE: true);
            //Order moves
            for (int i = 0; i < moves.Count; i++)
            {
                    if (moves[i].MoveScore < Evaluation.SeeCutoff)
                    {
                        _board.PlayMove(moves[i]);
                        eval = -QuiescenceSearch(-(alpha + 1), -alpha, plyFromRoot + 1, ss);
                        if (alpha < eval && eval < beta) eval = -QuiescenceSearch(-beta, -alpha, plyFromRoot + 1, ss);
                        _board.UndoMove(moves[i]);
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

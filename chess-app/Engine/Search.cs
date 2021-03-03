using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Engine
{
    using Chess.Game;
    public class Search
    {
        const short TargetDepth = 5;
        const bool IterativeDeepening = true;
        Board board;
        int BestEval;
        Move BestMove;
        int bestIterativeEval;
        long NodesChecked = 0;
        Move bestIterativeMove;

        public Search(Board b)
        {
            this.board = b;
        }

        public void StartSearch()
        {
            NodesChecked = 0;
            BestEval = bestIterativeEval = 0;
            BestMove = bestIterativeMove = null;

            if(IterativeDeepening)
            {
                for(int i = 1; i <= TargetDepth; i++)
                {
                    DoSearch(i, 0, -200000, 200000);
                    Console.WriteLine("Depth: " + i + " Nodes: " + NodesChecked + " Move: " + bestIterativeMove.ToString() + " Score: " + bestIterativeEval);
                }
                BestMove = bestIterativeMove;
                BestEval = bestIterativeEval;

            }
            else
            {
                DoSearch(TargetDepth, 0, -200000, 200000);
                BestMove = bestIterativeMove;
                BestEval = bestIterativeEval;
            }
        }

        public int DoSearch(int depth, int currentPly, int alpha, int beta)
        {
            int score;
            NodesChecked++;
            if (depth == 0) return Evaluation.Evaluate(board);

            List<Move> moves = MoveGeneration.GenerateLegalMoves(this.board);
            if(moves.Count == 0)
            {
                if (this.board.InCheck)
                {
                    return -Evaluation.MateValue + currentPly;
                }
                return 0;
            }
            for(int i = 0; i< moves.Count; i ++)
            {
                this.board.PlayMove(moves[i]);
                score = -DoSearch(depth - 1, currentPly + 1, -beta, -alpha);
                this.board.UndoMove(moves[i]);

                if(score >= beta)
                {
                    return beta;
                }

                if(score > alpha)
                {
                    alpha = score;
                    if(currentPly == 0)
                    {
                        bestIterativeEval = score;
                        bestIterativeMove = moves[i];
                    }
                }
            }
            return alpha;
        }

        public (int score, Move m) CurrentSearchResult()
        {
            return (BestEval, BestMove);
        }
    }
}

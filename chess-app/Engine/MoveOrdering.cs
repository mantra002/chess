using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chess.Game;


namespace Chess.Engine
{
    using static Enums;
    static public class MoveOrdering
    {
        public static void OrderMoves(Board b, TranspositionTable tt, List<Move> moves, bool UseSEE = false)
        {
            int score;
            Move ttMove = tt.LookupPosition(b.ZobristHash).MovePlayed;

            int thisPieceValue;


            foreach (Move m in moves)
            {
                score = 0;
                thisPieceValue = Evaluation.GetPieceValue(m.Piece);

                if (m.PieceCaptured != 0)
                {
                    if(UseSEE)
                    {
                        score += StaticExchangeEvaluation(b, m, thisPieceValue);
                    }
                    else
                    {
                        score -= thisPieceValue;
                        score += Evaluation.GetPieceValue(m.PieceCaptured) * Evaluation.CaptureBonusMultiplier;
                    }
                }
                if(m.PromoteIntoPiece != 0)
                {
                    score += Evaluation.GetPieceValue(m.PromoteIntoPiece);
                }

                if (ttMove != null && m.MoveHash == ttMove.MoveHash) score += 10000;
                m.MoveScore = -score; //Sorting small = good
            }

            moves.Sort();
        }

        public static int StaticExchangeEvaluation(Board b, Move m, int attackingPieceValue = 0)
        {
            int score = Evaluation.GetPieceValue(m.PieceCaptured);
            int currentDefenderScore = 0;
            byte opponentColor = (byte)(2 - (byte)b.ColorToMove);
            byte ourColor = (byte)((byte)b.ColorToMove - 1);
            int ourAttackingPiecesCount = 0;
            bool foundFirstAttackingPiece = false;

            if (attackingPieceValue == 0) attackingPieceValue = Evaluation.GetPieceValue(m.Piece);
            int currentAttackerScore = attackingPieceValue;

            if (b.AttackedSquares[ourColor][m.Destination] != null) ourAttackingPiecesCount = b.AttackedSquares[ourColor][m.Destination].Count;

            if (b.AttackedSquares[opponentColor][m.Destination] != null)
            {

                for(int i = 0; i < b.AttackedSquares[opponentColor][m.Destination].Count; i++)
                {
                    currentDefenderScore = Evaluation.GetPieceValue((byte)b.AttackedSquares[opponentColor][m.Destination][i]); //Value of the nth defender of the piece we're trying to capture
                    if (currentDefenderScore <= currentAttackerScore) score -= currentAttackerScore; //If it's less than or equal value, it could capture back
                    if (i < ourAttackingPiecesCount) // If we have another piece of less than or equal value of that defender we could capture back
                    {
                        currentAttackerScore = Evaluation.GetPieceValue((byte)b.AttackedSquares[ourColor][m.Destination][i]);
                        if (currentAttackerScore <= currentDefenderScore)
                        {
                            if (currentAttackerScore == attackingPieceValue && !foundFirstAttackingPiece) foundFirstAttackingPiece = true;
                            else score += currentDefenderScore;
                        }
                    }
                    else break; //We have no more attackers
                }
            }
            return score;
        }
    }
}

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
        public static void OrderMoves(Board b, TranspositionTable tt, List<Move> moves)
        {
            int score;
            Move ttMove = tt.LookupPosition(b.ZobristHash).MovePlayed;
            byte opponentColor = (byte)(2 - (byte)b.ColorToMove);
            byte ourColor = (byte)((byte)b.ColorToMove-1);
            int thisPieceValue;
            int lowestDefenderScore = 0;
            int currentDefenderScore = 0;

            foreach (Move m in moves)
            {
                score = 0;
                lowestDefenderScore = 0;
                thisPieceValue = Evaluation.GetPieceValue(m.Piece);

                if (m.PieceCaptured != 0)
                {
                    score -= thisPieceValue; 
                    score += Evaluation.GetPieceValue(m.PieceCaptured)*Evaluation.CaptureBonusMultiplier;

                    
                }
                if(m.PromoteIntoPiece != 0)
                {
                    score += Evaluation.GetPieceValue(m.PromoteIntoPiece);
                }
                if (b.AttackedSquares[opponentColor][m.Destination] != null)
                {
                    foreach (ushort piece in b.AttackedSquares[opponentColor][m.Destination])
                    {
                        currentDefenderScore = Evaluation.GetPieceValue((byte)piece);
                        if (currentDefenderScore < thisPieceValue) score -= Evaluation.DefendedByCheapPiecePenalty;
                        lowestDefenderScore = Math.Min(currentDefenderScore, lowestDefenderScore);
                    }
                }
                if (b.AttackedSquares[ourColor][m.Destination] != null)
                {
                    score -= Evaluation.DefendedByCheapPiecePenalty; //THis is going to get added back when checking for friendly defenders of the desitnation square.
                    foreach (ushort piece in b.AttackedSquares[ourColor][m.Destination])
                    {
                        if (Evaluation.GetPieceValue((byte)piece) <= lowestDefenderScore) score += Evaluation.DefendedByCheapPiecePenalty/2;
                    }
                }
                
                m.MoveScore = -score; //Sorting small = good
            }

            moves.Sort();
        }
    }
}

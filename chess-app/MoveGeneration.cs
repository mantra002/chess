using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_app
{
    using static Enums;
    public static class MoveGeneration
    {
        public static List<Move> GenerateLegalMoves(Board b)
        {
            List<Move> candidateMoves = new List<Move>();
            byte decodePiece;
            byte decodeLocation;

            for (int index = 0; index < b.PieceList.Count(); index++)
            {
                ushort piece = b.PieceList[index];
                if((piece & (byte)b.ColorToMove) == (byte)b.ColorToMove)
                {
                    decodePiece = Board.DecodePieceFromPieceList(piece);
                    decodeLocation = Board.DecodeLocationFromPieceList(piece);

                    //Console.WriteLine("Piece Decoded as " + Pieces.DecodePieceToChar(decodePiece) + " on Square " + Enum.GetName(typeof(Squares), decodeLocation));

                    if((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn && b.ColorToMove == Colors.White)
                    {
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailiblePawnAttacksWhite[decodeLocation], index, true, false));
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailiblePawnMovesWhite[decodeLocation], index, false));
                    }
                    else if ((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn && b.ColorToMove == Colors.Black)
                    {
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailiblePawnAttacksBlack[decodeLocation], index, true, false));
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailiblePawnMovesBlack[decodeLocation], index, false));
                    }
                    else if ((decodePiece & (byte)PieceNames.Knight) == (byte)PieceNames.Knight)
                    {
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailibleKnightMoves[decodeLocation], index));
                    }
                    else if ((decodePiece & (byte)PieceNames.King) == (byte)PieceNames.King)
                    {
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailibleKingMoves[decodeLocation], index));
                    }
                    else if ((decodePiece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                    {
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.QueenMoves, decodeLocation, b), index));
                    }
                    else if ((decodePiece & (byte)PieceNames.Bishop) == (byte)PieceNames.Bishop)
                    {
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.BishopMoves, decodeLocation, b), index));
                    }
                    else if ((decodePiece & (byte)PieceNames.Rook) == (byte)PieceNames.Rook)
                    {
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.RookMoves, decodeLocation, b), index));
                    }
                    else 
                    {
                        throw new Exception("Trying to generate moves for an invalid peice");
                    }
                }
            }

            return candidateMoves;
        }
        private static List<Move> GenerateMoves(Board b, byte origin, short[] AvailibleMoves, int plIndex, bool includeCaptures = true, bool includeQuietMoves = true)
        {
            byte destinationPiece;
            List<Move> candidateMoves = new List<Move>();
            Move m;

            foreach (byte destination in AvailibleMoves)
            {
                destinationPiece = b.GameBoard[destination];
                if (destinationPiece == 0 && includeQuietMoves)
                {
                    m = new Move(b.ColorToMove, b.GameBoard[origin], origin, destination, plIndex);
                    candidateMoves.Add(m);
                    //Console.WriteLine("Generating a quiet move with " + Pieces.DecodePieceToChar(m.Piece) + " - " + m.ToString()) ;
                }

                else if (destinationPiece != 0 && includeCaptures && ((destinationPiece & (byte)b.ColorToMove) != (byte)b.ColorToMove))
                {
                    m = new Move(Colors.White, b.GameBoard[origin], origin, destination, plIndex, destinationPiece);
                    candidateMoves.Add(m);
                    //Console.WriteLine("Generating a capture move " + Pieces.DecodePieceToChar(m.Piece) + " takes " + Pieces.DecodePieceToChar(m.PieceCaptured) + " - " + m.ToString());
                }
            }
            return candidateMoves;
        }
    }
}

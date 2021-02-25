using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Game
{
    using static Enums;
    public static class MoveGeneration
    {
        public static List<Move> GenerateLegalMoves(Board b)
        {
            List<Move> candidateMoves = new List<Move>();
            byte decodePiece;
            byte decodeLocation;
            int numberOfPieces = b.PieceList.Count();
            short[] pawnMoves;

            if (b.ColorToMove == Colors.White)
            {
                b.AttackedSquares[0] = GenerateAttackMap(b, Colors.Black);
                if (WhiteCastleIsValid(b, CastleFlags.WhiteShortCastle))
                {
                    candidateMoves.Add(new Move(b.ColorToMove, 0, 0, 0, 0, castleFlag: CastleFlags.WhiteShortCastle));
                }
               if (WhiteCastleIsValid(b, CastleFlags.WhiteLongCastle))
                {
                    candidateMoves.Add(new Move(b.ColorToMove, 0, 0, 0, 0, castleFlag: CastleFlags.WhiteLongCastle));
                }
            }
            else
            {
                b.AttackedSquares[1] = GenerateAttackMap(b, Colors.White);
                if (BlackCastleIsValid(b, CastleFlags.BlackShortCastle))
                {
                    candidateMoves.Add(new Move(b.ColorToMove, 0, 0, 0, 0, castleFlag: CastleFlags.BlackShortCastle));
                }
                if (BlackCastleIsValid(b, CastleFlags.BlackLongCastle))
                {
                    candidateMoves.Add(new Move(b.ColorToMove, 0, 0, 0, 0, castleFlag: CastleFlags.BlackLongCastle));
                }
            }

            if(CheckForCheck(b))
            {
                return GenerateCheckEvasion(b);
            }
            else
            {
                for (int index = 0; index < numberOfPieces; index++)
                {
                    ushort piece = b.PieceList[index];
                    if ((piece & (byte)b.ColorToMove) == (byte)b.ColorToMove)
                    {
                        decodePiece = Board.DecodePieceFromPieceList(piece);
                        decodeLocation = Board.DecodeLocationFromPieceList(piece);

                        //Console.WriteLine("Piece Decoded as " + Pieces.DecodePieceToChar(decodePiece) + " on Square " + Enum.GetName(typeof(Squares), decodeLocation));

                        if ((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn && b.ColorToMove == Colors.White)
                        {
                            pawnMoves = MoveData.AvailiblePawnMovesWhite[decodeLocation];
                            if (Board.GetRank((byte)decodeLocation) == 2 && b.GameBoard[decodeLocation - 16] == 0)
                            {
                                pawnMoves = pawnMoves.Concat(new short[] { (short)(decodeLocation - 16)}).ToArray();
                            }

                            candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailiblePawnAttacksWhite[decodeLocation], index, true, false));
                            candidateMoves.AddRange(GenerateMoves(b, decodeLocation, pawnMoves, index, false));
                        }
                        else if ((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn && b.ColorToMove == Colors.Black)
                        {
                            pawnMoves = MoveData.AvailiblePawnMovesBlack[decodeLocation];
                            if (Board.GetRank((byte)decodeLocation) == 7 && b.GameBoard[decodeLocation + 16] == 0 )
                            {
                                pawnMoves = pawnMoves.Concat(new short[] { (short)(decodeLocation + 16) }).ToArray();
                            }
                            candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailiblePawnAttacksBlack[decodeLocation], index, true, false));
                            candidateMoves.AddRange(GenerateMoves(b, decodeLocation, pawnMoves, index, false));
                        }
                        else if ((decodePiece & (byte)PieceNames.Knight) == (byte)PieceNames.Knight)
                        {
                            candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailibleKnightMoves[decodeLocation], index));
                        }
                        else if ((decodePiece & (byte)PieceNames.King) == (byte)PieceNames.King)
                        {
                            candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailibleKingMoves[decodeLocation], index, allowMoveIntoCheck: false));
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
            }
            return candidateMoves;
        }
        private static List<Move> GenerateCheckEvasion(Board b)
        {
            List<Move> candidateMoves = new List<Move>();
            candidateMoves.AddRange(GenerateMoves(b, b.KingSquares[(byte)b.ColorToMove-1], MoveData.AvailibleKingMoves[b.KingSquares[(byte)b.ColorToMove - 1]]));
            List<ushort> piecesAttacking = b.AttackedSquares[2 - (byte)b.ColorToMove][b.KingSquares[(byte)b.ColorToMove - 1]];
            /*
             * 
             *        Pawn = 128,
            *        Knight = 4,
            *        Bishop = 8,
            *        Rook = 16,
            *        Queen = 32,
            *        King = 64,
            *None=0

             */

            if (piecesAttacking.Count() > 1) return candidateMoves;

            return candidateMoves;
        }
        private static bool CheckForCheck(Board b)
        {
            int ksIndex = (byte)b.ColorToMove - 1;
            int asIndex = 2 - (byte)b.ColorToMove;
            return (b.AttackedSquares[asIndex][b.KingSquares[ksIndex]] != null);
        }
        public static List<ushort>[] GenerateAttackMap(Board b, Colors sideToGenerateAttacksFor = 0)
        {

            Colors side;
            byte decodePiece;
            byte decodeLocation;
            List<ushort>[] attackMap = new List<ushort>[64];

            if (sideToGenerateAttacksFor == 0) side = b.ColorToMove;
            else side = sideToGenerateAttacksFor;
            int numberOfPieces = b.PieceList.Count();
            for (int index = 0; index < numberOfPieces; index++)
            {
                ushort piece = b.PieceList[index];
                if ((piece & (byte)side) == (byte)side)
                {
                    decodePiece = Board.DecodePieceFromPieceList(piece);
                    decodeLocation = Board.DecodeLocationFromPieceList(piece);

                    //Console.WriteLine("Piece Decoded as " + Pieces.DecodePieceToChar(decodePiece) + " on Square " + Enum.GetName(typeof(Squares), decodeLocation));

                    if ((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn && side == Colors.White)
                    {
                        GenerateAttacks(b, sideToGenerateAttacksFor, decodePiece, decodeLocation, MoveData.AvailiblePawnAttacksWhite[decodeLocation], ref attackMap);
                    }
                    else if ((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn && side == Colors.Black)
                    {
                        GenerateAttacks(b, sideToGenerateAttacksFor, decodePiece, decodeLocation, MoveData.AvailiblePawnAttacksBlack[decodeLocation], ref attackMap);
                    }
                    else if ((decodePiece & (byte)PieceNames.Knight) == (byte)PieceNames.Knight)
                    {
                        GenerateAttacks(b, sideToGenerateAttacksFor, decodePiece, decodeLocation, MoveData.AvailibleKnightMoves[decodeLocation], ref attackMap);
                    }
                    else if ((decodePiece & (byte)PieceNames.King) == (byte)PieceNames.King)
                    {
                        GenerateAttacks(b, sideToGenerateAttacksFor, decodePiece, decodeLocation, MoveData.AvailibleKingMoves[decodeLocation], ref attackMap, allowMoveIntoCheck: false);
                    }
                    else if ((decodePiece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                    {
                        GenerateAttacks(b, sideToGenerateAttacksFor, decodePiece, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.QueenMoves, decodeLocation, b), ref attackMap);
                    }
                    else if ((decodePiece & (byte)PieceNames.Bishop) == (byte)PieceNames.Bishop)
                    {
                        GenerateAttacks(b, sideToGenerateAttacksFor, decodePiece, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.BishopMoves, decodeLocation, b), ref attackMap);
                    }
                    else if ((decodePiece & (byte)PieceNames.Rook) == (byte)PieceNames.Rook)
                    {
                        GenerateAttacks(b, sideToGenerateAttacksFor, decodePiece, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.RookMoves, decodeLocation, b), ref attackMap);
                    }
                    else
                    {
                        throw new Exception("Trying to generate moves for an invalid peice");
                    }
                }
            }
            return attackMap;
        }

        private static bool WhiteCastleIsValid(Board b, CastleFlags castellingTypes)
        {
            if((b.CastleMask & (byte) castellingTypes) != 0)
            {
                if(castellingTypes == CastleFlags.WhiteShortCastle)
                {
                    if(b.GameBoard[(byte)Squares.f1] == 0 && b.GameBoard[(byte)Squares.g1] == 0 && b.AttackedSquares[0][(byte)Squares.f1] == null && b.AttackedSquares[0][(byte)Squares.g1]==null)
                    {
                        return true;
                    }
                }
                else
                {
                    if (b.GameBoard[(byte)Squares.d1] == 0 && b.GameBoard[(byte)Squares.c1] == 0 && b.GameBoard[(byte)Squares.b1] == 0 && b.AttackedSquares[0][(byte)Squares.c1] == null && b.AttackedSquares[0][(byte)Squares.d1] == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool BlackCastleIsValid(Board b, CastleFlags castellingTypes)
        {

            if ((b.CastleMask & (byte)castellingTypes) != 0)
            {
                if (castellingTypes == CastleFlags.BlackShortCastle)
                {
                    if (b.GameBoard[(byte)Squares.f8] == 0 && b.GameBoard[(byte)Squares.g8] == 0 && b.AttackedSquares[1][(byte)Squares.f8] == null && b.AttackedSquares[1][(byte)Squares.g8] == null)
                    {
                        return true;
                    }
                }
                else
                {
                    if (b.GameBoard[(byte)Squares.d8] == 0 && b.GameBoard[(byte)Squares.c8] == 0 && b.GameBoard[(byte)Squares.b8] == 0 && b.AttackedSquares[1][(byte)Squares.d8] == null && b.AttackedSquares[1][(byte)Squares.c8] == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static void GenerateAttacks(Board b, Colors c, byte decodePiece, byte origin, short[] availibleMoves, ref List<ushort>[] attackMaps, bool allowMoveIntoCheck = true)
        {
            byte destinationPiece;
            if (b.AttackedSquares[2 - (byte)c] == null) allowMoveIntoCheck = true;
            if (PinCheck(b, origin)) return;

            foreach (byte destination in availibleMoves)
            {
                destinationPiece = b.GameBoard[destination];
                
                if (destinationPiece == 0 || ((destinationPiece & (byte)c) != (byte)c))
                {
                    if (!allowMoveIntoCheck && b.AttackedSquares[2 - (byte)c][destination] != null)
                    {

                        /*Console.WriteLine("Looking at attacking " + destination + " with " + Pieces.DecodePieceToChar(decodePiece));
                        Console.WriteLine("Currently attacked by " + Pieces.DecodePieceToChar(Board.DecodePieceFromPieceList(b.AttackedSquares[2 - (byte)c][destination][0])) + " at " + Board.DecodeLocationFromPieceList(b.AttackedSquares[2 - (byte)c][destination][0]));
                        Console.WriteLine(b.ToString());*/
                    }
                    else
                    {
                        if (attackMaps[destination] == null) attackMaps[destination] = new List<ushort>();
                        attackMaps[destination].Add(Board.EncodePieceForPieceList(decodePiece, origin));
                    }
                }
            }
        }
        private static bool PinCheck(Board b, byte origin)
        {
            byte checkPieceLocation;
            byte opponentColor = (byte)(2 - (byte)b.ColorToMove);
            if (b.AttackedSquares[opponentColor] == null) return false;
            if (b.AttackedSquares[opponentColor][origin] != null)
            {
                foreach (ushort piece in b.AttackedSquares[2 - (byte)b.ColorToMove][origin])
                {
                    checkPieceLocation = Board.DecodeLocationFromPieceList(piece);

                    if ((piece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                    {
                        if (MoveData.AvailibleQueenMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.QueenMoves, checkPieceLocation, b, origin).Contains(b.KingSquares[(byte)b.ColorToMove - 1]))
                        {
                            //Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]));
                            //b.PrintBoard();
                            return true;
                        }
                    }
                    else if ((piece & (byte)PieceNames.Bishop) == (byte)PieceNames.Bishop)
                    {
                        if (MoveData.AvailibleBishopMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.BishopMoves, checkPieceLocation, b, origin).Contains(b.KingSquares[(byte)b.ColorToMove - 1]))
                        {
                            //Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]));
                            //b.PrintBoard();
                            return true;
                        }
                    }
                    else if ((piece & (byte)PieceNames.Rook) == (byte)PieceNames.Rook)
                    {
                        if (MoveData.AvailibleRookMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.RookMoves, checkPieceLocation, b, origin).Contains(b.KingSquares[(byte)b.ColorToMove - 1]))
                        {
                            //Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]));
                            //b.PrintBoard();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static List<Move> GenerateMoves(Board b, byte origin, short[] availibleMoves, int plIndex = -1, bool includeCaptures = true, bool includeQuietMoves = true, bool includePinCheck = true, bool allowMoveIntoCheck = true)
        {
            byte destinationPiece;
            List<Move> candidateMoves = new List<Move>();
            Move m;
            
            //Need to check if this piece is pinned.
            if (includePinCheck && PinCheck(b, origin))
            {
                return candidateMoves;
            }

            foreach (byte destination in availibleMoves)
            {
                destinationPiece = b.GameBoard[destination];
                
                if (destinationPiece == 0 && includeQuietMoves)
                {
                    if (!allowMoveIntoCheck && b.AttackedSquares[2 - (byte)b.ColorToMove][destination] != null)
                    {
                        //do nothing
                    }
                    else
                    {
                        m = new Move(b.ColorToMove, b.GameBoard[origin], origin, destination, plIndex);
                        candidateMoves.Add(m);
                        //Console.WriteLine("Generating a quiet move with " + Pieces.DecodePieceToChar(m.Piece) + " - " + m.ToString()) ;
                    }
                }

                else if (destinationPiece != 0 && ((destinationPiece & (byte)b.ColorToMove) != (byte)b.ColorToMove))
                {
                    if (includeCaptures)
                    {
                        if (!allowMoveIntoCheck && b.AttackedSquares[2 - (byte)b.ColorToMove][destination] != null)
                        { 
                            
                        }
                        else
                        {
                            m = new Move(Colors.White, b.GameBoard[origin], origin, destination, plIndex, destinationPiece);
                            candidateMoves.Add(m);
                        }
                    }
                    //Console.WriteLine("Generating a capture move " + Pieces.DecodePieceToChar(m.Piece) + " takes " + Pieces.DecodePieceToChar(m.PieceCaptured) + " - " + m.ToString());
                }
            }
            return candidateMoves;
        }

    }
}

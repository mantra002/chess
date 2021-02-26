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
            if (CheckForCheck(b))
            {
                candidateMoves = GenerateCheckEvasion(b);
                if (candidateMoves.Count() == 0) b.CheckMate = true;
                return candidateMoves;
            }


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
                            if (Board.GetRank((byte)decodeLocation) == 2 && b.GameBoard[decodeLocation - 8] == 0)
                            {
                                pawnMoves = pawnMoves.Concat(new short[] { (short)(decodeLocation - 16) }).ToArray();
                            }

                            candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailiblePawnAttacksWhite[decodeLocation], index, true, false, isPawn: true));
                            candidateMoves.AddRange(GenerateMoves(b, decodeLocation, pawnMoves, index, false));
                        }
                        else if ((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn && b.ColorToMove == Colors.Black)
                        {
                            pawnMoves = MoveData.AvailiblePawnMovesBlack[decodeLocation];
                            if (Board.GetRank((byte)decodeLocation) == 7 && b.GameBoard[decodeLocation + 8] == 0)
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
                            candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailibleKingMoves[decodeLocation], index, allowMoveIntoCheck: false, canBePinned: false));
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
        private static List<Move> GenerateCheckEvasion(Board b)
        {
            byte opponentColor = (byte)(2 - (byte)b.ColorToMove);
            byte kingSquare = b.KingSquares[(byte)b.ColorToMove - 1];
            List<Move> candidateMoves = new List<Move>();
            Move m;
            byte decodeDefender;

            candidateMoves.AddRange(GenerateMoves(b, kingSquare, MoveData.AvailibleKingMoves[kingSquare], canBePinned: false, allowMoveIntoCheck: false));

            List<ushort> piecesAttacking = b.AttackedSquares[opponentColor][kingSquare];

            //No need to go further if it's a double check, the king MUST move.
            if (piecesAttacking.Count() > 1) return candidateMoves;

            byte decodePiece = Board.DecodePieceFromPieceList(piecesAttacking[0]);
            byte decodeLocation = Board.DecodeLocationFromPieceList(piecesAttacking[0]);

            //Try to generate blocking moves if it's not attacked by a Knight or pawn.
            if (((decodePiece & (byte)PieceNames.Knight) != (byte)PieceNames.Knight) && ((decodePiece & (byte)PieceNames.Pawn) != (byte)PieceNames.Pawn))
            {
                List<byte> blockingSquares = FindBlockingSquares(piecesAttacking[0], kingSquare);

                foreach (byte bSquare in blockingSquares)
                {
                    if (b.AttackedSquares[(byte)b.ColorToMove - 1][bSquare] != null)
                    {
                        foreach (ushort defendingPiece in b.AttackedSquares[(byte)b.ColorToMove - 1][bSquare])
                        {
                            decodeDefender = Board.DecodePieceFromPieceList(defendingPiece);
                            if ((decodeDefender & (byte)PieceNames.King) != (byte)PieceNames.King)
                            {
                                m = new Move(b.ColorToMove, decodeDefender, Board.DecodeLocationFromPieceList(defendingPiece), bSquare);
                                candidateMoves.Add(m);
                            }
                        }
                    }
                }
            }

            //Generate capturing the checking piece
            if (b.AttackedSquares[(byte)b.ColorToMove - 1][decodeLocation] != null)
            {
                foreach (ushort defendingPiece in b.AttackedSquares[(byte)b.ColorToMove - 1][decodeLocation])
                {
                    if ((defendingPiece & (byte)PieceNames.King) != (byte)PieceNames.King)
                    {
                        decodeDefender = Board.DecodePieceFromPieceList(defendingPiece);
                        m = new Move(b.ColorToMove, decodeDefender, Board.DecodeLocationFromPieceList(defendingPiece), decodeLocation);
                        candidateMoves.Add(m);
                    }
                }
            }


            return candidateMoves;
        }
        private static List<byte> FindBlockingSquares(ushort attackingPiece, ushort kingSquare)
        {
            List<byte> blockingSquares = new List<byte>();
            byte decodePiece;
            byte decodeLocation;
            decodePiece = Board.DecodePieceFromPieceList(attackingPiece);
            decodeLocation = Board.DecodeLocationFromPieceList(attackingPiece);

            short distance = (short)(kingSquare - decodeLocation);
            short direction;
            short squareTraverse = decodeLocation;

            if (distance % 7 == 0)
            {
                //Forward diagonal /
                direction = 7;
            }
            else if (distance % 8 == 0)
            {
                //Up-Down
                direction = 8;
            }
            else if (distance % 9 == 0)
            {
                //Backslash diagonal \
                direction = 9;
            }
            else
            {
                //Left-Right
                direction = 1;
            }
            if (distance < 0) direction = (short)-direction;
            squareTraverse += direction;
            while (squareTraverse != kingSquare)
            {
                blockingSquares.Add((byte)squareTraverse);
                squareTraverse += direction;
            }
            return blockingSquares;
        }
        private static bool CheckForCheck(Board b)
        {
            int ksIndex = (byte)b.ColorToMove - 1;
            int asIndex = 2 - (byte)b.ColorToMove;
            bool result = (b.AttackedSquares[asIndex][b.KingSquares[ksIndex]] != null);
            if (result) b.InCheck = true;
            return result;
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
                        GenerateAttacks(b, sideToGenerateAttacksFor, decodePiece, decodeLocation, MoveData.AvailibleKingMoves[decodeLocation], ref attackMap, allowMoveIntoCheck: false, canBePinned: false);
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
            if ((b.CastleMask & (byte)castellingTypes) != 0)
            {
                if (castellingTypes == CastleFlags.WhiteShortCastle)
                {
                    if (b.GameBoard[(byte)Squares.f1] == 0 && b.GameBoard[(byte)Squares.g1] == 0 && b.AttackedSquares[0][(byte)Squares.f1] == null && b.AttackedSquares[0][(byte)Squares.g1] == null && b.GameBoard[(byte)Squares.h1] == ((byte)PieceNames.Rook | (byte)Colors.White))
                    {
                        return true;
                    }
                }
                else
                {
                    if (b.GameBoard[(byte)Squares.d1] == 0 && b.GameBoard[(byte)Squares.c1] == 0 && b.GameBoard[(byte)Squares.b1] == 0 && b.AttackedSquares[0][(byte)Squares.c1] == null && b.AttackedSquares[0][(byte)Squares.d1] == null && b.GameBoard[(byte)Squares.a1] == ((byte)PieceNames.Rook | (byte)Colors.White))
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
                    if (b.GameBoard[(byte)Squares.f8] == 0 && b.GameBoard[(byte)Squares.g8] == 0 && b.AttackedSquares[1][(byte)Squares.f8] == null && b.AttackedSquares[1][(byte)Squares.g8] == null && b.GameBoard[(byte)Squares.h8] == ((byte)PieceNames.Rook | (byte)Colors.Black))
                    {
                        return true;
                    }
                }
                else
                {
                    if (b.GameBoard[(byte)Squares.d8] == 0 && b.GameBoard[(byte)Squares.c8] == 0 && b.GameBoard[(byte)Squares.b8] == 0 && b.AttackedSquares[1][(byte)Squares.d8] == null && b.AttackedSquares[1][(byte)Squares.c8] == null && b.GameBoard[(byte)Squares.a8] == ((byte)PieceNames.Rook | (byte)Colors.Black))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static void GenerateAttacks(Board b, Colors c, byte decodePiece, byte origin, short[] availibleMoves, ref List<ushort>[] attackMaps, bool allowMoveIntoCheck = true, bool canBePinned = true)
        {
            byte destinationPiece;
            //This is a hacky way to avoid accessing an attack map that hasn't been generated yet.
            if (b.AttackedSquares[2 - (byte)c] == null) allowMoveIntoCheck = true;
            if (canBePinned && PinCheck(b, origin)) return;

            foreach (byte destination in availibleMoves)
            {
                destinationPiece = b.GameBoard[destination];

                if (true)//destinationPiece == 0 || ((destinationPiece & (byte)c) != (byte)c))
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
        private static bool PinCheckByRay(Board b, MoveData.MoveDirections md, byte origin, byte destination)
        {
            short distance = (short)(origin - destination);
            short direction;
            short squareTraverse = origin;

            if (distance < 0) direction = (short)-direction;
            squareTraverse += direction;
            while (squareTraverse != kingSquare)
            {
                blockingSquares.Add((byte)squareTraverse);
                squareTraverse += direction;
            }

            byte checkPieceLocation;
            byte opponentColor = (byte)(2 - (byte)b.ColorToMove);
            if (b.AttackedSquares[opponentColor] == null) return false;
            if (b.AttackedSquares[opponentColor][origin] != null)
            {
                foreach (ushort piece in b.AttackedSquares[opponentColor][origin])
                {
                    checkPieceLocation = Board.DecodeLocationFromPieceList(piece);

                    if ((piece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                    {
                        if (MoveData.AvailibleQueenMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.QueenMoves, checkPieceLocation, b, origin).Contains(b.KingSquares[(byte)b.ColorToMove - 1]))
                        {
                            Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]) + " at " + origin + " but it's pinned to the king.");
                            Console.WriteLine(b.ToString());
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
        private static bool PinCheck(Board b, byte origin)
        {
            byte checkPieceLocation;
            byte opponentColor = (byte)(2 - (byte)b.ColorToMove);
            if (b.AttackedSquares[opponentColor] == null) return false;
            if (b.AttackedSquares[opponentColor][origin] != null)
            {
                foreach (ushort piece in b.AttackedSquares[opponentColor][origin])
                {
                    checkPieceLocation = Board.DecodeLocationFromPieceList(piece);

                    if ((piece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                    {
                        if (MoveData.AvailibleQueenMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.QueenMoves, checkPieceLocation, b, origin).Contains(b.KingSquares[(byte)b.ColorToMove - 1]))
                        {
                            Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]) + " at " + origin + " but it's pinned to the king.");
                            Console.WriteLine(b.ToString());
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
        private static List<Move> GenerateMoves(Board b, byte origin, short[] availibleMoves, int plIndex = -1, bool includeCaptures = true, bool includeQuietMoves = true, bool canBePinned = true, bool allowMoveIntoCheck = true, bool isPawn = false)
        {
            byte destinationPiece;
            List<Move> candidateMoves = new List<Move>();
            Move m;

            //Need to check if this piece is pinned.
            if (canBePinned && PinCheck(b, origin))
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
                        if(isPawn)
                        {
                            if (origin - destination == 16)
                            {
                                m.AllowsEnPassantTarget = (Squares)(origin - 8);
                            }
                            else if (origin - destination == -16)
                            {
                                m.AllowsEnPassantTarget = (Squares)(origin + 8);
                            }
                        }
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
                            m = new Move(b.ColorToMove, b.GameBoard[origin], origin, destination, plIndex, destinationPiece);
                            candidateMoves.Add(m);
                        }
                    }
                    //Console.WriteLine("Generating a capture move " + Pieces.DecodePieceToChar(m.Piece) + " takes " + Pieces.DecodePieceToChar(m.PieceCaptured) + " - " + m.ToString());
                }
                else if(destination == (byte)b.EnPassantTarget && isPawn)
                {
                    if (b.ColorToMove == Colors.White)
                    {
                        m = new Move(Colors.White, b.GameBoard[origin], origin, destination, plIndex, b.GameBoard[destination - 8]);
                    }
                    else
                    {
                        m = new Move(Colors.Black, b.GameBoard[origin], origin, destination, plIndex, b.GameBoard[destination + 8]);
                    }
                    m.CaptureEnPassant = true;
                    candidateMoves.Add(m);
                }
            }
            return candidateMoves;
        }

    }
}

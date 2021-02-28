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
                (b.AttackedSquares[0], b.AttackedSquaresWithoutPins[0]) = MoveGeneration.GenerateAttackMap(b, Colors.Black);

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
                (b.AttackedSquares[1], b.AttackedSquaresWithoutPins[1]) = MoveGeneration.GenerateAttackMap(b, Colors.White);
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
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, pawnMoves, index, false, isPawn: true));
                    }
                    else if ((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn && b.ColorToMove == Colors.Black)
                    {
                        pawnMoves = MoveData.AvailiblePawnMovesBlack[decodeLocation];
                        if (Board.GetRank((byte)decodeLocation) == 7 && b.GameBoard[decodeLocation + 8] == 0)
                        {
                            pawnMoves = pawnMoves.Concat(new short[] { (short)(decodeLocation + 16) }).ToArray();
                        }
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.AvailiblePawnAttacksBlack[decodeLocation], index, true, false, isPawn: true));
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, pawnMoves, index, false, isPawn: true));
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
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.FQueenMoves, decodeLocation, b), index));
                    }
                    else if ((decodePiece & (byte)PieceNames.Bishop) == (byte)PieceNames.Bishop)
                    {
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.FBishopMoves, decodeLocation, b), index));
                    }
                    else if ((decodePiece & (byte)PieceNames.Rook) == (byte)PieceNames.Rook)
                    {
                        candidateMoves.AddRange(GenerateMoves(b, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.FRookMoves, decodeLocation, b), index));
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
            byte decodePiecePl, decodeLocationPl;

            //TODO: This sometimes returns moves that are in check.
            candidateMoves.AddRange(GenerateMoves(b, kingSquare, MoveData.AvailibleKingMoves[kingSquare], canBePinned: false, allowMoveIntoCheck: false));

            List<ushort> piecesAttacking = b.AttackedSquaresWithoutPins[opponentColor][kingSquare];

            //No need to go further if it's a double check, the king MUST move.
            if (piecesAttacking.Count() > 1) return candidateMoves;

            byte decodePiece = Board.DecodePieceFromPieceList(piecesAttacking[0]);
            byte decodeLocation = Board.DecodeLocationFromPieceList(piecesAttacking[0]);

            //Try to generate blocking moves if it's not attacked by a Knight or pawn.
            if (((decodePiece & (byte)PieceNames.Knight) != (byte)PieceNames.Knight) && ((decodePiece & (byte)PieceNames.Pawn) != (byte)PieceNames.Pawn))
            {
                List<byte> blockingSquares = FindBlockingSquares(piecesAttacking[0], kingSquare);
                int numberOfPieces = b.PieceList.Count();
                foreach (byte bSquare in blockingSquares)
                {
                    if (b.AttackedSquares[(byte)b.ColorToMove - 1][bSquare] != null)
                    {
                        foreach (ushort defendingPiece in b.AttackedSquares[(byte)b.ColorToMove - 1][bSquare])
                        {
                            decodeDefender = Board.DecodePieceFromPieceList(defendingPiece);
                            //Pawn attacks actually don't defend the king, their moves need to be checked.
                            if ((decodeDefender & (byte)PieceNames.King) != (byte)PieceNames.King && (decodeDefender & (byte)PieceNames.Pawn) != (byte)PieceNames.Pawn)
                            {
                                m = new Move(b.ColorToMove, decodeDefender, Board.DecodeLocationFromPieceList(defendingPiece), bSquare);
                                candidateMoves.Add(m);
                            }
                        }
                    }

                    //Need to check if a pawn can move into a bocking square
                    short[] pawnMoves;
                    for (int index = 0; index < numberOfPieces; index++)
                    {
                        ushort piece = b.PieceList[index];
                        if ((piece & (byte)b.ColorToMove) == (byte)b.ColorToMove && (piece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn)
                        {
                            decodePiecePl = Board.DecodePieceFromPieceList(piece);
                            decodeLocationPl = Board.DecodeLocationFromPieceList(piece);

                            if (b.ColorToMove == Colors.White)
                            {
                                pawnMoves = MoveData.AvailiblePawnMovesWhite[decodeLocationPl];
                                if (Board.GetRank((byte)decodeLocationPl) == 2 && b.GameBoard[decodeLocationPl - 8] == 0)
                                {
                                    pawnMoves = pawnMoves.Concat(new short[] { (short)(decodeLocationPl - 16) }).ToArray();
                                }
                                if (pawnMoves.Contains(bSquare))
                                {
                                    candidateMoves.Add(GenerateMoves(b, decodeLocationPl, pawnMoves, index, false).Find(x => x.Destination == bSquare));
                                }
                            }
                            else
                            {
                                pawnMoves = MoveData.AvailiblePawnMovesBlack[decodeLocationPl];
                                if (Board.GetRank((byte)decodeLocationPl) == 7 && b.GameBoard[decodeLocationPl + 8] == 0)
                                {
                                    pawnMoves = pawnMoves.Concat(new short[] { (short)(decodeLocationPl + 16) }).ToArray();
                                }
                                if (pawnMoves.Contains(bSquare))
                                {
                                    Move mPawn = GenerateMoves(b, decodeLocationPl, pawnMoves, index, false).Find(x => x.Destination == bSquare);
                                    if (mPawn.Destination == bSquare) candidateMoves.Add(mPawn);
                                }
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
                    byte decodeDefendPos = Board.DecodeLocationFromPieceList(defendingPiece);
                    if ((defendingPiece & (byte)PieceNames.King) != (byte)PieceNames.King && !PinCheckByRay(b, decodeDefendPos, decodeLocation, b.ColorToMove))
                    {
                        decodeDefender = Board.DecodePieceFromPieceList(defendingPiece);
                        m = new Move(b.ColorToMove, decodeDefender, decodeDefendPos, decodeLocation);
                        candidateMoves.Add(m);
                    }
                }
            }


            return candidateMoves;
        }
        private static List<byte> FindBlockingSquares(ushort attackingPiece, byte kingSquare)
        {
            byte decodeLocation = Board.DecodeLocationFromPieceList(attackingPiece);
            return FindBlockingSquares(decodeLocation, kingSquare);
        }
        private static List<byte> FindBlockingSquares(byte attackingPieceLocation, byte defendingPieceLocation)
        {
            List<byte> blockingSquares = new List<byte>();
            short distance, direction, squareTraverse;

            GetRayInCommon(attackingPieceLocation, defendingPieceLocation, out direction, out distance);

            squareTraverse = attackingPieceLocation;

            squareTraverse += direction;
            while (squareTraverse != defendingPieceLocation)
            {
                blockingSquares.Add((byte)squareTraverse);
                squareTraverse += direction;
            }
            return blockingSquares;
        }
        private static List<byte> FindBlockingSquares(byte attackingPieceLocation, byte defendingPieceLocation, short direction, short distance)
        {
            List<byte> blockingSquares = new List<byte>();
            short squareTraverse;
            squareTraverse = attackingPieceLocation;

            squareTraverse += direction;
            while (squareTraverse != defendingPieceLocation)
            {
                blockingSquares.Add((byte)squareTraverse);
                squareTraverse += direction;
            }
            return blockingSquares;
        }

        private static bool GetRayInCommon(byte pieceLocation1, byte pieceLocation2, out short direction, out short distance)
        {
            direction = 0;
            distance = (short)(pieceLocation2 - pieceLocation1);

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
                if ((distance < 0 && -distance > MoveData.DistanceToEdge[pieceLocation1][(byte)MoveData.MoveDirectionsIndex.Left]) || (distance > 0 && distance > MoveData.DistanceToEdge[pieceLocation1][(byte)MoveData.MoveDirectionsIndex.Right]))
                {
                    return false;
                }
                //Left-Right
                direction = 1;
            }
            //Need to add the sign back in from the subtraction.
            if (distance < 0) direction = (short)-direction;



            return true;
        }

        private static bool CheckForCheck(Board b)
        {
            int ksIndex = (byte)b.ColorToMove - 1;
            int asIndex = 2 - (byte)b.ColorToMove;
            bool result = (b.AttackedSquaresWithoutPins[asIndex][b.KingSquares[ksIndex]] != null);
            if (result) b.InCheck = true;
            return result;
        }
        public static (List<ushort>[], List<ushort>[]) GenerateAttackMap(Board b, Colors sideToGenerateAttacksFor = 0)
        {

            Colors side;
            byte decodePiece;
            byte decodeLocation;
            List<ushort>[] attackMap = new List<ushort>[64];
            List<ushort>[] attackMapWithoutPins = new List<ushort>[64];
            

            if (sideToGenerateAttacksFor == 0) side = b.ColorToMove;
            else side = sideToGenerateAttacksFor;

            byte kingSquare = b.KingSquares[2-(byte)side];

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
                        GenerateAttacks(b, side, decodePiece, decodeLocation, MoveData.AvailiblePawnAttacksWhite[decodeLocation], ref attackMap, ref attackMapWithoutPins);
                    }
                    else if ((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn && side == Colors.Black)
                    {
                        GenerateAttacks(b, side, decodePiece, decodeLocation, MoveData.AvailiblePawnAttacksBlack[decodeLocation], ref attackMap, ref attackMapWithoutPins);
                    }
                    else if ((decodePiece & (byte)PieceNames.Knight) == (byte)PieceNames.Knight)
                    {
                        GenerateAttacks(b, side, decodePiece, decodeLocation, MoveData.AvailibleKnightMoves[decodeLocation], ref attackMap, ref attackMapWithoutPins);
                    }
                    else if ((decodePiece & (byte)PieceNames.King) == (byte)PieceNames.King)
                    {
                        GenerateAttacks(b, side, decodePiece, decodeLocation, MoveData.AvailibleKingMoves[decodeLocation], ref attackMap, ref attackMapWithoutPins, allowMoveIntoCheck: false, canBePinned: false);
                    }
                    else if ((decodePiece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                    {
                        GenerateAttacks(b, side, decodePiece, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.FQueenMoves, decodeLocation, b, kingSquare), ref attackMap, ref attackMapWithoutPins);
                    }
                    else if ((decodePiece & (byte)PieceNames.Bishop) == (byte)PieceNames.Bishop)
                    {
                        GenerateAttacks(b, side, decodePiece, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.FBishopMoves, decodeLocation, b, kingSquare), ref attackMap, ref attackMapWithoutPins);
                    }
                    else if ((decodePiece & (byte)PieceNames.Rook) == (byte)PieceNames.Rook)
                    {
                        GenerateAttacks(b, side, decodePiece, decodeLocation, MoveData.GenerateSlidingMoves(MoveData.FRookMoves, decodeLocation, b, kingSquare), ref attackMap, ref attackMapWithoutPins);
                    }
                    else
                    {
                        throw new Exception("Trying to generate moves for an invalid peice");
                    }
                }
            }
            return (attackMap, attackMapWithoutPins);
        }

        private static bool WhiteCastleIsValid(Board b, CastleFlags castellingTypes)
        {
            if ((b.CastleMask & (byte)castellingTypes) != 0)
            {
                if (castellingTypes == CastleFlags.WhiteShortCastle)
                {
                    if (b.GameBoard[(byte)Squares.f1] == 0 && b.GameBoard[(byte)Squares.g1] == 0 && b.AttackedSquaresWithoutPins[0][(byte)Squares.f1] == null && b.AttackedSquaresWithoutPins[0][(byte)Squares.g1] == null && b.GameBoard[(byte)Squares.h1] == ((byte)PieceNames.Rook | (byte)Colors.White))
                    {
                        return true;
                    }
                }
                else
                {
                    if (b.GameBoard[(byte)Squares.d1] == 0 && b.GameBoard[(byte)Squares.c1] == 0 && b.GameBoard[(byte)Squares.b1] == 0 && b.AttackedSquaresWithoutPins[0][(byte)Squares.c1] == null && b.AttackedSquaresWithoutPins[0][(byte)Squares.d1] == null && b.GameBoard[(byte)Squares.a1] == ((byte)PieceNames.Rook | (byte)Colors.White))
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
                    if (b.GameBoard[(byte)Squares.f8] == 0 && b.GameBoard[(byte)Squares.g8] == 0 && b.AttackedSquaresWithoutPins[1][(byte)Squares.f8] == null && b.AttackedSquaresWithoutPins[1][(byte)Squares.g8] == null && b.GameBoard[(byte)Squares.h8] == ((byte)PieceNames.Rook | (byte)Colors.Black))
                    {
                        return true;
                    }
                }
                else
                {
                    if (b.GameBoard[(byte)Squares.d8] == 0 && b.GameBoard[(byte)Squares.c8] == 0 && b.GameBoard[(byte)Squares.b8] == 0 && b.AttackedSquaresWithoutPins[1][(byte)Squares.d8] == null && b.AttackedSquaresWithoutPins[1][(byte)Squares.c8] == null && b.GameBoard[(byte)Squares.a8] == ((byte)PieceNames.Rook | (byte)Colors.Black))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static void GenerateAttacks(Board b, Colors c, byte decodePiece, byte origin, short[] availibleMoves, ref List<ushort>[] attackMaps, ref List<ushort>[] attackMapWithoutPin, bool allowMoveIntoCheck = true, bool canBePinned = true)
        {
            byte destinationPiece;
            //This is a hacky way to avoid accessing an attack map that hasn't been generated yet.
            if (b.AttackedSquares[2 - (byte)c] == null) allowMoveIntoCheck = true;

            foreach (byte destination in availibleMoves)
            {
                if (!allowMoveIntoCheck && b.AttackedSquaresWithoutPins[2 - (byte)c][destination] != null)
                {
                    /*Console.WriteLine("Looking at attacking " + destination + " with " + Pieces.DecodePieceToChar(decodePiece));
                    Console.WriteLine("Currently attacked by " + Pieces.DecodePieceToChar(Board.DecodePieceFromPieceList(b.AttackedSquares[2 - (byte)c][destination][0])) + " at " + Board.DecodeLocationFromPieceList(b.AttackedSquares[2 - (byte)c][destination][0]));
                    Console.WriteLine(b.ToString());*/
                }
                else
                {
                    ushort encodedPiece = Board.EncodePieceForPieceList(decodePiece, origin);
                    if (attackMapWithoutPin[destination] == null) attackMapWithoutPin[destination] = new List<ushort>();
                    attackMapWithoutPin[destination].Add(encodedPiece);

                    if (canBePinned && PinCheckByRay(b, origin, destination, c))
                    {

                    }

                    destinationPiece = b.GameBoard[destination];
                    if (attackMaps[destination] == null) attackMaps[destination] = new List<ushort>();
                    attackMaps[destination].Add(encodedPiece);

                }
            }
        }
        private static bool PinCheckByRay(Board b, byte origin, byte destination, Colors color)
        {
            byte checkPieceLocation;
            byte kingSquare = b.KingSquares[(byte)color - 1];
            bool possiblePin = false;
            byte opponentColor = (byte)(2 - (byte)color);
            short rayToKing;
            short desintationRay, destinationDistance;
            short attackingRay;
            short distance;
            if (!GetRayInCommon(origin, kingSquare, out rayToKing, out distance)) return false; //The piece isn't on the same ray as the king
            GetRayInCommon(destination, kingSquare, out desintationRay, out destinationDistance);
            if (rayToKing == desintationRay) return false; // Verify the move stays on the same ray.
            if (b.AttackedSquaresWithoutPins[opponentColor] == null) return false;
            if (b.AttackedSquaresWithoutPins[opponentColor][origin] != null)
            {
                foreach (ushort piece in b.AttackedSquaresWithoutPins[opponentColor][origin])
                {
                    checkPieceLocation = Board.DecodeLocationFromPieceList(piece);

                    if ((piece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                    {
                        if (MoveData.AvailibleQueenMoves[checkPieceLocation].Contains(kingSquare))
                        {
                            possiblePin = true;
                        }
                    }
                    else if ((piece & (byte)PieceNames.Bishop) == (byte)PieceNames.Bishop)
                    {
                        if (MoveData.AvailibleBishopMoves[checkPieceLocation].Contains(kingSquare))
                        {
                            possiblePin = true;
                        }
                    }
                    else if ((piece & (byte)PieceNames.Rook) == (byte)PieceNames.Rook)
                    {
                        if (MoveData.AvailibleRookMoves[checkPieceLocation].Contains(kingSquare))
                        {
                            possiblePin = true;
                        }
                    }
                    if (possiblePin)
                    {
                        if (!GetRayInCommon(origin, checkPieceLocation, out attackingRay, out distance)) return false;
                        if (attackingRay != rayToKing && attackingRay != -rayToKing) return false;
                        List<byte> potentialDefenders = FindBlockingSquares(origin, kingSquare);
                        foreach (byte potD in potentialDefenders)
                        {
                            if (b.GameBoard[potD] != 0) return false;
                        }
                        return true;
                        //Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]) + " at " + origin + " but it's pinned to the king.");
                        //Console.WriteLine(b.ToString());

                    }
                }
            }
            return false;
        }

        private static bool EpPinCheck(Board b, byte origin, byte epSquare)
        {
            byte checkPieceLocation;
            byte opponentColor = (byte)(2 - (byte)b.ColorToMove);
            byte kingSquare = b.KingSquares[(byte)b.ColorToMove - 1];
            
            //Need to get the square where the pawn is now.
            if (b.ColorToMove == Colors.White) epSquare += 8; 
            else epSquare -= 8; 

            if (b.AttackedSquaresWithoutPins[opponentColor] == null) return false;
            if (b.AttackedSquaresWithoutPins[opponentColor][epSquare] != null)
            {
                foreach (ushort piece in b.AttackedSquaresWithoutPins[opponentColor][epSquare])
                {
                    checkPieceLocation = Board.DecodeLocationFromPieceList(piece);

                    if ((piece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                    {
                        if (MoveData.AvailibleQueenMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.FQueenMoves, checkPieceLocation, b, origin, epSquare).Contains(kingSquare))
                        {
                            //Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]) + " at " + origin + " but it's pinned to the king.");
                            //Console.WriteLine(b.ToString());
                            return true;
                        }
                    }
                    else if ((piece & (byte)PieceNames.Bishop) == (byte)PieceNames.Bishop)
                    {
                        if (MoveData.AvailibleBishopMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.FBishopMoves, checkPieceLocation, b, origin, epSquare).Contains(kingSquare))
                        {
                            //Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]));
                            //b.PrintBoard();
                            return true;
                        }
                    }
                    else if ((piece & (byte)PieceNames.Rook) == (byte)PieceNames.Rook)
                    {
                        if (MoveData.AvailibleRookMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.FRookMoves, checkPieceLocation, b, origin, epSquare).Contains(kingSquare))
                        {
                            //Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]));
                            //b.PrintBoard();
                            return true;
                        }
                    }
                }
            }
            if (b.AttackedSquaresWithoutPins[opponentColor] == null) return false;
            if (b.AttackedSquaresWithoutPins[opponentColor][origin] != null)
            {
                foreach (ushort piece in b.AttackedSquaresWithoutPins[opponentColor][origin])
                {
                    checkPieceLocation = Board.DecodeLocationFromPieceList(piece);

                    if ((piece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                    {
                        if (MoveData.AvailibleQueenMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.FQueenMoves, checkPieceLocation, b, origin, epSquare).Contains(kingSquare))
                        {
                            //Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]) + " at " + origin + " but it's pinned to the king.");
                            //Console.WriteLine(b.ToString());
                            return true;
                        }
                    }
                    else if ((piece & (byte)PieceNames.Bishop) == (byte)PieceNames.Bishop)
                    {
                        if (MoveData.AvailibleBishopMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.FBishopMoves, checkPieceLocation, b, origin, epSquare).Contains(kingSquare))
                        {
                            //Console.WriteLine("Looking at moving the " + Pieces.DecodePieceToChar(b.GameBoard[origin]));
                            //b.PrintBoard();
                            return true;
                        }
                    }
                    else if ((piece & (byte)PieceNames.Rook) == (byte)PieceNames.Rook)
                    {
                        if (MoveData.AvailibleRookMoves[checkPieceLocation].Contains(b.KingSquares[(byte)b.ColorToMove - 1]) && MoveData.GenerateSlidingMoves(MoveData.FRookMoves, checkPieceLocation, b, origin, epSquare).Contains(kingSquare))
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
            bool alreadyLoggedMoves = false;
            List<Move> candidateMoves = new List<Move>();
            Move m;

            //Need to check if this piece is pinned.


            foreach (byte destination in availibleMoves)
            {
                if (canBePinned && PinCheckByRay(b, origin, destination, b.ColorToMove))
                {
                    //Do nothing, we're pinned
                }
                else
                {
                    destinationPiece = b.GameBoard[destination];
                    //Don't need to worry about a pawn move (non-capture) happening here, only a diagonal move
                    //can get into the EP square.
                    if (destination == (byte)b.EnPassantTarget && isPawn && includeCaptures && destinationPiece == 0 &&!EpPinCheck(b, origin, (byte)b.EnPassantTarget))
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
                    else if (destinationPiece == 0 && includeQuietMoves)
                    {
                        if (!allowMoveIntoCheck && b.AttackedSquaresWithoutPins[2 - (byte)b.ColorToMove][destination] != null)
                        {
                            //do nothing
                        }
                        else
                        {
                            m = new Move(b.ColorToMove, b.GameBoard[origin], origin, destination, plIndex);
                            if (isPawn)
                            {
                                if (origin - destination == 16)
                                {
                                    m.AllowsEnPassantTarget = (Squares)(origin - 8);
                                }
                                else if (origin - destination == -16)
                                {
                                    m.AllowsEnPassantTarget = (Squares)(origin + 8);
                                }
                                if ((b.ColorToMove == Colors.White && destination < 8) || (b.ColorToMove == Colors.Black && destination > 55))
                                {
                                    alreadyLoggedMoves = true;
                                    for (int i = 1; i < 5; i++)
                                    {
                                        m = new Move(b.ColorToMove, b.GameBoard[origin], origin, destination, plIndex, promoteIntoPiece: (byte)((byte)(2 * Math.Pow(2, i)) | (byte) b.ColorToMove));
                                        candidateMoves.Add(m);
                                    }
                                }
                            }
                            if (!alreadyLoggedMoves) candidateMoves.Add(m);
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
                            else if (isPawn && ((b.ColorToMove == Colors.White && destination < 8) || (b.ColorToMove == Colors.Black && destination > 55)))
                            {

                                alreadyLoggedMoves = true;
                                for (int i = 1; i < 5; i++)
                                {
                                    m = new Move(b.ColorToMove, b.GameBoard[origin], origin, destination, plIndex, destinationPiece, promoteIntoPiece: (byte)((byte)(2 * Math.Pow(2, i)) | (byte)b.ColorToMove));
                                    candidateMoves.Add(m);
                                }
                            }
                            else
                            {
                                m = new Move(b.ColorToMove, b.GameBoard[origin], origin, destination, plIndex, destinationPiece);
                                if (!alreadyLoggedMoves) candidateMoves.Add(m);
                            }
                        }
                        //Console.WriteLine("Generating a capture move " + Pieces.DecodePieceToChar(m.Piece) + " takes " + Pieces.DecodePieceToChar(m.PieceCaptured) + " - " + m.ToString());
                    }
                }
            }
            return candidateMoves;
        }

    }
}

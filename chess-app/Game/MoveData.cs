using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Game
{
    static class MoveData
    {
        //Need a way to generate the index offsets for pieces

        public readonly static FakeMoveDirections[] FPawnMovesWhite = { FakeMoveDirections.Up };
        public readonly static FakeMoveDirections[] FPawnMovesBlack = { FakeMoveDirections.Down };
        public readonly static FakeMoveDirections[] FPawnAttacksWhite = { FakeMoveDirections.UpLeft, FakeMoveDirections.UpRight };
        public readonly static FakeMoveDirections[] FPawnAttacksBlack = { FakeMoveDirections.DownLeft, FakeMoveDirections.DownRight };
        public readonly static FakeMoveDirections[] FKnightMoves = { FakeMoveDirections.DownLeft2, FakeMoveDirections.DownRight2, FakeMoveDirections.TwoDownLeft, FakeMoveDirections.TwoDownRight, FakeMoveDirections.UpLeft2, FakeMoveDirections.UpRight2, FakeMoveDirections.TwoUpLeft, FakeMoveDirections.TwoUpRight };
        public readonly static FakeMoveDirections[] FBishopMoves = { FakeMoveDirections.UpLeft, FakeMoveDirections.UpRight, FakeMoveDirections.DownLeft, FakeMoveDirections.DownRight };
        public readonly static FakeMoveDirections[] FRookMoves = { FakeMoveDirections.Up, FakeMoveDirections.Down, FakeMoveDirections.Left, FakeMoveDirections.Right };
        public readonly static FakeMoveDirections[] FKingMoves = { FakeMoveDirections.UpLeft, FakeMoveDirections.UpRight, FakeMoveDirections.DownLeft, FakeMoveDirections.DownRight, FakeMoveDirections.Up, FakeMoveDirections.Down, FakeMoveDirections.Left, FakeMoveDirections.Right };
        public readonly static FakeMoveDirections[] FQueenMoves = FKingMoves;

        public readonly static MoveDirections[] PawnMovesWhite = { MoveDirections.Up };
        public readonly static MoveDirections[] PawnMovesBlack = { MoveDirections.Down };
        public readonly static MoveDirections[] PawnAttacksWhite = { MoveDirections.UpLeft, MoveDirections.UpRight };
        public readonly static MoveDirections[] PawnAttacksBlack = { MoveDirections.DownLeft, MoveDirections.DownRight };
        public readonly static MoveDirections[] KnightMoves = { MoveDirections.DownLeft2, MoveDirections.DownRight2, MoveDirections.TwoDownLeft, MoveDirections.TwoDownRight, MoveDirections.UpLeft2, MoveDirections.UpRight2, MoveDirections.TwoUpLeft, MoveDirections.TwoUpRight };
        public readonly static MoveDirections[] BishopMoves = { MoveDirections.UpLeft, MoveDirections.UpRight, MoveDirections.DownLeft, MoveDirections.DownRight };
        public readonly static MoveDirections[] RookMoves = { MoveDirections.Up, MoveDirections.Down, MoveDirections.Left, MoveDirections.Right };
        public readonly static MoveDirections[] KingMoves = { MoveDirections.UpLeft, MoveDirections.UpRight, MoveDirections.DownLeft, MoveDirections.DownRight, MoveDirections.Up, MoveDirections.Down, MoveDirections.Left, MoveDirections.Right };
        public readonly static MoveDirections[] QueenMoves = KingMoves;
        public readonly static bool[] ValidBoardPositions;

        public readonly static short[][] AvailiblePawnMovesWhite;
        public readonly static short[][] AvailiblePawnMovesBlack;
        public readonly static short[][] AvailiblePawnAttacksWhite;
        public readonly static short[][] AvailiblePawnAttacksBlack;
        public readonly static short[][] AvailibleBishopMoves;
        public readonly static short[][] AvailibleRookMoves;
        public readonly static short[][] AvailibleKnightMoves;
        public readonly static short[][] AvailibleKingMoves;
        public readonly static short[][] AvailibleQueenMoves;

        public readonly static byte[][] DistanceToEdge;

        public enum FakeMoveDirections : short
        {
            Up = -14,
            Down = 14,
            Left = -1,
            Right = 1,
            UpLeft = Up + Left,
            UpRight = Up + Right,
            DownLeft = Down + Left,
            DownRight = Down + Right,
            TwoUpLeft = Up + Up + Left,
            TwoUpRight = Up + Up + Right,
            UpRight2 = Up + Right + Right,
            UpLeft2 = Up + Left + Left,
            TwoDownLeft = Down + Down + Left,
            TwoDownRight = Down + Down + Right,
            DownRight2 = Down + Right + Right,
            DownLeft2 = Down + Left + Left
        }

        public enum MoveDirections : short
        {
            Up = -8,
            Down = 8,
            Left = -1,
            Right = 1,
            UpLeft = Up + Left,
            UpRight = Up + Right,
            DownLeft = Down + Left,
            DownRight = Down + Right,
            TwoUpLeft = Up + Up + Left,
            TwoUpRight = Up + Up + Right,
            UpRight2 = Up + Right + Right,
            UpLeft2 = Up + Left + Left,
            TwoDownLeft = Down + Down + Left,
            TwoDownRight = Down + Down + Right,
            DownRight2 = Down + Right + Right,
            DownLeft2 = Down + Left + Left
        }
        public enum MoveDirectionsIndex : short
        {
            Up,
            Down,
            Left,
            Right,
            UpLeft,
            UpRight,
            DownLeft,
            DownRight
        }

        static MoveData()
        {
            ValidBoardPositions = new bool[210];
            AvailiblePawnMovesWhite = new short[64][];
            AvailiblePawnMovesBlack = new short[64][];
            AvailiblePawnAttacksWhite = new short[64][];
            AvailiblePawnAttacksBlack = new short[64][];
            AvailibleBishopMoves = new short[64][];
            AvailibleRookMoves = new short[64][];
            AvailibleKnightMoves = new short[64][];
            AvailibleKingMoves = new short[64][];
            AvailibleQueenMoves = new short[64][];

            DistanceToEdge = new byte[64][];

            GenerateValidBoardPositions();

            Array fakeDirections = Enum.GetValues(typeof(FakeMoveDirections));

            for (short i = 0; i < 64; i++)
            {
                DistanceToEdge[i] = new byte[8];
                DistanceToEdge[i][(short)MoveDirectionsIndex.Up] = GenerateDistanceToEdge(FakeMoveDirections.Up, i);
                DistanceToEdge[i][(short)MoveDirectionsIndex.Down] = GenerateDistanceToEdge(FakeMoveDirections.Down, i);
                DistanceToEdge[i][(short)MoveDirectionsIndex.Left] = GenerateDistanceToEdge(FakeMoveDirections.Left, i);
                DistanceToEdge[i][(short)MoveDirectionsIndex.Right] = GenerateDistanceToEdge(FakeMoveDirections.Right, i);

                DistanceToEdge[i][(short)MoveDirectionsIndex.UpLeft] = GenerateDistanceToEdge(FakeMoveDirections.UpLeft, i);
                DistanceToEdge[i][(short)MoveDirectionsIndex.UpRight] = GenerateDistanceToEdge(FakeMoveDirections.UpRight, i);
                DistanceToEdge[i][(short)MoveDirectionsIndex.DownLeft] = GenerateDistanceToEdge(FakeMoveDirections.DownLeft, i);
                DistanceToEdge[i][(short)MoveDirectionsIndex.DownRight] = GenerateDistanceToEdge(FakeMoveDirections.DownRight, i);

                AvailiblePawnMovesWhite[i] = GenerateNonSlidingMoves(FPawnMovesWhite, i, true, true);
                AvailiblePawnMovesBlack[i] = GenerateNonSlidingMoves(FPawnMovesBlack, i, false, true);
                AvailiblePawnAttacksWhite[i] = GenerateNonSlidingMoves(FPawnAttacksWhite, i);
                AvailiblePawnAttacksBlack[i] = GenerateNonSlidingMoves(FPawnAttacksBlack, i);
                AvailibleBishopMoves[i] = GenerateSlidingMoves(FBishopMoves, i);
                AvailibleRookMoves[i] = GenerateSlidingMoves(FRookMoves, i);
                AvailibleKnightMoves[i] = GenerateNonSlidingMoves(FKnightMoves, i);
                AvailibleKingMoves[i] = GenerateNonSlidingMoves(FKingMoves, i);
                AvailibleQueenMoves[i] = GenerateSlidingMoves(FQueenMoves, i);
            }
        }
        static byte GenerateDistanceToEdge(FakeMoveDirections d, short index)
        {
            short square = ConvertRealSquareToDummyBoard(index);
            byte count = 0;
            square = (short)(square + (short)d);
            if (!ValidBoardPositions[square]) return 0;
            while (ValidBoardPositions[square])
            {
                square = (short)(square + (short)d);
                count++;
            }
            return count;
        }
        static void GenerateValidBoardPositions()
        {
            for(short i = 0; i < 14; i++)
            {
                for(short j = 0; j < 14; j++)
                {
                    if (i < 3 || i > 10 || j < 3 || j > 10)
                    {
                        ValidBoardPositions[i * 14 + j] = false;
                    }
                    else ValidBoardPositions[i * 14 + j] = true;
                }
               
            }
        }

        public static short ConvertDummyBoardToRealSquare(short dummyBoardIndex)
        {
            short rank = (short) ((dummyBoardIndex / 14) - 3);
            return (short)(dummyBoardIndex - 45 - 6*rank);
        }

        public static short ConvertRealSquareToDummyBoard(short realBoardIndex)
        {
            return (short)(realBoardIndex + 45 + (6 * (realBoardIndex / 8)));
        }

        private static short[] GenerateNonSlidingMoves(FakeMoveDirections[] moves, short square, bool white = true, bool pawn = false)
        {
            square = ConvertRealSquareToDummyBoard(square);
            List<short> availibleMoves = new List<short>();
            short resultDummySquare;
            foreach(FakeMoveDirections moveOffset in moves)
            {
                resultDummySquare = (short)(square + (short)moveOffset);

                if(ValidBoardPositions[resultDummySquare])
                {
                    availibleMoves.Add(ConvertDummyBoardToRealSquare(resultDummySquare));
                }
            }
            return availibleMoves.ToArray();
        }

        public static short[] GenerateSlidingMoves(FakeMoveDirections[] moves, short square, Board b = null, short considerSquareEmpty = -1)
        {
            square = ConvertRealSquareToDummyBoard(square);
            List<short> availibleMoves = new List<short>();
            short resultDummySquare;
            short destinationPiece;
            short realSquare=0;

            foreach (FakeMoveDirections moveOffset in moves)
            {
                resultDummySquare = (short)(square + (short)moveOffset);
                while (ValidBoardPositions[resultDummySquare])
                { 
                    realSquare = ConvertDummyBoardToRealSquare(resultDummySquare);
                    if(b != null)
                    {
                        destinationPiece = b.GameBoard[realSquare];
                        availibleMoves.Add(realSquare);
                        if (destinationPiece != 0 && realSquare != considerSquareEmpty) break;
                    }
                    else
                    {
                        availibleMoves.Add(realSquare);
                    }
                    resultDummySquare = (short)(resultDummySquare + (short)moveOffset);
                }
            }
            return availibleMoves.ToArray();
        }
    }
}

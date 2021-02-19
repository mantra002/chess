using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_app
{
    static class MoveData
    {
        //Need a way to generate the index offsets for pieces

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

        public enum MoveDirections : short
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

            GenerateValidBoardPositions();

            for (short i = 0; i < 64; i++)
            {
                AvailiblePawnMovesWhite[i] = GenerateNonSlidingMoves(PawnMovesWhite, i, true, true);
                AvailiblePawnMovesBlack[i] = GenerateNonSlidingMoves(PawnMovesBlack, i, false, true);
                AvailiblePawnAttacksWhite[i] = GenerateNonSlidingMoves(PawnAttacksWhite, i);
                AvailiblePawnAttacksBlack[i] = GenerateNonSlidingMoves(PawnAttacksBlack, i);
                AvailibleBishopMoves[i] = GenerateSlidingMoves(BishopMoves, i);
                AvailibleRookMoves[i] = GenerateSlidingMoves(RookMoves, i);
                AvailibleKnightMoves[i] = GenerateNonSlidingMoves(KnightMoves, i);
                AvailibleKingMoves[i] = GenerateNonSlidingMoves(KingMoves, i);
                AvailibleQueenMoves[i] = GenerateNonSlidingMoves(QueenMoves, i);
            }
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

        private static short[] GenerateNonSlidingMoves(MoveDirections[] moves, short square, bool white = true, bool pawn = false)
        {
            square = ConvertRealSquareToDummyBoard(square);
            List<short> availibleMoves = new List<short>();
            short resultDummySquare, realSquare;
            foreach(MoveDirections moveOffset in moves)
            {
                resultDummySquare = (short)(square + (short)moveOffset);

                if(ValidBoardPositions[resultDummySquare])
                {
                    availibleMoves.Add(ConvertDummyBoardToRealSquare(resultDummySquare));
                }
            }
            if(pawn && ValidBoardPositions[square])
            {
                realSquare = ConvertDummyBoardToRealSquare(square);
                if(Board.GetRank((byte)realSquare) == 7 && !white)
                {
                    availibleMoves.Add(ConvertDummyBoardToRealSquare((short)(square + (short)MoveDirections.Down * 2)));
                }
                if (Board.GetRank((byte)realSquare) == 2 && white)
                {
                    availibleMoves.Add(ConvertDummyBoardToRealSquare((short)(square + (short)MoveDirections.Up * 2)));
                }

            }
            return availibleMoves.ToArray();
        }

        public static short[] GenerateSlidingMoves(MoveDirections[] moves, short square, Board b = null)
        {
            square = ConvertRealSquareToDummyBoard(square);
            List<short> availibleMoves = new List<short>();
            short resultDummySquare;
            short destinationPiece;
            short realSquare=0;
            foreach (MoveDirections moveOffset in moves)
            {
                resultDummySquare = (short)(square + (short)moveOffset);
                while (ValidBoardPositions[resultDummySquare])
                { 
                    realSquare = ConvertDummyBoardToRealSquare(resultDummySquare);
                    if(b != null)
                    {
                        destinationPiece = b.GameBoard[realSquare];
                        availibleMoves.Add(realSquare);
                        if (destinationPiece != 0) break;
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

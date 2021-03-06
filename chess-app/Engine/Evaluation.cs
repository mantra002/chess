﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Engine
{
    using Chess.Game;
    using static Chess.Game.Enums;

    static public class Evaluation

    {
        public const int MateValue = 100000;
        public const int PawnValue = 100;
        public const int BishopValue = 325;
        public const int KnightValue = 325;
        public const int RookValue = 500;
        public const int QueenValue = 975;
        public const int KingValue = 20000;

        public const int CaptureBonusMultiplier = 5;

        public const int SeeCutoff = 100;

        const int BishopPairValue = 50;
        const int KnightPawnBonus = 6; // per pawn > 5 pawns
        const int RookPawnPenalty = -12; // per pawn > 5 pawns

        static public int Evaluate(Board b)
        {
            short pawnCountWhite = 0, pawnCountBlack = 0;
            short bishopCountWhite = 0, bishopCountBlack = 0;
            short knightCountWhite = 0, knightCountBlack = 0;
            short rookCountWhite = 0, rookCountBlack = 0;

            byte decodePiece, decodeLocation;
            int score = 0;
            int numberOfPieces = b.PieceList.Count();
            for (int index = 0; index < numberOfPieces; index++)
            {
                ushort piece = b.PieceList[index];

                decodePiece = Board.DecodePieceFromPieceList(piece);
                decodeLocation = Board.DecodeLocationFromPieceList(piece);

                if ((decodePiece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn)
                {
                    if ((decodePiece & (byte)Colors.White) == (byte)Colors.White)
                    {
                        score += PawnValue + PieceSquareTable.GetTableValue(PieceSquareTable.Pawn, decodeLocation, Colors.White);
                        pawnCountWhite++;
                    }
                    else
                    {
                        score -= PawnValue + PieceSquareTable.GetTableValue(PieceSquareTable.Pawn, decodeLocation, Colors.Black); ;
                        pawnCountBlack++;
                    }
                }
                else if ((decodePiece & (byte)PieceNames.Knight) == (byte)PieceNames.Knight)
                {
                    if ((decodePiece & (byte)Colors.White) == (byte)Colors.White)
                    {
                        score += KnightValue + PieceSquareTable.GetTableValue(PieceSquareTable.Knight, decodeLocation, Colors.White);
                        knightCountWhite++;
                    }
                    else
                    {
                        score -= KnightValue + PieceSquareTable.GetTableValue(PieceSquareTable.Knight, decodeLocation, Colors.Black);
                        knightCountBlack++;
                    }
                }
                else if ((decodePiece & (byte)PieceNames.King) == (byte)PieceNames.King)
                {
                    if ((decodePiece & (byte)Colors.White) == (byte)Colors.White)
                    {
                        score += KingValue + PieceSquareTable.GetTableValue(PieceSquareTable.King, decodeLocation, Colors.White);
                    }
                    else
                    {
                        score -= KingValue + PieceSquareTable.GetTableValue(PieceSquareTable.King, decodeLocation, Colors.Black);
                    }
                }
                else if ((decodePiece & (byte)PieceNames.Queen) == (byte)PieceNames.Queen)
                {
                    if ((decodePiece & (byte)Colors.White) == (byte)Colors.White)
                    {
                        score += QueenValue + PieceSquareTable.GetTableValue(PieceSquareTable.Queen, decodeLocation, Colors.White);
                    }
                    else
                    {
                        score -= QueenValue + PieceSquareTable.GetTableValue(PieceSquareTable.Queen, decodeLocation, Colors.Black);
                    }
                }
                else if ((decodePiece & (byte)PieceNames.Bishop) == (byte)PieceNames.Bishop)
                {
                    if ((decodePiece & (byte)Colors.White) == (byte)Colors.White)
                    {
                        score += BishopValue + PieceSquareTable.GetTableValue(PieceSquareTable.Bishop, decodeLocation, Colors.White);
                        bishopCountWhite++;
                    }
                    else
                    {
                        score -= BishopValue + PieceSquareTable.GetTableValue(PieceSquareTable.Bishop, decodeLocation, Colors.Black);
                        bishopCountBlack++;
                    }
                }
                else if ((decodePiece & (byte)PieceNames.Rook) == (byte)PieceNames.Rook)
                {
                    if ((decodePiece & (byte)Colors.White) == (byte)Colors.White)
                    {
                        score += RookValue + PieceSquareTable.GetTableValue(PieceSquareTable.Rook, decodeLocation, Colors.White);
                        rookCountWhite++;
                    }
                    else
                    {
                        score -= RookValue + PieceSquareTable.GetTableValue(PieceSquareTable.Rook, decodeLocation, Colors.Black);
                        rookCountBlack++;
                    }
                }
                else
                {
                    throw new Exception("Trying to score an invalid peice");
                }
            }
            if (bishopCountBlack > 1) score -= BishopPairValue;
            if (bishopCountWhite > 1) score += BishopPairValue;
            score += RookPawnPenalty * (pawnCountWhite - 5) * rookCountWhite;
            score -= RookPawnPenalty * (pawnCountBlack - 5) * rookCountBlack;
            score += KnightPawnBonus * (pawnCountWhite - 5) * knightCountWhite;
            score -= KnightPawnBonus * (pawnCountBlack - 5) * knightCountBlack;

            if (b.ColorToMove == Colors.Black) return -score;
            else
                return score;
        }

        public static int GetPieceValue(byte piece)
        {
            if ((piece & (byte)PieceNames.Pawn) != 0)
            {
                return Evaluation.PawnValue;
            }
            else if ((piece & (byte)PieceNames.Knight) != 0)
            {
                return Evaluation.KnightValue;
            }
            else if ((piece & (byte)PieceNames.Bishop) != 0)
            {
                return Evaluation.BishopValue;
            }
            else if ((piece & (byte)PieceNames.Rook) != 0)
            {
                return Evaluation.RookValue;
            }
            else if ((piece & (byte)PieceNames.Queen) != 0)
            {
                return Evaluation.QueenValue;
            }
            else
            {
                return Evaluation.KingValue;
            }
        }

    }
}

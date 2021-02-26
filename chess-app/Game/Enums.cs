﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Game
{
    public static class Enums
    {
        public enum CastleFlags : byte //White Short - White Long - Black Short - Black Long
        {
            WhiteShortCastle = 0b1000,
            WhiteLongCastle = 0b0100,
            BlackShortCastle = 0b0010,
            BlackLongCastle = 0b0001,
            None = 0b10000,
        }
        public enum PieceNames : byte
        {
            Pawn = 128,
            Knight = 4,
            Bishop = 8,
            Rook = 16,
            Queen = 32,
            King = 64,
            None=0
        }

        public enum Colors : byte
        {
            White = 2,
            Black = 1
        }

        public enum Squares : byte
        {
            a8 = 0,
            b8 = 1,
            c8 = 2,
            d8 = 3,
            e8 = 4,
            f8 = 5,
            g8 = 6,
            h8 = 7,
            a7 = 8,
            b7 = 9,
            c7 = 10,
            d7 = 11,
            e7 = 12,
            f7 = 13,
            g7 = 14,
            h7 = 15,
            a6 = 16,
            b6 = 17,
            c6 = 18,
            d6 = 19,
            e6 = 20,
            f6 = 21,
            g6 = 22,
            h6 = 23,
            a5 = 24,
            b5 = 25,
            c5 = 26,
            d5 = 27,
            e5 = 28,
            f5 = 29,
            g5 = 30,
            h5 = 31,
            a4 = 32,
            b4 = 33,
            c4 = 34,
            d4 = 35,
            e4 = 36,
            f4 = 37,
            g4 = 38,
            h4 = 39,
            a3 = 40,
            b3 = 41,
            c3 = 42,
            d3 = 43,
            e3 = 44,
            f3 = 45,
            g3 = 46,
            h3 = 47,
            a2 = 48,
            b2 = 49,
            c2 = 50,
            d2 = 51,
            e2 = 52,
            f2 = 53,
            g2 = 54,
            h2 = 55,
            a1 = 56,
            b1 = 57,
            c1 = 58,
            d1 = 59,
            e1 = 60,
            f1 = 61,
            g1 = 62,
            h1 = 63,
            None
        }

    }
}

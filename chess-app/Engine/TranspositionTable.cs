using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


using Chess.Game;

namespace Chess.Engine
{

    public class TranspositionTable
    {
        Position[] tt;
        public uint TableSizeInMb;
        readonly ulong TableSizeInPositions;
        ulong TtEntries = 0;
        public double PercentFull { get { return TtEntries / (double)TableSizeInPositions; } }

        public TranspositionTable(uint sizeInMb = 64)
        {
            TableSizeInMb = sizeInMb;
            TableSizeInPositions = (ulong)sizeInMb * 1000000 / (ulong)(Position.GetSize());
            tt = new Position[TableSizeInPositions];
#if DEBUG
            Console.WriteLine($"Intializing Transposition Table with {TableSizeInMb} mb of space / {TableSizeInPositions} positions");
#endif 
        }

        public void ClearTable()
        {
            tt = new Position[TableSizeInPositions];
        }
        private int GetTTIndex(ulong hashKey)
        {
            return (int)(hashKey % TableSizeInPositions);
        }

        public Position LookupPosition(ulong hashKey)
        {
            Position p = (Position)tt[GetTTIndex(hashKey)];
            if (p != null && p.HashKey == hashKey)
            {
                //Console.WriteLine($"Retrived position {hashKey} successfully!");
                return p; }
            return null;
        }
        public void AddPosition(ulong key, int score, Move movePlayed, byte depth, byte plyFromRoot, NodeType nt)
        {
            //Console.WriteLine($"Saving position with key {key} at index {GetTTIndex(key)}");
            Position p = new Position(key, score, movePlayed, depth, plyFromRoot, nt);
            tt[GetTTIndex(key)] = p;
            TtEntries++;
        }
        private static int AdjustedScoreIntoTT(int score, int plyFromRoot)
        {
            if(Search.ScoreNearCheckmate(score))
            {
                if (score > 0) return score + plyFromRoot;
                else return score - plyFromRoot;
            }
            return score;
        }
        private static int AdjustedScoreOutOfTT(int score, int plyFromRoot)
        {
            if (Search.ScoreNearCheckmate(score))
            {
                if (score > 0) return score - plyFromRoot;
                else return score + plyFromRoot;
            }
            return score;
        }
        public enum NodeType
        {
            Exact,
            Beta,
            Alpha
        }
    
        [StructLayout(LayoutKind.Sequential)]
        public class Position
        {
            public readonly ulong HashKey;
            public int Score;
            public readonly Move MovePlayed;
            public readonly byte Depth;
            public readonly NodeType NType;


            public Position(ulong hk, int score, Move movePlayed, byte depth, byte plyFromRoot, NodeType nt)
            {
                this.HashKey = hk;
                this.MovePlayed = movePlayed;
                this.Depth = depth;
                this.NType = nt;
                this.Score = AdjustedScoreIntoTT(score, plyFromRoot);
            }
            public static int GetSize()
            {
                return System.Runtime.InteropServices.Marshal.SizeOf<Position>();
            }

            public int GetScore(byte plyFromRoot)
            {
                return AdjustedScoreOutOfTT(this.Score, plyFromRoot);
            }
        }
    }

}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


using Chess.Game;

namespace Chess.Engine
{

    internal class TranspositionTable
    {
        Position[] tt;
        internal uint TableSizeInMb;
        readonly ulong TableSizeInPositions;
        ulong TtEntries = 0;
        internal double PercentFull { get { return TtEntries / (double)TableSizeInPositions; } }

        internal TranspositionTable(uint sizeInMb = 64)
        {
            TableSizeInMb = sizeInMb;
            TableSizeInPositions = (ulong)sizeInMb * 1000000 / (ulong)(Position.GetSize());
            tt = new Position[TableSizeInPositions];
#if DEBUG
            Console.WriteLine($"Intializing Transposition Table with {TableSizeInMb} mb of space / {TableSizeInPositions} positions");
#endif 
        }

        internal void ClearTable()
        {
            tt = new Position[TableSizeInPositions];
        }
        private int GetTTIndex(ulong hashKey)
        {
            return (int)(hashKey % TableSizeInPositions);
        }

        internal Position LookupPosition(ulong hashKey)
        {
            Position p = (Position)tt[GetTTIndex(hashKey)];
            if (p != null && p.HashKey == hashKey)
            {
                //Console.WriteLine($"Retrived position {hashKey} successfully!");
                return p; }
            return null;
        }
        internal void AddPosition(ulong key, int score, Move movePlayed, byte depth, byte plyFromRoot, NodeType nt)
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
        internal enum NodeType
        {
            Exact,
            Beta,
            Alpha
        }
    
        [StructLayout(LayoutKind.Sequential)]
        internal class Position
        {
            internal readonly ulong HashKey;
            internal int Score;
            internal readonly Move MovePlayed;
            internal readonly byte Depth;
            internal readonly NodeType NType;


            internal Position(ulong hk, int score, Move movePlayed, byte depth, byte plyFromRoot, NodeType nt)
            {
                this.HashKey = hk;
                this.MovePlayed = movePlayed;
                this.Depth = depth;
                this.NType = nt;
                this.Score = AdjustedScoreIntoTT(score, plyFromRoot);
            }
            internal static int GetSize()
            {
                return System.Runtime.InteropServices.Marshal.SizeOf<Position>();
            }

            internal int GetScore(byte plyFromRoot)
            {
                return AdjustedScoreOutOfTT(this.Score, plyFromRoot);
            }
        }
    }

}


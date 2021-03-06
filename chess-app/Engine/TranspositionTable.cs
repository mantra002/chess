using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public TranspositionTable(uint sizeInMb = 512)
        {
            TableSizeInMb = sizeInMb;
            TableSizeInPositions = (ulong)sizeInMb * 1000000 / (ulong)(Position.GetSize());
            tt = new Position[TableSizeInPositions];
#if DEBUG
            Console.WriteLine("Intializing Transposition Table with " + TableSizeInMb + "mb of space");
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
        public int LookupScore(ulong hashKey, int depth, int ply, int alpha, int beta)
        {
            //Console.WriteLine("Looking for a position with key " + hashKey + " at index " + GetTTIndex(hashKey));
            Position p = (Position)tt[GetTTIndex(hashKey)];
            if (p.HashKey == hashKey)
            {
                if (p.Depth >= depth)
                {
                    //Console.WriteLine("*****************Found it!");
                    switch (p.NType)
                    {
                        case NodeType.Exact:
                            return p.Score;
                        case NodeType.LowerBound:
                            if (p.Score >= beta) return p.Score;
                            break;
                        case NodeType.UpperBound:
                            if (p.Score <= alpha) return p.Score;
                            break;
                    }
                }
            }

            return int.MinValue;
        }
        public Position LookupPosition(ulong hashKey)
        {
            Position p = (Position)tt[GetTTIndex(hashKey)];
            return p;
        }
        public void AddPosition(ulong key, int score, Move movePlayed, byte depth, NodeType nt)
        {
            //Console.WriteLine("Saving position with key " + key + " at index " + GetTTIndex(key));
            Position p = new Position(key, score, movePlayed, depth, nt);
            tt[GetTTIndex(key)] = p;
            TtEntries++;
        }

        public enum NodeType
        {
            Exact,
            LowerBound,
            UpperBound
        }
        public struct Position
        {
            public readonly ulong HashKey;
            public readonly int Score;
            public readonly Move MovePlayed;
            public readonly byte Depth;
            public readonly NodeType NType;

            public Position(ulong hk, int score, Move movePlayed, byte depth, NodeType nt)
            {
                this.HashKey = hk;
                this.Score = score;
                this.MovePlayed = movePlayed;
                this.Depth = depth;
                this.NType = nt;
            }
            public static int GetSize()
            {
                return System.Runtime.InteropServices.Marshal.SizeOf<Position>();
            }
        }

    }
}

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
        const int TABLE_SIZE = 64000;

        public TranspositionTable()
        {
            tt = new Position[TABLE_SIZE];
        }

        public void ClearTable()
        {
            tt = new Position[TABLE_SIZE];
        }
        private int GetTTIndex(ulong hashKey)
        {
            return (int)(hashKey % TABLE_SIZE);
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

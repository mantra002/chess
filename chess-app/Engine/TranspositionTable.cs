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
        public void AddPosition(ulong key, int score, Move movePlayed, byte depth, NodeType nt)
        {
            //Console.WriteLine($"Saving position with key {key} at index {GetTTIndex(key)}");
            Position p = new Position(key, score, movePlayed, depth, nt);
            tt[GetTTIndex(key)] = p;
            TtEntries++;
        }
        private static int AdjustedScoreIntoTT(int score, int depth)
        {
            if(Search.ScoreNearCheckmate(score))
            {
                if (score > 0) return score + depth;
                else return score - depth;
            }
            return score;
        }
        private static int AdjustedScoreOutOfTT(int score, int depth)
        {
            if (Search.ScoreNearCheckmate(score))
            {
                if (score > 0) return score - depth;
                else return score + depth;
            }
            return score;
        }
        public enum NodeType
        {
            Exact,
            LowerBound,
            UpperBound
        }
        [StructLayout(LayoutKind.Sequential)]
        public class Position
        {
            private int _score;
            public readonly ulong HashKey;
            public int Score { 
                get { return AdjustedScoreOutOfTT(_score, this.Depth); }
                set { _score = AdjustedScoreIntoTT(value, this.Depth); }
          
           }
            public readonly Move MovePlayed;
            public readonly byte Depth;
            public readonly NodeType NType;


            public Position(ulong hk, int score, Move movePlayed, byte depth, NodeType nt)
            {
                this.HashKey = hk;
                this.MovePlayed = movePlayed;
                this.Depth = depth;
                this.NType = nt;
                this.Score = score;
            }
            public static int GetSize()
            {
                return System.Runtime.InteropServices.Marshal.SizeOf<Position>();
            }
            public int GetScore(int depth, int alpha, int beta)
            {
                if (this.Depth >= depth)
                {
                    //Console.WriteLine("*****************Found it!");
                    switch (this.NType)
                    {
                        case NodeType.Exact:
                            return this.Score;
                        case NodeType.LowerBound:
                            if (this.Score >= beta) return this.Score;
                            break;
                        case NodeType.UpperBound:
                            if (this.Score <= alpha) return this.Score;
                            break;
                    }
                }
                return -9999999;
            }

        }
    }

}


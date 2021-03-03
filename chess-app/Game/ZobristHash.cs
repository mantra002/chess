using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Game
{
    static class ZobristHash
    {
        public readonly static ulong[][][] PieceKeys;
        public readonly static ulong[] CastleKeys;
        public readonly static ulong[] EpKeys;
        public readonly static ulong BlackToPlay;

        static ZobristHash()
        {
            Random rng = new Random(584796851);
            PieceKeys = GeneratePieceKeys(rng);
            CastleKeys = Generate1DKeys(rng, 16);
            EpKeys = Generate1DKeys(rng, 64);
            BlackToPlay = (ulong)(rng.NextDouble() * UInt64.MaxValue);
        }
 
        private static ulong[] Generate1DKeys(Random rng, int numberOfKeys)
        {
            ulong[] keys = new ulong[numberOfKeys];
            for(int i =0; i< numberOfKeys; i++)
            {
                keys[i] = (ulong)(rng.NextDouble() * UInt64.MaxValue);
            }
            return keys;
        }
        private static ulong[][][] GeneratePieceKeys(Random rng)
        {
            ulong[][][] pKeys = new ulong[64][][];
            for (int s = 0; s < 64; s++)
            {
                pKeys[s] = new ulong[2][];
                for (int c = 0; c < 2; c++)
                {
                    pKeys[s][c] = new ulong[6];
                    for (int p = 0; p < 6; p++)
                    {
                        pKeys[s][c][p] = (ulong)(rng.NextDouble() * UInt64.MaxValue);
                    }
                }
            }
            return pKeys;
        }
    }
}

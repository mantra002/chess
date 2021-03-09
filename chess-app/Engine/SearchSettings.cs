using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Engine
{
    public class SearchSettings
    {
        public bool IterativeDeepeningEnable = true;
        public bool QuiescenceSearchEnable = true;
        public bool UseMoveOrdering = true;
        public bool UseSeeInQuiescenceSeach = true;
        public uint TranspositionTableSizeMb = 128;
        public bool UseOpeningBook = true;

        public int WhiteTimeInMs = 0;
        public int BlackTimeInMs = 0;
        public int WhiteIncrementInMs = 0;
        public int BlackIncrementInMs = 0;
        public int MovesToGoUntilAdditionalTime = 0;
        public int Depth = 0;
        public int MaxNodesToSearch = 0;
        public int TimeLimitInMs = 0;
        public int AssumedGameLength = 60;

        public bool InfiniteSearch = false;
        public bool SearchForMate = false;
        public bool Ponder = false;
    }
}

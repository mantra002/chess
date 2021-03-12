using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Engine
{
    internal class SearchSettings
    {
        internal bool IterativeDeepeningEnable = true;
        internal bool QuiescenceSearchEnable = true;
        internal bool UseMoveOrdering = true;
        internal bool UseSeeInQuiescenceSeach = true;
        internal uint TranspositionTableSizeMb = 128;
        internal bool UseOpeningBook = true;

        internal int WhiteTimeInMs = 0;
        internal int BlackTimeInMs = 0;
        internal int WhiteIncrementInMs = 0;
        internal int BlackIncrementInMs = 0;
        internal int MovesToGoUntilAdditionalTime = 0;
        internal int Depth = 0;
        internal int MaxNodesToSearch = 0;
        internal int TimeLimitInMs = 0;
        internal int AssumedGameLength = 60;

        internal bool InfiniteSearch = false;
        internal bool SearchForMate = false;
        internal bool Ponder = false;
    }
}

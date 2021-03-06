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
    }
}

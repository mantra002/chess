using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Chess
{
    using Chess.Interface;
    class Program
    {
        static void Main(string[] args)

        {
            UCI uci = new UCI();
            uci.StartCommandLoop();
        }
    }
}

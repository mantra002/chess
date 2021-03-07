using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Chess.Interface;
using Chess.Game;
using Chess.Engine;
using Chess.Management;

namespace Chess
{ 
    class Program
    {
        // 1r1qr3/p1p2kpp/2ppbb2/8/4P3/2NQ4/PPP2PPP/1R2K2R w K - 0 1
        // R7/3r2pp/3Qbq2/1k6/1p2P3/8/2P2PP1/6K1 w - - 0 14

        static void Main(string[] args)

        {
            UCI uci = new UCI();
            uci.StartCommandLoop();
            // OpeningBook<string> ob = OpeningBook<string>.InitializeOpeningBook();
            //GameManager gm = new GameManager("R7/3r2pp/3Qbq2/1k6/1p2P3/8/2P2PP1/6K1 w - - 0 0");
            //gm.PerformSearch(6);
            //Console.ReadLine();
        }
    }
}

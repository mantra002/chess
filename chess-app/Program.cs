using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_app
{
    class Program
    {
        static void Main(string[] args)
        {
            Board g = new Board();
            Random r = new Random();
            List<Move> cms;
            int counter = 0;
            System.Diagnostics.Stopwatch t = new System.Diagnostics.Stopwatch();
            t.Start();
            while (counter < 250000)
            {
                cms = MoveGeneration.GenerateLegalMoves(g);
                if (cms.Count() == 0)
                {
                    g = new Board();
                    cms = MoveGeneration.GenerateLegalMoves(g);
                }
                int i = r.Next(0, cms.Count());
                //Console.WriteLine("Playing " + cms[i].ToString());
                g.PlayMove(cms[i]);
                counter++;
            }
            t.Stop();
            Console.WriteLine(250000 / (t.ElapsedMilliseconds/1000.0) + " moves/second");
            Console.ReadLine();
        }
    }
}

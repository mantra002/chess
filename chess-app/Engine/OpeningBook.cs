using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Engine
{
    public class OpeningBook<T>
    {
        //Book from https://sites.google.com/site/computerschess/balsa-opening-test-suite

        private List<OpeningBook<string>> openingMoves;
        private string move;


        public OpeningBook(string move)
        {
            this.move = move;
            this.openingMoves = new List<OpeningBook<string>>();
        }
        public static OpeningBook<string> InitializeOpeningBook(string location = "Engine\\data\\opening-db.dat")
        {
            OpeningBook<string> root = new OpeningBook<string>("*");

            List<List<string>> algebraicMoves = new List<List<string>>();
            root.openingMoves = new List<OpeningBook<string>>();
            string move;
            int openingCount = 0;
            StreamReader sr = new StreamReader(location);
            while((move = sr.ReadLine()) != null)
            {
                algebraicMoves.Add(new List<string>());
                foreach(string s in move.Split(' '))
                {
                    if(!string.IsNullOrWhiteSpace(s))algebraicMoves[openingCount].Add(s);
                }
                openingCount++;
            }
            OpeningBook<string> child;
            OpeningBook<string> child2;
            foreach (List<string> opening in algebraicMoves)
            {
                child = root.GetChildList(opening[0]);
                if (child == null)
                {
                    root.AddChildList(opening[0]);
                    openingCount++;
                    child = root.GetChildList(opening[0]);
                }

                for (int i = 1; i< opening.Count; i++)
                {
                    child2 = child.GetChildList(opening[i]);
                    if (child2 == null)
                    {
                        child.AddChildList(opening[i]);
                        openingCount++;
                    }
                    child2 = child.GetChildList(opening[i]);
                    child = child2;
                }    
            }
#if DEBUG
            Console.WriteLine(openingCount + " opening lines loaded into the openings book.");
#endif
            return root;
        }

        public OpeningBook<string> GetChildList(string parentMove)
        {
            foreach(OpeningBook<string> ob in this.openingMoves)
            {
                if (ob.move == parentMove) return ob;
            }

            return null;
        }

        public List<string> ListAllChildren()
        {
            List<string> children = new List<string>();
            foreach (OpeningBook<string> ob in this.openingMoves)
            {

                children.Add(ob.move);
            }
            return children;
        }
        public void AddChildList(string childMove)
        {
            openingMoves.Add(new OpeningBook<string>(childMove));
        }
        public override string ToString()
        {
            return this.move;
        }
    }
}

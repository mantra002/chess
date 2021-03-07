using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.Interface
{
    public class UCI
    {
        private Management.GameManager gmgr;
        Thread search;

        public void StartCommandLoop()
        {
            gmgr = new Management.GameManager();
            new Thread(() => CommandLoop()).Start();
        }

        private void CommandLoop()
        {
            Console.WriteLine("Simple Chess Engine by Travis Hagen");
            while (true)
            {
                string command = Console.ReadLine();
                string[] splitCmd = command.Split(' ');
                switch (splitCmd[0])
                {
                    case "uci":
                        SendOptions();
                        break;
                    case "quit":
                        return;
                    case "go":
                        search = new Thread(() => PerformSearchWithStatus(splitCmd));
                        search.IsBackground = true;
                        search.Start();
                        break;
                    case "position":
                        if (splitCmd.Length >= 3)
                        {
                            Position(splitCmd[1], splitCmd);
                        }
                        else Console.WriteLine("Position error");
                        break;
                    case "isready":
                        Console.WriteLine("readyok");
                        break;
                    case "setoption":
                        break;
                    case "stop":
                        search.Abort();
                        if (gmgr.AbSearch.BestMove != null) Console.Write("bestmove " + gmgr.AbSearch.BestMove.ToString());
                        if (gmgr.AbSearch.PrincipalVariation.Count() >= 2 && gmgr.AbSearch.PrincipalVariation[1] != null) Console.Write(" ponder " + gmgr.AbSearch.PrincipalVariation[1]);
                        Console.WriteLine();
                        break;
                    case "ucinewgame":
                        gmgr = new Management.GameManager();
                        break;
                    default:
                        Console.WriteLine("Invalid command");
                        break;
                }
            }
        }
        void SendOptions()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("id name Simple Chess Engine");
            sb.AppendLine("id author Travis Hagen");
            string[] options = { "QuiescenceSearch", "IterativeDeepening", "MoveOrdering", "StaticExchangeEvaluation", "UseOpeningBook" };
            foreach (string s in options)
            {
                sb.Append("option name ");
                sb.Append(s);
                sb.AppendLine(" type check default true");
            }
            Console.Write(sb.ToString());
            Console.WriteLine("uciok");
        }
        void PerformSearchWithStatus(string[] splitCmd)
        {
            short depth = 7;
            try
            {
                if (splitCmd.Length > 1)
                {
                    if (splitCmd[1] == "depth")
                    {
                        depth = short.Parse(splitCmd[2]);
                        gmgr.PerformSearch(depth);
                    }
                    else if (splitCmd[1] == "infinite")
                    {
                        gmgr.PerformSearch(999);
                    }
                    else if (splitCmd[1] == "perft") gmgr.RunPerft(int.Parse(splitCmd[2]));
                    else
                    {
                        gmgr.PerformSearch(7);
                        Console.WriteLine("bestmove " + gmgr.AbSearch.BestMove.ToString());
                    }
                }
            }
            catch (ThreadAbortException e)
            {

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }


        }

        void Position(string fenOrStart, string[] splitCmd)
        {
            int moveIndex = 0;

            if (fenOrStart.ToLowerInvariant() == "fen")
            {
                try
                {
                    string fen = string.Join(" ", splitCmd, 2, splitCmd.Length - 2);
                    gmgr.Board = new Game.Board(fen);
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
            else gmgr.Board = new Game.Board();
            for (int i = 0; i < splitCmd.Length; i++)
            {
                if (splitCmd[i] == "moves")
                {
                    moveIndex = i + 1;
                    for (int j = moveIndex; j < splitCmd.Length; j++)
                    {
                        gmgr.PlayMove(new Game.Move(splitCmd[j], gmgr.Board));
                    }
                    break;
                }
            }

        }

        void Send(string commandToGui)
        {

        }
    }
}

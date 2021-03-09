using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.Interface
{
    public class UCI
    {
        private Management.GameManager gmgr;
        bool startingFromStartpos = true;

        public void StartCommandLoop()
        {
            gmgr = new Management.GameManager();
            new Thread(() => CommandLoop()).Start();
        }

        private void CommandLoop()
        {
            Console.WriteLine("Simple Chess Engine by Travis Hagen");
            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));
            while (true)
            {
                string command = Console.ReadLine();
                if (command.Length > 0)
                {
                    string[] splitCmd = command.Split(' ');
                    string mainCommand = splitCmd[0].ToLowerInvariant();
                    bool performSearch = true;
                    switch (mainCommand)
                    {
                        case "uci":
                            SendOptions();
                            break;
                        case "debug":
                            if (splitCmd[1].ToLowerInvariant() == "on") { } //Turn on debug
                            else { }; //Turn off debug
                            break;
                        case "quit":
                            return;
                        case "go":
                            gmgr.SearchSet = new Engine.SearchSettings();
                            for (int i = 1; i < splitCmd.Length; i++)
                            {
                                switch (splitCmd[i])
                                {
                                    case "searchmoves":
                                        break;
                                    case "ponder":
                                        gmgr.SearchSet.Ponder = true;
                                        break;
                                    case "wtime":
                                        gmgr.SearchSet.WhiteTimeInMs = int.Parse(splitCmd[i+1]);
                                        i++;
                                        break;
                                    case "btime":
                                        gmgr.SearchSet.BlackTimeInMs = int.Parse(splitCmd[i+1]);
                                        i++;
                                        break;
                                    case "winc":
                                        gmgr.SearchSet.WhiteIncrementInMs = int.Parse(splitCmd[i+1]);
                                        i++;
                                        break;
                                    case "binc":
                                        gmgr.SearchSet.BlackIncrementInMs = int.Parse(splitCmd[i+1]);
                                        i++;
                                        break;
                                    case "movestogo":
                                        gmgr.SearchSet.MovesToGoUntilAdditionalTime = int.Parse(splitCmd[i+1]);
                                        i++;
                                        break;
                                    case "depth":
                                        gmgr.SearchSet.Depth = int.Parse(splitCmd[i+1]);
                                        break;
                                    case "nodes":
                                        gmgr.SearchSet.MaxNodesToSearch = int.Parse(splitCmd[i+1]);
                                        i++;
                                        break;
                                    case "mate":
                                        gmgr.SearchSet.SearchForMate = true;
                                        break;
                                    case "movetime":
                                        gmgr.SearchSet.TimeLimitInMs = int.Parse(splitCmd[i+1]);
                                        i++;
                                        break;
                                    case "infinite":
                                        gmgr.SearchSet.InfiniteSearch = true;
                                        break;
                                    case "perft":
                                        gmgr.PerftDivided(int.Parse(splitCmd[i + 1]));
                                        performSearch = false;
                                        i++;
                                        break;
                                }
                            }
                            if (performSearch) gmgr.PerformSearch(startingFromStartpos);
                            break;
                        case "position":
                            string subCommand = splitCmd[1].ToLowerInvariant();
                            Position(subCommand, splitCmd);
                            break;
                        case "isready":
                            Console.WriteLine("readyok");
                            break;
                        case "setoption":
                            int valueIndex = 0;
                            string value = "";
                            string name = "";
                            for (int i = 0; i < splitCmd.Length; i++)
                            {
                                if (splitCmd[i].ToLowerInvariant() == "value")
                                {
                                    valueIndex = i;
                                    value = string.Join(" ", splitCmd, valueIndex + 1, splitCmd.Length - valueIndex - 1);
                                    break;
                                }
                            }
                            name = string.Join(" ", splitCmd, 2, valueIndex - 2);
                            SetOption(name, value);
                            break;
                        case "stop":
                            gmgr.AbortSearch();
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
            void SetOption(string name, string value)
            {

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




            void Position(string fenOrStart, string[] splitCmd)
            {
                int moveIndex = 0;

                if (fenOrStart.ToLowerInvariant() == "fen")
                {
                    try
                    {
                        string fen = string.Join(" ", splitCmd, 2, splitCmd.Length - 2);
                        gmgr.Board = new Game.Board(fen);
                        startingFromStartpos = false;
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                }
                else if (fenOrStart.ToLowerInvariant() == "startpos")
                {
                    gmgr.Board = new Game.Board();
                    startingFromStartpos = true;
                }
                else
                {
                    Console.WriteLine("Invalid position");
                }
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

        }
    }
}

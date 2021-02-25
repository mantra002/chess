using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Chess.Test
{
    using Chess.Management;
    [TestClass]
    public class PerftTests
    {
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void Perft1(int depth)
        {

            long perft = RunPerft("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", depth);
            switch (depth)
            {
                case 0:
                    Assert.AreEqual(1, perft);
                    break;
                 case 1:
                    Assert.AreEqual(20, perft);
                    break;
                case 2:
                    Assert.AreEqual(400, perft);
                    break;
                case 3:
                    Assert.AreEqual(8902, perft);
                    break;
                case 4:
                    Assert.AreEqual(197281, perft);
                    break;
                }

        }
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void Perft2(int depth)
        {

            long perft = RunPerft("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ", depth);
            switch (depth)
            {
                case 0:
                    Assert.AreEqual(1, perft);
                    break;
                case 1:
                    Assert.AreEqual(48, perft);
                    break;
                case 2:
                    Assert.AreEqual(2039, perft);
                    break;
                case 3:
                    Assert.AreEqual(97862, perft);
                    break;
                case 4:
                    Assert.AreEqual(4085603, perft);
                    break;
            }

        }
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void Perft3(int depth)
        {

            long perft = RunPerft("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - ", depth);
            switch (depth)
            {
                case 0:
                    Assert.AreEqual(1, perft);
                    break;
                case 1:
                    Assert.AreEqual(14, perft);
                    break;
                case 2:
                    Assert.AreEqual(191, perft);
                    break;
                case 3:
                    Assert.AreEqual(2812, perft);
                    break;
                case 4:
                    Assert.AreEqual(43238, perft);
                    break;
            }

        }
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void Perft4(int depth)
        {

            long perft = RunPerft("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", depth);
            switch (depth)
            {
                case 0:
                    Assert.AreEqual(1, perft);
                    break;
                case 1:
                    Assert.AreEqual(6, perft);
                    break;
                case 2:
                    Assert.AreEqual(264, perft);
                    break;
                case 3:
                    Assert.AreEqual(9467, perft);
                    break;
                case 4:
                    Assert.AreEqual(422333, perft);
                    break;
            }

        }
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void Perft5(int depth)
        {

            long perft = RunPerft("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", depth);
            switch (depth)
            {
                case 0:
                    Assert.AreEqual(1, perft);
                    break;
                case 1:
                    Assert.AreEqual(14, perft);
                    break;
                case 2:
                    Assert.AreEqual(1486, perft);
                    break;
                case 3:
                    Assert.AreEqual(62379, perft);
                    break;
                case 4:
                    Assert.AreEqual(2103487, perft);
                    break;
            }

        }
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void Perft6(int depth)
        {

            long perft = RunPerft("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10 ", depth);
            switch (depth)
            {
                case 0:
                    Assert.AreEqual(1, perft);
                    break;
                case 1:
                    Assert.AreEqual(46, perft);
                    break;
                case 2:
                    Assert.AreEqual(2079, perft);
                    break;
                case 3:
                    Assert.AreEqual(89890, perft);
                    break;
                case 4:
                    Assert.AreEqual(3894594, perft);
                    break;
            }

        }

        private long RunPerft(string fen, int depth)
        {
            GameManager gm = new GameManager(fen);
            return gm.Perft(depth);
        }

    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chess.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Chess.Test
{
    [TestClass()]
    public class BoardTests
    {
        [DataTestMethod]
        [DataRow("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8")]
        [DataRow("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1")]
        [DataRow("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
        [DataRow("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ")]
        public void MoveUnmoveTest(string fen)
        {
            byte[] originalBoard = new byte[64];
            List<ushort> originalPieceList = new List<ushort>();

            Management.GameManager gm = new Management.GameManager(fen);

            Enums.Colors ColorToMove = gm.Board.ColorToMove;
            byte CastleMask = gm.Board.CastleMask; //White Short - White Long - Black Short - Black Long
            Enums.Squares EnPassantTarget = gm.Board.EnPassantTarget;
            bool InCheck = gm.Board.InCheck;
            bool CheckMate = gm.Board.CheckMate;
            byte[] KingSquares = gm.Board.KingSquares;

            for (int i = 0; i < 64; i++)
            {
                originalBoard[i] = gm.Board.GameBoard[i];
            }
            originalPieceList = gm.Board.PieceList.ConvertAll(x => x);

            gm.Perft(4);

            for (int i = 0; i < 64; i++)
            {
                if (gm.Board.GameBoard[i] != originalBoard[i]) Assert.Fail("Game board is mismatched after play/unplay at " + i);
            }
            if (originalPieceList.Count() != gm.Board.PieceList.Count()) Assert.Fail("Piecelist has extra/missing pieces");
            foreach (ushort og in originalPieceList)
            {
                if (!gm.Board.PieceList.Contains(og)) Assert.Fail("Piecelists are not identical");
            }
            if (ColorToMove != gm.Board.ColorToMove) Console.WriteLine("Color to play desynced!");
            if (CastleMask != gm.Board.CastleMask) Console.WriteLine("Castle mask desynced!"); //White Short - White Long - Black Short - Black Long
            if (EnPassantTarget != gm.Board.EnPassantTarget) Console.WriteLine("EP target desynced!");
            if (InCheck != gm.Board.InCheck) Console.WriteLine("In check desynced!");
            if (CheckMate != gm.Board.CheckMate) Console.WriteLine("Checkmate desynced!");
            if (KingSquares != gm.Board.KingSquares) Console.WriteLine("King squares desynced!");
        }

        [DataTestMethod]
        [DataRow("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8")]
        [DataRow("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1")]
        [DataRow("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
        [DataRow("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ")]
        public void ZobristConsistencyTest(string fen)
        {
            byte[] originalBoard = new byte[64];
            List<ushort> originalPieceList = new List<ushort>();

            Management.GameManager gm = new Management.GameManager(fen);

            ulong startingZhash = gm.Board.ZobristHash;

            for (int i = 0; i < 64; i++)
            {
                originalBoard[i] = gm.Board.GameBoard[i];
            }
            originalPieceList = gm.Board.PieceList.ConvertAll(x => x);

            gm.Perft(3);

            Assert.AreEqual(startingZhash, gm.Board.ZobristHash);
        }

        [TestMethod()]
        [DataRow("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8")]
        [DataRow("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1")]
        [DataRow("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
        [DataRow("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 7")]
        [DataRow("rnbqkbnr/pppp2pp/8/4Pp2/3p4/8/PPP2PPP/RNBQKBNR w KQkq f6 0 4")]
        public void ToFENTest(string fen)
        {
            Board b = new Board(fen);
            Assert.AreEqual(fen, b.ToFEN());
        }
    }
}
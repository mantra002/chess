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
        [TestMethod()]
        public void UndoMoveTestComplex()
        {
            byte[] originalBoard = new byte[64];
            List<ushort> originalPieceList = new List<ushort>();
            Management.GameManager gm = new Management.GameManager("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
            for(int i = 0; i < 64; i++)
            {
                originalBoard[i] = gm.Board.GameBoard[i];
            }
            originalPieceList = gm.Board.PieceList.ConvertAll(x => x);

            gm.Perft(3);

            for (int i = 0; i < 64; i++)
            {
                if (gm.Board.GameBoard[i] != originalBoard[i]) Assert.Fail("Game board is mismatched after play/unplay at " + i);
            }
            if (originalPieceList.Count() != gm.Board.PieceList.Count()) Assert.Fail("Piecelist has extra/missing pieces");
            foreach(ushort og in originalPieceList)
            {
                if (!gm.Board.PieceList.Contains(og)) Assert.Fail("Piecelists are not identical");
            }    
        }
        [TestMethod()]
        public void UndoMoveTestSimple()
        {
            byte[] originalBoard = new byte[64];
            List<ushort> originalPieceList = new List<ushort>();
            Management.GameManager gm = new Management.GameManager("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ");
            for (int i = 0; i < 64; i++)
            {
                originalBoard[i] = gm.Board.GameBoard[i];
            }
            originalPieceList = gm.Board.PieceList.ConvertAll(x => x);

            gm.Perft(3);

            for (int i = 0; i < 64; i++)
            {
                if (gm.Board.GameBoard[i] != originalBoard[i]) Assert.Fail("Game board is mismatched after play/unplay at " + i);
            }
            if (originalPieceList.Count() != gm.Board.PieceList.Count()) Assert.Fail("Piecelist has extra/missing pieces");
            foreach (ushort og in originalPieceList)
            {
                if (!gm.Board.PieceList.Contains(og)) Assert.Fail("Piecelists are not identical");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_app
{
    public class Move
    {
        public byte Piece { get; set; }
        public int PieceListIndex { get; set; }
        public byte PieceCaptured { get; set; }
        public byte Origin { get; set; }
        public byte Destination { get; set; }

        public byte PromoteIntoPiece { get; set; }

        public Enums.CastellingTypes CastleType { get; set; }
        public Enums.CastellingTypes BreaksCastle { get; set; }
        public Enums.Colors SideToMove { get; set; }
        public bool AllowsEnPassant { get; set; }

        public Move(Enums.Colors sideToPlay, byte piece, byte origin, byte destination, int pieceListIndex, byte pieceCaptured = 0, Enums.CastellingTypes castle = Enums.CastellingTypes.None, bool allowEnPassant = false, byte promoteIntoPiece = 0)
        {
            Piece = piece;
            Origin = origin;
            Destination = destination;
            SideToMove = sideToPlay;

            CastleType = castle;
            PieceCaptured = pieceCaptured;
            PromoteIntoPiece = promoteIntoPiece;

            AllowsEnPassant = allowEnPassant;
            PieceListIndex = pieceListIndex;
        }

        public override string ToString()
        {
            return Board.BoardIndexToString(this.Origin) + Board.BoardIndexToString(this.Destination);
        }
    }
}

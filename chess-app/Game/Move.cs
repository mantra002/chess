using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Game
{
    public class Move
    {
        public byte Piece { get; set; }
        public int PieceListIndex { get; set; }
        public byte PieceCaptured { get; set; }
        public byte Origin { get; set; }
        public byte Destination { get; set; }

        public byte PromoteIntoPiece { get; set; }

        public Enums.CastleFlags CastleFlags { get; set; }
        public Enums.Colors SideToMove { get; set; }
        public bool AllowsEnPassant { get; set; }

        public Move(Enums.Colors sideToPlay, byte piece, byte origin, byte destination, int pieceListIndex, byte pieceCaptured = 0, Enums.CastleFlags castleFlag = Enums.CastleFlags.None, bool allowEnPassant = false, byte promoteIntoPiece = 0)
        {
            Piece = piece;
            Origin = origin;
            Destination = destination;
            SideToMove = sideToPlay;

            CastleFlags = castleFlag;
            PieceCaptured = pieceCaptured;
            PromoteIntoPiece = promoteIntoPiece;

            AllowsEnPassant = allowEnPassant;
            PieceListIndex = pieceListIndex;
        }

        public Move(string move, Board b)
        {
            (Origin, Destination) = GetSquaresFromString(move);
            
            Piece = b.GameBoard[Origin];
            SideToMove = b.ColorToMove;
            CastleFlags = Enums.CastleFlags.None;
            if(SideToMove == Enums.Colors.White)
            {
                if (Origin == 255)
                {
                    CastleFlags = Enums.CastleFlags.WhiteShortCastle;
                }
                else if (Origin == 254)
                {
                    CastleFlags = Enums.CastleFlags.WhiteLongCastle;
                }
            }
            else
            {
                if (Origin == 255)
                {
                    CastleFlags = Enums.CastleFlags.BlackShortCastle;
                }
                else if (Origin == 254)
                {
                    CastleFlags = Enums.CastleFlags.BlackLongCastle;
                }
            }
  
            PieceCaptured = b.GameBoard[Destination];
            PromoteIntoPiece = 0; //not implemented
            AllowsEnPassant = false; //not implemented
            PieceListIndex = b.PieceList.IndexOf(Board.EncodePieceForPieceList(PieceCaptured, Destination));
        }
        public static (byte, byte) GetSquaresFromString(string move)
        {
            if (move == "O-O")
            {
                return (255, 255);
            }
            if (move == "O-O-O")
            {
                return (254, 254);
            }
            string origin = move.Substring(0, 2);
            string destination = move.Substring(2, 2);
            Enums.Squares originS, destinationS;
            Enum.TryParse(origin, out originS);
            Enum.TryParse(destination, out destinationS);

            return ((byte)originS, (byte)destinationS);
        }
        public override string ToString()
        {
            if (CastleFlags == Enums.CastleFlags.None) return Board.BoardIndexToString(this.Origin) + Board.BoardIndexToString(this.Destination);
            else if (CastleFlags == Enums.CastleFlags.BlackShortCastle || CastleFlags == Enums.CastleFlags.WhiteShortCastle) return "O-O";
            else return "O-O-O";
        }
    }
}

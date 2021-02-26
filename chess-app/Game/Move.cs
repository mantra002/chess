using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Game
{
    using static Enums;
    public class Move
    {
        public byte Piece { get; set; }
        public int PieceListIndex { get; set; }
        public byte PieceCaptured { get; set; }
        public byte Origin { get; set; }
        public byte Destination { get; set; }
        public bool CaptureEnPassant { get; set; }
        public byte PromoteIntoPiece { get; set; }

        public CastleFlags CastleFlags { get; set; }
        public Colors SideToMove { get; set; }
        public Squares AllowsEnPassantTarget { get; set; }

        public Move(Colors sideToPlay, byte piece, byte origin, byte destination, int pieceListIndex=-1, byte pieceCaptured = 0, CastleFlags castleFlag = CastleFlags.None, Squares allowsEnPassantTarget = Squares.None, byte promoteIntoPiece = 0)
        {
            Piece = piece;
            Origin = origin;
            Destination = destination;
            SideToMove = sideToPlay;

            CastleFlags = castleFlag;
            PieceCaptured = pieceCaptured;
            PromoteIntoPiece = promoteIntoPiece;

            AllowsEnPassantTarget = allowsEnPassantTarget;
            PieceListIndex = pieceListIndex;
            CaptureEnPassant = false;
        }

        public Move(string move, Board b)
        {
            (Origin, Destination) = GetSquaresFromString(move);
            Piece = b.GameBoard[Origin];

            
            SideToMove = b.ColorToMove;
            if (Destination == (byte)b.EnPassantTarget)
            {
                PieceListIndex = b.PieceList.IndexOf(Board.EncodePieceForPieceList(PieceCaptured, Destination));
                CaptureEnPassant = true;
                if (SideToMove == Colors.White)
                {
                    PieceCaptured = b.GameBoard[Destination + 8];
                }
                else PieceCaptured = b.GameBoard[Destination - 8];
            }
            else
            {
                PieceListIndex = b.PieceList.IndexOf(Board.EncodePieceForPieceList(PieceCaptured, Destination));
                CaptureEnPassant = false;
                PieceCaptured = b.GameBoard[Destination];
            }
            CastleFlags = CastleFlags.None;
            if(SideToMove == Colors.White)
            {
                if (Origin == 255)
                {
                    CastleFlags = CastleFlags.WhiteShortCastle;
                }
                else if (Origin == 254)
                {
                    CastleFlags = CastleFlags.WhiteLongCastle;
                }
            }
            else
            {
                if (Origin == 255)
                {
                    CastleFlags = CastleFlags.BlackShortCastle;
                }
                else if (Origin == 254)
                {
                    CastleFlags = CastleFlags.BlackLongCastle;
                }
            }
  
            
            PromoteIntoPiece = 0; //not implemented
            if((Piece & (byte) PieceNames.Pawn) == (byte)PieceNames.Pawn)
            {
                if(Origin - Destination == 16)
                {
                    AllowsEnPassantTarget = (Squares)(Origin-8); 
                }
                else if(Origin - Destination == -16)
                {
                    AllowsEnPassantTarget = (Squares)(Origin + 8);
                }
                else
                {
                    AllowsEnPassantTarget = Squares.None;
                }

            }
            

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
            Squares originS, destinationS;
            Enum.TryParse(origin, out originS);
            Enum.TryParse(destination, out destinationS);

            return ((byte)originS, (byte)destinationS);
        }
        public override string ToString()
        {
            if (CastleFlags == CastleFlags.None) return Board.BoardIndexToString(this.Origin) + Board.BoardIndexToString(this.Destination);
            else if (CastleFlags == CastleFlags.BlackShortCastle || CastleFlags == CastleFlags.WhiteShortCastle) return "O-O";
            else return "O-O-O";
        }
    }
}

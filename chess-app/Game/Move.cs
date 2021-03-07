using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Game
{
    using static Enums;
    public class Move : IComparable<Move>
    {
        public byte Piece;
        public int PieceListIndex;
        public byte PieceCaptured;
        public byte Origin;
        public byte Destination;
        public bool CaptureEnPassant;
        public byte PromoteIntoPiece;
        public int MoveScore = 999999999;
        public int MoveHash
        {
            get
            {
                return (this.Piece << 8 ^ this.PieceCaptured) ^ this.Origin ^ this.Destination ^ ((byte)this.AllowsEnPassantTarget << 12) ^ ((byte)this.CastleFlags << 12);
            }
        }

        public CastleFlags CastleFlags;
        public Colors SideToMove;
        public Squares AllowsEnPassantTarget;

        public Move(Colors sideToPlay, byte piece, byte origin, byte destination, int pieceListIndex = -1, byte pieceCaptured = 0, CastleFlags castleFlag = CastleFlags.None, Squares allowsEnPassantTarget = Squares.None, byte promoteIntoPiece = 0)
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

        public int SortByNameAscending(string name1, string name2)
        {

            return name1.CompareTo(name2);
        }

        // Default comparer for Part type.
        public int CompareTo(Move compareMove)
        {
            // A null value means that this object is greater.
            if (compareMove == null)
                return 1;

            else if (compareMove.MoveScore == 999999999) //If no score set 
                return this.ToString().CompareTo(compareMove.ToString());

            else return this.MoveScore.CompareTo(compareMove.MoveScore);
        }
        public Move(string move, Board b)
        {
            move = move.Trim();
            SideToMove = b.ColorToMove;
            (Origin, Destination) = GetSquaresFromString(move, b);
            CastleFlags = CastleFlags.None;
            if (SideToMove == Colors.White)
            {

                if (Origin == 255)
                {
                    CastleFlags = CastleFlags.WhiteShortCastle;
                    Origin = (byte)Squares.e1;
                    Destination = (byte)Squares.g1;
                }
                else if (Origin == 254)
                {
                    CastleFlags = CastleFlags.WhiteLongCastle;
                    Origin = (byte)Squares.e1;
                    Destination = (byte)Squares.c1;
                }
            }
            else
            {
                if (Origin == 255)
                {
                    CastleFlags = CastleFlags.BlackShortCastle;
                    Origin = (byte)Squares.e8;
                    Destination = (byte)Squares.g8;
                }
                else if (Origin == 254)
                {
                    CastleFlags = CastleFlags.BlackLongCastle;
                    Origin = (byte)Squares.e8;
                    Destination = (byte)Squares.c8;
                }
            }
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
            

            if (move.Length == 5)
            {
                switch (move[4])
                {
                    case 'b':
                        PromoteIntoPiece = (byte)((byte)PieceNames.Bishop | (byte)SideToMove);
                        break;
                    case 'n':
                        PromoteIntoPiece = (byte)((byte)PieceNames.Knight | (byte)SideToMove);
                        break;
                    case 'q':
                        PromoteIntoPiece = (byte)((byte)PieceNames.Queen | (byte)SideToMove);
                        break;
                    case 'r':
                        PromoteIntoPiece = (byte)((byte)PieceNames.Rook | (byte)SideToMove);
                        break;
                }
            }

            if ((Piece & (byte)PieceNames.Pawn) == (byte)PieceNames.Pawn)
            {
                if (Origin - Destination == 16)
                {
                    AllowsEnPassantTarget = (Squares)(Origin - 8);
                }
                else if (Origin - Destination == -16)
                {
                    AllowsEnPassantTarget = (Squares)(Origin + 8);
                }
                else
                {
                    AllowsEnPassantTarget = Squares.None;
                }

            }


        }
        public static (byte, byte) GetSquaresFromString(string move, Board b)
        {

            string origin = move.Substring(0, 2);
            string destination = move.Substring(2, 2);
            Squares originS, destinationS;
            Enum.TryParse(origin, out originS);
            Enum.TryParse(destination, out destinationS);

            if ((b.GameBoard[(byte)originS] & (byte)PieceNames.King) != 0)
            {
                if (b.ColorToMove == Colors.White)
                {
                    if (move == "e1g1")
                    {
                        return (255, 255);
                    }
                    if (move == "e1c1")
                    {
                        return (254, 254);
                    }
                }
                else
                {
                    if (move == "e8g8")
                    {
                        return (255, 255);
                    }
                    if (move == "e8c8")
                    {
                        return (254, 254);
                    }
                }
            }
            return ((byte)originS, (byte)destinationS);
        }
        public override string ToString()
        {
            if (CastleFlags == CastleFlags.None)
            {
                if (this.PromoteIntoPiece == 0) return Board.BoardIndexToString(this.Origin) + Board.BoardIndexToString(this.Destination);
                else return Board.BoardIndexToString(this.Origin) + Board.BoardIndexToString(this.Destination) + Pieces.DecodePieceToChar(this.PromoteIntoPiece);
            }
            else if (CastleFlags == CastleFlags.WhiteShortCastle) return "e1g1";
            else if (CastleFlags == CastleFlags.BlackShortCastle) return "e8g8";
            else if (CastleFlags == CastleFlags.BlackLongCastle) return "e8c8";
            else return "e1c1";
        }
    }
}

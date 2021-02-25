using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Game
{
    using static Enums;
    public class Board
    {

        public byte[] GameBoard = new byte[64];
        public Colors ColorToMove = Colors.White;
        public byte CastleMask = 0b1111; //White Short - White Long - Black Short - Black Long
        public ushort EnPassantOkMask = 0;
        public bool InCheck = false;
        public bool CheckMate = false;
        public List<ushort> PieceList = new List<ushort>(); //Forammatted as 0bLLLLLLLLPPPPPPCC L = Location; P = Piece; C = Color
        public Stack<Move> GameHistory = new Stack<Move>();
        public List<ushort>[][] AttackedSquares = new List<ushort>[2][];
        public byte[] KingSquares = new byte[2];
        private byte castleMaskAtCastleWhite;
        private byte castleMaskAtCastleBlack;
        public short Ply = 0;
       

        public Board(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            NewBoard(fen);
        }

        public void AddPiece(byte piece, byte location)
        {
            GameBoard[location] = piece;
            if(piece != 0) PieceList.Add(EncodePieceForPieceList(piece, location));
            if ((byte)piece == ((byte)PieceNames.King | (byte)Colors.White)) KingSquares[1] = location;
            if ((byte)piece == ((byte)PieceNames.King | (byte)Colors.Black)) KingSquares[0] = location;
        }
        public void RemovePiece(byte piece, byte location, int index = -1)
        {
            //if(index != -1) PieceList.RemoveAt(index);
            //else 
            if(piece != 0) PieceList.Remove(EncodePieceForPieceList(piece, location));
            GameBoard[location] = 0;
        }

        public void PlayMove(Move move)
        {
            Ply++;
            if (move.CastleFlags == CastleFlags.None)
            {
                if((byte)move.Piece == ((byte)PieceNames.King | (byte)Colors.White))
                {
                    CastleMask = (byte)(CastleMask & 0b0011);
                }
                else if ((byte)move.Piece == ((byte)PieceNames.Rook | (byte)Colors.White))
                {
                    if(move.Origin == (byte) Squares.a1)
                    {
                        CastleMask = (byte)(CastleMask & 0b0111);
                    }
                    else if(move.Origin == (byte)Squares.h1)
                    {
                        CastleMask = (byte)(CastleMask & 0b1011);
                    }

                }
                else if ((byte)move.Piece == ((byte)PieceNames.King | (byte)Colors.Black))
                {
                    CastleMask = (byte)(CastleMask & 0b1100);
                }
                else if ((byte)move.Piece == ((byte)PieceNames.Rook | (byte)Colors.Black))
                {
                    if (move.Origin == (byte)Squares.a8)
                    {
                        CastleMask = (byte)(CastleMask & 0b0010);
                    }
                    else if (move.Origin == (byte)Squares.h8)
                    {
                        CastleMask = (byte)(CastleMask & 0b0001);
                    }
                }
                if (move.PromoteIntoPiece != 0)
                {
                    //Need to handle this
                }
                else
                {
                    RemovePiece(move.Piece, move.Origin, move.PieceListIndex);
                    AddPiece(move.Piece, move.Destination);
                }
                if (move.PieceCaptured != 0)
                {
                    PieceList.Remove(EncodePieceForPieceList(move.PieceCaptured, move.Destination));
                }
              
            }
            else if(move.SideToMove==Colors.White)
            {
                RemovePiece((byte)PieceNames.King | (byte)Colors.White, (byte)Squares.e1);

                castleMaskAtCastleWhite = (byte)(CastleMask & 0b1100);
                CastleMask = (byte)(CastleMask & 0b0011);

                if (move.CastleFlags == CastleFlags.WhiteShortCastle)
                {
                    RemovePiece((byte)PieceNames.Rook | (byte)Colors.White, (byte)Squares.h1);
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.White), (byte) Squares.g1);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.White), (byte)Squares.f1);
                }
                else if (move.CastleFlags == CastleFlags.WhiteLongCastle)
                {
                    RemovePiece((byte)PieceNames.Rook | (byte)Colors.White, (byte)Squares.a1);
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.White), (byte)Squares.c1);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.White), (byte)Squares.d1);
                }
            }
            else 
            {
                castleMaskAtCastleBlack = (byte)(CastleMask & 0b0011);

                CastleMask = (byte)(CastleMask & 0b1100);
                RemovePiece((byte)PieceNames.King | (byte)Colors.Black, (byte)Squares.e8);
                
                if (move.CastleFlags == CastleFlags.BlackShortCastle)
                {
                    RemovePiece((byte)PieceNames.Rook | (byte)Colors.Black, (byte)Squares.h8);
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.Black), (byte)Squares.g8);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.Black), (byte)Squares.f8);
                }
                else if (move.CastleFlags == CastleFlags.BlackLongCastle)
                {
                    RemovePiece((byte)PieceNames.Rook | (byte)Colors.Black, (byte)Squares.a8);
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.Black), (byte)Squares.c8);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.Black), (byte)Squares.d8);
                }
            }
            //AttackedSquares[(byte)ColorToMove - 1] = MoveGeneration.GenerateAttackMap(this);

            if (ColorToMove == Colors.White)
            {
                //InCheck = AttackedSquares[0][KingSquares[1]];
                ColorToMove = Colors.Black;
            }
            else
            {
                //InCheck = AttackedSquares[1][KingSquares[0]];
                ColorToMove = Colors.White;
            }
            GameHistory.Push(move);
        }

        public void UndoMove(Move move)
        {
            Ply--;
            if (CheckMate) CheckMate = false;
            if (InCheck) InCheck = false;
            if (move.CastleFlags == CastleFlags.None)
            {
                #region "Handle castling rights"
                if ((byte)move.Piece == ((byte)PieceNames.King | (byte)Colors.White))
                {
                    CastleMask = (byte)(CastleMask & 0b0011);
                }
                else if ((byte)move.Piece == ((byte)PieceNames.Rook | (byte)Colors.White))
                {
                    if (move.Origin == (byte)Squares.a1)
                    {
                        CastleMask = (byte)(CastleMask & 0b0111);
                    }
                    else if (move.Origin == (byte)Squares.h1)
                    {
                        CastleMask = (byte)(CastleMask & 0b1011);
                    }

                }
                else if ((byte)move.Piece == ((byte)PieceNames.King | (byte)Colors.Black))
                {
                    CastleMask = (byte)(CastleMask & 0b1100);
                }
                else if ((byte)move.Piece == ((byte)PieceNames.Rook | (byte)Colors.Black))
                {
                    if (move.Origin == (byte)Squares.a8)
                    {
                        CastleMask = (byte)(CastleMask & 0b0010);
                    }
                    else if (move.Origin == (byte)Squares.h8)
                    {
                        CastleMask = (byte)(CastleMask & 0b0001);
                    }
                }
                #endregion
                RemovePiece(move.Piece, move.Destination); //Remove the piece that's going
                AddPiece(move.PieceCaptured, move.Destination);
                if (move.PromoteIntoPiece != 0)
                {
                    //Need to implement promotion
                }
                else
                {
                    AddPiece(move.Piece, move.Origin);
                }

            }
            else if (move.SideToMove == Colors.White)
            {
                CastleMask = (byte)(CastleMask | castleMaskAtCastleWhite);
                if (move.CastleFlags == CastleFlags.WhiteShortCastle)
                {
                    RemovePiece((byte)PieceNames.King | (byte)Colors.White, (byte)Squares.g1);
                    RemovePiece((byte)PieceNames.Rook | (byte)Colors.White, (byte)Squares.f1);
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.White), (byte)Squares.e1);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.White), (byte)Squares.h1);
                }
                else if (move.CastleFlags == CastleFlags.WhiteLongCastle)
                {
                    RemovePiece((byte)PieceNames.King | (byte)Colors.White, (byte)Squares.c1);
                    RemovePiece((byte)PieceNames.Rook | (byte)Colors.White, (byte)Squares.d1);
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.White), (byte)Squares.e1);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.White), (byte)Squares.a1);
                }
            }
            else
            {
                CastleMask = (byte)(CastleMask | castleMaskAtCastleBlack);
                if (move.CastleFlags == CastleFlags.BlackShortCastle)
                {
                    RemovePiece((byte)PieceNames.King | (byte)Colors.Black, (byte)Squares.g8);
                    RemovePiece((byte)PieceNames.Rook | (byte)Colors.Black, (byte)Squares.f8);
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.Black), (byte)Squares.e8);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.Black), (byte)Squares.h8);
                }
                else if (move.CastleFlags == CastleFlags.BlackLongCastle)
                {
                    RemovePiece((byte)PieceNames.King | (byte)Colors.Black, (byte)Squares.c8);
                    RemovePiece((byte)PieceNames.Rook | (byte)Colors.Black, (byte)Squares.d8);
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.Black), (byte)Squares.e8);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.Black), (byte)Squares.a8);
                }
            }
            this.AttackedSquares[2 - (byte)ColorToMove] = null;

            if (ColorToMove == Colors.White) ColorToMove = Colors.Black;
            else ColorToMove = Colors.White;

            GameHistory.Pop();
        }

        public void LoadFEN(string fen)
        {
            byte boardIndex = 0;
            int index = 0;
            char c;
            bool allPiecesDone = false;
            bool castlingDone = false;


            ClearBoard();
            
            while (!allPiecesDone)
            {
                c = fen[index];
                if (c != '/')
                {
                    if (c == ' ')
                    {
                        allPiecesDone = true;
                    }
                    else if (Char.IsDigit(c))
                    {
                        boardIndex += (byte) (c - '0');
                    }
                    else
                    {
                        AddPiece(Pieces.EncodePieceFromChar(c), boardIndex++);
                    }
                }
                index++;
            }
            if (fen[index] == 'w')
            {
                ColorToMove = Colors.White;
            }
            else
            {
                ColorToMove = Colors.Black;
            }
            index++;
            while (!castlingDone)
            {
                c = fen[index];

                if (c == ' ')
                {
                    castlingDone = true;
                }
                else
                {
                    switch (c)
                    {
                        case 'K':
                            CastleMask = (byte)(CastleMask | 0b1000);
                            break;
                        case 'Q':
                            CastleMask = (byte)(CastleMask | 0b0100);
                            break;
                        case 'k':
                            CastleMask = (byte)(CastleMask | 0b0010);
                            break;
                        case 'q':
                            CastleMask = (byte)(CastleMask | 0b0001);
                            break;
                    }
                }
                
            }
            this.AttackedSquares[0] = MoveGeneration.GenerateAttackMap(this, Colors.Black);
            this.AttackedSquares[1] = MoveGeneration.GenerateAttackMap(this, Colors.White);
        }

        public void NewBoard(string fen)
        {
            LoadFEN(fen);
        }

        public void ClearBoard()
        {
            GameBoard = new byte[64];
            PieceList = new List<ushort>();
            CastleMask = 0;
            EnPassantOkMask = 0;
            castleMaskAtCastleBlack = 0;
            castleMaskAtCastleWhite = 0;
            Ply = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (GameBoard[i * 8 + j] != 0)
                    {
                        sb.Append(Pieces.DecodePieceToChar(GameBoard[i * 8 + j]));
                    }
                    else sb.Append(' ');

                }
                if(i != 7) sb.AppendLine();
            }
            //
            return sb.ToString();
        }

        public static string BoardIndexToString(byte index)
        {
            char rank, file;
            int offset;

            rank = (char)(56 - index / 8);

            offset = index % 8;

            file = (char)(97 + offset);
          
            return String.Concat(file, rank);
        }

        public static int GetRank(byte index)
        {
            return 8-(index / 8);
        }

        public static ushort EncodePieceForPieceList(byte piece, byte location)
        {
            /*
            Console.WriteLine("Piece = " + piece.ToString());
            Console.WriteLine("Location = " + location.ToString());
   
            Console.WriteLine("Location Enc = " + ((ushort)location << 7).ToString());
            Console.WriteLine("Piece Enc = " + ((ushort)(((ushort)location << 7) | piece)).ToString());
            */
            return (ushort)(((ushort)location << 8) | piece);
        }

        public static byte DecodeLocationFromPieceList(ushort plPiece)
        {
            return (byte)(plPiece >> 8);
        }

        public static byte DecodePieceFromPieceList(ushort plPiece)
        {
            return (byte) (plPiece & 0b000000011111111);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_app
{
    using static Enums;
    public class Board
    {

        public byte[] GameBoard = new byte[64];
        public Colors ColorToMove = Colors.White;
        public byte CastleMask = 0b1111; //White Short - White Long - Black Short - Black Long
        public ushort EnPassantOkMask = 0;
        public List<ushort> PieceList = new List<ushort>(); //Forammatted as 0bLLLLLLLLPPPPPPCC L = Location; P = Piece; C = Color
       

        public Board()
        {
            NewBoard();
        }

        public void AddPiece(byte piece, byte location)
        {
            GameBoard[location] = piece;
            PieceList.Add(EncodePieceForPieceList(piece, location));
        }

        public void PlayMove(Move move)
        {
            if (move.CastleType == CastellingTypes.None)
            {
                GameBoard[move.Origin] = 0;
                if (move.PromoteIntoPiece != 0)
                {
                    GameBoard[move.Destination] = move.PromoteIntoPiece;
                }
                else
                {
                    GameBoard[move.Destination] = move.Piece;
                    PieceList.RemoveAt(move.PieceListIndex);
                    AddPiece(move.Piece, move.Destination);
                }
                if (move.PieceCaptured != 0)
                {
                    PieceList.Remove(EncodePieceForPieceList(move.PieceCaptured, move.Destination));
                }
              
            }
            else if(move.SideToMove==Colors.White)
            {
                PieceList.Remove(EncodePieceForPieceList((byte)PieceNames.King | (byte)Colors.White, (byte)Squares.e1));
                CastleMask = (byte)(CastleMask & 0b0011);

                if (move.CastleType == CastellingTypes.ShortCastle)
                {
                    
                    PieceList.Remove(EncodePieceForPieceList((byte)PieceNames.Rook | (byte)Colors.White, (byte)Squares.h1));
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.White), (byte) Squares.g1);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.White), (byte)Squares.f1);
                }
                else
                {
                    PieceList.Remove(EncodePieceForPieceList((byte)PieceNames.Rook | (byte)Colors.White, (byte)Squares.a1));
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.White), (byte)Squares.c1);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.White), (byte)Squares.d1);
                }
            }
            else 
            {
                CastleMask = (byte)(CastleMask & 0b1100);
                PieceList.Remove((ushort)(((byte)Squares.e8 << 7) & ((byte)PieceNames.King | (byte)Colors.Black)));
                if (move.CastleType == CastellingTypes.ShortCastle)
                {
                    PieceList.Remove(EncodePieceForPieceList((byte)PieceNames.Rook | (byte)Colors.Black, (byte)Squares.h8));
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.Black), (byte)Squares.g8);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.Black), (byte)Squares.f8);
                }
                else
                {
                    PieceList.Remove(EncodePieceForPieceList((byte)PieceNames.Rook | (byte)Colors.Black, (byte)Squares.a8));
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.Black), (byte)Squares.c8);
                    AddPiece(Pieces.EncodePiece(PieceNames.Rook, Colors.Black), (byte)Squares.d8);
                }
            }
            if (ColorToMove == Colors.White) ColorToMove = Colors.Black;
            else ColorToMove = Colors.White;
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
        }

        public void NewBoard()
        {
            LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            CastleMask = 0b1111; //White Short - White Long - Black Short - Black Long
            EnPassantOkMask = 0;
        }

        public void ClearBoard()
        {
            GameBoard = new byte[64];
            PieceList = new List<ushort>();
            CastleMask = 0;
            EnPassantOkMask = 0;
        }

        public void PrintBoard()
        {
            for(int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (GameBoard[i * 8 + j] != 0)
                    {
                        Console.Write(Pieces.DecodePieceToChar(GameBoard[i * 8 + j]));
                    }
                    else Console.Write(' ');

                }
                Console.WriteLine();
            }
            Console.WriteLine(ColorToMove.ToString() + " to play");
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

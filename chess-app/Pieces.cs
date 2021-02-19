using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_app
{
    static public class Pieces
    {
        

        public struct Piece 
        {
            public Enums.Colors Color;
            public Enums.PieceNames Name;
        }

        static public byte EncodePiece(Enums.PieceNames n, Enums.Colors c)
        {
            return (byte)((byte)n | (byte)c);
        }

        static public Piece DecodePiece(byte p)
        {
            Piece np = new Piece();
            if((p & (byte)Enums.Colors.White) == (byte)Enums.Colors.White)
            {
                np.Color = Enums.Colors.White;
            }
            else
            {
                np.Color = Enums.Colors.Black;
            }
            foreach (Enums.PieceNames b in Enum.GetValues(typeof(Enums.PieceNames)))
            {
                if ((p & (byte) b) == (byte) b) np.Name = b;
            }

            return np;
        }

        static public char DecodePieceToChar(Piece p)
        {
            char c;
            if (p.Color == Enums.Colors.White)
            {
                if (p.Name == Enums.PieceNames.Pawn)
                {
                    c = 'P';
                }
                else if (p.Name == Enums.PieceNames.Knight)
                {
                    c = 'N';
                }
                else if (p.Name == Enums.PieceNames.Bishop)
                {
                    c = 'B';
                }
                else if (p.Name == Enums.PieceNames.Rook)
                {
                    c = 'R';
                }
                else if (p.Name == Enums.PieceNames.Queen)
                {
                    c = 'Q';
                }
                else
                {
                    c = 'K';
                }
            }
            else
            {
                if (p.Name == Enums.PieceNames.Pawn)
                {
                    c = 'p';
                }
                else if (p.Name == Enums.PieceNames.Knight)
                {
                    c = 'n';
                }
                else if (p.Name == Enums.PieceNames.Bishop)
                {
                    c = 'b';
                }
                else if (p.Name == Enums.PieceNames.Rook)
                {
                    c = 'r';
                }
                else if (p.Name == Enums.PieceNames.Queen)
                {
                    c = 'q';
                }
                else
                {
                    c = 'k';
                }
            }
            return c;
        }

        static public char DecodePieceToChar(byte p)
        {
            return DecodePieceToChar(DecodePiece(p));
        }

        static public byte EncodePieceFromChar(char c)
        {
            byte b = 0;

            switch (c)
            {
                case 'p':
                    b = (byte)Enums.PieceNames.Pawn | (byte) Enums.Colors.Black;
                    break;
                case 'n':
                    b = (byte)Enums.PieceNames.Knight | (byte)Enums.Colors.Black;
                    break;
                case 'b':
                    b = (byte)Enums.PieceNames.Bishop | (byte)Enums.Colors.Black;
                    break;
                case 'r':
                    b = (byte)Enums.PieceNames.Rook | (byte)Enums.Colors.Black;
                    break;
                case 'q':
                    b = (byte)Enums.PieceNames.Queen | (byte)Enums.Colors.Black;
                    break;
                case 'k':
                    b = (byte)Enums.PieceNames.King | (byte)Enums.Colors.Black;
                    break;

                case 'P':
                    b = (byte)Enums.PieceNames.Pawn | (byte)Enums.Colors.White;
                    break;
                case 'N':
                    b = (byte)Enums.PieceNames.Knight | (byte)Enums.Colors.White;
                    break;
                case 'B':
                    b = (byte)Enums.PieceNames.Bishop | (byte)Enums.Colors.White;
                    break;
                case 'R':
                    b = (byte)Enums.PieceNames.Rook | (byte)Enums.Colors.White;
                    break;
                case 'Q':
                    b = (byte)Enums.PieceNames.Queen | (byte)Enums.Colors.White;
                    break;
                case 'K':
                    b = (byte)Enums.PieceNames.King | (byte)Enums.Colors.White;
                    break;

            }

            return b;
        }
    }
}

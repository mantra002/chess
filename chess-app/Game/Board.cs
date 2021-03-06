using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Game
{
    using static Enums;
    public class Board : ICloneable
    {

        public byte[] GameBoard = new byte[64];
        public Colors ColorToMove = Colors.White;
        public byte CastleMask = 0b0000; //White Short - White Long - Black Short - Black Long
        public Squares EnPassantTarget = Squares.None;
        private Squares EnPassantTargetTimeZero = Squares.None;
        public bool InCheck = false;
        public bool CheckMate = false;
        public List<ushort> PieceList = new List<ushort>(); //Formmatted as 0bLLLLLLLLPPPPPPCC L = Location; P = Piece; C = Color
        public Stack<GameState> GameHistory = new Stack<GameState>();
        public List<ushort>[][] AttackedSquares = new List<ushort>[2][];
        public List<ushort>[][] AttackedSquaresWithoutPins = new List<ushort>[2][];
        public byte[] KingSquares = new byte[2];
        public ulong ZobristHash;

        public short MoveCounter = 0;
        public byte FiftyMoveCounter = 0; //In Ply

        public struct GameState
        {
            public List<ushort>[][] AttackedSquares;
            public List<ushort>[][] AttackedSquaresWithoutPins;
            public Move PlayedMove;
            public byte CastleMask;

            public GameState(List<ushort>[][] atksq, List<ushort>[][] aswp, Move m, byte cm)
            {
                this.AttackedSquares = atksq;
                this.AttackedSquaresWithoutPins = aswp;
                this.PlayedMove = m;
                this.CastleMask = cm;
            }
        }


        public Board(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            NewBoard(fen);
        }

        public void AddPiece(byte piece, byte location)
        {
            GameBoard[location] = piece;
            if (piece != 0) PieceList.Add(EncodePieceForPieceList(piece, location));
            ZobristPiece(piece, location);
        }
        public void RemovePiece(byte piece, byte location, int index = -1)
        {
            //This method doesn't work because the piece list is being changed with every remove or add.
            //if(index != -1) PieceList.RemoveAt(index);
            //else 
            if (piece != 0) PieceList.Remove(EncodePieceForPieceList(piece, location));
            GameBoard[location] = 0;
            ZobristPiece(piece, location);
        }

        private void ZobristPiece(byte piece, byte location)
        {
            if (((byte)piece & (byte)Colors.White) != 0)
            {
                if (((byte)piece & (byte)PieceNames.Pawn) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][1][0];
                }
                else if (((byte)piece & (byte)PieceNames.Knight) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][1][1];
                }
                else if (((byte)piece & (byte)PieceNames.Bishop) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][1][2];
                }
                else if (((byte)piece & (byte)PieceNames.Rook) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][1][3];
                }
                else if (((byte)piece & (byte)PieceNames.Queen) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][1][4];
                }
                else if (((byte)piece & (byte)PieceNames.King) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][1][5];
                    KingSquares[1] = location;
                }
            }
            else
            {
                if (((byte)piece & (byte)PieceNames.Pawn) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][0][0];
                }
                else if (((byte)piece & (byte)PieceNames.Knight) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][0][1];
                }
                else if (((byte)piece & (byte)PieceNames.Bishop) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][0][2];
                }
                else if (((byte)piece & (byte)PieceNames.Rook) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][0][3];
                }
                else if (((byte)piece & (byte)PieceNames.Queen) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][0][4];
                }
                else if (((byte)piece & (byte)PieceNames.King) != 0)
                {
                    ZobristHash ^= Game.ZobristHash.PieceKeys[location][0][5];
                    KingSquares[0] = location;
                }
            }
        }

        public void PlayMove(Move move)
        {
            if (CheckMate) return;
            GameHistory.Push(new GameState(this.AttackedSquares, this.AttackedSquaresWithoutPins, move, this.CastleMask));

            ZobristHash ^= Game.ZobristHash.CastleKeys[CastleMask];
            if (this.EnPassantTarget != Squares.None) ZobristHash ^= Game.ZobristHash.EpKeys[(byte)this.EnPassantTarget];

            MoveCounter++;

            if (move.CastleFlags == CastleFlags.None)
            {
                if ((byte)move.Piece == ((byte)PieceNames.King | (byte)Colors.White))
                {
                    CastleMask = (byte)(CastleMask & 0b0011); //White Short - White Long - Black Short - Black Long
                }
                else if ((byte)move.Piece == ((byte)PieceNames.Rook | (byte)Colors.White))
                {
                    if (move.Origin == (byte)Squares.a1)
                    {
                        CastleMask = (byte)(CastleMask & 0b1011);
                    }
                    else if (move.Origin == (byte)Squares.h1)
                    {
                        CastleMask = (byte)(CastleMask & 0b0111);
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
                        CastleMask = (byte)(CastleMask & 0b1110);
                    }
                    else if (move.Origin == (byte)Squares.h8)
                    {
                        CastleMask = (byte)(CastleMask & 0b1101);
                    }
                }
#if DEBUG
                if (GameBoard[move.Origin] != move.Piece)
                {
                    throw new Exception("Trying to move the wrong piece!");
                }
#endif
                RemovePiece(move.Piece, move.Origin, move.PieceListIndex); //Remove the moving piece
                if (move.PieceCaptured != 0) //If there is a capture
                {
                    if (!move.CaptureEnPassant)
                    {
#if DEBUG
                        if (GameBoard[move.Destination] != move.PieceCaptured)
                        {
                            throw new Exception("Trying to capture the wrong piece!");
                        }
#endif
                        RemovePiece(move.PieceCaptured, move.Destination); //Remove captured piece
                    }
                    else
                    {
                        if (move.SideToMove == Colors.White) RemovePiece(move.PieceCaptured, (byte)(move.Destination + 8)); //Remove pawn captured by EP
                        else RemovePiece(move.PieceCaptured, (byte)(move.Destination - 8));
                    }
                }
                if (move.PromoteIntoPiece != 0) //If promotion
                {
#if DEBUG
                    if (GameBoard[move.Destination] != 0)
                    {
                        throw new Exception("Trying to promote into an occupied square!");
                    }
#endif
                    AddPiece(move.PromoteIntoPiece, move.Destination); //Add the promoted piece
                }
                else
                {
#if DEBUG
                    if (GameBoard[move.Destination] != 0)
                    {
                        throw new Exception("Trying to move into an occupied square!");
                    }
#endif
                    AddPiece(move.Piece, move.Destination); //Add the moved piece back in.
                }


            }
            else if (move.SideToMove == Colors.White)
            {
                RemovePiece((byte)PieceNames.King | (byte)Colors.White, (byte)Squares.e1);

                CastleMask = (byte)(CastleMask & 0b0011);

                if (move.CastleFlags == CastleFlags.WhiteShortCastle)
                {
                    RemovePiece((byte)PieceNames.Rook | (byte)Colors.White, (byte)Squares.h1);
                    AddPiece(Pieces.EncodePiece(PieceNames.King, Colors.White), (byte)Squares.g1);
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
            ZobristHash ^= Game.ZobristHash.CastleKeys[CastleMask];
            ZobristHash ^= Game.ZobristHash.BlackToPlay;
            EnPassantTarget = move.AllowsEnPassantTarget;
            if (this.EnPassantTarget != Squares.None) ZobristHash ^= Game.ZobristHash.EpKeys[(byte)this.EnPassantTarget];
        }

        public void UndoMove(Move move)
        {
            MoveCounter--;
            if (CheckMate) CheckMate = false;
            if (InCheck) InCheck = false;
            ZobristHash ^= Game.ZobristHash.CastleKeys[CastleMask];
            if (this.EnPassantTarget != Squares.None) ZobristHash ^= Game.ZobristHash.EpKeys[(byte)this.EnPassantTarget];
            if (move.CastleFlags == CastleFlags.None)
            {
                if (move.PromoteIntoPiece != 0) //If promotion
                {
#if DEBUG
                    if (GameBoard[move.Destination] != move.PromoteIntoPiece)
                    {
                        throw new Exception("Removing non existant promotion piece!");
                    }
#endif

                    RemovePiece(move.PromoteIntoPiece, move.Destination); //remove the promoted piece
                }
                else
                {
#if DEBUG
                    if (GameBoard[move.Destination] != move.Piece)
                    {
                        throw new Exception("Removing non existant piece!");
                    }
#endif

                    RemovePiece(move.Piece, move.Destination); //Remove the moved piece
                }


                if (move.PieceCaptured != 0) //If there is a capture
                {
                    if (!move.CaptureEnPassant) AddPiece(move.PieceCaptured, move.Destination); //Add the captured piece
                    else
                    {
                        if (move.SideToMove == Colors.White) AddPiece(move.PieceCaptured, (byte)(move.Destination + 8)); //Add pawn captured by EP
                        else AddPiece(move.PieceCaptured, (byte)(move.Destination - 8));
                    }
                }
#if DEBUG
                if (GameBoard[move.Origin] != 0)
                {
                    throw new Exception("Trying to place piece into an occupied square!");
                }
#endif
                AddPiece(move.Piece, move.Origin); //Add the moving piece back to the origin

            }
            else if (move.SideToMove == Colors.White)
            {
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

            if (ColorToMove == Colors.White)
            {
                ColorToMove = Colors.Black;
            }
            else
            {
                ColorToMove = Colors.White;
            }
            GameState gs = GameHistory.Pop();
            if (MoveCounter == 0)
            {
                EnPassantTarget = EnPassantTargetTimeZero;

            }
            else EnPassantTarget = gs.PlayedMove.AllowsEnPassantTarget;

            CastleMask = gs.CastleMask;

            ZobristHash ^= Game.ZobristHash.CastleKeys[CastleMask];
            ZobristHash ^= Game.ZobristHash.BlackToPlay;
            if (this.EnPassantTarget != Squares.None) ZobristHash ^= Game.ZobristHash.EpKeys[(byte)this.EnPassantTarget];
        }

        private void LoadFEN(string fen)
        {
            byte boardIndex = 0;
            string[] splitFen = fen.Split(' ');

            ClearBoard();
            //Setup pieces
            foreach (char c in splitFen[0].Trim())
            {
                if (c != '/')
                {
                    if (Char.IsDigit(c))
                    {
                        boardIndex += (byte)(c - '0');
                    }
                    else
                    {
                        AddPiece(Pieces.EncodePieceFromChar(c), boardIndex++);
                    }
                }
            }
            //Determine side to move

            //Castling rights
            if (splitFen[2].Trim() != "-")
            {
                foreach (char c in splitFen[2].Trim())
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
            //Enpassant

            if (splitFen[3].Trim() != "-")
            {
                EnPassantTarget = (Squares)Enum.Parse(typeof(Squares), splitFen[3].Trim(), true);
                EnPassantTargetTimeZero = EnPassantTarget;
            }

            //50 Move Timer
            if (splitFen.Length >= 5 && splitFen[4].Trim() != "")
            {
                this.FiftyMoveCounter = byte.Parse(splitFen[4].Trim());
            }

            //Move Counter
            if (splitFen.Length >= 6 && splitFen[5].Trim() != "")
            {
                this.MoveCounter = (short)(short.Parse(splitFen[5].Trim()) * 2);
            }

            (this.AttackedSquares[0], this.AttackedSquaresWithoutPins[0]) = MoveGeneration.GenerateAttackMap(this, Colors.Black);
            (this.AttackedSquares[1], this.AttackedSquaresWithoutPins[1]) = MoveGeneration.GenerateAttackMap(this, Colors.White);
            (this.AttackedSquares[0], this.AttackedSquaresWithoutPins[0]) = MoveGeneration.GenerateAttackMap(this, Colors.Black);

            if (splitFen[1].Trim() == "w")
            {
                ColorToMove = Colors.White;
            }
            else
            {
                MoveCounter++;
                ColorToMove = Colors.Black;
                ZobristHash ^= Game.ZobristHash.BlackToPlay;
            }
            ZobristHash ^= Game.ZobristHash.CastleKeys[this.CastleMask];
            if (this.EnPassantTarget != Enums.Squares.None) ZobristHash ^= Game.ZobristHash.EpKeys[(byte)this.EnPassantTarget];
        }

        public string ToFEN()
        {
            byte piece = 0;
            byte blankSquareCount = 0;
            StringBuilder fen = new StringBuilder();

            //Setup pieces
            for (int rank = 0; rank < 8; rank++)
            {
                blankSquareCount = 0;
                for (int file = 0; file < 8; file++)
                {
                    piece = GameBoard[rank * 8 + file];
                    if (piece != 0)
                    {
                        if (blankSquareCount != 0) fen.Append(blankSquareCount);
                        fen.Append(Pieces.DecodePieceToChar(piece));
                        blankSquareCount = 0;
                    }
                    else blankSquareCount++;
                }
                if (blankSquareCount != 0)
                {
                    fen.Append(blankSquareCount);
                }
                if (rank != 7) fen.Append("/");
            }

            //Determine side to move
            if (ColorToMove == Colors.White)
            {
                fen.Append(" w ");
            }
            else
            {
                fen.Append(" b ");
            }

            //Castling rights
            if (CastleMask != 0)
            {
                if ((CastleMask & 0b1000) != 0) fen.Append("K");
                if ((CastleMask & 0b0100) != 0) fen.Append("Q");
                if ((CastleMask & 0b0010) != 0) fen.Append("k");
                if ((CastleMask & 0b0001) != 0) fen.Append("q");
            }
            else fen.Append("-");

            fen.Append(" ");

            //Enpassant

            if (EnPassantTarget != Squares.None)
            {
                fen.Append(EnPassantTarget.ToString());
            }
            else fen.Append("-");

            fen.Append(" ");

            //50 Move Timer
            fen.Append(FiftyMoveCounter);
            fen.Append(" ");

            //Move Counter
            fen.Append(MoveCounter / 2);

            return fen.ToString();
        }
        public void NewBoard(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            LoadFEN(fen);
        }

        private void ClearBoard()
        {
            GameBoard = new byte[64];
            PieceList = new List<ushort>();
            CastleMask = 0b0000;
            EnPassantTarget = Squares.None;
            MoveCounter = 0;
            GameHistory.Clear();
            InCheck = false;
            CheckMate = false;
            FiftyMoveCounter = 0; //Not functional
            ZobristHash = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (GameBoard[i * 8 + j] != 0)
                    {
                        sb.Append(Pieces.DecodePieceToChar(GameBoard[i * 8 + j]));
                    }
                    else sb.Append(' ');

                }
                if (i != 7) sb.AppendLine();
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

        public static byte GetRank(byte index)
        {
            return (byte)(8 - (index / 8));
        }

        public static byte GetFile(byte index)
        {
            return (byte)(index % 8 + 1);
        }

        public static ushort EncodePieceForPieceList(byte piece, byte location)
        {
            return (ushort)(((ushort)location << 8) | piece);
        }

        public static byte DecodeLocationFromPieceList(ushort plPiece)
        {
            return (byte)(plPiece >> 8);
        }

        public static byte DecodePieceFromPieceList(ushort plPiece)
        {
            return (byte)(plPiece & 0b000000011111111);
        }

        public object Clone()
        {
            Board temp = new Board(this.ToFEN());
            return temp;
        }
    }
}

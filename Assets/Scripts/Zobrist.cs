using Unity.Mathematics;

namespace Chess
{
    public struct Zobrist
    {
        public readonly ulong Key => key;

        private readonly ulong key;

        public static void Initialize(ref Board board)
        {
            var random = new Random(29426028);
            var startIndex = new Piece(Figure.Pawn, Color.White).Index;
            var endIndex = new Piece(Figure.King, Color.Black).Index;

            for (var square = Square.Min.Index; square <= Square.Max.Index; square++)
            {
                for (var piece = startIndex; piece <= endIndex; piece++)
                {
                    board.ZobristPiecesArray[piece * Board.Area + square] = RandomULong(random);
                }
            }

            for (var i = 0; i < board.ZobristCastlingRights.Length; i++)
            {
                board.ZobristCastlingRights[i] = RandomULong(random);
            }

            for (var i = 0; i < board.ZobristEnPassantFile.Length; i++)
            {
                board.ZobristEnPassantFile[i] = RandomULong(random);
            }

            board.ZobristSideToMove = RandomULong(random);
        }

        private static ulong RandomULong(in Random random)
        {
            var value = random.NextUInt2();
            var result = 0ul;
            result |= value.x << sizeof(uint);
            result |= value.y;
            return result;
        }

        public Zobrist(in Board board)
        {
            key = 0;

            for (var square = Square.Min.Index; square <= Square.Max.Index; square++)
            {
                var piece = board[new Square(square)];

                if (!piece.IsEmpty)
                {
                    key ^= board.ZobristPiecesArray[piece.Index * Board.Area + square];
                }

                key ^= board.ZobristEnPassantFile[board.State.EnPassantFile];

                if (board.AlliedColor == Color.Black)
                {
                    key ^= board.ZobristSideToMove;
                }

                key ^= board.ZobristCastlingRights[board.State.CastlingRights];
            }
        }
    }
}

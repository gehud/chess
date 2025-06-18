using Unity.Collections;
using Unity.Mathematics;

namespace Chess
{
    public struct Evaluation
    {
        public const int Pawn = 100;
        public const int Knight = 300;
        public const int Bishop = 320;
        public const int Rook = 500;
        public const int Queen = 900;
        public const float Endgame = Rook * 2 + Bishop + Knight;

        public static int Evaluate(in Board board, in PieceSquareTables pieceSquareTables)
        {
            var whiteEvaluation = 0;
            var blackEvaluation = 0;

            var whiteMaterial = CountMaterial(board, Color.White);
            var blackMaterial = CountMaterial(board, Color.Black);

            var whiteMaterialWithoutPawns = whiteMaterial - board.Pawns[(int)Color.White].Length * Pawn;
            var blackMaterialWithoutPawns = blackMaterial - board.Pawns[(int)Color.Black].Length * Pawn;

            var whiteEndgamePhaseWeight = EndgamePhaseWeight(whiteMaterialWithoutPawns);
            var blackEndgamePhaseWeight = EndgamePhaseWeight(blackMaterialWithoutPawns);

            whiteEvaluation += whiteMaterial;
            blackEvaluation += blackMaterial;
            whiteEvaluation += MopUpEvaluation(board, Color.White, Color.Black, whiteMaterial, blackMaterial, blackEndgamePhaseWeight);
            blackEvaluation += MopUpEvaluation(board, Color.Black, Color.White, blackMaterial, whiteMaterial, whiteEndgamePhaseWeight);

            whiteEvaluation += EvaluatePieceSquareTables(board, pieceSquareTables, Color.White, blackEndgamePhaseWeight);
            blackEvaluation += EvaluatePieceSquareTables(board, pieceSquareTables, Color.Black, whiteEndgamePhaseWeight);

            var evaluation = whiteEvaluation - blackEvaluation;

            var perspective = board.IsWhiteAllied ? 1 : -1;
            return evaluation * perspective;
        }

        public static int GetFigureScore(Figure figure)
        {
            return figure switch
            {
                Figure.Pawn => Pawn,
                Figure.Knight => Knight,
                Figure.Bishop => Bishop,
                Figure.Rook => Rook,
                Figure.Queen => Queen,
                _ => 0,
            };
        }

        private static float EndgamePhaseWeight(int materialWithoutPawns)
        {
            const float multiplier = 1f / Endgame;
            return 1f - math.min(1f, materialWithoutPawns * multiplier);
        }

        private static int MopUpEvaluation(in Board board, Color alliedColor, Color enemyColor, int alliedMaterial, int enemyMaterial, float endgameWeight)
        {
            var mopUpScore = 0;

            if (alliedMaterial > enemyMaterial + Pawn * 2 && endgameWeight > 0)
            {

                var alliedKingSquare = board.Kings[(int)alliedColor];
                var enemyKingSquare = board.Kings[(int)enemyColor];
                mopUpScore += board.GetCenterManhattanDistance(enemyKingSquare) * 10;
                mopUpScore += (14 - board.GetManhattanDistance(alliedKingSquare, enemyKingSquare)) * 4;

                return (int)(mopUpScore * endgameWeight);
            }

            return 0;
        }

        private static int CountMaterial(in Board board, Color color)
        {
            return
                board.Pawns[(int)color].Length * Pawn +
                board.Knights[(int)color].Length * Knight +
                board.Bishops[(int)color].Length * Bishop +
                board.Rooks[(int)color].Length * Rook +
                board.Queens[(int)color].Length * Queen;
        }

        private static int EvaluatePieceSquareTables(in Board board, in PieceSquareTables pieceSquareTables, Color color, float endgamePhaseWeight)
        {
            var value = 0;
            var isWhite = color == Color.White;
            var index = (int)color;
            value += EvaluatePieceSquareTable(pieceSquareTables.Pawns, board.Pawns[index], isWhite);
            value += EvaluatePieceSquareTable(pieceSquareTables.Rooks, board.Rooks[index], isWhite);
            value += EvaluatePieceSquareTable(pieceSquareTables.Knights, board.Knights[index], isWhite);
            value += EvaluatePieceSquareTable(pieceSquareTables.Bishops, board.Bishops[index], isWhite);
            value += EvaluatePieceSquareTable(pieceSquareTables.Queens, board.Queens[index], isWhite);
            var kingEarlyPhase = PieceSquareTables.Read(pieceSquareTables.KingMiddle, board.Kings[index], isWhite);
            value += (int)(kingEarlyPhase * (1 - endgamePhaseWeight));
            return value;
        }

        private static int EvaluatePieceSquareTable(NativeArray<int> table, PieceSet pieceSet, bool isWhite)
        {
            var value = 0;

            for (var i = 0; i < pieceSet.Length; i++)
            {
                value += PieceSquareTables.Read(table, pieceSet[i], isWhite);
            }

            return value;
        }
    }
}

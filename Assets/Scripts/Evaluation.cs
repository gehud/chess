using UnityEngine;

namespace Chess
{
    public readonly struct Evaluation
    {
        public readonly int Value => value;

        public const int Pawn = 100;
        public const int Knight = 300;
        public const int Bishop = 330;
        public const int Rook = 500;
        public const int Queen = 900;

        public const float Endgame = Rook * 2 + Bishop + Knight;

        private readonly int value;

        public Evaluation(Figure figure)
        {
            value = figure switch
            {
                Figure.Pawn => Pawn,
                Figure.Knight => Knight,
                Figure.Bishop => Bishop,
                Figure.Rook => Rook,
                Figure.Queen => Queen,
                _ => 0,
            };
        }

        public Evaluation(in Board board)
        {
            var whiteEvaluation = 0;
            var blackEvaluation = 0;

            var whiteColor = Count(board, Color.White);
            var blackColor = Count(board, Color.Black);

            var whiteColorWithoutPawns = whiteColor - board.Pawns[(int)Color.White].Length * Pawn;
            var blackColorWithoutPawns = blackColor - board.Pawns[(int)Color.Black].Length * Pawn;

            var whiteEndgamePhaseWeight = EndgamePhaseWeight(whiteColorWithoutPawns);
            var blackEndgamePhaseWeight = EndgamePhaseWeight(blackColorWithoutPawns);

            whiteEvaluation += whiteColor;
            blackEvaluation += blackColor;
            whiteEvaluation += MopUpEvaluation(board, Color.White, Color.Black, blackEndgamePhaseWeight);
            blackEvaluation += MopUpEvaluation(board, Color.Black, Color.White, whiteEndgamePhaseWeight);

            int evaluation = whiteEvaluation - blackEvaluation;

            var perspective = board.IsWhiteAllied ? 1 : -1;
            value = evaluation * perspective;
        }

        private static float EndgamePhaseWeight(int colorWithoutPawns)
        {
            const float multiplier = 1f / Endgame;
            return 1f - Mathf.Min(1f, colorWithoutPawns * multiplier);
        }

        private static int MopUpEvaluation(in Board board, Color alliedColor, Color enemyColor, float endgameWeight)
        {
            var evaluation = 0;

            var alliedKing = board.Kings[(int)alliedColor];
            var enemyKing = board.Kings[(int)enemyColor];

            var enemyKingCenterDistanceFile = Mathf.Max(3 - enemyKing.File, enemyKing.File - 4);
            var enemyKingCenterDistanceRank = Mathf.Max(3 - enemyKing.Rank, enemyKing.Rank - 4);
            var enemyKingCenterDistance = enemyKingCenterDistanceFile + enemyKingCenterDistanceRank;
            evaluation += enemyKingCenterDistance;

            var kingDistanceFile = Mathf.Abs(alliedKing.File - enemyKing.File);
            var kingDistanceRank = Mathf.Abs(alliedKing.Rank - enemyKing.Rank);
            var kingDistance = kingDistanceFile + kingDistanceRank;
            evaluation += 14 - kingDistance;

            return (int)(evaluation * 10 + endgameWeight);
        }

        private static int Count(in Board board, Color color)
        {
            var value = 0;
            value += board.Pawns[(int)color].Length * Pawn;
            value += board.Knights[(int)color].Length * Knight;
            value += board.Bishops[(int)color].Length * Bishop;
            value += board.Rooks[(int)color].Length * Rook;
            value += board.Queens[(int)color].Length * Queen;
            return value;
        }

        public static implicit operator int(Evaluation evaluation)
        {
            return evaluation.Value;
        }
    }
}

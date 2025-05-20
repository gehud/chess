using UnityEngine;

namespace Chess
{
    [CreateAssetMenu]
    public class PieceSprites : ScriptableObject
    {
        [SerializeField]
        private Sprite whitePawn;
        [SerializeField]
        private Sprite whiteKnight;
        [SerializeField]
        private Sprite whiteBishop;
        [SerializeField]
        private Sprite whiteRook;
        [SerializeField]
        private Sprite whiteQueen;
        [SerializeField]
        private Sprite whiteKing;
        [Space]
        [SerializeField]
        private Sprite blackPawn;
        [SerializeField]
        private Sprite blackKnight;
        [SerializeField]
        private Sprite blackBishop;
        [SerializeField]
        private Sprite blackRook;
        [SerializeField]
        private Sprite blackQueen;
        [SerializeField]
        private Sprite blackKing;

        public Sprite GetPieceSprite(Figure figure, Color color)
        {
            return figure switch
            {
                Figure.Pawn => color == Color.White ? whitePawn : blackPawn,
                Figure.Knight => color == Color.White ? whiteKnight : blackKnight,
                Figure.Bishop => color == Color.White ? whiteBishop : blackBishop,
                Figure.Rook => color == Color.White ? whiteRook : blackRook,
                Figure.Queen => color == Color.White ? whiteQueen : blackQueen,
                Figure.King => color == Color.White ? whiteKing : blackKing,
                _ => null,
            };
        }
    }
}

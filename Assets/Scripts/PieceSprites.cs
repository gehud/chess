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

        public Sprite GetPieceSprite(Piece piece, Color color)
        {
            return piece switch
            {
                Piece.Pawn => color == Color.White ? whitePawn : blackPawn,
                Piece.Knight => color == Color.White ? whiteKnight : blackKnight,
                Piece.Bishop => color == Color.White ? whiteBishop : blackBishop,
                Piece.Rook => color == Color.White ? whiteRook : blackRook,
                Piece.Queen => color == Color.White ? whiteQueen : blackQueen,
                Piece.King => color == Color.White ? whiteKing : blackKing,
                _ => null,
            };
        }
    }
}

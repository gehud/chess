using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private BoardPainter board;

        private Game game;

        private void OnPiecePicked(GameObject pieceSlot)
        {
            board.ShowMoves(pieceSlot, game.Moves);
        }

        private void OnPieceDroppped(Square from, Square to)
        {
            var actualMove = default(Move);

            foreach (var move in game.Moves)
            {
                if (move.From == from && move.To == to)
                {
                    actualMove = move;
                    break;
                }
            }

            game.MakeMove(actualMove);

            board.Repaint(game.Board);
        }

        private void Awake()
        {
            game = new Game(Allocator.Persistent);
            game.Start();
            board.Repaint(game.Board);
        }

        private void OnEnable()
        {
            DraggablePiece.Picked += OnPiecePicked;
            PieceSlot.PieceDropped += OnPieceDroppped;
        }

        private void OnDisable()
        {
            DraggablePiece.Picked -= OnPiecePicked;
            PieceSlot.PieceDropped -= OnPieceDroppped;
        }

        private void OnDestroy()
        {
            game.Dispose();
        }
    }
}

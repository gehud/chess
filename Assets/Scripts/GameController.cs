using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private BoardPainter board;
        [SerializeField]
        private string fen;

        private Game game;
        private Controls controls;

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
            game.GenerateMoves();
            board.Repaint(game.Board);
        }

        private void UnmakeMove()
        {
            game.UnmakeMove();
            game.GenerateMoves();
            board.Repaint(game.Board);
        }

        private void Awake()
        {
            game = new Game(Allocator.Persistent);

            if (string.IsNullOrEmpty(fen))
            {
                game.Start();
            }
            else
            {
                game.Load(fen);
            }

            game.GenerateMoves();
            board.Repaint(game.Board);

            controls = new();
            controls.Player.Undo.performed += (ctx) => UnmakeMove();
        }

        private void OnEnable()
        {
            DraggablePiece.Picked += OnPiecePicked;
            PieceSlot.PieceDropped += OnPieceDroppped;
            controls.Enable();
        }

        private void OnDisable()
        {
            DraggablePiece.Picked -= OnPiecePicked;
            PieceSlot.PieceDropped -= OnPieceDroppped;
            controls.Disable();
        }

        private void OnDestroy()
        {
            game.Dispose();
        }
    }
}

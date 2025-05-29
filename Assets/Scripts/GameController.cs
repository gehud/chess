using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private BoardPainter painter;
        [SerializeField]
        private string fen;

        private Board board;
        private Controls controls;

        private void OnPiecePicked(GameObject pieceSlot)
        {
            painter.ShowMoves(pieceSlot, board);
        }

        private void OnPieceDroppped(Square from, Square to)
        {
            var actualMove = default(Move);

            foreach (var move in board.Moves)
            {
                if (move.From == from && move.To == to)
                {
                    actualMove = move;
                    break;
                }
            }

            board.MakeMove(actualMove);
            board.GenerateMoves();
            painter.Repaint(board);
        }

        private void OnUndo()
        {
            board.UnmakeLastMove();
            board.GenerateMoves();
            painter.Repaint(board);
        }

        private void Awake()
        {
            board = new Board(Allocator.Persistent);

            if (string.IsNullOrEmpty(fen))
            {
                board.Load(Fen.Start);
            }
            else
            {
                board.Load(fen);
            }

            //board.MakeMove(new Move((Square)SquareName.F3, (Square)SquareName.H3));
            //board.MakeMove(new Move((Square)SquareName.H8, (Square)SquareName.H3));
            //board.MakeMove(new Move((Square)SquareName.E1, (Square)SquareName.F1));
            //board.MakeMove(new Move((Square)SquareName.H3, (Square)SquareName.D3));

            //Debug.Log(board.Fen);
            //var perft = new Perft(board.Fen, Allocator.Temp);
            //perft.Run(2, true);
            //perft.Dispose();

            board.GenerateMoves();
            painter.Repaint(board);

            controls = new();
            controls.Player.Undo.performed += (ctx) => OnUndo();
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
            board.Dispose();
        }
    }
}

﻿using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public class BoardPainter : MonoBehaviour
    {
        [SerializeField]
        private PieceImage piecePrefab;

        public void Repaint(in Board board)
        {
            AssertSquares();

            for (var i = Square.Min.Index; i <= Square.Max.Index; i++)
            {
                var square = transform.GetChild(i);

                if (square.childCount != 0)
                {
                    DestroyImmediate(square.GetChild(0).gameObject);
                }

                if (board[new Square(i)].IsEmpty)
                {
                    continue;
                }

                var pieceImage = Instantiate(piecePrefab, square);
                pieceImage.UpdateImage(board[new Square(i)]);
            }

            ResetSquares();
        }

        public void ShowMoves(GameObject pieceSlot, in MoveList moves)
        {
            foreach (var move in moves)
            {
                if (move.From.Index == pieceSlot.transform.GetSiblingIndex())
                {
                    var square = transform.GetChild(move.To.Index);
                    square.GetComponent<SquareImage>().SetAvailableColor();
                    square.GetComponent<PieceSlot>().IsAvailable = true;
                }
            }
        }

        public void ResetSquares()
        {
            AssertSquares();

            for (var i = Square.Min.Index; i <= Square.Max.Index; i++)
            {
                var square = transform.GetChild(i);
                square.GetComponent<SquareImage>().SetDefaultColor();
                square.GetComponent<PieceSlot>().IsAvailable = false;
            }
        }

        private void AssertSquares()
        {
            Debug.Assert(transform.childCount == Board.Area, "Board squares is out of range.");
        }

        private void Awake()
        {
            ResetSquares();
        }

        private void OnEnable()
        {
            DraggablePiece.Reverted += ResetSquares;
        }

        private void OnDisable()
        {
            DraggablePiece.Reverted -= ResetSquares;
        }

        private void OnValidate()
        {
            ResetSquares();
        }
    }
}

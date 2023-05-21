using System;
using UnityEngine;

namespace Chess {
	public class PieceViewFactory : MonoBehaviour, IPieceViewFactory {
		[SerializeField] private GameObject whitePawn;
		[SerializeField] private GameObject whiteKnight;
		[SerializeField] private GameObject whiteBishop;
		[SerializeField] private GameObject whiteRook;
		[SerializeField] private GameObject whiteQueen;
		[SerializeField] private GameObject whiteKing;
		[SerializeField] private GameObject blackPawn;
		[SerializeField] private GameObject blackKnight;
		[SerializeField] private GameObject blackBishop;
		[SerializeField] private GameObject blackRook;
		[SerializeField] private GameObject blackQueen;
		[SerializeField] private GameObject blackKing;

		public GameObject Get(Piece piece) {
			var color = piece.Color;
			if (color == Piece.Colors.None)
				throw new ArgumentException($"Missing view for piece color: {color}", "piece");

			bool isWhite = color == Piece.Colors.White;
			return piece.Type switch {
				Piece.Types.Pawn => isWhite ? whitePawn : blackPawn,
				Piece.Types.Knight => isWhite ? whiteKnight : blackKnight,
				Piece.Types.Bishop => isWhite ? whiteBishop : blackBishop,
				Piece.Types.Rook => isWhite ? whiteRook : blackRook,
				Piece.Types.Queen => isWhite ? whiteQueen : blackQueen,
				Piece.Types.King => isWhite ? whiteKing : blackKing,
				_ => throw new ArgumentException($"Missing view for piece type {piece.Type}", "piece")
			};
		}
	}
}
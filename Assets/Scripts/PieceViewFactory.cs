using System;
using UnityEngine;

namespace Chess {
	public class PieceViewFactory : MonoBehaviour, IPieceViewFactory {
		[SerializeField] private GameObject pawn;
		[SerializeField] private GameObject knight;
		[SerializeField] private GameObject bishop;
		[SerializeField] private GameObject rook;
		[SerializeField] private GameObject queen;
		[SerializeField] private GameObject king;

		[SerializeField]
		private Color blackColor = Color.black;

		public GameObject Create(Piece piece) {
			var color = piece.Color;
			if (color == PieceColor.None)
				throw new ArgumentException($"Missing view for piece color: {color}", "piece");

			var instance = Instantiate(piece.Type switch {
				PieceType.Pawn => pawn,
				PieceType.Knight => knight,
				PieceType.Bishop => bishop,
				PieceType.Rook => rook,
				PieceType.Queen => queen,
				PieceType.King => king,
				_ => throw new ArgumentException($"Missing view for piece type {piece.Type}", "piece")
			});

			bool isBlack = color == PieceColor.Black;
			if (isBlack) {
				instance.transform.Rotate(Vector3.up, 180.0f);
				instance.GetComponent<Renderer>().material.color = blackColor;
			}

			return instance;
		}
	}
}
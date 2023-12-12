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
		private UnityEngine.Color blackColor = UnityEngine.Color.black;

		public GameObject Create(Piece piece) {
			var color = piece.Color;
			if (color == Color.None)
				throw new ArgumentException($"Missing view for piece color: {color}", "piece");

			var instance = Instantiate(piece.Figure switch {
				Figure.Pawn => pawn,
				Figure.Knight => knight,
				Figure.Bishop => bishop,
				Figure.Rook => rook,
				Figure.Queen => queen,
				Figure.King => king,
				_ => throw new ArgumentException($"Missing view for piece type {piece.Figure}", "piece")
			});

			bool isBlack = color == Color.Black;
			if (isBlack) {
				instance.transform.Rotate(Vector3.up, 180.0f);
				instance.GetComponent<Renderer>().material.color = blackColor;
			}

			return instance;
		}
	}
}
using System;
using UnityEngine;

namespace Chess {
	public class Game : MonoBehaviour {
		[SerializeField]
		private GameObject cursor;

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

		private int[] pieces;

		private int selected = -1;

		private void Awake() {
			cursor.SetActive(false);
			Board.OnCell += OnCell;
			pieces = new int[64];

			pieces[0] = Piece.ROOK | Piece.WHITE;
			pieces[1] = Piece.KNIGHT | Piece.WHITE;
			pieces[2] = Piece.BISHOP | Piece.WHITE;
			pieces[3] = Piece.KING | Piece.WHITE;
			pieces[4] = Piece.QUEEN | Piece.WHITE;
			pieces[5] = Piece.BISHOP | Piece.WHITE;
			pieces[6] = Piece.KNIGHT | Piece.WHITE;
			pieces[7] = Piece.ROOK | Piece.WHITE;
			pieces[8] = Piece.PAWN | Piece.WHITE;
			pieces[9] = Piece.PAWN | Piece.WHITE;
			pieces[10] = Piece.PAWN | Piece.WHITE;
			pieces[11] = Piece.PAWN | Piece.WHITE;
			pieces[12] = Piece.PAWN | Piece.WHITE;
			pieces[13] = Piece.PAWN | Piece.WHITE;
			pieces[14] = Piece.PAWN | Piece.WHITE;
			pieces[15] = Piece.PAWN | Piece.WHITE;

			pieces[63] = Piece.ROOK | Piece.BLACK;
			pieces[62] = Piece.KNIGHT | Piece.BLACK;
			pieces[61] = Piece.BISHOP | Piece.BLACK;
			pieces[60] = Piece.KING | Piece.BLACK;
			pieces[59] = Piece.QUEEN | Piece.BLACK;
			pieces[58] = Piece.BISHOP | Piece.BLACK;
			pieces[57] = Piece.KNIGHT | Piece.BLACK;
			pieces[56] = Piece.ROOK | Piece.BLACK;
			pieces[55] = Piece.PAWN | Piece.BLACK;
			pieces[54] = Piece.PAWN | Piece.BLACK;
			pieces[53] = Piece.PAWN | Piece.BLACK;
			pieces[52] = Piece.PAWN | Piece.BLACK;
			pieces[51] = Piece.PAWN | Piece.BLACK;
			pieces[50] = Piece.PAWN | Piece.BLACK;
			pieces[49] = Piece.PAWN | Piece.BLACK;
			pieces[48] = Piece.PAWN | Piece.BLACK;
			UpdateBoard();
		}

		private void UpdateBoard() {
			for (int i = 0; i < transform.childCount; i++)
				Destroy(transform.GetChild(i).gameObject);

			for (int i = 0; i < 64; i++) {
				int x = i % 8;
				int y = i / 8;
				var position = new Vector3 {
					x = x - 4 + 0.5f,
					y = 0.3f,
					z = y - 4 + 0.5f
				};
				int piece = pieces[i];
				bool isWhite = (piece & Piece.WHITE) != 0;
				piece &= 0b111;
				switch (piece) {
					case Piece.PAWN:
						Instantiate(isWhite ? whitePawn : blackPawn, position, Quaternion.identity, transform);
						break;
					case Piece.KNIGHT:
						Instantiate(isWhite ? whiteKnight : blackKnight, position, Quaternion.identity, transform);
						break;
					case Piece.BISHOP:
						Instantiate(isWhite ? whiteBishop : blackBishop, position, Quaternion.identity, transform);
						break;
					case Piece.ROOK:
						Instantiate(isWhite ? whiteRook : blackRook, position, Quaternion.identity, transform);
						break;
					case Piece.QUEEN:
						Instantiate(isWhite ? whiteQueen : blackQueen, position, Quaternion.identity, transform);
						break;
					case Piece.KING:
						Instantiate(isWhite ? whiteKing : blackKing, position, Quaternion.identity, transform);
						break;
				}
			}
		}

		private void OnCell(int index) {
			if (selected == index || index == -1) {
				selected = -1;
				cursor.SetActive(false);
				return;
			}

			if (selected != -1 && selected != index) {
				if (pieces[selected] != 0) {
					pieces[index] = pieces[selected];
					pieces[selected] = 0;
					UpdateBoard();
				}
			}

			selected = index;
			int x = index % 8;
			int y = index / 8;
			cursor.SetActive(true);
			cursor.transform.position = new Vector3 {
				x = x - 4 + 0.5f,
				y = cursor.transform.position.y,
				z = y - 4 + 0.5f
			};
		}
	}
}
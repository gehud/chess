using UnityEngine;

namespace Chess {
	public interface IPieceViewFactory {
		GameObject Get(Piece piece);
	}
}
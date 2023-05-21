using UnityEngine;

namespace Chess {
	public interface IPieceViewFactory {
		GameObject Create(Piece piece);
	}
}
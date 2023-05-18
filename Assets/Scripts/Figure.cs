using System;
using UnityEngine;

namespace Chess {
	public class Figure : MonoBehaviour {
		public static event Action<Figure> OnSelected;

		private void OnMouseDown() {
			OnSelected?.Invoke(this);
		}
	}
}
using System;
using UnityEngine;

namespace Chess {
	public class Board : MonoBehaviour {
		public static event Action<int> OnCell;

		private void Update() {
			if (!Input.GetMouseButtonDown(0))
				return;

			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(ray, out RaycastHit hitInfo)) {
				OnCell?.Invoke(-1);
				return;
			}

			var point = hitInfo.point;

			var cell = Vector3Int.FloorToInt(point);
			if (cell.x < -4 || cell.x > 3 || cell.z < -4 || cell.z > 3) {
				OnCell?.Invoke(-1);
				return;
			}

			OnCell?.Invoke((cell.z + 4) * 8 + cell.x + 4);
		}
	}
}
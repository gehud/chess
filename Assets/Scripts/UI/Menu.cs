using TMPro;
using UnityEngine;

namespace Chess.UI {
	public class Menu : MonoBehaviour {
		[SerializeField]
		private BoardView boardView;
		[SerializeField]
		private GameObject panel;
		[SerializeField]
		private TMP_Text winnerText;

		private bool isCheckmate = false;

		private void Update() {
			if (Input.GetKeyDown(KeyCode.Escape) && !isCheckmate) {
				panel.SetActive(!panel.activeSelf);
			}
		}

		private void OnEnable() {
			boardView.OnCheckmate += OnCheckmate;
		}

		private void OnDisable() {
			boardView.OnCheckmate -= OnCheckmate;
		}

		private void OnCheckmate(PieceColor color) {
			panel.SetActive(true);
			winnerText.gameObject.SetActive(true);
			switch (color) {
				case PieceColor.White:
					winnerText.text = "White Wins!";
					break;
				case PieceColor.Black:
					winnerText.text = "Black Wins!";
					break;
			}
			isCheckmate = true;
		}
	}
}
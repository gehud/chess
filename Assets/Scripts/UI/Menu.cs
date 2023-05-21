using TMPro;
using UnityEngine;

namespace Chess.UI {
	public class Menu : MonoBehaviour {
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
			Board.OnCheckmate += OnCheckmate;
		}

		private void OnDisable() {
			Board.OnCheckmate -= OnCheckmate;
		}

		private void OnCheckmate(Piece.Colors color) {
			panel.SetActive(true);
			winnerText.gameObject.SetActive(true);
			switch (color) {
				case Piece.Colors.White:
					winnerText.text = "White Wins!";
					break;
				case Piece.Colors.Black:
					winnerText.text = "Black Wins!";
					break;
			}
			isCheckmate = true;
		}
	}
}
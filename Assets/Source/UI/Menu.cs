﻿using TMPro;
using UnityEngine;

namespace Chess.UI {
	public class Menu : MonoBehaviour {
		[SerializeField]
		private GameController boardView;
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

		private void OnCheckmate(Color color) {
			panel.SetActive(true);
			winnerText.gameObject.SetActive(true);
			switch (color) {
				case Color.White:
					winnerText.text = "White Wins!";
					break;
				case Color.Black:
					winnerText.text = "Black Wins!";
					break;
			}
			isCheckmate = true;
		}
	}
}
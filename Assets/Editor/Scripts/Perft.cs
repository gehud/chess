using Chess.Utilities;
using UnityEditor;
using UnityEngine;

namespace Chess {
	public class Perft : EditorWindow {
		[SerializeField]
		private string fen;
		[SerializeField]
		private int depth = 2;

		[MenuItem("Utilities/Perft")]
		private static void Create() {
			GetWindow<Perft>().Show();
		}

		private void OnEnable() {
			var data = EditorPrefs.GetString("Chess.Editor.Perft", JsonUtility.ToJson(this, false));
			JsonUtility.FromJsonOverwrite(data, this);
		}

		private void OnDisable() {
			var data = JsonUtility.ToJson(this, false);
			EditorPrefs.SetString("Chess.Editor.Perft", data);
		}

		private void OnGUI() {
			fen = EditorGUILayout.TextField("FEN", fen);
			depth = Mathf.Clamp(EditorGUILayout.IntField("Depth", depth), 2, int.MaxValue);
			if (GUILayout.Button("Go")) {
				var game = new Game();
				game.Load(fen);
				var moves = game.GenerateMoves();

				foreach (var move in moves) {
					game.Move(move);
					int count = PerftUtility.Perft(game, depth - 1);
					Debug.Log($"{move}: {count}");
					game.Undo();
				}
			}
		}
	}
}

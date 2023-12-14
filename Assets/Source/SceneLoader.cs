using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chess {
	public class SceneLoader : MonoBehaviour {
		public void LoadScene(string name) {
			SceneManager.LoadScene(name);
		}
	}
}
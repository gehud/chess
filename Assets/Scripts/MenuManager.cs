using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chess
{
    public class MenuManager : MonoBehaviour
    {
        public bool IsOpened => origin.activeSelf;

        [SerializeField]
        private PromotionSelector promotionSelector;
        [SerializeField]
        private GameObject origin;
        [SerializeField]
        private GameObject close;
        [SerializeField]
        private GameObject blackWon;
        [SerializeField]
        private GameObject whiteWon;
        [SerializeField]
        private GameObject draw;

        private Controls controls;
        private bool isGameOver;

        public void Close()
        {
            origin.SetActive(false);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void Restart()
        {
            SceneManager.LoadScene(0);
        }

        private void OnGameOver()
        {
            origin.SetActive(true);
            isGameOver = true;
            close.SetActive(false);
        }

        public void ShowDraw()
        {
            OnGameOver();
            draw.SetActive(true);
        }

        public void ShowWinner(Color color)
        {
            OnGameOver();   
            switch (color)
            {
                case Color.Black:
                    blackWon.SetActive(true);
                    break;
                case Color.White:
                    whiteWon.SetActive(true);
                    break;
            }
        }

        private void Toggle()
        {
            if (isGameOver || promotionSelector.IsOpened)
            {
                return;
            }

            origin.SetActive(!origin.activeSelf);
        }

        private void Awake()
        {
            controls = new();
            controls.Player.Cancel.performed += (ctx) => Toggle();
        }

        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }
    }
}

using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private BoardPainter board;

        private Game game;

        private void OnSquarePicked(GameObject square)
        {
        }

        private void Awake()
        {
            game = new Game(Allocator.Persistent);
            game.Start();
            board.Repaint(game.Board);
        }

        private void OnEnable()
        {
            PickableSquare.Picked += OnSquarePicked;
        }

        private void OnDisable()
        {
            PickableSquare.Picked -= OnSquarePicked;
        }

        private void OnDestroy()
        {
            game.Dispose();
        }
    }
}

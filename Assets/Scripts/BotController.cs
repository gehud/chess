using System.Collections;
using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public class BotController : MonoBehaviour
    {
        public delegate void SearchCompletedHandler(Move move);
        public static event SearchCompletedHandler SearchCompleted;

        [SerializeField, Min(0f)]
        private float searchTime = 3;

        private Bot bot;

        public void StartSearch(Board board)
        {
            StartCoroutine(Timer(board));
        }

        private IEnumerator Timer(Board board)
        {
            bot.StartSearch(board);

            var time = 0f;
            while (time <= searchTime)
            {
                if (bot.IsSearchCompleted)
                {
                    break;
                }

                time += Time.deltaTime;
                yield return null;
            }

            bot.StopSearch();
            SearchCompleted?.Invoke(bot.BestMove);
        }

        private void Awake()
        {
            bot = new(Allocator.Persistent);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            bot.StopSearch();
            bot.Dispose();
        }
    }
}

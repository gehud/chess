using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Chess
{
    public class BotController : MonoBehaviour
    {
        public delegate void SearchCompletedHandler(Move move);
        public static event SearchCompletedHandler SearchCompleted;

        [SerializeField, Min(0)]
        private int searchTime = 3000;

        private CancellationTokenSource cancelSearchTimer;

        private Bot bot;
        private Task searchTask;

        public void StartSearch(Board board)
        {
            searchTask = Task.Factory.StartNew(() => bot.StartSearch(board), TaskCreationOptions.LongRunning);
            cancelSearchTimer = new CancellationTokenSource();
            Task.Delay(searchTime, cancelSearchTimer.Token).ContinueWith((t) => TimeOutThreadedSearch());
        }

        private void TimeOutThreadedSearch()
        {
            if (!cancelSearchTimer.IsCancellationRequested)
            {
                bot.EndSearch();
            }
        }

        private void Awake()
        {
            bot = new();
        }

        private void Update()
        {
            if (searchTask != null && searchTask.IsCompleted)
            {
                cancelSearchTimer?.Cancel();
                SearchCompleted?.Invoke(bot.BestMove);
                searchTask = null;
            }
        }

        private void OnDestroy()
        {
            cancelSearchTimer.Cancel();
        }
    }
}

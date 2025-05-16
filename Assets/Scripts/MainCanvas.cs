using UnityEngine;

namespace Chess
{
    [RequireComponent(typeof(Canvas))]
    public class MainCanvas : Singleton<MainCanvas>
    {
        public Canvas Canvas => GetComponent<Canvas>();
    }
}

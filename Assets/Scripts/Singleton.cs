using UnityEngine;

namespace Chess
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance => instance;

        private static T instance;

        protected virtual void Awake()
        {
            instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            instance = null;
        }
    }
}

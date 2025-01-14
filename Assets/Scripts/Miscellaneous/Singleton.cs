using UnityEngine;

namespace TeleopReachy
{
    public class Singleton<T> : MonoBehaviour where T : UnityEngine.Component
    {
        public static T Instance { get; protected set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this as T;
            Init();
        }

        protected virtual void Init()
        {

        }

    }
}
using UnityEngine;

namespace Ulbe
{
    public class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _Instance;
        public static T Instance => _Instance ?? new GameObject("UnitySingleton").AddComponent<T>();

        protected virtual void Awake()
        {
            if (_Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (this == _Instance)
                _Instance = null;
        }
    }
}

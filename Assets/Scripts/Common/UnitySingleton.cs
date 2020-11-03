using UnityEngine;

namespace Ulbe
{
    public class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _Instance;
        public static T Instance => _Instance ?? new GameObject().AddComponent<T>();

        protected virtual void Awake()
        {
            if (_Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            (this as T).name = (this as T).GetType().Name;

            _Instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (this == _Instance)
                _Instance = null;
        }
    }
}

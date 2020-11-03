using System.Collections.Generic;
using UnityEngine;

namespace Emojigame
{
    [System.Serializable]
    public class Pool<T>
    {
        public string tag;
        public T prefab;
        public int size;
    }

    public class Pooler : MonoBehaviour
    {
        private static Pooler s_instance;
        public static Pooler Instance => s_instance ?? new GameObject("GameManager").AddComponent<Pooler>();
        protected Dictionary<string, Queue<GameObject>> poolDic;
        protected Pool<GameObject>[] pools;

        public void Awake()
        {
            if (s_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            poolDic = new Dictionary<string, Queue<GameObject>>();

            pools = new Pool<GameObject>[]
            {
            new Pool<GameObject>() { size = 25, tag = "explode", prefab = Resources.Load<GameObject>("prefabs/DeathFX") },
            };

            // Instantiate
            for (int i = 0; i < pools.Length; i++)
            {
                Queue<GameObject> q = new Queue<GameObject>();
                for (int j = 0; j < pools[i].size; j++)
                {
                    GameObject go = Instantiate(pools[i].prefab);
                    go.SetActive(false);
                    q.Enqueue(go);
                }

                poolDic.Add(pools[i].tag, q);
            }
        }

        public GameObject Dequeue(string prefab, Vector3 pos)
        {
            return Dequeue(prefab, pos, Quaternion.identity, null);
        }

        public GameObject Dequeue(string prefab)
        {
            return Dequeue(prefab, Vector3.zero, Quaternion.identity, null);
        }

        public GameObject Dequeue(string prefab, Vector3 pos, Quaternion rotation, Transform parent = null)
        {
            if (!poolDic.ContainsKey(prefab) || poolDic[prefab].Count == 0)
                return null;

            Transform go = poolDic[prefab].Dequeue().transform;

            go.position = pos;
            go.rotation = rotation;
            go.SetParent(parent);

            go.gameObject.SetActive(true);
            poolDic[prefab].Enqueue(go.gameObject);

            return go.gameObject;
        }

        public void Enqueue(string prefab, GameObject go)
        {
            if (!poolDic.ContainsKey(prefab) && go != null)
                return;
            poolDic[prefab].Enqueue(go);
        }

        private void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
                Destroy(gameObject);
                return;
            }
        }
    }
}



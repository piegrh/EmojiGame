using System.Collections;
using UnityEngine;

namespace Emojigame
{
    public class PrefabSpawner : MonoBehaviour
    {
        public GameObject prefab;
        public int cnt = 1;
        public float delay = 0;

        private void Awake()
        {
            StartCoroutine(Spawn());
        }

        protected virtual IEnumerator Spawn()
        {
            GameObject g;
            for (int i = 0; i < cnt; i++)
            {
                g = Instantiate(prefab);
                if (delay > 0.01)
                    yield return new WaitForSeconds(delay);
                g.GetComponent<Rigidbody2D>().angularVelocity = 1000f * Random.value;
            }
            Destroy(this);
        }
    }

}


using UnityEngine;
using UnityEngine.UI;

namespace Ulbe.UI
{
    public class FlashingText : MonoBehaviour
    {
        public Gradient g;
        public float speed;
        [HideInInspector] public Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            text.color = g.Evaluate(Mathf.Repeat(Time.time * speed, 1));
        }
    }
}
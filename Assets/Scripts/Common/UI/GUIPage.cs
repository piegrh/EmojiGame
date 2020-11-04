using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulbe.UI
{
    public class GUIPage : MonoBehaviour
    {
        [SerializeField] public GUIPage prev;

        public virtual void Back()
        {
            if (prev != null)
            {
                prev.gameObject.SetActive(true);
                gameObject.SetActive(false);
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
                Back();
        }
    }
}
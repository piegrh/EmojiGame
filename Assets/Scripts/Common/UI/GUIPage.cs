using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIPage : MonoBehaviour
{
    [SerializeField] GUIPage prev;

    public virtual void Back()
    {
        if(prev != null)
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

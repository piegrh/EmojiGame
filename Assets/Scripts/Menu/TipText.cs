using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipText : MonoBehaviour
{
    public string id;
    public float time;
    protected string cvarname;

    public void Awake()
    {
        cvarname = string.Format("tooltip_{0}", id);
        Cvars.Instance.Get(cvarname, "0", Cvar.CvarFlags.ROM);
    }

    private void Start()
    {
        if (!Cvars.Instance.Cvar_Find(cvarname).BoolValue)
        {
            Cvars.Instance.ForceSet(cvarname, "1");
            Destroy(gameObject, time);
            return;
        }
        Destroy(gameObject);
    }
}

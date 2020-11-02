using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingSlider : MonoBehaviour
{
    public string cvarName;
    public Scrollbar bar;
    public InputField field;
    public bool updated;
    Cvar cvar;

    protected void Awake()
    {
        bar.onValueChanged.AddListener(delegate { ValueUpdated(); });
        field.onValueChanged.AddListener(delegate { InputvalueChanged(); });
        ResetValue();
        cvar = Cvars.Instance.Cvar_Find(cvarName);
    }

    protected void ValueUpdated()
    {
        field.text = Mathf.Clamp(bar.value, 0, 1).ToString();
        updated = false;
    }

    protected void InputvalueChanged()
    {
        bar.value = float.TryParse(field.text, out float f) ? Mathf.Clamp(f, 0,1) : 0;
        field.text = bar.value.ToString(".00");
        updated = false;
    }

    public void Apply()
    {
        if(!updated)
           cvar = Cvars.Instance.Set(cvarName, bar.value.ToString(), false);
        updated = true;
    }

    public void ResetValue()
    {
        bar.value = cvar.floatValue;
        updated = false;
        Apply();
    }

    public static void ResetAll()
    {
        SettingSlider[] sliders = FindObjectsOfType<SettingSlider>();
        foreach (SettingSlider ss in sliders)
            ss.ResetValue();
    }

    public static bool AllUpdated()
    {
        SettingSlider[] sliders = FindObjectsOfType<SettingSlider>();
        foreach (SettingSlider ss in sliders)
        {
            if (!ss.updated)
                return false;
        }
        return true;
    }

    public static void ApplyAll()
    {
        SettingSlider[] sliders = FindObjectsOfType<SettingSlider>();
        foreach (SettingSlider ss in sliders)
            if (!ss.updated)
                ss.Apply();
    }
}

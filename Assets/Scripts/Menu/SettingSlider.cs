using UnityEngine;
using UnityEngine.UI;
using Ulbe;

public class SettingSlider : MonoBehaviour
{
    public string cvarName;
    public Scrollbar bar;
    public InputField field;
    public bool updated;

    protected void Awake()
    {
        bar.onValueChanged.AddListener(delegate { ValueUpdated(); });
        field.onValueChanged.AddListener(delegate { InputvalueChanged(); });
    }

    protected void Start()
    {
        ResetValue();
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
          Cvars.Instance.Set(cvarName, bar.value.ToString(), false);
        updated = true;
    }

    public void ResetValue()
    {
        bar.value = Cvars.Instance.Cvar_Find(cvarName).floatValue;
        updated = false;
        Apply();
    }
}

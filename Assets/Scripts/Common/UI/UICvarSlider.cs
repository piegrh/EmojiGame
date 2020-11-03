using UnityEngine;
using UnityEngine.UI;
using Ulbe;

namespace Ulbe.UI
{
    public class UICvarSlider : UICvarSetting
    {
        public Scrollbar bar;
        public InputField field;

        public override void Awake()
        {
            base.Awake();
            bar.onValueChanged.AddListener(delegate { ValueUpdated(); });
            field.onValueChanged.AddListener(delegate { InputvalueChanged(); });
        }

        protected void ValueUpdated()
        {
            field.text = Mathf.Clamp(bar.value, 0, 1).ToString();
            base.ValueChanged();
        }

        protected void InputvalueChanged()
        {
            bar.value = float.TryParse(field.text, out float f) ? Mathf.Clamp(f, 0, 1) : 0;
            field.text = bar.value.ToString(".00");
            base.ValueChanged();
        }

        public override void Apply()
        {
            SetCvarValue(bar.value.ToString());
            base.Apply();
        }

        public override void ResetValue()
        {
            bar.value = Cvars.Instance.Cvar_Find(cvarName).floatValue;
            base.ResetValue();
        }
    }

}



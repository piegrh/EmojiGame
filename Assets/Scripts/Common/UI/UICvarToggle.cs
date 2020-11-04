using UnityEngine.UI;

namespace Ulbe.UI
{
    public class UICvarToggle : UICvarSetting
    {
        public Toggle toggle;

        public override void Awake()
        {
            base.Awake();
            toggle.onValueChanged.AddListener(delegate { ValueChanged(); });
        }

        public override void ValueChanged()
        {
            base.ValueChanged();
        }

        public override void Apply()
        {
            SetCvarValue(toggle.isOn ? "1" : "0");
            base.ValueChanged();
        }

        public override void ResetValue()
        {
            toggle.isOn = Cvars.Instance.Cvar_Find(cvarName).BoolValue;
            base.ValueChanged();
        }
    }

}

using UnityEngine;

namespace Ulbe.UI
{
    public abstract class UICvarSetting : MonoBehaviour
    {
        public string cvarName;
        public string alias = "";
        public UnityEngine.UI.Text lable;
        [HideInInspector] public bool updated;

        public virtual void Awake()
        {
            lable.text = alias == "" ? cvarName : alias;
        }

        public virtual void Start()
        {
            ResetValue();
        }

        public virtual void ValueChanged()
        {
            updated = false;
        }

        public virtual void Apply()
        {
            updated = true;
        }

        public virtual void ResetValue()
        {
            updated = false;
            Apply();
        }

        protected virtual void SetCvarValue(string value)
        {
            if (!updated)
                Cvars.Instance.Set(cvarName, value, false);
        }
    }
}


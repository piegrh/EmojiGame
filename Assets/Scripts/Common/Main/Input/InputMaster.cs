using System.Collections.Generic;
using UnityEngine;


public class InputMaster : MonoBehaviour
{
    [System.Serializable]
    protected internal struct Keybind
    {
        public string action;
        public string button;
    }

    private static InputMaster s_instance;
    public static InputMaster Instance => s_instance ?? new GameObject("InputManager").AddComponent<InputMaster>();
    protected Dictionary<string, KeyCode> keys;
    [SerializeField] protected Keybind[] DefaultKeys;

    private void Awake()
    {
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        keys = new Dictionary<string, KeyCode>(System.StringComparer.InvariantCultureIgnoreCase);
        DontDestroyOnLoad(gameObject);

        // Add Commands
        Commands.Instance.Cmd_AddCommand("bind", Bind_f);
        Commands.Instance.Cmd_AddCommand("unbind", Unbind_f);
        Commands.Instance.Cmd_AddCommand("bindlist", Bindlist_f);

        // Set default
        foreach (Keybind kb in DefaultKeys)
            Bind(kb.action, kb.button);

        s_instance = this;
    }

    private KeyCode SeKteycode(string action, string button)
    {
        return (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(action, button));
    }

    public bool Bind(string action, string button)
    {
        foreach(Keybind kb in DefaultKeys)
        {
            if (action.Equals(kb.action))
            {
                if (!keys.ContainsKey(action))
                    AddKey(action, button);
                keys[action] = SeKteycode(action, button);
                Cvars.Instance.ForceSet(string.Format("bind {0}", action), button);
                return true;
            }
        }
        return false;
    }

    public bool Unbind(string action)
    {
        if (keys.ContainsKey(action))
        {
            keys.Remove(action);
            return true;
        }
        return false;
    }

    public KeyCode GetKeyCode(string action)
    {
        return keys.ContainsKey(action) ? keys[action] : KeyCode.None;
    }

    public string GetButtonName(string action)
    {
        return keys[action].ToString();
    }

    public void SetToDefault(string action)
    {
        Bind(action, GetDefault(action));
    }

    public string GetDefault(string action)
    {
        foreach (Keybind kb in DefaultKeys)
            if (kb.action == action)
                return kb.button;
        return KeyCode.None.ToString();
    }

    protected bool AddKey(string action,string button)
    {
        if (!keys.ContainsKey(action))
        {
            keys.Add(action, SeKteycode(action, button));
            return true;
        }
        return false;
    }

    // ========================================================
    // ==== KONSOLE COMMANDS                               ====
    // ========================================================

    private void Unbind_f()
    {
        if (Commands.Instance.Cmd_Argc() >= 2)
        {
            string actionName = Commands.Instance.Cmd_argv(1);
            if (!Unbind(actionName))
                Konsole.Instance.Com_printlinef("Unable to unbind \"{0}\". Action not found.", actionName);
        } else
        {
            Konsole.Instance.Com_printlinef("unbind <action>");
        }
    }

    private void Bind_f()
    {
        if (Commands.Instance.Cmd_Argc() >= 3)
        {
            string actionName = Commands.Instance.Cmd_argv(1);
            string buttonName = Commands.Instance.Cmd_argv(2);
            // Bind to default key
            if(buttonName.Equals("default", System.StringComparison.InvariantCultureIgnoreCase))
            {
                string defaultValue = GetDefault(actionName);
                if (!defaultValue.Equals(KeyCode.None.ToString()))
                    buttonName = defaultValue;
                else
                {
                    Konsole.Instance.Com_printlinef("Unable to bind \"{0}\" to default. Action not found!", actionName);
                    return;
                }
            }
            try
            {
                if(!Bind(actionName, buttonName))
                    Konsole.Instance.Com_printlinef("Unable to unbind \"{0}\". Action not found!", actionName);
            }
            catch (System.Exception e)
            {
                Konsole.Instance.Com_printlinef("{0}{1}", UlbeColorString.S_COLOR_RED ,e.Message);
            }
        }
        else if (Commands.Instance.Cmd_Argc() == 2 && keys.ContainsKey(Commands.Instance.Cmd_argv(1)))
        {
            Konsole.Instance.Com_printlinef("{0} => {1}", Commands.Instance.Cmd_argv(1), GetKeyCode(Commands.Instance.Cmd_argv(1)));
        }
        else
        {
            Konsole.Instance.Com_printlinef("bind <action> <buttonname>");
        }
    }

    private void Bindlist_f()
    {
        foreach (KeyValuePair<string, KeyCode> bind in keys)
            Konsole.Instance.Com_printlinef("{0} => {1}", bind.Key, bind.Value.ToString());
    }
}

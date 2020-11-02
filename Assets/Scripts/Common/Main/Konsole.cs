using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Konsole : MonoBehaviour
{
    private static Konsole s_instance = null;
    public static Konsole Instance => s_instance ?? Instantiate(Resources.Load<GameObject>("prefabs/ulbe/Konsole")).GetComponent<Konsole>();
    const int CON_MAX_LEN = 512;
    const int CON_MAX_HISTORY_LEN = 512;
    List<string> conText;
    [SerializeField] protected GameObject cns;
    [SerializeField] protected Text consoleText;
    [SerializeField] protected Image background;
    [SerializeField] protected InputField CommandLine;
    protected bool _active;
    protected bool ey = false;

    public bool Active
    {
        get
        {
            return _active && !ey;
        }
        set
        {
            this._active = value;
        }
    }
    string[] history;
    int historySize = 0;
    int historyIndex = 0;
    int index = 0;

    private void Awake()
    {
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        conText = new List<string>();
        history = new string[CON_MAX_HISTORY_LEN];
        InitCvars();
        ToggleConsole();
        DontDestroyOnLoad(gameObject);
        historyIndex = 0;
        s_instance = this;
    }

    private void Start()
    {
        UpdateColors();
        ToggleConsole();
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string text = "";
        switch (type)
        {
            case LogType.Error:
                text = "^1ERROR: ^3";
                break;
            case LogType.Warning:
                text = "^1WARNING: ^3";
                Com_printlinef("{0}{1}", text, logString);
                return;
            case LogType.Exception:
                text = "^4EXCEPTION: ^$";
                break;
            case LogType.Assert:
                text = "^5ASSERT: ^$";
                break;
            case LogType.Log:
                Com_printlinef("{0}",logString);
                return;
            default:
                return;
        }

        Com_printlinef("{0}{1}", text, logString);
        Com_printlinef("{0}", stackTrace);
    }

    protected void InitCvars()
    {
        Cvars.Instance.Get("console_font_color", "23B21F");
        Cvars.Instance.Get("console_bg_color", "000000");
        Cvars.Instance.Get("console_bg_alpha", "0.8");
        Cvars.Instance.Get("console_font_alpha", "0.8");
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        DrawConsole();
    }

    protected void HandleInput()
    {
        if (!Input.anyKey)
            return;

        if (Input.GetKeyDown(InputMaster.Instance.GetKeyCode("Console")))
        {
            if (!ey)
            {
                ToggleConsole();
            }
            else
            {
                ey = false;
                CommandLine.enabled = true;
                FocusCommandLine();
            }
        }

        if (_active)
        {
            UpdateColors();

            // Toggle focus
            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (CommandLine.isFocused)
                {
                    ey = true;
                    CommandLine.enabled = false;
                    ClearFocusCommandLine();
                }
                else
                {
                    ey = false;
                    CommandLine.enabled = true;
                    FocusCommandLine();
                }
            }

            if (CommandLine.enabled)
            {
                // autocomplete
                if (Input.GetKeyDown(KeyCode.Tab))
                    AutoComplete(Cvars.Instance.cvarList.Keys.ToArray(), Commands.Instance.cmd_functions.Keys.ToArray());
              
                // Scroll  upp in console output
                if (Input.GetKey(KeyCode.PageUp))
                    index = Mathf.Clamp(index -= 1, 17, conText.Count - 1);

                // Scroll down in console output
                if (Input.GetKey(KeyCode.PageDown))
                    index = Mathf.Clamp(index += 1, 0, conText.Count - 1);

                // Scroll up in history
                if (Input.GetKeyDown(KeyCode.UpArrow) && historySize > 0)
                {
                    historyIndex = Mathf.Clamp(historyIndex -= 1, 0, historySize - 1);
                    CommandLine.text = GetHistory(historyIndex);
                    CommandLine.caretPosition = (CommandLine.text.Length);
                }

                // Scroll down in history
                if (Input.GetKeyDown(KeyCode.DownArrow) && historySize > 0)
                {
                    CommandLine.text = "";
                    historyIndex = historySize;
                    CommandLine.caretPosition = (CommandLine.text.Length);
                }

                // Submit
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    SubmitCommandLine();
                    ClearCommandLine();
                    FocusCommandLine();
                }
            }
        }
    }

    protected void UpdateColors()
    {
        if (ColorUtility.TryParseHtmlString(string.Format("#{0}", Cvars.Instance.Cvar_Find("console_font_color").stringValue), out Color fc))
        {
            fc.a = Cvars.Instance.Get("console_font_alpha", "0.8").floatValue;
            CommandLine.textComponent.color = consoleText.color = fc;
        }

        if (ColorUtility.TryParseHtmlString(string.Format("#{0}", Cvars.Instance.Cvar_Find("console_bg_color").stringValue), out Color bgc))
        {
            bgc.a = Cvars.Instance.Get("console_bg_alpha", "0.8").floatValue;
            background.color = bgc;
        }
    }

    protected void ToggleConsole()
    {
        if (_active)
            HideConsole();
        else
            ShowConsole();

        ClearCommandLine();
    }

    protected void ShowConsole()
    {
        Active = true;
        cns.gameObject.SetActive(true);
        FocusCommandLine();
    }

    protected void HideConsole()
    {
        Active = false;
        cns.gameObject.SetActive(false);
    }

    public void SubmitCommandLine()
    {
        CommandLine.text = CommandLine.text.Replace(Environment.NewLine, "");
        AddToHistroy(CommandLine.text);
        Com_printf("{0}\n", CommandLine.text);
        Commands.Instance.Cbuf_ExecuteText(CbufExec_t.EXEC_INSERT, CommandLine.text);
    }

    protected string GetHistory(int index)
    {
        return index >= 0 && index < historySize ? history[index] : "";
    }

    protected void AddToHistroy(string s)
    {
        history[historySize++] = s;
        historyIndex = historySize;
    }

    protected void DrawConsole()
    {
        if (!cns.activeInHierarchy)
            return;
        if (conText.Count == 0)
            return;
    //    index = conText.Count;
        int n = 15;
        int start = index - n < 0 ? 0 : index - n;
        consoleText.text = "";
        for (int i = start; i < start + n; i++)
        {
            if (i >= conText.Count)
                break;
            consoleText.text += conText[i];
        }
    }

    public void Com_printf(string format, params object[] args)
    {
        if (conText.Count > CON_MAX_LEN)
            ClearConsoleText();
        string s = string.Format(format, args);
        conText.Add(UlbeColorString.ColorizeString(s));
        index++;
    }

    public void Debug_printf(string format, params object[] arg)
    {
        if (!Application.isEditor)
            return;
        Com_printf(format, arg);
    }

    public void Debug_printlinef(string format, params object[] arg)
    {
        if (!Application.isEditor)
            return;
        Com_printf(string.Format("{0}\n", format), arg);
    }

    public void Com_printlinef(string format, params object[] arg)
    {
        Com_printf(string.Format("{0}\n", format), arg);
    }

    protected void ClearFocusCommandLine()
    {
        CommandLine.DeactivateInputField();
    }

    protected void FocusCommandLine()
    {
        CommandLine.ActivateInputField();
    }

    public void ClearConsoleText()
    {
        conText.Clear();
        index = 0;
        consoleText.text = "";
        DrawConsole();
    }

    protected void ClearCommandLine()
    {
        CommandLine.text = "";
    }

    private void OnApplicationQuit()
    {
        Commands.Instance.WriteConfig("default.cfg");
    }

    protected void AutoComplete(params string[][] arrs)
    {
        if (CommandLine.text == "")
            return;
        List<string> matches = new List<string>();
        // Add all matching strings

        foreach (string[] arr in arrs)
        {
            foreach (string s in arr)
            {
                if (s.StartsWith(CommandLine.text.ToLower()))
                    matches.Add(s);
            }
        }

        if (matches.Count == 0)
            return;
        if (matches.Count == 1)
            CommandLine.text = string.Format("{0} ", matches[0]);
        else
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < matches.Count; i++)
            {
                if (i != 0)
                    sb.Append(System.Environment.NewLine);
                sb.Append(matches[i]);
            }
            Com_printlinef("");
            // print
            foreach (string s in matches)
                Com_printlinef(s);
            CommandLine.text = LongestMatchingString(matches);
        }
        CommandLine.caretPosition = (CommandLine.text.Length);
    }

    protected string LongestMatchingString(List<string> words)
    {
        if (words.Count == 1)
            return words[0];
        if (words.Count == 0)
            return "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        string logest = "";
        foreach (string s in words)
            if (s.Length > logest.Length)
                logest = s;
        char temp;
        bool valid;
        for (int i = 0; i < logest.Length; i++)
        {
            temp = logest[i];
            valid = true;
            for(int j =0;j < words.Count; j++)
            {
                if (words[j].Length > i && words[j][i] == temp)
                    continue;
                valid = false;
                break;
            }
            if (!valid)
                break;
            sb.Append(temp);
        }
        return sb.ToString(); ;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class KeyPairData
{
    public KeyPairItem[] items;
}

[System.Serializable]
public class KeyPairItem
{
    public string key;
    public string value;
}

public class Commands : MonoBehaviour
{
    const int MAX_CMD_LENGTH = 512;
    public Dictionary<string, Command> cmd_functions;
    private static Commands s_instance;
    public static Commands Instance => s_instance ?? new GameObject("Commands").AddComponent<Commands>();
    protected const int MAX_STRING_TOKENS = 256;
    protected int cmd_argc;
    protected string cmd_cmd;
    protected string cmd_tokenized;
    protected string[] cmd_argv;
    protected string cmd_text;
    protected int cmd_wait;

    public void Awake()
    {
        if (s_instance != null)
            Destroy(gameObject);
        cmd_argv = new string[MAX_STRING_TOKENS];
        cmd_functions = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);
        Cmd_Init();
        DontDestroyOnLoad(gameObject);
        s_instance = this;
    }

    public void FixedUpdate()
    {
        Cbuf_Execute();
    }

    // Number of arguments
    public int Cmd_Argc()
    {
        return cmd_argc;
    }

    // Argument value at index "arg"
    public string Cmd_argv(int arg)
    {
        if (arg >= Cmd_Argc())
            return "";
        return cmd_argv[arg];
    }

    //Returns a single string containing argv(arg) to argv(argc()-1)
    protected string Cmd_argsFrom(int arg)
    {
        if (arg < 0)
            arg = 0;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = arg; i < Cmd_Argc(); i++)
            sb.Append(Cmd_argv(i)).Append(" ");
        return sb.ToString();
    }

    public string Cmd_Cmd()
    {
        return cmd_cmd;
    }

    protected void Cmd_TokenizeString(string text_in)
    {
        // clear previous args
        cmd_argc = 0;

        if (text_in == "")
            return;

        string text = text_in;

        if (cmd_argc == MAX_STRING_TOKENS)
            return;

        // Skip whitespace
        text = text.Trim();

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == ' ')
            {
                cmd_argv[cmd_argc] = sb.ToString();
                cmd_argc++;
                sb.Clear();
            }

            // handle qoutes
            else if (c == '\"')
            {
                i++;
                while (text[i] != '\"') { sb.Append(text[i]); i++; }
                cmd_argv[cmd_argc] = sb.ToString();
                cmd_argc++;
                sb.Clear();
            }
            else if (i + 1 >= text.Length)
            {
                sb.Append(c);
                cmd_argv[cmd_argc] = sb.ToString();
                cmd_argc++;
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
    }

    public void Cmd_AddCommand(string cmd_name, Command.Command_t fuction)
    {
        if (cmd_functions.ContainsKey(cmd_name))
        {
            if (fuction != null)
                Konsole.Instance.Com_printf("Cmd_AddCommand: {0} already defined\n", cmd_name);

            return;
        }

        cmd_functions.Add(cmd_name, new Command() { func = fuction, name = name });
    }

    public void Cmd_RemoveCommand(string cmd_name)
    {
        if (cmd_functions.ContainsKey(cmd_name))
            cmd_functions.Remove(cmd_name);
    }

    protected void Cmd_ExecuteString(string text)
    {
        cmd_cmd = text;

        Cmd_TokenizeString(text);

        if ((Cmd_Argc() <= 0) || text == "")
            return;

        Command cmd = cmd_functions.ContainsKey(Cmd_argv(0)) ? cmd_functions[Cmd_argv(0)] : null;

        if (cmd != null)
        {
            cmd.func?.Invoke();
            return;
        }

        if (Cvars.Instance.Cvar_Find(Cmd_argv(0)) != null)
        {
            if (Cmd_Argc() >= 2 && Cmd_argv(1) != "")
            {
                Cvars.Instance.Set(Cmd_argv(0), Cmd_argv(1), Application.isEditor);
            }
            else
            {
                Cvar c = Cvars.Instance.Cvar_Find(Cmd_argv(0));
                Konsole.Instance.Com_printlinef("\"{0}\" is: \"{1}\" default:\"{2}\"", new string[] { c.name, c.stringValue, c.resetString });
            }
        }
        else
        {
            Konsole.Instance.Com_printlinef("Unknown command: ^$\"{0}^$\"", Cmd_argv(0));
        }
    }

    //=======================================================================

    public void Cbuf_ExecuteText(CbufExec_t when, string text)
    {
        switch (when)
        {
            case CbufExec_t.EXEC_NOW:
                if (text != "")
                    Cmd_ExecuteString(text);
                else
                    Cbuf_Execute();
                break;
            case CbufExec_t.EXEC_INSERT:
                Cbuf_InsertText(text);
                break;
            case CbufExec_t.EXEC_APPEND:
                Cbuf_AddText(text);
                break;
            default:
                break;
        }
    }

    protected void Cbuf_InsertText(string text)
    {
        cmd_text = string.Format("{0}{1}\n", cmd_text, text);
    }

    // Adds command text at the end of the buffer, does NOT add a final \n
    protected void Cbuf_AddText(string text)
    {
        cmd_text = string.Format("{0}{1}", cmd_text, text);
    }

    protected const int MAX_CMD_LINE = 1024;

    // Pulls off \n terminated lines of text from the command buffer and sends
    // them through Cmd_ExecuteString.  Stops when the buffer is empty.
    // Normally called once per frame, but may be explicitly invoked.
    // Do not call inside a command function, or current args will be destroyed.
    protected void Cbuf_Execute()
    {
        string text;
        int quotes = 0;
        while (true)
        {
            if (cmd_text == null || cmd_text == "")
                break;

            if (cmd_wait > 0)
            {
                // skip out while text still remains in buffer, leaving it
                // for next frame
                cmd_wait--;
                break;
            }

            text = cmd_text;
            quotes = 0;

            int i;
            // find a \n or ; line break
            for (i = 0; i < cmd_text.Length; i++)
            {
                if (text[i] == '"')
                    quotes++;
                if (text[i] == ';' && (quotes % 2) == 0)
                    break;
                if (text[i] == '\n' || text[i] == '\r')
                    break;
            }

            if (i > MAX_CMD_LINE)
                i = MAX_CMD_LINE;

            string line = text.Substring(0, i);
            cmd_text = text.Substring(i + 1, text.Length - line.Length - 1);

            Cmd_ExecuteString(line);

            if (cmd_text.Length == 0)
                break;
        }
    }

    protected void Cmd_Vstr_f()
    {
        if (Cmd_Argc() != 2)
        {
            Konsole.Instance.Com_printf("vstr <variablename> : execute a variable command\n");
            return;
        }

        Cbuf_InsertText(string.Format("{0}\n", Cvars.Instance.Cvar_VariableString(Cmd_argv(1))));
    }

    protected void Cmd_Wait_f()
    {
        if (Cmd_Argc() == 2)
            cmd_wait = Cvars.Atoi(Cmd_argv(1));
        else
            cmd_wait = 0;
    }

    protected void Cmd_Echo_f()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 1; i < Cmd_Argc(); i++)
            sb.Append(Cmd_argv(i)).Append(i != Cmd_Argc() - 1 ? " " : "");
        Konsole.Instance.Com_printlinef("{0}", sb.ToString());
    }

    protected void Cmd_Clear_Console()
    {
        Konsole.Instance.ClearConsoleText();
    }

    protected void Cmd_Quit()
    {
        if (Application.isEditor)
            Konsole.Instance.Com_printf("{0}\n", "Unable to quit game in editor mode.");
        Application.Quit();
    }

    public void WriteConfig(string filename)
    {
        List<KeyPairItem> data = new List<KeyPairItem>();
        KeyPairData kpd = new KeyPairData();

        foreach (KeyValuePair<string, Cvar> sc in Cvars.Instance.cvarList)
            if ((sc.Value.flags & (Cvar.CvarFlags.INIT | Cvar.CvarFlags.ROM | Cvar.CvarFlags.CHEAT)) == 0)
                data.Add(new KeyPairItem() { key = sc.Key, value = sc.Value.stringValue });

        kpd.items = data.ToArray();

        string json = JsonUtility.ToJson(kpd);

        string path = string.Format("{0}/config/{1}", Application.persistentDataPath, filename);

        // No config folder
        if (!System.IO.File.Exists(path))
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.persistentDataPath, "config"));

        System.IO.File.WriteAllText(path, json);

        Konsole.Instance.Com_printlinef("{0} saved.", filename);
    }

    public void WriteConfig()
    {
        if (Cmd_Argc() != 2)
            return;
        string filename = Cmd_argv(1).EndsWith(".cfg") ?Cmd_argv(1) : string.Format("{0}.cfg", Cmd_argv(1));
        WriteConfig(filename);
    }

    protected void Cmd_Exec_f()
    {
        if (Cmd_Argc() < 2)
        {
            Konsole.Instance.Com_printlinef("Invalid args");
            return;
        }

        string filename = Cmd_argv(1).EndsWith(".cfg") ? Instance.Cmd_argv(1) : string.Format("{0}.cfg", Cmd_argv(1));

        string path = string.Format("{0}/config/{1}", Application.persistentDataPath, filename);

        if (System.IO.File.Exists(path))
        {
            string text = System.IO.File.ReadAllText(path);
            KeyPairData d = JsonUtility.FromJson<KeyPairData>(text);
            foreach (KeyPairItem kp in d.items)
                Cbuf_ExecuteText(CbufExec_t.EXEC_INSERT, string.Format("{0} {1}", kp.key, kp.value));
            Konsole.Instance.Com_printlinef("\"{0}\" loaded!", filename);
            return;
        }

        Konsole.Instance.Com_printlinef("File not found. ({0})", filename);
    }

    protected void Cmd_List_f()
    {
        foreach (KeyValuePair<string, Command> kp in cmd_functions)
            Konsole.Instance.Com_printlinef("\t{0}", kp.Key);
        Konsole.Instance.Com_printf("{0} commands found\n", cmd_functions.Count.ToString());
    }

    protected void Cvar_List_f()
    {

        foreach (KeyValuePair<string, Cvar> sc in Cvars.Instance.cvarList)
        {
            Konsole.Instance.Com_printlinef
                ("{0}: Value: \"{1}\" Default: \"{2}\"", 
                new string[] { sc.Value.name, sc.Value.stringValue, sc.Value.resetString});
        }
    }

    //=======================================================================

    protected void Cmd_Init()
    {
        // Cmd cmds
        Cmd_AddCommand("cmdlist", Cmd_List_f);
        Cmd_AddCommand("cvarlist", Cvar_List_f);
        Cmd_AddCommand("vstr", Cmd_Vstr_f);
        Cmd_AddCommand("echo", Cmd_Echo_f);
        Cmd_AddCommand("wait", Cmd_Wait_f);
        Cmd_AddCommand("clear", Cmd_Clear_Console);
        Cmd_AddCommand("quit", Cmd_Quit);
        Cmd_AddCommand("writeconfig", WriteConfig);
        Cmd_AddCommand("exec", Cmd_Exec_f);
    }
}

public class Command
{
    public delegate void Command_t();
    public string name;
    public Command_t func;
}

public enum CbufExec_t
{
    EXEC_NOW = 0,
    EXEC_INSERT,
    EXEC_APPEND
}
namespace Ulbe
{
    public class Cvar
    {
        [System.Flags]
        public enum CvarFlags
        {
            CHEAT = 1,
            INIT = 2,
            ROM = 4,
            USER_CREATED = 8,
        }

        public string name;
        public CvarFlags flags;

        public string resetString;
        public string stringValue;
        public float floatValue;
        public int intValue;
        public bool BoolValue { get { return intValue > 0; } }

        public string Info
        {
            get
            {
                return string.Format("Value: {0}, Default: {1}", stringValue, resetString);
            }
        }
    }
}
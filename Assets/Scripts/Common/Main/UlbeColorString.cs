using System.Text;

// Quake3 colors
public class UlbeColorString
{
    public const char COLOR_ESCAPE = '^';
    public const char COLOR_NONE = '$';
    public const char COLOR_BLACK = '0';
    public const char COLOR_RED = '1';
    public const char COLOR_GREEN = '2';
    public const char COLOR_YELLOW = '3';
    public const char COLOR_BLUE = '4';
    public const char COLOR_CYAN = '5';
    public const char COLOR_MAGENTA = '6';
    public const char COLOR_WHITE = '7';
    public const char COLOR_ORANGE = '8'; //FC660
    public const char COLOR_GOLD = 'a'; // F9A602
    public const char COLOR_AMBER = 'b'; // 8B4000
    public const char COLOR_MAROON = 'c'; // 800000
    public const char COLOR_RASPBERRY = 'd'; // D21F3C
    public const char COLOR_CARMINE = 'e'; // B80F0A
    public const char COLOR_RUBY = 'f'; // E0115F
    public const char COLOR_PINK = 'g'; // FC0FC0
    public const char COLOR_THULIAN = 'h'; // FDE6FA1
    public const char COLOR_EGGPLANT = 'i'; // 311432
    public const char COLOR_ELECTRIC = 'j'; // 8F00FF
    public const char COLOR_LILAC = 'k'; // B660CD
    public const char COLOR_YALE = 'l'; // 0E4C92
    public const char COLOR_DENIM = 'm'; // 131E3A
    public const char COLOR_SAPPHIRE = 'n'; // 0F52BA
    public const char COLOR_FOREST = 'o'; // 0B6623
    public const char COLOR_JADE = 'p'; // 00A86B
    public const char COLOR_FERN = 'q'; // 4F7942
    public const char COLOR_OLIVE = 'r'; // 708238
    public const char COLOR_ARMY = 's'; // 4B5320
    public const char COLOR_SHADOW = 't'; // 363636
    public const char COLOR_SMOKE = 'u'; // BEBDB8
    public const char COLOR_RHINO = 'v'; // B9BBB6
    public const string S_COLOR_BLACK = "^0";
    public const string S_COLOR_RED = "^1";
    public const string S_COLOR_GREEN = "^2";
    public const string S_COLOR_YELLOW = "^3";
    public const string S_COLOR_BLUE = "^4";
    public const string S_COLOR_CYAN = "^5";
    public const string S_COLOR_MAGENTA = "^6";
    public const string S_COLOR_WHITE = "^7";
    public const string S_COLOR_ORANGE = "^8"; //FC660
    public const string S_COLOR_GOLD = "^a"; // F9A602
    public const string S_COLOR_AMBER = "^b"; // 8B4000
    public const string S_COLOR_MAROON = "^c"; // 800000
    public const string S_COLOR_RASPBERRY = "^d"; // D21F3C
    public const string S_COLOR_CARMINE = "^e"; // B80F0A
    public const string S_COLOR_RUBY = "^f"; // E0115F
    public const string S_COLOR_PINK = "^g"; // FC0FC0
    public const string S_COLOR_THULIAN = "^h"; // FDE6FA1
    public const string S_COLOR_EGGPLANT = "^i"; // 311432
    public const string S_COLOR_ELECTRIC = "^j"; // 8F00FF
    public const string S_COLOR_LILAC = "^k"; // B660CD
    public const string S_COLOR_YALE = "^l"; // 0E4C92
    public const string S_COLOR_DENIM = "^m"; // 131E3A
    public const string S_COLOR_SAPPHIRE = "^n"; // 0F52BA
    public const string S_COLOR_FOREST = "^o"; // 0B6623
    public const string S_COLOR_JADE = "^p"; // 00A86B
    public const string S_COLOR_FERN = "^q"; // 4F7942
    public const string S_COLOR_OLIVE = "^r"; // 708238
    public const string S_COLOR_ARMY = "^s"; // 4B5320
    public const string S_COLOR_SHADOW = "^t"; // 363636
    public const string S_COLOR_SMOKE = "^u"; // BEBDB8
    public const string S_COLOR_RHINO = "^v"; // B9BBB6
    public const string S_COLOR_NONE = "^$";

    public static string GetColor(char c)
    {
        switch (c)
        {
            case COLOR_BLACK: return "black";
            case COLOR_RED: return "red";
            case COLOR_GREEN: return "green";
            case COLOR_BLUE: return "blue";
            case COLOR_YELLOW: return "yellow";
            case COLOR_CYAN: return "cyan";
            case COLOR_MAGENTA: return "magenta";
            case COLOR_WHITE: return "white";
            case COLOR_ORANGE: return "#FC6600";
            case COLOR_GOLD: return "#F9A602";
            case COLOR_AMBER: return "#8B4000";
            case COLOR_MAROON: return "#800000";
            case COLOR_RASPBERRY: return "#D21F3C";
            case COLOR_CARMINE: return "#B80F0A";
            case COLOR_RUBY: return "#E0115F";
            case COLOR_PINK: return "#FC0FC0";
            case COLOR_THULIAN: return "#DE6FA1";
            case COLOR_EGGPLANT: return "#311432";
            case COLOR_ELECTRIC: return "#8F00FF";
            case COLOR_LILAC: return "#B660CD";
            case COLOR_YALE: return "#0E4C92";
            case COLOR_DENIM: return "#131E3A";
            case COLOR_SAPPHIRE: return "#0F52BA";
            case COLOR_FOREST: return "#0B6623";
            case COLOR_JADE: return "#00A86B";
            case COLOR_FERN: return "#4F7942";
            case COLOR_OLIVE: return "#708238";
            case COLOR_ARMY: return "#4B5320";
            case COLOR_SHADOW: return "#363636";
            case COLOR_SMOKE: return "#BEBDB8";
            case COLOR_RHINO: return "#B9BBB6";
            default:
                return GetColor(COLOR_WHITE);
        }
    }

    protected static bool IsColorString(string text)
    {
        return text.Length >= 2 && text[0] == COLOR_ESCAPE && text[1] != COLOR_ESCAPE;
    }

    public static string EscapeColors(string text)
    {
        StringBuilder sb = new StringBuilder();
        int i = 0;
        while (i < text.Length)
        {
            if (IsColorString(text.Substring(i, text.Length - (i + 1))))
                i += 2;
            sb.Append(text[i++]);
        }
        return sb.ToString();
    }

    // Quake 3 style string coloring
    public static string ColorizeString(string text)
    {
        StringBuilder sb = new StringBuilder();
        int i = 0;
        bool isColor = false;

        while (i < text.Length)
        {
            if (IsColorString(text.Substring(i, text.Length - (i + 1))))
            {
                if (text[i + 1] == COLOR_NONE)
                {
                    // Use default text color
                    if (isColor)
                        sb.Append("</color>");
                    isColor = false;
                    i += 2;
                    continue;
                }

                // End of old color
                if (isColor)
                    sb.Append("</color>");

                // New color
                isColor = true;
                sb.Append("<color=").Append(GetColor(text[i + 1])).Append(">");
                i += 2;
                continue;
            }

            // Add char
            sb.Append(text[i++]);
        }

        // Close color tag
        if (isColor)
            sb.Append("</color>");

        return sb.ToString();
    }
}
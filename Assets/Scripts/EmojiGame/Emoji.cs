using UnityEngine;

namespace Emojigame
{
    [System.Serializable]
    public class Emoji
    {
        public Vector2Int pos = Vector2Int.zero;
        public const int NONE = -1;
        public int emojiType = NONE;
        public bool seleted = false;
        public Emoji(int x, int y, int emojiType = NONE)
        {
            pos = new Vector2Int(x, y);
            this.emojiType = emojiType;
        }
        public bool IsEmpty { get { return emojiType == NONE; } }
        public bool SameType(Emoji c) { return c.emojiType == emojiType; }
    }
}
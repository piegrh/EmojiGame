using System.Collections.Generic;
using UnityEngine;

namespace Emojigame
{
    public class EmojiMap
    {
        protected Emoji[,] map;
        public Vector2 EmojiSize { get; protected set; } = new Vector2(100, 100);
        public Vector2 Offset { get; protected set; }
        public Vector2 Size { get; protected set; }
        static readonly Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(1,0), // left
            new Vector2Int(-1,0), // right
            new Vector2Int(0,1), // up
            new Vector2Int(0,-1) // down
        };

        public EmojiMap(Vector2 area, Vector2 emojiSize)
        {
            Init(area, emojiSize);
        }

        protected void Init(Vector2 size, Vector2 emojiSize)
        {
            EmojiSize = emojiSize;
            Size = size;

            Offset = new Vector2(EmojiSize.x / 2, EmojiSize.y / 2);
            Size = new Vector2(Size.x / (EmojiSize.x), Size.y / (EmojiSize.y));
            map = new Emoji[(int)Size.x, (int)Size.y];

            CreateEmojis();
        }

        protected void CreateEmojis()
        {
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x; x++)
                    map[x, y] = new Emoji(x, y);
        }

        public EmojiMap(int x, int y)
        {
            Size = new Vector2(x, y);
            map = new Emoji[x, y];
            Offset = Vector2.zero;
            EmojiSize = Vector2.zero;
            CreateEmojis();
        }

        public virtual Emoji[] ShiftDown()
        {
            return ShiftDown(LengthX - 1, 0, LengthY - 2);
        }

        // Returns emoji that were moved
        public virtual Emoji[] ShiftDown(int startX, int endX, int startY)
        {
            startY = Mathf.Clamp(startY, 0, LengthY - 2);
            List<Emoji> movedEmojisY = new List<Emoji>();

            // Move vertical
            for (int y = startY; y >= 0; y--)
            {
                for (int x = startX; x >= endX; x--)
                {
                    Emoji temp = GetEmoji(x, y);

                    if (temp.IsEmpty)
                        continue;

                    Emoji under;
                    int cnt = 1;
                    bool valid = false;

                    // shift down while the emoji below is empty
                    while ((under = GetEmoji(x, y + cnt)) != null && under.IsEmpty)
                    {
                        valid = true;
                        cnt++;
                    };

                    if (valid)
                    {
                        Swap(new Vector2Int(x, y), new Vector2Int(x, y + --cnt));
                        movedEmojisY.Add(GetEmoji(x, y));
                        movedEmojisY.Add(GetEmoji(x, y + cnt));
                    }
                }
            }

            return movedEmojisY.ToArray();
        }

        // Returns emoji that were moved
        public virtual Emoji[] ShiftLeft(int startX = 0)
        {
            List<Emoji> movedEmojisX = new List<Emoji>();
            // Move Horizontal
            for (int x = startX; x < LengthX; x++)
            {
                Emoji temp = GetEmoji(x, LengthY - 1);
                if (temp.IsEmpty)
                {
                    for (int i = x; i < LengthX; i++)
                    {
                        if (!GetEmoji(i, LengthY - 1).IsEmpty)
                        {
                            // shift vertical row to the left
                            for (int y = 0; y < LengthY; y++)
                            {
                                Swap(new Vector2Int(x, y), new Vector2Int(i, y));
                                movedEmojisX.Add(GetEmoji(x, y));
                                movedEmojisX.Add(GetEmoji(i, y));
                            }
                            break;
                        }
                    }
                }
            }
            return movedEmojisX.ToArray();
        }

        public Emoji[] GetNeighbors(Emoji c)
        {
            List<Emoji> emoji = new List<Emoji>();
            foreach (Vector2Int dir in dirs)
                if (TryGetNeighbor(c, dir, out Emoji temp))
                    emoji.Add(temp);
            return emoji.ToArray();
        }

        bool TryGetNeighbor(Emoji origin, Vector2Int dir, out Emoji neighbor)
        {
            return (neighbor = GetEmoji(origin.pos.x + dir.x, origin.pos.y + dir.y)) != null && neighbor.SameType(origin);
        }

        public bool HasNeighbours(Emoji c)
        {
            return GetNeighbors(c).Length > 0;
        }

        protected void Swap(Vector2Int a, Vector2Int b)
        {
            Emoji emojiA = GetEmoji(a);
            Emoji emojiB = GetEmoji(b);
            emojiA.pos = b;
            emojiB.pos = a;
            map[b.x, b.y] = emojiA;
            map[a.x, a.y] = emojiB;
        }

        public Emoji GetEmoji(int x, int y)
        {
            return x >= 0 && x < LengthX && (y >= 0 && y < LengthY) ? map[x, y] : null;
        }

        public Emoji GetEmoji(Vector2Int v)
        {
            return GetEmoji(v.x, v.y);
        }

        public int LengthX
        {
            get { return map.GetLength(0); }
        }

        public int LengthY
        {
            get { return map.GetLength(1); }
        }

        public int Count
        {
            get
            {
                return LengthX * LengthY;
            }
        }

        public Vector3 GetCanvasPosition(Vector2Int pos)
        {
            return GetCanvasPosition(pos.x, pos.y);
        }

        public Vector3 GetCanvasPosition(int x, int y)
        {
            return new Vector3((EmojiSize.x * x) + Offset.x, (-EmojiSize.y * y) - Offset.y, 0);
        }
    }
}
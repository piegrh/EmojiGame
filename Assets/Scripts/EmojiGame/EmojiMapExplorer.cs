using System.Collections.Generic;
using UnityEngine;

namespace Emojigame
{
    public class EmojiMapExplorer
    {
        public Emoji[] visited;
        public List<KeyValuePair<Emoji, Emoji>> path;
        protected LineRenderer[] lrend;

        public EmojiMapExplorer(EmojiMap map, Emoji start)
        {
            visited = Explore(map, start, out path).ToArray();
        }

        public static List<Emoji> Explore(EmojiMap map, Emoji start, out List<KeyValuePair<Emoji, Emoji>> path)
        {
            path = new List<KeyValuePair<Emoji, Emoji>>();
            List<Emoji> visitedEmojis = new List<Emoji>();
            Queue<Emoji> q = new Queue<Emoji>();
            q.Enqueue(start);
            Emoji temp;
            Emoji[] adjEmojis;
            while (q.Count > 0)
            {
                temp = q.Dequeue();
                if (!visitedEmojis.Contains(temp))
                    visitedEmojis.Add(temp);
                else
                    continue;
                adjEmojis = map.GetNeighbors(temp);
                for (int i = 0; i < adjEmojis.Length; i++)
                {
                    if (!visitedEmojis.Contains(adjEmojis[i]))
                    {
                        q.Enqueue(adjEmojis[i]);
                        path.Add(new KeyValuePair<Emoji, Emoji>(temp, adjEmojis[i]));
                    }
                }
            }
            return visitedEmojis;
        }

        public static List<Emoji> Explore(EmojiMap map, Emoji start)
        {
            return Explore(map, start, out List<KeyValuePair<Emoji, Emoji>> path);
        }
    }

}

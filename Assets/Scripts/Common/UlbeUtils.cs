using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UlbeUtils
    {
        public static T GetRandomFromCollection<T>(params T[] arr)
        {
            return arr != null && arr.Length > 0 ? arr[Random.Range(0, arr.Length)] : default;
        }

        public static T GetRandomFromCollection<T>(List<T> arr)
        {
            return arr != null && arr.Count > 0 ? arr[Random.Range(0, arr.Count)] : default;
        }

        public static Color GetColorFromRGBA(int r, int g, int b, float a = 1f)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a);
        }

        public static Color GetRandomColor(float a = 1f)
        {
            return GetColorFromRGBA(Random.Range(0, 256), Random.Range(0, 256), Random.Range(0, 256), a);
        }

        public static void WriteTextToPersistentDataPath(string filename, string text)
        {
            string path = string.Format("{0}/{1}", Application.persistentDataPath, filename);
            System.IO.File.WriteAllText(path, text);
        }

        // TotalMilliseconds since 1970 01 01...
        public static long Timestamp()
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            long currentEpochTime = (long)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;
            return currentEpochTime;
        }

        public static System.DateTime TimestampToDateTime(long timestamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(timestamp).ToLocalTime();
            return dtDateTime;
        }

        public static Gradient GradientRainbow(float alpha = 0.25f)
        {
            return GradientAtoN(alpha, Color.red,Color.yellow, Color.green,Color.blue,Color.magenta);
        }

        public static Gradient GradientRandom(float alpha = 0.25f)
        {
            return GradientRandom(alpha, Random.Range(1,9));
        }

        public static Gradient GradientRandom(float alpha, int colorCount)
        {
            colorCount = Mathf.Clamp(colorCount, 1, 8);
            List<Color> colors = new List<Color>();

            for(int i = 0; i < colorCount; i++)
                colors.Add(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));

            return GradientAtoN(alpha, colors.ToArray());
        }

        public static Color RandomFromGradient(Gradient g)
        {
            return g.Evaluate(Random.Range(0, 100) / 100f);
        }

        public static Gradient GradientAtoN(float alpha, params Color[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int max = Mathf.Clamp(colors.Length, 0, 8);

            Gradient g = new Gradient();
            GradientColorKey[] gck = new GradientColorKey[colors.Length];

            float time = ((1f) / (max - 1f));

            for (int i = 0; i < max; i++)
            {
                gck[i].color = colors[i];
                gck[i].time = i * time;
            }

            GradientAlphaKey[] gak = new GradientAlphaKey[1];
            gak[0].alpha = 1;
            gak[0].time = 1f;

            g.SetKeys(gck, gak);
            return g;
        }
}
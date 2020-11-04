using UnityEngine;
using UnityEngine.UI;


namespace Ulbe.UI
{
    public class Fps : MonoBehaviour
    {
        Text fpstext;
        float deltaTime = 0;
        Cvar drawfps;

        public void Awake()
        {
            drawfps = Cvars.Instance.Get("hud_drawfps", "0");
            fpstext = GetComponent<Text>();
            fpstext.text = "";
        }

        public string GetFPS
        {
            get
            {
                return string.Format("{0} Fps", (1.0f / deltaTime).ToString("#."));
            }
        }

        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

            if (!drawfps.BoolValue)
            {
                if (fpstext.text.Length > 0)
                    fpstext.text = "";
                return;
            }

            fpstext.text = GetFPS;
        }
    }
}
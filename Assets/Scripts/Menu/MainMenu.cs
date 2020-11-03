using System.Collections.Generic;
using Ulbe;
using UnityEngine;
using UnityEngine.UI;

namespace Emojigame
{

    public class MainMenu : MonoBehaviour
    {
        public Dropdown Levelsize;
        public Dropdown Difficulty;

        // Start is called before the first frame update
        void Start()
        {
            List<Dropdown.OptionData> sizeOpts = new List<Dropdown.OptionData>();

            int cnt = -1;

            foreach (CellGameSettings.LevelSize val in System.Enum.GetValues(typeof(CellGameSettings.LevelSize)))
            {
                sizeOpts.Add(new Dropdown.OptionData() { text = val.ToString() });
                cnt++;
            }

            Levelsize.options = sizeOpts;
            Levelsize.value = Mathf.Clamp(Cvars.Instance.Get("g_levelsize", "3").intValue, 0, cnt);


            cnt = -1;
            List<Dropdown.OptionData> sizeOptsB = new List<Dropdown.OptionData>();
            foreach (CellGameSettings.Difficulty val in System.Enum.GetValues(typeof(CellGameSettings.Difficulty)))
            {
                sizeOptsB.Add(new Dropdown.OptionData() { text = val.ToString() });
                cnt++;
            }

            Difficulty.options = sizeOptsB;
            Difficulty.value = Mathf.Clamp(Cvars.Instance.Get("g_difficulty", "0").intValue, 0, cnt);
        }

        public void StartGame()
        {
            Cvars.Instance.ForceSet("g_difficulty", Difficulty.value.ToString());
            Cvars.Instance.ForceSet("g_levelsize", Levelsize.value.ToString());
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }

        public void Exit()
        {
            Application.Quit();
        }

    }

}
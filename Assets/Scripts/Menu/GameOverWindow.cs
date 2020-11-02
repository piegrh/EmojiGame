using Ulbe;
using UnityEngine;
using UnityEngine.UI;

namespace Emojigame
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] public GameObject window;
        [SerializeField] public Text ScoreBox;
        Image _image;

        public void Awake()
        {
            GameEvents.Instance.OnGameOver += SetScoreTable;
            GameEvents.Instance.OnReset += Hide;
            _image = GetComponent<Image>();
        }

        public void Start()
        {
            SetEnable(false);
        }

        public void SetScoreTable(int baseScore, int celltypes, bool bonus)
        {
            string s = string.Format(
                    "Base Score: {0}\nElimination Bonus: {1}\n^6Final Score: {2}",
                    baseScore.ToString(),
                    bonus ? string.Format("x{0}", celltypes) : "None",
                    GameScorer.FinalScore(baseScore, celltypes, bonus)
                );
            ScoreBox.text = ColorString.ColorizeString(s);
            SetEnable(true);
        }

        public void Hide()
        {
            SetEnable(false);
        }

        public void SetEnable(bool value)
        {
            _image.enabled = value;
            window.gameObject.SetActive(value);
        }

        private void OnDestroy()
        {
            GameEvents.Instance.OnGameOver -= SetScoreTable;
            GameEvents.Instance.OnReset -= Hide;
        }
    }

}

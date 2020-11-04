using Ulbe;
using UnityEngine;
using UnityEngine.UI;

namespace Emojigame.UI.Menu
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
            string bonusmsg = bonus ? string.Format("x{0}", celltypes) : "None";
            string finalScore = GameScorer.FinalScore(baseScore, celltypes, bonus).ToString();
            string msg = $"Base Score: {baseScore}\nElimination Bonus: {bonusmsg}\n^6Final Score: {finalScore}";
            ScoreBox.text = ColorString.ColorizeString(msg);
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

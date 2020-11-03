using System.Collections;
using Ulbe;
using UnityEngine;
using UnityEngine.UI;

namespace Emojigame
{
    public class GameHud : MonoBehaviour
    {
        public Text scoreText;
        public Text selectionValue;
        int score = 0;
        int addValue = 0;

        [SerializeField] public Pulse pulse;

        void Start()
        {
            GameEvents.Instance.OnScore += SetScore;
            GameEvents.Instance.OnCellSelect += CellSelection;
            GameEvents.Instance.OnReset += ResetGame;
            SetScore(0);
        }

        void SetScore(int s)
        {
            addValue += s;
            score += addValue;
            StartCoroutine(CountUpCoroutine());
            CellSelection(0);
        }

        protected IEnumerator CountUpCoroutine()
        {
            while (addValue > 0)
            {
                pulse.enabled = true;
                scoreText.text = (score - addValue).ToString();
                yield return new WaitForFixedUpdate();
                addValue -= GetSpeed(addValue);
            }

            pulse.enabled = false;
            addValue = 0;
        }

        protected int GetSpeed(int v)
        {
            const int den = 10;

            int value = v / den;
            return value > 0 ? value : v % den;
        }

        protected void CellSelection(int cnt)
        {
            if (cnt == 0)
            {
                selectionValue.text = "";
                return;
            }
            selectionValue.text = ColorString.ColorizeString(string.Format("^3{0} ^7emojis will get you ^3{1} ^7points!", cnt, GameScorer.GetScore(cnt)));
        }

        private void ResetGame()
        {
            addValue = 0;
            score = 0;
            SetScore(0);
        }

        private void OnDestroy()
        {
            GameEvents.Instance.OnScore -= SetScore;
            GameEvents.Instance.OnReset -= ResetGame;
            GameEvents.Instance.OnCellSelect -= CellSelection;
        }
    }

}
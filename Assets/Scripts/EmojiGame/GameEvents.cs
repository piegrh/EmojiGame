﻿using Ulbe;

namespace Emojigame
{
    public class GameEvents : UnitySingleton<GameEvents>
    {
        public event System.Action<Emoji> OnEmojiClick;
        public event System.Action<int> OnCellSelect;
        public event System.Action<int> OnScore;
        public event System.Action OnReset;
        public event System.Action<int, int, bool> OnGameOver;

        protected override void Awake()
        {
            base.Awake();
            if (_Instance != this)
                return;
            DontDestroyOnLoad(gameObject);
        }

        public void GameOver(int score, int celltypes, bool perfect)
        {
            OnGameOver?.Invoke(score, celltypes, perfect);
        }

        public void ResetGame()
        {
            OnReset?.Invoke();
        }

        public void Score(int score)
        {
            OnScore?.Invoke(score);
        }

        public void CellClick(Emoji e)
        {
            OnEmojiClick?.Invoke(e);
        }

        public void EmojiSelect(int selectedCnt)
        {
            OnCellSelect?.Invoke(selectedCnt);
        }
    }
}

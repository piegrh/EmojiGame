using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Ulbe;
using Emojigame.Effects;

namespace Emojigame
{
    public struct EmojiGameSettings
    {
        public enum LevelSize { Small = 0, Normal, Big, Huge, Ginormous, Extreme, GIGANTISCH };
        public enum Difficulty { Normal = 0, Intermediate, Hard, Nightmare, UltraNightmare };
        public LevelSize size;
        public Difficulty difficulty;
    }

    public class EmojiGame : MonoBehaviour
    {
        enum GameState { WaitForClick, Selected, Clicked, GameOver }
        GameState state;
        public GameObject gamePanel;
        public GameObject emojiPrefab;
        public Vector2 padding = new Vector2(0, 0);
        public Sprite[] Sprites { get; protected set; }
        public AudioClip killSfx;
        EmojiMap _map;
        Emoji selected;
        EmojiGameSettings settings;
        Dictionary<Emoji, EmojiView> emojis;
        int score;

        void Start()
        {
            GameEvents.Instance.OnEmojiClick += EmojiClickEvent;
            Init();
        }

        private void Init()
        {
            emojis = new Dictionary<Emoji, EmojiView>();

            settings = new EmojiGameSettings
            {
                difficulty = (EmojiGameSettings.Difficulty)Cvars.Instance.Get("g_difficulty", "0").intValue,
                size = (EmojiGameSettings.LevelSize)Cvars.Instance.Get("g_levelsize", "2").intValue
            };

            Vector2 emojiSize = new Vector2(100, 100);
            emojiSize /= (1 + (int)settings.size);
            emojiPrefab.GetComponent<RectTransform>().sizeDelta = emojiSize;

            Vector2 levelSize = gamePanel.GetComponent<RectTransform>().sizeDelta;
            _map = new EmojiMap(levelSize, emojiSize);

            Sprites = new Sprite[2 + (int)settings.difficulty];

            SpawnEmojis();
            ResetGame();
        }

        private void SpawnEmojis()
        {
            Transform tParent = gamePanel.transform;
            EmojiView cv;
            for (int y = 0; y < _map.LengthY; y++)
            {
                for (int x = 0; x < _map.LengthX; x++)
                {
                    cv = Instantiate(emojiPrefab, tParent).GetComponent<EmojiView>();
                    cv.SetPadding(padding);
                    cv.emoji = _map.GetEmoji(x, y);
                    cv.name = string.Format("({0},{1})", x, y);
                    emojis.Add(cv.emoji, cv);
                    cv.SetPosition(_map.GetCanvasPosition(x, y));
                }
            }
        }

        public void ResetGame()
        {
            GameEvents.Instance.ResetGame();
            state = GameState.WaitForClick;
            score = 0;

            EmojiView view;

            for (int y = 0; y < _map.LengthY; y++)
            {
                for (int x = 0; x < _map.LengthX; x++)
                {
                    view = GetView(x, y);
                    view.emoji.seleted = false;
                    view.emoji.emojiType = Random.Range(0, Sprites.Length);
                    view.Show();
                }
            }

            ChangeAllEmojiSprites();
        }

        public void EmojiClickEvent(Emoji emoji)
        {
            switch (state)
            {
                case GameState.Selected:
                    // Double click?
                    if (IsSelected(emoji))
                    {
                        DeSelectConnectedEmojis(emoji);
                        if (_map.HasNeighbours(emoji))
                        {
                            state = GameState.Clicked;
                            StartCoroutine(ClickEmojiCoroutine(emoji));
                        }
                        return;
                    }
                    // No double click
                    DeSelectConnectedEmojis(selected);
                    Select(emoji);
                    break;
                case GameState.WaitForClick:
                    Select(emoji);
                    state = GameState.Selected;
                    break;
                default:
                    return;
            }
        }

        private void GameOver()
        {
            state = GameState.GameOver;
            bool perfect = IsPerfectGame(_map);
            GameEvents.Instance.GameOver(score, Sprites.Length, perfect);
            if (perfect)
                SoundMaster.Instance.PlayGlobalSound(Resources.Load<AudioClip>("sound/feedback/perfect"), 0.5f);
        }

        public static bool IsPerfectGame(EmojiMap map)
        {
            return map.GetEmoji(0, map.LengthY - 1).IsEmpty;
        }

        public static bool IsGameOver(EmojiMap map)
        {
            for (int y = 0; y < map.LengthY; y++)
            {
                for (int x = 0; x < map.LengthX; x++)
                {
                    Emoji c = map.GetEmoji(x, y);
                    if (c != null && !c.IsEmpty && map.HasNeighbours(c))
                        return false;
                }
            }
            return true;
        }

        private void Select(Emoji c)
        {
            int connectedCnt = SelectConnectedEmojis(c);
            selected = c;
            c.seleted = true;
            GameEvents.Instance.EmojiSelect(connectedCnt);
        }

        private IEnumerator ClickEmojiCoroutine(Emoji c)
        {
            // Draw lines
            EmojiMapExplorer explorer = new EmojiMapExplorer(_map, c);
            List<LineRenderer> lines = new List<LineRenderer>();
            Vector3 a, b;

            foreach (KeyValuePair<Emoji, Emoji> visited in explorer.path)
            {
                a = GetView(visited.Key).RectTransofrm.position;
                b = GetView(visited.Value).RectTransofrm.position;
                lines.Add(PathRenderer.Render(a, b));
            }

            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < lines.Count; i++)
                Destroy(lines[i].gameObject);

            int startY = -1;
            int startX = -1;
            int endX = int.MaxValue;
            int _score = GameScorer.GetScore(explorer.visited.Length);
            int scorePerFrame = _score / explorer.visited.Length;

            foreach (Emoji emoji in explorer.visited)
            {
                // Get upper and lower x bounds to avoid checking unaffected rows
                startX = emoji.pos.x > startX ? emoji.pos.x : startX;
                endX = emoji.pos.x < endX ? emoji.pos.x : endX;

                // Get lower y bounds.
                startY = emoji.pos.y > startY ? emoji.pos.y : startY;
                GetView(emoji).Explode();
                AddScore(scorePerFrame);
                SoundMaster.Instance.PlayGlobalSound(killSfx, 0.15f, SoundMaster.SoundType.SFX, Random.Range(1f, 3f));
                yield return new WaitForFixedUpdate();
            }

            AddScore(_score % explorer.visited.Length);

            // Update map
            Emoji[] movedEmojisY = _map.ShiftDown(startX, endX, startY);
            Emoji[] movedEmojisX = _map.ShiftLeft(endX);

            List<KeyValuePair<EmojiView, Vector3>> moveUs;

            moveUs = GetEmojisToBeMovedY(movedEmojisY);

            // Move down
            yield return new WaitUntil(() => MoveAll(moveUs));

            moveUs.Clear();

            moveUs = GetEmojisToBeMovedX(movedEmojisX);

            // Move left
            yield return new WaitUntil(() => MoveAll(moveUs));

            state = GameState.WaitForClick;

            if (IsGameOver(_map))
                GameOver();
        }

        private List<KeyValuePair<EmojiView, Vector3>> GetEmojisToBeMovedX(Emoji[] movedEmojisX)
        {
            List<KeyValuePair<EmojiView, Vector3>> toBeMoved = new List<KeyValuePair<EmojiView, Vector3>>();

            for (int i = 0; i < movedEmojisX.Length; i++)
            {
                EmojiView temp = GetView(movedEmojisX[i]);
                Vector3 oldPos = temp.transform.localPosition;
                Vector3 newPos = _map.GetCanvasPosition(movedEmojisX[i].pos);
                Vector3 targetPosition = new Vector3(newPos.x, oldPos.y, 0);
                toBeMoved.Add(new KeyValuePair<EmojiView, Vector3>(temp, targetPosition));
            }

            return toBeMoved;
        }

        private List<KeyValuePair<EmojiView, Vector3>> GetEmojisToBeMovedY(Emoji[] movedEmojisY)
        {
            List<KeyValuePair<EmojiView, Vector3>> toBeMoved = new List<KeyValuePair<EmojiView, Vector3>>();
            for (int i = 0; i < movedEmojisY.Length; i++)
            {
                EmojiView temp = GetView(movedEmojisY[i]);
                Vector3 oldPos = temp.transform.localPosition;
                Vector3 newPos = _map.GetCanvasPosition(temp.emoji.pos);
                Vector3 targetPosition = new Vector3(oldPos.x, newPos.y, 0);
                toBeMoved.Add(new KeyValuePair<EmojiView, Vector3>(temp, targetPosition));
            }
            return toBeMoved;
        }

        private bool ArrivedAt(EmojiView c, Vector3 pos) => Vector3.Distance(c.RectTransofrm.localPosition, pos) <= 0.01f;

        private bool MoveAll(List<KeyValuePair<EmojiView, Vector3>> emojis)
        {
            EmojiView temp;
            Vector3 targetPosition;
            bool done = true;
            for (int i = emojis.Count - 1; i >= 0; i--)
            {
                temp = emojis[i].Key;
                targetPosition = emojis[i].Value;

                if (ArrivedAt(temp, targetPosition))
                    continue;

                done = false;
                temp.Move(targetPosition);
            }

            return done;
        }

        private void AddScore(int s)
        {
            if (s == 0)
                return;
            score += s;
            GameEvents.Instance.Score(s);
        }

        public void ChangeAllEmojiSprites()
        {
            SetNewEmojiSprites();
            Emoji temp;
            for (int y = 0; y < _map.Size.y; y++)
            {
                for (int x = 0; x < _map.Size.x; x++)
                {
                    temp = _map.GetEmoji(x, y);
                    if (temp.IsEmpty)
                        continue;
                    GetView(temp).SetSprite(Sprites[temp.emojiType]);
                }
            }
        }

        public int SelectConnectedEmojis(Emoji c)
        {
            return EnableConnectedEmojis(c, true);
        }

        public int DeSelectConnectedEmojis(Emoji c)
        {
            return EnableConnectedEmojis(c, false);
        }

        private int EnableConnectedEmojis(Emoji c, bool value)
        {
            Emoji[] emojis = GetAllConnectedCells(c);
            for (int i = 0; i < emojis.Length; i++)
                emojis[i].seleted = value;
            return emojis.Length;
        }

        public bool IsSelected(Emoji c)
        {
            return EmojiMapExplorer.Explore(_map, c).Contains(c) && c.seleted;
        }

        private void SetNewEmojiSprites()
        {
            HashSet<string> added = new HashSet<string>();
            for (int i = 0; i < Sprites.Length; i++)
            {
                Sprites[i] = RandomEmoji.GetRandomEmoji();
                if (!added.Add(Sprites[i].name))
                    i--; // sprite already added, get a new one
            }
        }

        public Emoji[] GetAllConnectedCells(Emoji c)
        {
            return EmojiMapExplorer.Explore(_map, c).ToArray();
        }

        public EmojiView GetView(Vector2Int pos)
        {
            return GetView(_map.GetEmoji(pos.x, pos.y));
        }

        public EmojiView GetView(int x, int y)
        {
            return GetView(_map.GetEmoji(x, y));
        }

        public EmojiView GetView(Emoji c)
        {
            return emojis[c];
        }

        private void OnDestroy()
        {
            GameEvents.Instance.OnEmojiClick -= EmojiClickEvent;
        }
    }

}
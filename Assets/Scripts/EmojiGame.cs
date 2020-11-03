using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Ulbe;

namespace Emojigame
{
    public class EmojiGame : MonoBehaviour
    {
        protected enum GameState { WaitForClick, Selected, Clicked, GameOver }
        protected GameState state;
        public GameObject gamePanel;
        public GameObject cellPrefab;
        public Vector2 padding = new Vector2(0, 0);
        public Sprite[] Sprites { get; protected set; }
        public AudioClip killSfx;
        CellMap _map;
        Cell selected;
        CellGameSettings settings;
        Dictionary<Cell, CellView> cells;
        int score;

        void Start()
        {
            GameEvents.Instance.OnCellClick += CellClickEvent;
            Init();
        }

        protected void Init()
        {
            cells = new Dictionary<Cell, CellView>();

            settings = new CellGameSettings
            {
                difficulty = (CellGameSettings.Difficulty) Cvars.Instance.Get("g_difficulty", "0").intValue,
                size = (CellGameSettings.LevelSize) Cvars.Instance.Get("g_levelsize", "2").intValue
            };

            Vector2 cellSize = new Vector2(100, 100);
            cellSize /= (1 + (int)settings.size);

            Vector2 levelSize = gamePanel.GetComponent<RectTransform>().sizeDelta;

            _map = new CellMap(levelSize, cellSize);

            cellPrefab.GetComponent<RectTransform>().sizeDelta = cellSize;

            Sprites = new Sprite[2+(int)settings.difficulty];
            Transform tParent = gamePanel.transform;
            CellView cv;

            for (int y = 0; y < _map.LengthY; y++)
            {
                for (int x = 0; x < _map.LengthX; x++)
                {
                    cv = Instantiate(cellPrefab, tParent).GetComponent<CellView>();
                    cv.SetPadding(padding);
                    cv.cell = _map.GetCell(x, y);
                    cv.name = string.Format("({0},{1})",x,y);
                    cells.Add(cv.cell, cv);
                    cv.SetPosition(_map.GetCanvasPosition(x, y));
                }
            }

            ResetGame();
        }

        public void ResetGame()
        {
            GameEvents.Instance.ResetGame();
            state = GameState.WaitForClick;
            score = 0;

            CellView view;

            for (int y = 0; y < _map.LengthY; y++)
            {
                for (int x = 0; x < _map.LengthX; x++)
                {
                    view = GetView(x, y);
                    view.cell.seleted = false;
                    view.cell.cellType = Random.Range(0, Sprites.Length);
                    view.Show();
                }
            }

            ChangeAllCellSprites();
        }

        public virtual void CellClickEvent(Cell cell)
        {
            switch (state)
            {
                case GameState.Selected:
                    // Double click?
                    if (IsSelected(cell))
                    {
                        DeSelectConnectedCells(cell);
                        if (_map.HasNeighbours(cell))
                        {
                            state = GameState.Clicked;
                            StartCoroutine(ClickCellCoroutine(cell));
                        }
                        return;
                    }
                    // No double click
                    DeSelectConnectedCells(selected);
                    Select(cell);
                    break;
                case GameState.WaitForClick:
                    Select(cell);
                    state = GameState.Selected;
                    break;
                default:
                    return;
            }
        }

        protected virtual void GameOver()
        {
            state = GameState.GameOver;
            bool perfect = IsPerfectGame();
            GameEvents.Instance.GameOver(score, Sprites.Length, perfect);
            if (perfect)
                SoundMaster.Instance.PlayGlobalSound(Resources.Load<AudioClip>("sound/feedback/perfect"), 0.5f);
        }

        protected void Select(Cell c)
        {
            int connectedCnt = SelectConnectedCells(c);
            selected = c;
            c.seleted = true;
            GameEvents.Instance.CellSelect(connectedCnt);
        }

        protected IEnumerator ClickCellCoroutine(Cell c)
        {
            // Draw lines
            CellMapExplorer explorer = new CellMapExplorer(_map, c);
            List<LineRenderer> lines = new List<LineRenderer>();

            foreach (KeyValuePair<Cell, Cell> visited in explorer.path)
            {
                Vector3 a = GetView(visited.Key).RectTransofrm.position;
                Vector3 b = GetView(visited.Value).RectTransofrm.position;
                lines.Add(PathRenderer.Render(a, b));
            }

            yield return new WaitForSeconds(0.5f);

            // Destroy lines
            for (int i = 0; i < lines.Count; i++)
                Destroy(lines[i].gameObject);

            int startY = -1;
            int startX = -1;
            int endX = int.MaxValue;

            foreach (Cell cell in explorer.visited)
            {
                // Get upper and lower x bounds to avoid checking unaffected rows
                startX = cell.pos.x > startX ? cell.pos.x : startX;
                endX = cell.pos.x < endX ? cell.pos.x : endX;
               
                // Get lower y bounds.
                startY = cell.pos.y > startY ? cell.pos.y : startY;

                GetView(cell).Explode();

                SoundMaster.Instance.PlayGlobalSound(killSfx, 0.15f, SoundMaster.SoundType.SFX, Random.Range(1f, 3f));

                yield return new WaitForFixedUpdate();
            }

            AddScore(GameScorer.GetScore(explorer.visited.Length));

            // Update map
            Cell[] movedCellsY = _map.ShiftDown(startX, endX, startY);
            Cell[] movedCellsX = _map.ShiftLeft(startY);

            List<Cell> movingCells = new List<Cell>();

            CellView temp;

            // Move cellobjects down
            for (int i = 0; i < movedCellsY.Length; i++)
            {
                temp = GetView(movedCellsY[i]);
                Vector3 oldPos = temp.transform.localPosition;
                Vector3 newPos = _map.GetCanvasPosition(temp.cell.pos);
                StartCoroutine(MoveToCoroutine(temp, new Vector3(oldPos.x, newPos.y, 0), movingCells));
            }

            yield return new WaitUntil(() => movingCells.Count == 0);

            movingCells.Clear();

            // Move cellobjects to the left
            for (int i = 0; i < movedCellsX.Length; i++)
            {
                temp = GetView(movedCellsX[i]);
                Vector3 oldPos = temp.transform.localPosition;
                Vector3 newPos = _map.GetCanvasPosition(movedCellsX[i].pos);
                StartCoroutine(MoveToCoroutine(temp, new Vector3(newPos.x, oldPos.y, 0), movingCells));
            }

            yield return new WaitUntil(() => movingCells.Count == 0);

            state = GameState.WaitForClick;

            if (IsGameOver())
                GameOver();
        }

        protected IEnumerator MoveToCoroutine(CellView c, Vector3 pos, List<Cell> movingCells, float speed = 1000f)
        {
            movingCells.Add(c.cell);

            while (true)
            {
                c.RectTransofrm.localPosition = Vector3.MoveTowards(c.RectTransofrm.localPosition, pos, speed * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
                if (Vector3.Distance(c.RectTransofrm.localPosition, pos) <= 0.01f)
                {
                    c.RectTransofrm.localPosition = pos;
                    break;
                }
            }

            movingCells.Remove(c.cell);
        }

        protected bool IsPerfectGame()
        {
            return _map.GetCell(0, _map.LengthY - 1).IsEmpty;
        }

        protected bool IsGameOver()
        {
            for (int y = 0; y < _map.LengthY; y++)
            {
                for (int x = 0; x < _map.LengthX; x++)
                {
                    Cell c = _map.GetCell(x, y);
                    if (c != null && !c.IsEmpty && _map.HasNeighbours(c))
                        return false;
                }
            }
            return true;
        }

        public void ResetGameButton()
        {
            ResetGame();
        }

        protected void AddScore(int s)
        {
            score += s;
            GameEvents.Instance.Score(s);
        }

        public void ChangeAllCellSprites()
        {
            GetNewEmojiSprites();
            Cell temp;
            for (int y = 0; y < _map.Size.y; y++)
            {
                for (int x = 0; x < _map.Size.x; x++)
                {
                    temp = _map.GetCell(x, y);
                    if (temp.IsEmpty)
                        continue;
                    GetView(temp).SetSprite(Sprites[temp.cellType]);
                }
            }
        }

        public int SelectConnectedCells(Cell c)
        {
            return EnableConnectedCells(c, true);
        }

        public int DeSelectConnectedCells(Cell c)
        {
            return EnableConnectedCells(c, false);
        }

        protected int EnableConnectedCells(Cell c, bool value)
        {
            Cell[] cells = GetAllConnectedCells(c);
            for (int i = 0; i < cells.Length; i++)
                cells[i].seleted = value;
            return cells.Length;
        }

        public bool IsSelected(Cell c)
        {
            return CellMapExplorer.Explore(_map, c).Contains(c) && c.seleted;
        }

        protected void GetNewEmojiSprites()
        {
            HashSet<string> added = new HashSet<string>();
            for (int i = 0; i < Sprites.Length; i++)
            {
                Sprites[i] = RandomEmoji.GetRandomEmoji();
                if (!added.Add(Sprites[i].name))
                    i--; // sprite already added, get a new one
            }
        }

        public Cell[] GetAllConnectedCells(Cell c)
        {
            return CellMapExplorer.Explore(_map, c).ToArray();
        }

        public CellView GetView(Vector2Int pos)
        {
            return GetView(_map.GetCell(pos.x, pos.y));
        }

        public CellView GetView(int x,int y)
        {
            return GetView(_map.GetCell(x, y));
        }

        public CellView GetView(Cell c)
        {
            return cells[c];
        }

        private void OnDestroy()
        {
            GameEvents.Instance.OnCellClick -= CellClickEvent;
        }
    }

}
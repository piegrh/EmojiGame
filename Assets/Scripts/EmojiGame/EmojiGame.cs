using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Ulbe;
using Emojigame.Effects;

namespace Emojigame
{
    public struct CellGameSettings
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

        private void Init()
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

        public void CellClickEvent(Cell cell)
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

        private void GameOver()
        {
            state = GameState.GameOver;
            bool perfect = IsPerfectGame(_map);
            GameEvents.Instance.GameOver(score, Sprites.Length, perfect);
            if (perfect)
                SoundMaster.Instance.PlayGlobalSound(Resources.Load<AudioClip>("sound/feedback/perfect"), 0.5f);
        }

        public static bool IsPerfectGame(CellMap map)
        {
            return map.GetCell(0, map.LengthY - 1).IsEmpty;
        }

        public static bool IsGameOver(CellMap map)
        {
            for (int y = 0; y < map.LengthY; y++)
            {
                for (int x = 0; x < map.LengthX; x++)
                {
                    Cell c = map.GetCell(x, y);
                    if (c != null && !c.IsEmpty && map.HasNeighbours(c))
                        return false;
                }
            }
            return true;
        }

        private void Select(Cell c)
        {
            int connectedCnt = SelectConnectedCells(c);
            selected = c;
            c.seleted = true;
            GameEvents.Instance.CellSelect(connectedCnt);
        }

        private IEnumerator ClickCellCoroutine(Cell c)
        {
            // Draw lines
            CellMapExplorer explorer = new CellMapExplorer(_map, c);
            List<LineRenderer> lines = new List<LineRenderer>();
            Vector3 a, b;

            foreach (KeyValuePair<Cell, Cell> visited in explorer.path)
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

            foreach (Cell cell in explorer.visited)
            {
                // Get upper and lower x bounds to avoid checking unaffected rows
                startX = cell.pos.x > startX ? cell.pos.x : startX;
                endX = cell.pos.x < endX ? cell.pos.x : endX;

                // Get lower y bounds.
                startY = cell.pos.y > startY ? cell.pos.y : startY;
                GetView(cell).Explode();
                AddScore(scorePerFrame);
                SoundMaster.Instance.PlayGlobalSound(killSfx, 0.15f, SoundMaster.SoundType.SFX, Random.Range(1f, 3f));
                yield return new WaitForFixedUpdate();
            }

            AddScore(_score % explorer.visited.Length);

            // Update map
            Cell[] movedCellsY = _map.ShiftDown(startX, endX, startY);
            Cell[] movedCellsX = _map.ShiftLeft(endX);

            List<KeyValuePair<CellView, Vector3>> cellToBeMoved;

            cellToBeMoved = GetCellsToBeMovedY(movedCellsY);

            // Move down
            yield return new WaitUntil(() => MoveAll(cellToBeMoved));

            cellToBeMoved.Clear();

            cellToBeMoved = GetCellsToBeMovedX(movedCellsX);

            // Move left
            yield return new WaitUntil(() => MoveAll(cellToBeMoved));

            state = GameState.WaitForClick;

            if (IsGameOver(_map))
                GameOver();
        }

        private List<KeyValuePair<CellView, Vector3>> GetCellsToBeMovedX(Cell[] movedCellsX)
        {
            List<KeyValuePair<CellView, Vector3>> cellToBeMoved = new List<KeyValuePair<CellView, Vector3>>();

            for (int i = 0; i < movedCellsX.Length; i++)
            {
                CellView temp = GetView(movedCellsX[i]);
                Vector3 oldPos = temp.transform.localPosition;
                Vector3 newPos = _map.GetCanvasPosition(movedCellsX[i].pos);
                Vector3 targetPosition = new Vector3(newPos.x, oldPos.y, 0);
                cellToBeMoved.Add(new KeyValuePair<CellView, Vector3>(temp, targetPosition));
            }

            return cellToBeMoved;
        }

        private List<KeyValuePair<CellView, Vector3>> GetCellsToBeMovedY(Cell[] movedCellsY)
        {
            List<KeyValuePair<CellView, Vector3>> cellToBeMoved = new List<KeyValuePair<CellView, Vector3>>();
            for (int i = 0; i < movedCellsY.Length; i++)
            {
                CellView temp = GetView(movedCellsY[i]);
                Vector3 oldPos = temp.transform.localPosition;
                Vector3 newPos = _map.GetCanvasPosition(temp.cell.pos);
                Vector3 targetPosition = new Vector3(oldPos.x, newPos.y, 0);
                cellToBeMoved.Add(new KeyValuePair<CellView, Vector3>(temp, targetPosition));
            }
            return cellToBeMoved;
        }

        private bool ArrivedAt(CellView c, Vector3 pos) => Vector3.Distance(c.RectTransofrm.localPosition, pos) <= 0.01f;

        private bool MoveAll(List<KeyValuePair<CellView, Vector3>> cells)
        {
            CellView temp;
            Vector3 targetPosition;

            for (int i = cells.Count - 1; i >= 0; i--)
            {
                temp = cells[i].Key;
                targetPosition = cells[i].Value;

                if (ArrivedAt(temp, targetPosition))
                {
                    cells.RemoveAt(i);
                    continue;
                }

                temp.Move(targetPosition);
            }

            return cells.Count == 0;
        }

        private void AddScore(int s)
        {
            if (s == 0)
                return;
            score += s;
            GameEvents.Instance.Score(s);
        }

        public void ChangeAllCellSprites()
        {
            SetNewEmojiSprites();
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

        private int EnableConnectedCells(Cell c, bool value)
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
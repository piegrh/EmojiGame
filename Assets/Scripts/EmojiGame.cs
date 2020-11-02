﻿using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Ulbe;

namespace Emojigame
{
    public class EmojiGame : MonoBehaviour
    {
        protected enum GameState { WaitForClick, Selected, Clicked, GameOver }
        protected GameState state;
        public GameObject GamePanel;
        public GameObject CellPrefab;
        CellMap _map;
        Cell selected;
        CellGameSettings settings;
        int score;

        void Start()
        {
            GameEvents.Instance.OnCellClick += CellClickEvent;
            Init();
        }

        protected void Init()
        {
            state = GameState.WaitForClick;
            score = 0;
            settings = new CellGameSettings
            {
                difficulty = (CellGameSettings.Difficulty)Cvars.Instance.Get("g_difficulty", "0").intValue,
                size = (CellGameSettings.LevelSize)Cvars.Instance.Get("g_levelsize", "2").intValue
            };
            _map = new CellMap(settings, GamePanel, CellPrefab);
        }

        public virtual void CellClickEvent(Cell cell)
        {
            switch (state)
            {
                case GameState.Selected:
                    // Double click?
                    if (_map.IsSelected(cell))
                    {
                        _map.DeSelectConnectedCells(cell);
                        if (_map.HasNeighbours(cell))
                        {
                            state = GameState.Clicked;
                            StartCoroutine(ClickCellCoroutine(cell));
                        }
                        return;
                    }
                    // No double click
                    _map.DeSelectConnectedCells(selected);
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
            GameEvents.Instance.GameOver(score, _map.NumberOfCellTypes(), perfect);
            if (perfect)
                SoundMaster.Instance.PlayGlobalSound(Resources.Load<AudioClip>("sound/feedback/perfect"), 0.5f);
            ChangeAllCellSpritesButton();
        }

        protected void Select(Cell c)
        {
            int connectedCnt = _map.SelectConnectedCells(c);
            selected = c;
            GameEvents.Instance.CellSelect(connectedCnt);
        }

        protected IEnumerator ClickCellCoroutine(Cell c)
        {
            // Draw lines
            CellMapExplorer explorer = new CellMapExplorer(_map, c);
            LineRenderer[] lines = PathRenderer.Render(explorer.path);

            yield return new WaitForSeconds(0.5f);

            // Destroy lines
            for (int i = 0; i < lines.Length; i++)
                Destroy(lines[i].gameObject);

            AudioClip clip = Resources.Load<AudioClip>("sound/feedback/kill");

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

                cell.Explode();

                SoundMaster.Instance.PlayGlobalSound(clip, 0.15f, SoundMaster.SoundType.SFX, Random.Range(1f, 3f));
                yield return new WaitForFixedUpdate();
            }

            AddScore(GameScorer.GetScore(explorer.visited.Length));

            // Update map
            Cell[] movedCellsY = _map.ShiftDown(startX, endX, startY);
            Cell[] movedCellsX = _map.ShiftLeft(startY);

            List<Cell> movingCells = new List<Cell>();

            Cell temp;
            // Move cellobjects down
            for (int i = 0; i < movedCellsY.Length; i++)
            {
                temp = movedCellsY[i];
                StartCoroutine(MoveToCoroutine(temp,
                    new Vector3(temp.transform.localPosition.x, _map.GetCanvasPosition(temp.pos).y, 0), movingCells)
                    );
            }

            yield return new WaitUntil(() => movingCells.Count == 0);

            movingCells.Clear();

            // Move cellobjects to the left
            for (int i = 0; i < movedCellsX.Length; i++)
            {
                temp = movedCellsX[i];
                StartCoroutine(MoveToCoroutine(temp,
                    new Vector3(temp.transform.localPosition.x, _map.GetCanvasPosition(temp.pos).y, 0), movingCells)
                    );
            }

            yield return new WaitUntil(() => movingCells.Count == 0);

            state = GameState.WaitForClick;

            if (IsGameOver())
                GameOver();
        }

        protected IEnumerator MoveToCoroutine(Cell c, Vector3 pos, List<Cell> movingCells, float speed = 1000f)
        {
            Transform t = c.transform;

            movingCells.Add(c);

            while (true)
            {
                t.localPosition = Vector3.MoveTowards(t.localPosition, pos, speed * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
                if (Vector3.Distance(t.localPosition, pos) <= 0.01f)
                {
                    t.localPosition = pos;
                    break;
                }
            }

            movingCells.Remove(c);
        }


        // check if lower left corner is clear
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
            GameEvents.Instance.ResetGame();
            score = 0;
            _map.ResetLevel();
            state = GameState.WaitForClick;
        }

        protected void AddScore(int s)
        {
            this.score += s;
            GameEvents.Instance.Score(s);
        }

        public void ChangeAllCellSpritesButton()
        {
            _map.ChangeAllCellSprites();
        }

        private void OnDestroy()
        {
            GameEvents.Instance.OnCellClick -= CellClickEvent;
        }
    }

}
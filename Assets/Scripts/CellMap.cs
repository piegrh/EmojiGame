using System.Collections.Generic;
using UnityEngine;

public struct CellGameSettings
{
    public enum LevelSize { Small = 0, Normal, Big, Huge, Ginormous, Extreme, GIGANTISCH};
    public enum Difficulty { Normal = 0, Intermediate, Hard, Nightmare, UltraNightmare };
    public LevelSize size;
    public Difficulty difficulty;
}

public class CellMap
{
    protected Cell[,] map;
    public Vector2 CellSize { get; protected set; } = new Vector2(100, 100);
    public Vector2 Offset { get; protected set; }
    public Vector2 Size { get; protected set; }
    public Sprite[] Sprites { get; protected set; }
    protected CellGameSettings settings;

    public int NumberOfCellTypes()
    {
        return 2 + (int)settings.difficulty;
    }

    public CellMap(CellGameSettings settings, GameObject gamePanel, GameObject cellPrefab)
    {
        this.settings = settings;
        Init(settings, gamePanel, cellPrefab);
    }

    protected virtual void Init(CellGameSettings settings, GameObject gamePanel, GameObject cellPrefab)
    {
        Size = gamePanel.GetComponent<RectTransform>().sizeDelta;
        CellSize = new Vector2(100, 100);
        CellSize /= (1 + (int)settings.size);

        cellPrefab.GetComponent<RectTransform>().sizeDelta = CellSize;

        Sprites = new Sprite[NumberOfCellTypes()];
        Offset = new Vector2(CellSize.x / 2, CellSize.y / 2);
        Size = new Vector2(Size.x / (CellSize.x), Size.y / (CellSize.y));

        SetRandomSprites();

        GameObject g;
        Cell c;

        map = new Cell[(int)Size.x, (int)Size.y];

        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                g = Object.Instantiate(cellPrefab, null);
                c = CreateRandomCell(x, y, g);
                g.transform.SetParent(gamePanel.transform);
                g.transform.localScale = new Vector3(1, 1, 1);
                g.transform.localPosition = GetCanvasPosition(x, y);
                map[x, y] = c;
            }
        }
    }

    public Vector3 GetCanvasPosition(Vector2Int pos)
    {
        return GetCanvasPosition(pos.x, pos.y);
    }

    public Vector3 GetCanvasPosition(int x,int y)
    {
        return new Vector3((CellSize.x * x) + Offset.x, (-CellSize.y * y) - Offset.y, 0);
    }

    protected Cell CreateRandomCell(int x, int y, GameObject g)
    {
        Cell c = g.GetComponent<Cell>();
        c.name = string.Format("cell({0},{1})", x, y);
        c.CellTypeID = Random.Range(0, Sprites.Length);
        c.pos = new Vector2Int(x, y);
        c.GetComponent<UnityEngine.UI.Image>().sprite = Sprites[c.CellTypeID];
        c.EnableCell();
        return c;
    }

    public void SelectCell(Vector2Int pos)
    {
        SelectCell(GetCell(pos.x, pos.y));
    }

    public void SelectCell(Cell c)
    {
        if (c != null)
            c.SetSelected(true);
    }

    public virtual Cell[] ShiftDown(int startX, int endX, int startY)
    {
        startY = Mathf.Clamp(startY, 0, LengthY - 2);
        List<Cell> movedCellsY = new List<Cell>();
       
        // Move vertical
        for (int y = startY; y >= 0; y--)
        {
            for (int x = startX; x >= endX; x--)
            {
                Cell temp = GetCell(x, y);

                if (temp.IsEmpty)
                    continue;

                Cell under;
                int cnt = 1;
                bool valid = false;

                while ((under = GetCell(x, y + cnt)) != null && under.IsEmpty)
                {
                    valid = true;
                    cnt++;
                };

                if (valid)
                {
                    Swap(new Vector2Int(x, y), new Vector2Int(x, y + --cnt));
                    movedCellsY.Add(GetCell(x, y));
                    movedCellsY.Add(GetCell(x, y + cnt));
                }
            }
        }

        return movedCellsY.ToArray();
    }

    public virtual Cell[] ShiftLeft(int startY)
    {
        startY = Mathf.Clamp(startY, 0, LengthY - 2);
        List<Cell> movedCellsX = new List<Cell>();
      
        // Move Horizontal
        for (int x = 0; x < LengthX; x++)
        {
            Cell temp = GetCell(x, LengthY - 1);
            if (temp.IsEmpty)
            {
                for (int i = x; i < LengthX; i++)
                {
                    if (!GetCell(i, LengthY - 1).IsEmpty)
                    {
                        // Move Vertical line to the left
                        for (int y = 0; y < LengthY; y++)
                        {
                            Swap(new Vector2Int(x, y), new Vector2Int(i, y));
                            movedCellsX.Add(GetCell(x, y));
                            movedCellsX.Add(GetCell(i, y));
                        }
                        break;
                    }
                }
            }
        }

        return movedCellsX.ToArray();
    }

    public Cell[] GetNeighbours(Cell c)
    {
        List<Cell> cells = new List<Cell>();
        Cell temp;
        // right
        if ((temp = GetCell(c.pos.x + 1, c.pos.y + 0)) != null && temp.CellTypeID == c.CellTypeID)
            cells.Add(temp);
        // left
        if ((temp = GetCell(c.pos.x - 1, c.pos.y + 0)) != null && temp.CellTypeID == c.CellTypeID)
            cells.Add(temp);
        // Up
        if ((temp = GetCell(c.pos.x + 0, c.pos.y - 1)) != null && temp.CellTypeID == c.CellTypeID)
            cells.Add(temp);
        // Down
        if ((temp = GetCell(c.pos.x + 0, c.pos.y + 1)) != null && temp.CellTypeID == c.CellTypeID)
            cells.Add(temp);
        return cells.ToArray();
    }

    public Cell[] GetAllConnectedCells(Cell c)
    {
        return CellMapExplorer.Explore(this,c).ToArray();
    }

    public bool HasNeighbours(Cell c)
    {
        return GetNeighbours(c).Length > 0;
    }

    public bool IsSelected(Cell c)
    {
        return CellMapExplorer.Explore(this, c).Contains(c) && c.seleted;
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
            cells[i].SetSelected(value);

        return cells.Length;
    }

    protected void Swap(Vector2Int a, Vector2Int b)
    {
        Cell cellA = GetCell(a);
        Cell cellB = GetCell(b);
        cellA.pos = b;
        cellB.pos = a;
        map[b.x, b.y] = cellA;
        map[a.x, a.y] = cellB;
    }

    public Cell GetCell(int x, int y)
    {
        if ((x >= 0 && x < LengthX) && (y >= 0 && y < LengthY))
            return map[x, y];
        return null;
    }

    public Cell GetCell(Vector2Int v)
    {
        return GetCell(v.x, v.y);
    }

    public int LengthX
    {
        get { return map.GetLength(0); }
    }

    public int LengthY
    {
        get { return map.GetLength(1); }
    }

    protected void SetRandomSprites()
    {
        HashSet<string> added = new HashSet<string>();
        for (int i = 0; i < Sprites.Length; i++)
        {
            Sprites[i] = RandomEmoji.GetRandomEmoji();
            if (!added.Add(Sprites[i].name))
                i--; // sprite is already added, get a new one
        }
    }

    public void ResetLevel()
    {
        for (int y = 0; y < Size.y; y++)
            for (int x = 0; x < Size.x; x++)
                CreateRandomCell(x, y, map[x, y].gameObject);
    }

    public void ChangeAllCellSprites()
    {
        SetRandomSprites();
        for (int y = 0; y < Size.y; y++)
            for (int x = 0; x < Size.x; x++)
                if (!map[x, y].IsEmpty)
                    map[x, y].GetComponent<UnityEngine.UI.Image>().sprite = Sprites[map[x, y].CellTypeID];
    }
}

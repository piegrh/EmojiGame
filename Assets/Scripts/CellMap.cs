using System.Collections.Generic;
using UnityEngine;

namespace Emojigame
{ 
    public class CellMap
    {
        protected Cell[,] map;
        public Vector2 CellSize { get; protected set; } = new Vector2(100, 100);
        public Vector2 Offset { get; protected set; }
        public Vector2 Size { get; protected set; }

        public CellMap(int x, int y)
        {
            Size = new Vector2(x, y);
            map = new Cell[x, y];
            Offset = Vector2.zero;
            CellSize = Vector2.zero;

            Init();
        }

        public CellMap(Vector2 Size, Vector2 Cellsize)
        {
            Init(Size, Cellsize);
        }

        protected void Init(Vector2 size, Vector2 cellSize)
        {
            Size = size;
            CellSize = cellSize;
            Offset = new Vector2(CellSize.x / 2, CellSize.y / 2);
            Size = new Vector2(Size.x / (CellSize.x), Size.y / (CellSize.y));
            map = new Cell[(int)Size.x, (int)Size.y];
            Init();
        }

        protected void Init()
        {
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x; x++)
                    map[x, y] = new Cell(x, y);
        }

        // Returns cell that were moved
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

        // Returns cell that were moved
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
            if ((temp = GetCell(c.pos.x + 1, c.pos.y + 0)) != null && temp.SameType(c))
                cells.Add(temp);

            // left
            if ((temp = GetCell(c.pos.x - 1, c.pos.y + 0)) != null && temp.SameType(c))
                cells.Add(temp);

            // Up
            if ((temp = GetCell(c.pos.x + 0, c.pos.y - 1)) != null && temp.SameType(c))
                cells.Add(temp);

            // Down
            if ((temp = GetCell(c.pos.x + 0, c.pos.y + 1)) != null && temp.SameType(c))
                cells.Add(temp);

            return cells.ToArray();
        }

        public bool HasNeighbours(Cell c)
        {
            return GetNeighbours(c).Length > 0;
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

        public int Count
        {
            get
            {
                return LengthX * LengthY;
            }
        }

        public Vector3 GetCanvasPosition(Vector2Int pos)
        {
            return GetCanvasPosition(pos.x, pos.y);
        }

        public Vector3 GetCanvasPosition(int x, int y)
        {
            return new Vector3((CellSize.x * x) + Offset.x, (-CellSize.y * y) - Offset.y, 0);
        }
    }
}
using UnityEngine;

namespace Emojigame
{
    [System.Serializable]
    public class Cell 
    {
        public Vector2Int pos = Vector2Int.zero;
        public const int NONE = -1;
        public int cellType = NONE;
        public bool seleted = false;
        public Cell(int x,int y, int cellType = NONE)
        {
            pos = new Vector2Int(x, y);
            this.cellType = cellType;
        }
        public bool IsEmpty { get { return cellType == NONE; } }
        public bool SameType(Cell c) { return c.cellType == cellType; }
    }
}
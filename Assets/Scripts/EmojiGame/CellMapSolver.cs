using UnityEngine;
using System.Text;
using System.Collections.Generic;

namespace Emojigame.AI
{
    public class CellMapSolver
    {
        public CellMapSolver(CellMap S)
        {
            CellMap G = new CellMap(S.LengthX, S.LengthY);
        }

        protected static CellMap Click(CellMap S, Vector2Int a, bool gravity = true)
        {
            CellMap nextState = GetMap(S.LengthX, S.LengthY, GetState(S));

            int startY = -1;
            int startX = -1;
            int endX = int.MaxValue;

            CellMapExplorer exp = new CellMapExplorer(nextState, nextState.GetCell(a.x,a.y));

            foreach (Cell cell in exp.visited)
            {
                cell.cellType = Cell.NONE;

                if (!gravity) continue;

                startX = cell.pos.x > startX ? cell.pos.x : startX;
                endX = cell.pos.x < endX ? cell.pos.x : endX;
                startY = cell.pos.y > startY ? cell.pos.y : startY;
            }

            if (gravity)
            {
                nextState.ShiftDown(startX, endX, startY);
                nextState.ShiftLeft(startY);
            }

            return nextState;
        }

        protected static void Solve(CellMap S, CellMap G)
        {

        }

        public static CellMap GetMap(int sizex,int sizey, int[] state)
        {
            CellMap map = new CellMap(sizex, sizey);
            int i = 0;
            for (int y = 0; y < map.LengthY; y++)
                for (int x = 0; x < map.LengthX; x++)
                    map.GetCell(x,y).cellType = state[i++];
            return map;
        }

        public static int[] GetState(CellMap m)
        {
            int[] state = new int[m.Count];
            int i = 0;
            for (int y = 0; y < m.LengthY; y++)
                for (int x = 0; x < m.LengthX; x++)
                    state[i++] = m.GetCell(x, y).cellType;
            return state;
        }

        public static List<Vector2Int> GetActions(CellMap m)
        {
            List<Vector2Int> actions = new List<Vector2Int>();
            int sizeX = m.LengthX;
            int sizeY = m.LengthY;

            CellMap temp = GetMap(m.LengthX, m.LengthY, GetState(m));

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if (temp.GetCell(x, y).IsEmpty)
                        continue;

                    actions.Add(new Vector2Int(x, y));

                    temp = Click(temp, new Vector2Int(x, y), false);
                }
            }

            return actions;
        }

        public static void PrintMap(CellMap m)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < m.LengthY; y++)
            {
                for (int x = 0; x < m.LengthX; x++)
                {
                    sb.Append(m.GetCell(x, y).cellType);
                }
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }
    }
}



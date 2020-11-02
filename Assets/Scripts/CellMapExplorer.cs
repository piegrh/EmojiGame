using System.Collections.Generic;
using UnityEngine;

namespace Emojigame
{
    public class CellMapExplorer
    {
        public Cell[] visited;
        public List<KeyValuePair<Vector3, Vector3>> path;
        protected LineRenderer[] lrend;

        public CellMapExplorer(CellMap map, Cell start)
        {
            visited = Explore(map, start, out path).ToArray();
        }

        public static List<Cell> Explore(CellMap map, Cell start, out List<KeyValuePair<Vector3, Vector3>> path)
        {
            path = new List<KeyValuePair<Vector3, Vector3>>();
            List<Cell> visitedCells = new List<Cell>();
            Queue<Cell> q = new Queue<Cell>();
            q.Enqueue(start);
            Cell temp;
            Cell[] adjCells;
            while (q.Count > 0)
            {
                temp = q.Dequeue();
                if (!visitedCells.Contains(temp))
                    visitedCells.Add(temp);
                else
                    continue;
                adjCells = map.GetNeighbours(temp);
                for (int i = 0; i < adjCells.Length; i++)
                {
                    if (!visitedCells.Contains(adjCells[i]))
                    {
                        q.Enqueue(adjCells[i]);
                        path.Add(new KeyValuePair<Vector3, Vector3>(temp.transform.position, adjCells[i].transform.position));
                    }
                }
            }
            return visitedCells;

        }

        public static List<Cell> Explore(CellMap map, Cell start)
        {
            return Explore(map, start, out List<KeyValuePair<Vector3, Vector3>> path);
        }
    }

}

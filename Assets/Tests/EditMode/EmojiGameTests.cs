using UnityEngine;
using Emojigame;
using NUnit.Framework;

namespace Tests
{

    public class EmojiGameTests
    {
        [Test]
        public void LevelInstantiation()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            CellMap map = new CellMap(area, new Vector2(10, 10));
            const int finalsize = 100;
            Assert.AreEqual(map.Count, finalsize);
        }

        [Test]
        public void GameOverIsTrue()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            CellMap map = new CellMap(area, new Vector2(10, 10));
            map.GetCell(0, 0).cellType = 1;
            Assert.IsTrue(EmojiGame.IsGameOver(map));
        }

        [Test]
        public void GameOverIsFalse()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            CellMap map = new CellMap(area, new Vector2(10, 10));
            map.GetCell(0, 1).cellType = 1;
            map.GetCell(0, 0).cellType = 1;
            Assert.IsFalse(EmojiGame.IsGameOver(map));
        }

        [Test]
        public void PerfectGameIsTrue()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            CellMap map = new CellMap(area, new Vector2(10, 10));
            Assert.IsTrue(EmojiGame.IsPerfectGame(map));
        }

        [Test]
        public void PerfectGameIsFalse()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            CellMap map = new CellMap(area, new Vector2(10, 10));
            // Set lower left corner
            map.GetCell(0, 9).cellType = 1;
            Assert.IsFalse(EmojiGame.IsPerfectGame(map));
        }

        [Test]
        public void GravityVertical()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            CellMap map = new CellMap(area, new Vector2(10, 10));

            // Upper left
            Cell expected = map.GetCell(0, 0);

            expected.cellType = 1;

            map.ShiftDown();

            // Lower left
            Cell result = map.GetCell(0, 9);

            Assert.AreSame(expected, result);
        }

        [Test]
        public void GravityHorizontal()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            CellMap map = new CellMap(area, new Vector2(10, 10));

            // lower right
            Cell expected = map.GetCell(9, 9);
            expected.cellType = 1;

            map.ShiftLeft();

            // lower left
            Cell result = map.GetCell(0, 9);

            Assert.AreSame(expected, result);
        }

        [Test]
        public void GetConnectedCells()
        {
            const int expected = 7;
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            CellMap map = new CellMap(area, new Vector2(10, 10));

            Cell start = map.GetCell(0, 0);
            start.cellType = 1;

            map.GetCell(0, 1).cellType = 1;
            map.GetCell(0, 2).cellType = 1;
            map.GetCell(0, 3).cellType = 1;
            map.GetCell(0, 4).cellType = 0;
            map.GetCell(1, 0).cellType = 1;
            map.GetCell(2, 0).cellType = 1;
            map.GetCell(3, 0).cellType = 1;
            map.GetCell(4, 0).cellType = 0;

            CellMapExplorer cme = new CellMapExplorer(map, start);

            Assert.AreEqual(expected, cme.visited.Length);
        }
    }
}

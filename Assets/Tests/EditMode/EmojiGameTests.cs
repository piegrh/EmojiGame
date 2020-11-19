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
            EmojiMap map = new EmojiMap(area, new Vector2(10, 10));
            const int finalsize = 100;
            Assert.AreEqual(map.Count, finalsize);
        }

        [Test]
        public void GameOverIsTrue()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            EmojiMap map = new EmojiMap(area, new Vector2(10, 10));
            map.GetEmoji(0, 0).emojiType = 1;
            Assert.IsTrue(EmojiGame.IsGameOver(map));
        }

        [Test]
        public void GameOverIsFalse()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            EmojiMap map = new EmojiMap(area, new Vector2(10, 10));
            map.GetEmoji(0, 1).emojiType = 1;
            map.GetEmoji(0, 0).emojiType = 1;
            Assert.IsFalse(EmojiGame.IsGameOver(map));
        }

        [Test]
        public void PerfectGameIsTrue()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            EmojiMap map = new EmojiMap(area, new Vector2(10, 10));
            Assert.IsTrue(EmojiGame.IsPerfectGame(map));
        }

        [Test]
        public void PerfectGameIsFalse()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            EmojiMap map = new EmojiMap(area, new Vector2(10, 10));
            // Set lower left corner
            map.GetEmoji(0, 9).emojiType = 1;
            Assert.IsFalse(EmojiGame.IsPerfectGame(map));
        }

        [Test]
        public void GravityVertical()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            EmojiMap map = new EmojiMap(area, new Vector2(10, 10));

            // Upper left
            Emoji expected = map.GetEmoji(0, 0);

            expected.emojiType = 1;

            map.ShiftDown();

            // Lower left
            Emoji result = map.GetEmoji(0, 9);

            Assert.AreSame(expected, result);
        }

        [Test]
        public void GravityHorizontal()
        {
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            EmojiMap map = new EmojiMap(area, new Vector2(10, 10));

            // lower right
            Emoji expected = map.GetEmoji(9, 9);
            expected.emojiType = 1;

            map.ShiftLeft();

            // lower left
            Emoji result = map.GetEmoji(0, 9);

            Assert.AreSame(expected, result);
        }

        [Test]
        public void GetConnectedCells()
        {
            const int expected = 7;
            Vector2 area = new Vector2(100, 100);
            Vector2 cellsize = new Vector2(10, 10);
            EmojiMap map = new EmojiMap(area, new Vector2(10, 10));

            Emoji start = map.GetEmoji(0, 0);
            start.emojiType = 1;

            map.GetEmoji(0, 1).emojiType = 1;
            map.GetEmoji(0, 2).emojiType = 1;
            map.GetEmoji(0, 3).emojiType = 1;
            map.GetEmoji(0, 4).emojiType = 0;
            map.GetEmoji(1, 0).emojiType = 1;
            map.GetEmoji(2, 0).emojiType = 1;
            map.GetEmoji(3, 0).emojiType = 1;
            map.GetEmoji(4, 0).emojiType = 0;

            EmojiMapExplorer cme = new EmojiMapExplorer(map, start);

            Assert.AreEqual(expected, cme.visited.Length);
        }
    }
}

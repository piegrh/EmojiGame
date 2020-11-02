namespace Emojigame
{
    public class GameScorer
    {
        public static int FinalScore(int baseScore, int celltypes, bool perfectGame)
        {
            return baseScore * (perfectGame ? celltypes : 1);
        }

        public static int GetScore(int kills)
        {
            return kills > 1 ? kills * (kills + 128) : 0;
        }
    }

}

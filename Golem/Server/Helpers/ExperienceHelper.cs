using System;

namespace Golem.Game.Mobiles
{
    public class ExperienceHelper
    {
        public static readonly int FirstLevelXp = 1000;
        public static readonly int MaxLevelXp = 1_000_000;
        public static readonly int LevelMax = 50;

        private static double B = Math.Log((double) MaxLevelXp / FirstLevelXp) / (LevelMax - 1);
        private static double A = FirstLevelXp / (Math.Exp(B) - 1.0);

        public static bool CanLevelUp(int level, int experience) => experience >= ExperienceRequired(level + 1);

        public static int ExperienceRequired(int level)
        {
            int oldXp = Convert.ToInt32(A * Math.Exp(B * level - 1));
            int newXp = Convert.ToInt32(A * Math.Exp(B * level));
            return newXp - oldXp;
        }

        public static void ApplyExperience(IPlayer player, int experience) => player.Experience += experience;

        public static void LevelUp(IPlayer player)
        {
            player.Level++;

            // TODO: Uhh, idk, add some hitpoints with each stat?

            if (player.OutputWriter != null)
            {
                player.Send($"You feel like you've gained a new understanding of the world.");
            }
        }
    }
}
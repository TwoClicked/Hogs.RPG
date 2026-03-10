using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Services.GameplayServices
{
    public class LevelService
    {
        private const int MaxLevel = 50;

        public string CheckLevelUp(Player player)
        {
            string levelUpMessage = "";

            while (player.Level < MaxLevel && player.XP >= GetRequiredXP(player.Level))
            {
                player.XP -= GetRequiredXP(player.Level);
                player.Level++;

                // Stat rewards
                player.Attack += 1;
                player.Defense += 1;
                player.Health += 5;

                levelUpMessage += $"\n🔥 LEVEL UP!\nYou reached Level {player.Level}!\nAttack +1\nDefense +1\nHealth +5\n";
            }

            return levelUpMessage;
        }

        public int GetRequiredXP(int level)
        {
            return level * level * 100;
        }
    }
}
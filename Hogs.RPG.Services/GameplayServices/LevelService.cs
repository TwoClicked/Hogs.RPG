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
                player.Attack += 5;
                player.Defense += 5;
                player.MaxHealth += 10;

                levelUpMessage += $"\n🔥 LEVEL UP!\nYou reached Level {player.Level}!\nAttack +5\nDefense +5\nHealth +10\n";
            }

            return levelUpMessage;
        }

        public int GetRequiredXP(int level)
        {
            return level * level * 100;
        }
    }
}
using Hogs.RPG.Core.Entities.PlayerObjects;

namespace Hogs.RPG.Services.GameplayServices
{
    public class LevelService
    {
        private const int MaxLevel = 50;

        private readonly StatService _statService;

        public LevelService(StatService statService)
        {
            _statService = statService;
        }

        public (string message, int levelsGained) CheckLevelUp(Player player)
        {
            string levelUpMessage = "";
            int levels = 0;

            while (player.Level < MaxLevel && player.XP >= GetRequiredXP(player.Level))
            {
                player.XP -= GetRequiredXP(player.Level);
                player.Level++;
                levels++;

                // Base stat rewards
                player.Attack += 5;
                player.Defense += 5;
                player.MaxHealth += 10;

                // Recalculate FINAL max health (includes gear)
                var (_, _, maxHealth) = _statService.CalculateStats(player);

                // Fully heal to new max
                player.Health = maxHealth;

                levelUpMessage +=
                    $"\n🔥 LEVEL UP!\n" +
                    $"You reached Level {player.Level}!\n" +
                    $"Attack +5\n" +
                    $"Defense +5\n" +
                    $"Health +10\n";
            }

            return (levelUpMessage, levels);
        }

        public int GetRequiredXP(int level)
        {
            return level * level * 100;
        }
    }
}
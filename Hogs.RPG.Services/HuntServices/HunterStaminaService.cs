using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.Achievements;
using System;

namespace Hogs.RPG.Services.GameplayServices
{
    public class HunterStaminaService
    {
        private const int BaseMaxStamina = 100;
        private const int BoostedMaxStamina = 150;

        private static int GetMaxStamina(Player player)
        {
            if (player.StaminaBoostExpiry.HasValue &&
                player.StaminaBoostExpiry.Value > DateTime.UtcNow)
                return BoostedMaxStamina;

            return BaseMaxStamina;
        }

        public void Regenerate(Player player)
        {
            if (player == null) return;

            if (string.IsNullOrEmpty(player.LastHunterStaminaUpdate))
            {
                player.LastHunterStaminaUpdate = DateTimeOffset.UtcNow.ToString("o");
                return;
            }

            if (!DateTimeOffset.TryParse(player.LastHunterStaminaUpdate, out var last))
                return;

            var now = DateTimeOffset.UtcNow;
            var minutesPassed = (int)(now - last).TotalMinutes;

            if (minutesPassed <= 0) return;

            int max = GetMaxStamina(player);

            int regenRate = AchievementMilestones.GetBonus(player.AchievementCount).RegenRate;
            player.HunterStamina = Math.Min(max, player.HunterStamina + (minutesPassed * regenRate));

            player.LastHunterStaminaUpdate = last
                .AddMinutes(minutesPassed)
                .ToString("o");
        }

        public void Spend(Player player, int amount)
        {
            player.HunterStamina -= amount;
        }
    }
}
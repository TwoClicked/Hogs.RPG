using Hogs.RPG.Core.Entities;
using System;

namespace Hogs.RPG.Services.GameplayServices
{
    public class HunterStaminaService
    {
        private const int MaxStamina = 100;

        public void Regenerate(Player player)
        {
            if (string.IsNullOrEmpty(player.LastHunterStaminaUpdate))
            {
                player.LastHunterStaminaUpdate = DateTimeOffset.UtcNow.ToString("o");
                return;
            }

            if (!DateTimeOffset.TryParse(player.LastHunterStaminaUpdate, out var last))
                return;

            var now = DateTimeOffset.UtcNow;
            var minutesPassed = (int)(now - last).TotalMinutes;

            if (minutesPassed <= 0)
                return;

            player.HunterStamina = Math.Min(MaxStamina, player.HunterStamina + minutesPassed);
            player.LastHunterStaminaUpdate = now.ToString("o");
        }

        public void Spend(Player player, int amount)
        {
            player.HunterStamina -= amount;
        }
    }
}
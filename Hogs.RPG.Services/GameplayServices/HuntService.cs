using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.GameData.Hunts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.GameplayServices
{
    public class HuntService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly LevelService _levelService;

        private readonly Random _random = new();
        public HuntService(PlayerRepository playerRepository, InventoryService inventoryService, LevelService levelService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _levelService = levelService;
        }

        public async Task<string> HuntAsync(ulong userId, string targetId = null)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to use /startadventure first.";

            // Cooldown duration
            var cooldown = TimeSpan.FromMinutes(1);

            if (!string.IsNullOrEmpty(player.LastHunt))
            {
                if (DateTimeOffset.TryParse(player.LastHunt, out var lastHuntTime))
                {
                    var timeSinceLastHunt = DateTimeOffset.UtcNow - lastHuntTime;

                    if (timeSinceLastHunt < cooldown)
                    {
                        var remaining = cooldown - timeSinceLastHunt;
                        return $"You are tired. Try hunting again in {Math.Ceiling(remaining.TotalSeconds)} seconds.";
                    }
                }
            }

            HuntTarget target;



            if (!string.IsNullOrWhiteSpace(targetId))
            {
                var key = targetId.Trim().ToLower();

                if (HuntTargetRegistry.All.TryGetValue(key, out target))
                {
                    // targeted hunt
                    if (player.Level < target.RequiredLevel)
                    {
                        return $"You must be level {target.RequiredLevel} to hunt **{target.Name}**.";
                    }
                }
                else
                {
                    return "Unknown hunt target. Try: wolf, boar, stag, raven, fox.";
                }
            }
            else
            {
                // random hunt
                var availableTargets = HuntTargetRegistry.All.Values
                    .Where(t => player.Level >= t.RequiredLevel)
                    .ToList();

                if (availableTargets.Count == 0)
                {
                    return "You are not high enough level to hunt anything yet.";
                }

                var randomIndex = _random.Next(availableTargets.Count);
                target = availableTargets[randomIndex];
            }

            int xp = _random.Next(target.MinXP, target.MaxXP);
            int gold = _random.Next(target.MinGold, target.MaxGold);
            int dropAmount = _random.Next(target.MinDrop, target.MaxDrop + 1);

            player.XP += xp;
            player.Gold += gold;

            var levelMessage = _levelService.CheckLevelUp(player);

            await _inventoryService.GiveItemAsync(userId, target.DropItem, dropAmount);

            player.LastHunt = DateTimeOffset.UtcNow.ToString("o");

            await _playerRepository.UpdatePlayerAsync(player);

            return $"{target.Icon} You hunted a {target.Name}!\n\n+{xp} XP\n+{gold} Gold\n+{dropAmount} {target.DropItem}{levelMessage}";
        }
    }
}
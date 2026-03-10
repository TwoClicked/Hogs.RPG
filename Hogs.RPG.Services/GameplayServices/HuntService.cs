using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
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

        private readonly Dictionary<string, HuntTarget> _targets = new()
        {
            {
                "wolf",
                new HuntTarget
                {
                    Id = "wolf",
                    Name = "Wolf",
                    Icon = "🐺",
                    DropItem = "fur",
                    MinXP = 10,
                    MaxXP = 18,
                    MinGold = 5,
                    MaxGold = 12,
                    MinDrop = 1,
                    MaxDrop = 3
                }
            },
            {
                "boar",
                new HuntTarget
                {
                    Id = "boar",
                    Name = "Boar",
                    Icon = "🐗",
                    DropItem = "leather",
                    MinXP = 12,
                    MaxXP = 20,
                    MinGold = 6,
                    MaxGold = 14,
                    MinDrop = 1,
                    MaxDrop = 2
                }
            },
            {
                "stag",
                new HuntTarget
                {
                    Id = "stag",
                    Name = "Stag",
                    Icon = "🦌",
                    DropItem = "bone",
                    MinXP = 11,
                    MaxXP = 19,
                    MinGold = 6,
                    MaxGold = 13,
                    MinDrop = 1,
                    MaxDrop = 2
                }
            },
            {
                "raven",
                new HuntTarget
                {
                    Id = "raven",
                    Name = "Raven",
                    Icon = "🐦",
                    DropItem = "feather",
                    MinXP = 9,
                    MaxXP = 16,
                    MinGold = 4,
                    MaxGold = 10,
                    MinDrop = 1,
                    MaxDrop = 2
                }
            },
            {
                "fox",
                new HuntTarget
                {
                    Id = "fox",
                    Name = "Fox",
                    Icon = "🦊",
                    DropItem = "fur",
                    MinXP = 9,
                    MaxXP = 15,
                    MinGold = 4,
                    MaxGold = 9,
                    MinDrop = 1,
                    MaxDrop = 2
                }
            }
        };

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

                if (_targets.TryGetValue(key, out target))
                {
                    // targeted hunt
                }
                else
                {
                    return "Unknown hunt target. Try: wolf, boar, stag, raven, fox.";
                }
            }
            else
            {
                // random hunt
                var randomIndex = _random.Next(_targets.Count);
                target = _targets.Values.ElementAt(randomIndex);
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
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.GameData.Hunts;
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
        private readonly BuffService _buffService;

        private readonly Random _random = new();

        private readonly Dictionary<string, string> _rareDrops = new()
        {
            { "wolf", "wolf_trophy" },
            { "boar", "boar_tusk" },
            { "stag", "stag_antler" },
            { "raven", "ancient_feather" },
            { "fox", "shadow_claw" },

            { "bear", "bear_heart" },
            { "dire_wolf", "alpha_fang" },
            { "eagle", "storm_talon" },

            { "sabertooth", "saber_relic" },
            { "griffin", "griffin_core" },

            { "storm_eagle", "storm_relic" },
            { "ancient_bear", "ancient_core" },

            { "mythic_bear", "mythic_heart" },
            { "sky_tyrant", "sky_relic" }
        };

        public HuntService(PlayerRepository playerRepository, InventoryService inventoryService, LevelService levelService, BuffService buffService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _levelService = levelService;
            _buffService = buffService;
        }

        public async Task<string> HuntAsync(ulong userId, string targetId = null)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to use /startadventure first.";

            var cooldown = TimeSpan.FromMinutes(1);

            if (!string.IsNullOrEmpty(player.LastHunt))
            {
                if (DateTimeOffset.TryParse(player.LastHunt, out var lastHuntTime))
                {
                    var timeSinceLastHunt = DateTimeOffset.UtcNow - lastHuntTime;

                    if (timeSinceLastHunt < cooldown)
                    {
                        var remaining = cooldown - timeSinceLastHunt;
                        return $"You are tired. Try hunting again in {Math.Ceiling(remaining.TotalSeconds)} seconds." ;
                    }
                }
            }

            HuntTarget target;

            if (!string.IsNullOrWhiteSpace(targetId))
            {
                var key = targetId.Trim().ToLower();

                if (HuntTargetRegistry.All.TryGetValue(key, out target))
                {
                    if (player.Level < target.RequiredLevel)
                    {
                        return $"You must be level {target.RequiredLevel} to hunt **{target.Name}**.";
                    }
                }
                else
                {
                    return "Unknown hunt target.";
                }
            }
            else
            {
                var availableTargets = HuntTargetRegistry.All.Values
                    .Where(t => player.Level >= t.RequiredLevel)
                    .ToList();

                if (availableTargets.Count == 0)
                    return "You are not high enough level to hunt anything yet.";

                target = availableTargets[_random.Next(availableTargets.Count)];
            }

            // Base rewards
            int xp = _random.Next(target.MinXP, target.MaxXP);
            int gold = _random.Next(target.MinGold, target.MaxGold);
            int dropAmount = _random.Next(target.MinDrop, target.MaxDrop + 1);

            // Auto XP Potion Consumption
            if (player.AutoUseXpPotions)
            {
                var inventory = await _inventoryService.GetInventoryAsync(userId);

                var potion = inventory.FirstOrDefault(i => i.ItemId == "xp_potion");

                if (potion != null && potion.Quantity > 0)
                {
                    await _inventoryService.TakeItemAsync(userId, "xp_potion", 1);

                    double multiplier = 2;

                    player.ActiveBuffs.Add(new ActiveBuff
                    {
                        Type = BuffType.XP,
                        Value = multiplier,
                        RemainingUses = 1
                    });
                }
                else
                {
                    player.AutoUseXpPotions = false;
                }
            }

            // Apply Buffs
            double xpMultiplier = _buffService.ApplyXpBuff(player);
            double goldMultiplier = _buffService.ApplyGoldBuff(player);

            xp = (int)(xp * xpMultiplier);
            gold = (int)(gold * goldMultiplier);

            string encounterText = "";

            if (xpMultiplier > 1)
                encounterText += $"\n✨ XP Potion Activated! ({xpMultiplier}x XP)";

            double roll = _random.NextDouble();

            bool elite = false;
            bool jackpot = false;
            bool rareDrop = false;
            bool treasure = false;

            if (roll < 0.01)
                treasure = true;
            else if (roll < 0.04)
                rareDrop = true;
            else if (roll < 0.08)
                jackpot = true;
            else if (roll < 0.16)
                elite = true;

            if (elite)
            {
                xp = (int)(xp * 1.5);
                gold = (int)(gold * 1.5);
                dropAmount += 2;

                encounterText += "\n🔥 Elite creature encountered!";
            }

            player.XP += xp;
            player.Gold += gold;

            var levelMessage = _levelService.CheckLevelUp(player);

            await _inventoryService.GiveItemAsync(userId, target.DropItem, dropAmount);

            if (jackpot)
            {
                int bonus = _random.Next(5, 9);

                await _inventoryService.GiveItemAsync(userId, target.DropItem, bonus);

                encounterText += $"\n💰 Jackpot! You found {bonus} extra {target.DropItem}!";
            }

            if (rareDrop && _rareDrops.ContainsKey(target.Id))
            {
                var rareItem = _rareDrops[target.Id];

                await _inventoryService.GiveItemAsync(userId, rareItem, 1);

                encounterText += $"\n✨ Rare Drop! You found **{rareItem}**!";
            }

            if (treasure)
            {
                int treasureGold = _random.Next(120, 220);

                player.Gold += treasureGold;

                encounterText += $"\n💰 Treasure Creature! You gained {treasureGold} bonus gold!";
            }

            player.LastHunt = DateTimeOffset.UtcNow.ToString("o");

            await _playerRepository.UpdatePlayerAsync(player);

            return $"{target.Icon} You hunted a {target.Name}!\n\n" +
                   $"+{xp} XP\n" +
                   $"+{gold} Gold\n" +
                   $"+{dropAmount} {target.DropItem}" +
                   $"{encounterText}" +
                   $"{levelMessage}";
        }
    }
}
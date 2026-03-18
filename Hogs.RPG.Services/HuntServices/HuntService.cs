using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.GameData.Hunts;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.HuntServices
{
    public class HuntService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly LevelService _levelService;
        private readonly BuffService _buffService;
        private readonly HunterStaminaService _staminaService;

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

        public HuntService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            LevelService levelService,
            BuffService buffService,
            HunterStaminaService staminaService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _levelService = levelService;
            _buffService = buffService;
            _staminaService = staminaService;
        }

        public async Task<string> HuntAsync(ulong userId, string targetId = null, int stamina = 10)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to use /startadventure first.";

            // 🔄 Regenerate stamina
            _staminaService.Regenerate(player);

            bool usedMax = false;

            // ✅ Handle /hunt max
            if (stamina == -1)
            {
                stamina = player.HunterStamina;
                usedMax = true;
            }

            if (stamina <= 0)
                return "You have no stamina to hunt.";

            if (player.HunterStamina < stamina)
                return $"🏹 You only have {player.HunterStamina} stamina.";

            HuntTarget target;

            if (!string.IsNullOrWhiteSpace(targetId))
            {
                var key = targetId.Trim().ToLower();

                if (HuntTargetRegistry.All.TryGetValue(key, out target))
                {
                    if (player.Level < target.RequiredLevel)
                        return $"You must be level {target.RequiredLevel} to hunt **{target.Name}**.";
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

            // 🔋 Spend stamina (after validation)
            _staminaService.Spend(player, stamina);

            // 🔁 Aggregation
            int totalXp = 0;
            int totalGold = 0;
            int totalDrops = 0;

            int eliteCount = 0;
            int jackpotCount = 0;
            int rareCount = 0;
            int treasureCount = 0;

            // Auto XP Potion (once per hunt)
            if (player.AutoUseXpPotions)
            {
                var inventory = await _inventoryService.GetInventoryAsync(userId);

                var potion = inventory.FirstOrDefault(i => i.ItemId == "xp_potion");

                if (potion != null && potion.Quantity > 0)
                {
                    await _inventoryService.TakeItemAsync(userId, "xp_potion", 1);

                    player.ActiveBuffs.Add(new ActiveBuff
                    {
                        Type = BuffType.XP,
                        Value = 2,
                        RemainingUses = 1
                    });
                }
                else
                {
                    player.AutoUseXpPotions = false;
                }
            }

            for (int i = 0; i < stamina; i++)
            {
                int xp = _random.Next(target.MinXP, target.MaxXP);
                int gold = _random.Next(target.MinGold, target.MaxGold);
                int dropAmount = _random.Next(target.MinDrop, target.MaxDrop + 1);

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
                    eliteCount++;
                }

                totalXp += xp;
                totalGold += gold;
                totalDrops += dropAmount;

                if (jackpot)
                {
                    int bonus = _random.Next(5, 9);
                    totalDrops += bonus;
                    jackpotCount++;
                }

                if (rareDrop && _rareDrops.ContainsKey(target.Id))
                {
                    var rareItem = _rareDrops[target.Id];
                    await _inventoryService.GiveItemAsync(userId, rareItem, 1);
                    rareCount++;
                }

                if (treasure)
                {
                    int treasureGold = _random.Next(120, 220);
                    totalGold += treasureGold;
                    treasureCount++;
                }
            }

            // Apply buffs ONCE
            double xpMultiplier = _buffService.ApplyXpBuff(player);
            double goldMultiplier = _buffService.ApplyGoldBuff(player);

            totalXp = (int)(totalXp * xpMultiplier);
            totalGold = (int)(totalGold * goldMultiplier);

            player.XP += totalXp;
            player.Gold += totalGold;

            var levelMessage = _levelService.CheckLevelUp(player);

            await _inventoryService.GiveItemAsync(userId, target.DropItem, totalDrops);

            await _playerRepository.UpdatePlayerAsync(player);

            // 🧾 Build result
            var sb = new StringBuilder();

            var label = usedMax ? "ALL stamina" : $"{stamina} stamina";

            sb.AppendLine($"{target.Icon} You hunted {target.Name} using {label}!\n");

            sb.AppendLine($"+{totalXp} XP");
            sb.AppendLine($"+{totalGold} Gold");
            sb.AppendLine($"+{totalDrops} {target.DropItem}");

            if (xpMultiplier > 1)
                sb.AppendLine($"✨ XP Potion Active ({xpMultiplier}x)");

            if (eliteCount > 0)
                sb.AppendLine($"🔥 {eliteCount} Elite encounters!");

            if (jackpotCount > 0)
                sb.AppendLine($"💰 {jackpotCount} Jackpots!");

            if (rareCount > 0)
                sb.AppendLine($"✨ {rareCount} Rare drops!");

            if (treasureCount > 0)
                sb.AppendLine($"💰 {treasureCount} Treasure finds!");

            sb.AppendLine($"\n🏹 Stamina: {player.HunterStamina}/100");

            sb.AppendLine(levelMessage);

            return sb.ToString();
        }
    }
}
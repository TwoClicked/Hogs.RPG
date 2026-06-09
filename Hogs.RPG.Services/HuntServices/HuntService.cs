// Hogs.RPG.Services/HuntServices/HuntService.cs

using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities.GameLoopObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.GameData.Hunts;
using Hogs.RPG.Services.AchievementServices;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
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
        private readonly DiscordSocketClient _client;
        private readonly PetService _petService;
        private readonly GameEventService _gameEventService;
        private readonly AchievementService _achievementService;

        private readonly ulong _feedChannelId = 1485357755433750549;
        private readonly Random _random = new();

        public HuntService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            LevelService levelService,
            BuffService buffService,
            HunterStaminaService staminaService,
            DiscordSocketClient client,
            PetService petService,
            GameEventService gameEventService,
            AchievementService achievementService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _levelService = levelService;
            _buffService = buffService;
            _staminaService = staminaService;
            _client = client;
            _petService = petService;
            _gameEventService = gameEventService;
            _achievementService = achievementService;
        }

        public async Task<string> HuntAsync(ulong userId, string targetId = null, int stamina = 10)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to use /startadventure first.";

            _staminaService.Regenerate(player);

            bool usedMax = false;

            if (stamina == -1)
            {
                stamina = player.HunterStamina;
                usedMax = true;
            }

            if (stamina <= 0)
                return "You have no stamina to hunt.";

            if (player.HunterStamina < stamina)
                return $"🏹 You only have {player.HunterStamina} stamina.";

            // =========================
            // 🎯 TARGET
            // =========================
            HuntTarget target;

            if (!string.IsNullOrWhiteSpace(targetId))
            {
                var key = targetId.Trim().ToLower();

                if (!HuntTargetRegistry.All.TryGetValue(key, out target))
                    return "Unknown hunt target.";

                if (player.Level < target.RequiredLevel)
                    return $"You must be level {target.RequiredLevel} to hunt **{target.Name}**.";
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

            _staminaService.Spend(player, stamina);

            // =========================
            // 🏹 HUNTING GEAR BONUSES
            // =========================
            var equippedSlots = new[]
            {
                player.MainHand, player.OffHand, player.Helmet, player.Body,
                player.Legs, player.Gloves, player.Boots, player.Ring, player.Amulet
            };

            double huntXpBonus = 0;
            double huntMaterialBonus = 0;
            double huntRareBonus = 0;
            int huntingGearCount = 0;

            foreach (var slotId in equippedSlots)
            {
                if (string.IsNullOrEmpty(slotId)) continue;
                if (!EquipmentRegistry.All.TryGetValue(slotId, out var gearDef)) continue;

                huntXpBonus += gearDef.HuntXpBonus;
                huntMaterialBonus += gearDef.HuntMaterialBonus;

                if (gearDef.IsHuntingGear)
                    huntingGearCount++;
            }

            // Full 9-piece set bonus
            bool fullSetBonus = huntingGearCount == 9;
            if (fullSetBonus)
                huntRareBonus += 0.05;

            bool permanentSetBonus = player.HasHunterSetBonus;
            if (permanentSetBonus)
            {
                huntXpBonus += 0.045;
                huntMaterialBonus += 0.045;
                huntRareBonus += 0.05;
                fullSetBonus = true;
            }

            // Hunting pet bonus
            if (player.HasHuntingPet)
            {
                huntXpBonus += 0.05;
                huntMaterialBonus += 0.05;
                huntRareBonus += 0.03;
            }

            // =========================
            // 🧪 ALCHEMIST POTION BUFFS
            // =========================
            bool potionLootActive = false;
            bool potionXpActive = false;
            double potionLootBonus = 0;
            double potionXpBonus = 0;
            string potionBuffName = "";

            if (player.ActiveUtilityBuffId != null &&
                player.ActiveUtilityBuffExpiry.HasValue &&
                player.ActiveUtilityBuffExpiry.Value > DateTime.UtcNow)
            {
                if (AlchemyPotionRegistry.All.TryGetValue(player.ActiveUtilityBuffId, out var utilPotion))
                {
                    if (utilPotion.EffectId == "loot_boost")
                    {
                        potionLootActive = true;
                        potionLootBonus = utilPotion.EffectValue / 100.0;
                        huntRareBonus += potionLootBonus;
                        potionBuffName = utilPotion.Name;
                    }
                    else if (utilPotion.EffectId == "xp_boost")
                    {
                        potionXpActive = true;
                        potionXpBonus = utilPotion.EffectValue / 100.0;
                        potionBuffName = utilPotion.Name;
                    }

                    // Void Tincture — both loot and XP
                    if (utilPotion.SecondaryEffectId == "loot_boost")
                    {
                        potionLootActive = true;
                        potionLootBonus = utilPotion.SecondaryEffectValue / 100.0;
                        huntRareBonus += potionLootBonus;
                    }
                    else if (utilPotion.SecondaryEffectId == "xp_boost")
                    {
                        potionXpActive = true;
                        potionXpBonus = utilPotion.SecondaryEffectValue / 100.0;
                    }
                }
            }

            int totalXp = 0;
            int totalDrops = 0;
            int eliteCount = 0;
            int rareCount = 0;
            int xpPotionsUsed = 0;
            int remainingXpPotions = 0;

            // =========================
            // 🔁 HUNT LOOP
            // =========================
            for (int i = 0; i < stamina; i++)
            {
                int xp = _random.Next(target.MinXP, target.MaxXP);
                int dropAmount = _random.Next(target.MinDrop, target.MaxDrop + 1);

                double roll = _random.NextDouble();
                bool elite = roll < 0.16;
                bool rareDrop = roll < (0.04 + huntRareBonus);

                if (elite)
                {
                    xp = (int)(xp * 1.5);
                    eliteCount++;

                    if (!string.IsNullOrEmpty(target.RareDropItem))
                        rareDrop = _random.NextDouble() < (0.10 + huntRareBonus);
                }

                totalXp += xp;
                totalDrops += dropAmount;

                if (rareDrop && !string.IsNullOrEmpty(target.RareDropItem))
                {
                    await _inventoryService.GiveItemAsync(userId, target.RareDropItem, 1);
                    rareCount++;
                }
            }

            // =========================
            // 🏹 APPLY GEAR & PET BONUSES TO TOTALS
            // =========================
            if (huntXpBonus > 0)
                totalXp = (int)(totalXp * (1 + huntXpBonus));

            if (huntMaterialBonus > 0)
                totalDrops = (int)(totalDrops * (1 + huntMaterialBonus));

            // =========================
            // 🧪 APPLY POTION XP BONUS
            // =========================
            if (potionXpActive && potionXpBonus > 0)
                totalXp = (int)(totalXp * (1 + potionXpBonus));

            // =========================
            // 🧪 XP POTION (auto-use)
            // =========================
            bool xpBoostActive = player.XpBoostExpiry.HasValue && player.XpBoostExpiry.Value > DateTime.UtcNow;

            if (player.AutoUseXpPotions && !xpBoostActive)
            {
                int desiredPotions = stamina / 5;
                var inventory = await _inventoryService.GetInventoryAsync(userId);
                var potion = inventory.FirstOrDefault(i => i.ItemId == "xp_potion");
                int available = potion?.Quantity ?? 0;

                if (desiredPotions > 0 && available > 0)
                {
                    int potionsToUse = Math.Min(desiredPotions, available);
                    await _inventoryService.TakeItemAsync(userId, "xp_potion", potionsToUse);
                    xpPotionsUsed = potionsToUse;
                    remainingXpPotions = available - potionsToUse;
                    totalXp = (int)(totalXp * 1.5);
                }
                else
                {
                    remainingXpPotions = available;
                    if (available == 0)
                        player.AutoUseXpPotions = false;
                }
            }

            // =========================
            // 📈 OTHER ACTIVE BUFFS (non-potion)
            // =========================
            double xpMultiplier = 1.0;
            if (xpBoostActive)
                xpMultiplier = 2.0;
            else
                xpMultiplier = _buffService.ApplyXpBuff(player);

            if (xpMultiplier > 1)
                totalXp = (int)(totalXp * xpMultiplier);

            player.XP += totalXp;

            var (levelMessage, levelsGained) = _levelService.CheckLevelUp(player);

            // =========================
            // 🐾 PET XP
            // =========================
            int petXp = Math.Max(1, stamina / 2);
            var (petLeveledUp, petNewLevel) = await _petService.AddXPAsync(userId, petXp);

            string petLevelMessage = "";

            if (petLeveledUp)
            {
                var pet = await _petService.GetEquippedPetAsync(userId);

                if (pet != null && PetRegistry.All.TryGetValue(pet.PetId, out var petDef))
                    petLevelMessage = $"\n\n🐾 **{petDef.Icon} {petDef.Name}** leveled up! It is now **Level {petNewLevel}** 🎉";
            }

            // =========================
            // 📦 BASE DROPS
            // =========================
            await _inventoryService.GiveItemAsync(userId, target.DropItem, totalDrops);

            // =========================
            // 🧪 ALCHEMY XP — granted when hunting alchemy category monsters
            // =========================
            int alchemyLevelUps = 0;
            if (target.AlchemyXpReward > 0)
            {
                player.AlchemistXP += target.AlchemyXpReward;

                while (player.AlchemistLevel < 99)
                {
                    int xpNeeded = player.AlchemistLevel * player.AlchemistLevel * 50;
                    if (player.AlchemistXP >= xpNeeded)
                    {
                        player.AlchemistLevel++;
                        alchemyLevelUps++;
                    }
                    else break;
                }
            }

            // =========================
            // 📊 ACHIEVEMENT COUNTERS
            // Must be before UpdatePlayerAsync so they are persisted
            // =========================
            player.TotalHuntsCompleted++;
            player.TotalRareDrops += rareCount;
            player.TotalStaminaSpent += stamina;

            // =========================
            // 💾 SAVE PLAYER
            // All player field changes saved in one call
            // =========================
            await _playerRepository.UpdatePlayerAsync(player);

            // =========================
            // 🎉 PLAYER LEVEL UP FEED
            // =========================
            if (levelsGained > 0)
            {
                var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;
                if (channel != null)
                    await channel.SendMessageAsync($"🎉 <@{player.DiscordId}> reached **Level {player.Level}**!");
            }

            // =========================
            // 🧪 ALCHEMY LEVEL UP FEED
            // =========================
            if (alchemyLevelUps > 0)
                await _gameEventService.SendAlchemistLevelUpAsync(player);

            // =========================
            // 🏆 ACHIEVEMENT CHECK
            // =========================
            await _achievementService.CheckAndAwardAsync(userId);

            // =========================
            // 🧾 RESULT
            // =========================
            var sb = new StringBuilder();
            var label = usedMax ? "ALL stamina" : $"{stamina} stamina";

            sb.AppendLine($"{target.Icon} You hunted {target.Name} using {label}!\n");
            sb.AppendLine($"+{totalXp} XP");

            string dropName = target.DropItem;
            if (InventoryItemDefinitions.All.TryGetValue(target.DropItem, out var itemDef))
                dropName = itemDef.Name;

            sb.AppendLine($"+{totalDrops} {dropName}");

            if (xpPotionsUsed > 0)
                sb.AppendLine($"✨ Used {xpPotionsUsed} XP Potion(s) ({remainingXpPotions} left)");
            else if (player.AutoUseXpPotions)
                sb.AppendLine($"🧪 Double XP active, No XP Potions used");

            if (xpMultiplier > 1)
                sb.AppendLine($"✨ XP Multiplier Active ({xpMultiplier}x)");

            if (potionLootActive)
                sb.AppendLine($"🧪 {potionBuffName}: +{potionLootBonus * 100:0.#}% rare drop chance");

            if (potionXpActive)
                sb.AppendLine($"🧪 {potionBuffName}: +{potionXpBonus * 100:0.#}% XP boost");

            if (eliteCount > 0)
                sb.AppendLine($"🔥 {eliteCount} Elite encounters!");

            if (rareCount > 0)
                sb.AppendLine($"✨ {rareCount} Rare drops!");

            if (permanentSetBonus)
                sb.AppendLine($"🌟 Hunter Set: Permanent! (+4.5% XP, +4.5% mats, +5% rare)");
            else
            {
                if (huntingGearCount > 0)
                    sb.AppendLine($"🏹 Hunter's Gear: {huntingGearCount}/9 pieces " +
                        $"(+{huntXpBonus * 100:0.#}% XP, +{huntMaterialBonus * 100:0.#}% materials)");

                if (fullSetBonus)
                    sb.AppendLine($"🌟 Full Set Bonus: +5% rare drop rate");
            }

            if (player.HasHuntingPet)
                sb.AppendLine($"🐾 Hunting Companion: +5% XP, +5% materials, +3% rare drop");

            int displayMax = player.StaminaBoostExpiry.HasValue && player.StaminaBoostExpiry.Value > DateTime.UtcNow
                ? 150 : 100;

            sb.AppendLine($"\n🏹 Stamina: {player.HunterStamina}/{displayMax}");
            sb.AppendLine(levelMessage);
            sb.AppendLine(petLevelMessage);

            return sb.ToString();
        }
    }
}

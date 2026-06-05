using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.InventoryServices;
using System.Text;

namespace Hogs.RPG.Services.SmithingServices
{
    public class SmithingService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly SmithingShopRepository _shopRepository;
        private readonly GameEventService _gameEventService;

        public SmithingService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            SmithingShopRepository shopRepository,
            GameEventService gameEventService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _shopRepository = shopRepository;
            _gameEventService = gameEventService;
        }

        // =========================
        // SMELT
        // BlackSmith only — consumes ore, produces bars
        // =========================
        public async Task<string> SmeltAsync(ulong userId, string barId, int quantity)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null)
                return "❌ You need to start your adventure first.";

            if (!SmeltingRegistry.All.TryGetValue(barId, out var recipe))
                return "❌ Unknown bar.";

            if (player.SmithingLevel < recipe.RequiredSmithingLevel)
                return $"❌ You need Smithing level **{recipe.RequiredSmithingLevel}** to smelt {recipe.BarName}.";

            // =========================
            // HANDLE MAX QUANTITY
            // =========================
            if (quantity == -1)
            {
                var invForMax = await _inventoryService.GetInventoryAsync(userId);
                var invMaxLookup = invForMax.ToDictionary(i => i.ItemId, i => i.Quantity);

                int maxCanMake = int.MaxValue;
                foreach (var (oreId, orePerSmelt) in recipe.OreRequirements)
                {
                    invMaxLookup.TryGetValue(oreId, out int owned);
                    maxCanMake = Math.Min(maxCanMake, owned / orePerSmelt);
                }

                quantity = maxCanMake == int.MaxValue ? 0 : maxCanMake;

                if (quantity <= 0)
                    return $"❌ You don't have enough ore to smelt any {recipe.BarName}.";
            }

            // =========================
            // CHECK ORE REQUIREMENTS
            // =========================
            var inventory = await _inventoryService.GetInventoryAsync(userId);
            var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            foreach (var (oreId, orePerSmelt) in recipe.OreRequirements)
            {
                int totalNeeded = orePerSmelt * quantity;
                invLookup.TryGetValue(oreId, out int owned);

                if (owned < totalNeeded)
                {
                    var oreName = InventoryItemDefinitions.All.TryGetValue(oreId, out var oreDef)
                        ? oreDef.Name : oreId;
                    return $"❌ You need **{totalNeeded}x {oreName}** but only have **{owned}**.";
                }
            }

            // =========================
            // CONSUME ORES
            // =========================
            foreach (var (oreId, orePerSmelt) in recipe.OreRequirements)
            {
                await _inventoryService.TakeItemAsync(userId, oreId, orePerSmelt * quantity);
            }

            // =========================
            // GIVE BARS
            // =========================
            await _inventoryService.GiveItemAsync(userId, barId, quantity);

            var barName = InventoryItemDefinitions.All.TryGetValue(barId, out var barDef)
                ? barDef.Name : barId;

            return $"🔥 You smelted **{quantity}x {barName}**.";
        }

        // =========================
        // CRAFT
        // BlackSmith only — consumes bars/materials, grants XP, auto-lists in shop
        // =========================
        public async Task<string> CraftAsync(ulong userId, string itemId, int quantity)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null)
                return "❌ You need to start your adventure first.";

            if (!SmithingItemRegistry.All.TryGetValue(itemId, out var itemDef))
                return "❌ Unknown item.";

            if (player.SmithingLevel < itemDef.RequiredSmithingLevel)
                return $"❌ You need Smithing level **{itemDef.RequiredSmithingLevel}** to forge **{itemDef.Name}**.";

            // =========================
            // HANDLE MAX QUANTITY
            // =========================
            if (quantity == -1)
            {
                var invForMax = await _inventoryService.GetInventoryAsync(userId);
                var invMaxLookup = invForMax.ToDictionary(i => i.ItemId, i => i.Quantity);

                int maxCanForge = int.MaxValue;
                foreach (var (matId, matPerCraft) in itemDef.BarRequirements)
                {
                    invMaxLookup.TryGetValue(matId, out int owned);
                    maxCanForge = Math.Min(maxCanForge, owned / matPerCraft);
                }

                quantity = maxCanForge == int.MaxValue ? 0 : maxCanForge;

                if (quantity <= 0)
                    return $"❌ You don't have enough materials to forge any {itemDef.Name}.";
            }

            // =========================
            // CHECK MATERIAL REQUIREMENTS
            // =========================
            var inventory = await _inventoryService.GetInventoryAsync(userId);
            var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            foreach (var (matId, matPerCraft) in itemDef.BarRequirements)
            {
                int totalNeeded = matPerCraft * quantity;
                invLookup.TryGetValue(matId, out int owned);

                if (owned < totalNeeded)
                {
                    var matName = InventoryItemDefinitions.All.TryGetValue(matId, out var matDef)
                        ? matDef.Name : matId;
                    return $"❌ You need **{totalNeeded}x {matName}** but only have **{owned}**.";
                }
            }

            // =========================
            // CONSUME MATERIALS
            // =========================
            foreach (var (matId, matPerCraft) in itemDef.BarRequirements)
            {
                await _inventoryService.TakeItemAsync(userId, matId, matPerCraft * quantity);
            }

            // =========================
            // GRANT XP + LEVEL UP CHECK
            // =========================
            int xpGained = itemDef.SmithingXpReward * quantity;
            player.SmithingXP += xpGained;

            int levelUps = ProcessLevelUps(player);

            // =========================
            // AUTO-LIST IN NPC SHOP
            // =========================
            await _shopRepository.AddOrIncrementAsync(userId, itemId, quantity);

            // =========================
            // SAVE PLAYER
            // =========================
            await _playerRepository.UpdatePlayerAsync(player);

            if (levelUps > 0)
                await _gameEventService.SendSmithingLevelUpAsync(player);

            // =========================
            // BUILD RESPONSE
            // =========================
            var sb = new StringBuilder();
            sb.AppendLine($"⚒️ You forged **{quantity}x {itemDef.Name}**. (+{xpGained} Smithing XP)");
            sb.AppendLine($"🛒 Auto-listed in your NPC shop.");

            if (levelUps > 0)
                sb.AppendLine($"\n⬆️ **Smithing Level Up!** You are now Smithing level **{player.SmithingLevel}**!");

            if (player.SmithingLevel < 99)
            {
                int xpToNext = GetTotalXpForLevel(player.SmithingLevel + 1) - player.SmithingXP;
                sb.AppendLine($"✨ Smithing: Level **{player.SmithingLevel}** | **{xpToNext}** XP to next level");
            }
            else
            {
                sb.AppendLine($"✨ Smithing: Level **99** — MAX");
            }

            return sb.ToString().Trim();
        }

        // =========================
        // LEVEL UP PROCESSING
        // =========================
        private int ProcessLevelUps(Player player)
        {
            int levelUps = 0;

            while (player.SmithingLevel < 99)
            {
                int xpNeeded = GetTotalXpForLevel(player.SmithingLevel + 1);
                if (player.SmithingXP >= xpNeeded)
                {
                    player.SmithingLevel++;
                    levelUps++;
                }
                else break;
            }

            return levelUps;
        }

        // =========================
        // XP CURVE — Total XP required to reach a given level
        // Formula: level² × 50
        // =========================
        public static int GetTotalXpForLevel(int level)
        {
            return level * level * 50;
        }
    }
}
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.Alchemy;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AchievementServices;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System.Text;

namespace Hogs.RPG.Services.AlchemyServices
{
    public class AlchemyBrewService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly HunterStaminaService _staminaService;
        private readonly GameEventService _gameEventService;
        private readonly AchievementService _achievementService;

        private const int DailyPotionCap = 5;

        public AlchemyBrewService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            HunterStaminaService staminaService,
            GameEventService gameEventService,
            AchievementService achievementService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _staminaService = staminaService;
            _gameEventService = gameEventService;
            _achievementService = achievementService;
        }

        // =========================
        // BREW
        // =========================
        public async Task<string> BrewAsync(ulong userId, string potionId, int quantity)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null)
                return "❌ You need to start your adventure first.";

            if (!AlchemyPotionRegistry.All.TryGetValue(potionId, out var potion))
                return "❌ Unknown potion.";

            if (player.AlchemistLevel < potion.RequiredAlchemistLevel)
                return $"❌ You need Alchemist level **{potion.RequiredAlchemistLevel}** to brew **{potion.Name}**.";

            // =========================
            // HANDLE MAX QUANTITY
            // =========================
            if (quantity == -1)
            {
                var invForMax = await _inventoryService.GetInventoryAsync(userId);
                var invMaxLookup = invForMax.ToDictionary(i => i.ItemId, i => i.Quantity);

                int maxCanBrew = int.MaxValue;
                foreach (var (ingId, ingPerBrew) in potion.IngredientRequirements)
                {
                    invMaxLookup.TryGetValue(ingId, out int owned);
                    maxCanBrew = Math.Min(maxCanBrew, owned / ingPerBrew);
                }

                quantity = maxCanBrew == int.MaxValue ? 0 : maxCanBrew;

                if (quantity <= 0)
                    return $"❌ You don't have enough ingredients to brew any {potion.Name}.";
            }

            // =========================
            // CHECK INGREDIENT REQUIREMENTS
            // =========================
            var inventory = await _inventoryService.GetInventoryAsync(userId);
            var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            foreach (var (ingId, ingPerBrew) in potion.IngredientRequirements)
            {
                int totalNeeded = ingPerBrew * quantity;
                invLookup.TryGetValue(ingId, out int owned);

                if (owned < totalNeeded)
                {
                    var ingName = InventoryItemDefinitions.All.TryGetValue(ingId, out var ingDef)
                        ? ingDef.Name : ingId;
                    return $"❌ You need **{totalNeeded}x {ingName}** but only have **{owned}**.";
                }
            }

            // =========================
            // CONSUME INGREDIENTS
            // =========================
            foreach (var (ingId, ingPerBrew) in potion.IngredientRequirements)
            {
                await _inventoryService.TakeItemAsync(userId, ingId, ingPerBrew * quantity);
            }

            // =========================
            // GIVE POTIONS
            // =========================
            await _inventoryService.GiveItemAsync(userId, potionId, quantity);

            // =========================
            // GRANT XP + LEVEL UP CHECK
            // =========================
            int xpGained = potion.AlchemyXpReward * quantity;
            if (player.HasAlchemistPet)
                xpGained = (int)(xpGained * 1.10);
            player.AlchemistXP += xpGained;

            int levelUps = ProcessLevelUps(player);

            // =========================
            // 📊 ACHIEVEMENT COUNTERS
            // Must be before UpdatePlayerAsync
            // =========================
            player.TotalPotionsBrewed += quantity;
            if (potionId == "blacksmiths_elixir")
                player.BlacksmithElixirUsed = true;

            // =========================
            // SAVE PLAYER
            // =========================
            await _playerRepository.UpdatePlayerAsync(player);

            if (levelUps > 0)
                await _gameEventService.SendAlchemistLevelUpAsync(player);

            // =========================
            // 🏆 ACHIEVEMENT CHECK
            // =========================
            await _achievementService.CheckAndAwardAsync(userId);

            // =========================
            // BUILD RESPONSE
            // =========================
            var sb = new StringBuilder();
            sb.AppendLine($"🧪 You brewed **{quantity}x {potion.Name}**. (+{xpGained} Alchemy XP)");
            sb.AppendLine($"📦 Added to your inventory.");

            if (levelUps > 0)
                sb.AppendLine($"\n⬆️ **Alchemist Level Up!** You are now Alchemist level **{player.AlchemistLevel}**!");

            if (player.AlchemistLevel < 99)
            {
                int xpToNext = GetTotalXpForLevel(player.AlchemistLevel + 1) - player.AlchemistXP;
                sb.AppendLine($"✨ Alchemy: Level **{player.AlchemistLevel}** | **{xpToNext}** XP to next level");
            }
            else
            {
                sb.AppendLine($"✨ Alchemy: Level **99** — MAX");
            }

            return sb.ToString().Trim();
        }

        // =========================
        // DRINK
        // =========================
        public async Task<string> DrinkAsync(ulong userId, string potionId)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null)
                return "❌ You need to start your adventure first.";

            if (!AlchemyPotionRegistry.All.TryGetValue(potionId, out var potion))
                return "❌ Unknown potion.";

            // =========================
            // CHECK DAILY CAP
            // =========================
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            if (player.LastPotionDrankDate != today)
            {
                player.PotionsDrankToday = 0;
                player.LastPotionDrankDate = today;
            }

            if (player.PotionsDrankToday >= DailyPotionCap)
                return $"❌ You have already used **{DailyPotionCap} potions** today. Come back tomorrow.";

            // =========================
            // CHECK INVENTORY
            // =========================
            int owned = await _inventoryService.GetItemAmountAsync(userId, potionId);
            if (owned <= 0)
                return $"❌ You don't have any **{potion.Name}** in your inventory.";

            // =========================
            // CHECK BUFF SLOT CONFLICTS
            // =========================
            if (potion.PotionType == "stat")
            {
                if (player.ActiveStatBuffId != null &&
                    player.ActiveStatBuffExpiry.HasValue &&
                    player.ActiveStatBuffExpiry.Value > DateTime.UtcNow)
                {
                    if (InventoryItemDefinitions.All.TryGetValue(player.ActiveStatBuffId, out var activeDef))
                        return $"❌ You already have **{activeDef.Name}** active in your stat slot.\nWait for it to expire or it will be overwritten if you confirm.";
                }
            }
            else if (potion.PotionType == "utility")
            {
                if (player.ActiveUtilityBuffId != null &&
                    player.ActiveUtilityBuffExpiry.HasValue &&
                    player.ActiveUtilityBuffExpiry.Value > DateTime.UtcNow)
                {
                    if (InventoryItemDefinitions.All.TryGetValue(player.ActiveUtilityBuffId, out var activeDef))
                        return $"❌ You already have **{activeDef.Name}** active in your utility slot.\nWait for it to expire or it will be overwritten if you confirm.";
                }
            }

            // =========================
            // CONSUME POTION
            // =========================
            await _inventoryService.TakeItemAsync(userId, potionId, 1);

            player.PotionsDrankToday++;

            // =========================
            // APPLY EFFECT
            // =========================
            var result = await ApplyPotionEffectAsync(player, potion);

            await _playerRepository.UpdatePlayerAsync(player);

            var sb = new StringBuilder();
            sb.AppendLine($"🧪 You drank **{potion.Name}**!");
            sb.AppendLine(result);
            sb.AppendLine($"\n📊 Potions used today: **{player.PotionsDrankToday}/{DailyPotionCap}**");

            return sb.ToString().Trim();
        }

        // =========================
        // APPLY POTION EFFECT
        // =========================
        private async Task<string> ApplyPotionEffectAsync(Player player, AlchemyPotionDefinition potion)
        {
            switch (potion.PotionType)
            {
                case "instant":
                    return potion.EffectId switch
                    {
                        "restore_stamina" => ApplyRestoreStamina(player, (int)potion.EffectValue),
                        "restore_stamina_full" => ApplyRestoreStaminaFull(player),
                        "trail_reset" => ApplyTrailReset(player),
                        "revival" => ApplyRevival(player),
                        _ => "✅ Effect applied."
                    };

                case "stat":
                    player.ActiveStatBuffId = potion.Id;
                    player.ActiveStatBuffExpiry = DateTime.UtcNow.AddMinutes(potion.DurationMinutes);
                    return $"✅ **{potion.Description}**\n" +
                           $"⏳ Expires at **{player.ActiveStatBuffExpiry.Value:dd MMM HH:mm} UTC**";

                case "utility":
                    if (potion.EffectId == "npc_max_demand")
                    {
                        player.BlacksmithElixirActiveDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                        return "✅ **Blacksmith's Elixir active!**\nNPCs will buy the maximum amount from your shop at tomorrow's 12 UTC reset.";
                    }

                    if (potion.DurationMinutes == 0)
                    {
                        player.ActiveUtilityBuffId = potion.Id;
                        player.ActiveUtilityBuffExpiry = DateTime.UtcNow.AddHours(24);
                        return $"✅ **{potion.Description}**\nActive for your next dungeon run.";
                    }

                    player.ActiveUtilityBuffId = potion.Id;
                    player.ActiveUtilityBuffExpiry = DateTime.UtcNow.AddMinutes(potion.DurationMinutes);
                    return $"✅ **{potion.Description}**\n" +
                           $"⏳ Expires at **{player.ActiveUtilityBuffExpiry.Value:dd MMM HH:mm} UTC**";

                default:
                    return "✅ Effect applied.";
            }
        }

        // =========================
        // INSTANT EFFECT HELPERS
        // =========================
        private string ApplyRestoreStamina(Player player, int amount)
        {
            int max = player.StaminaBoostExpiry.HasValue &&
                      player.StaminaBoostExpiry.Value > DateTime.UtcNow ? 150 : 100;

            int before = player.HunterStamina;
            player.HunterStamina = Math.Min(max, player.HunterStamina + amount);
            int restored = player.HunterStamina - before;

            return $"✅ Restored **{restored} Stamina**. ({player.HunterStamina}/{max})";
        }

        private string ApplyRestoreStaminaFull(Player player)
        {
            int max = player.StaminaBoostExpiry.HasValue &&
                      player.StaminaBoostExpiry.Value > DateTime.UtcNow ? 150 : 100;

            player.HunterStamina = max;
            return $"✅ Stamina fully restored! ({max}/{max})";
        }

        private string ApplyTrailReset(Player player)
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            if (player.TrailResetUsedDate == today)
                return "❌ You've already used a Trail Tonic today. Come back tomorrow.";

            player.TrailsToday = Math.Max(0, player.TrailsToday - 1);
            player.LastTrailDate = today;
            player.TrailResetUsedDate = today;

            return "✅ **+1 Trail run granted!** You have an extra trail available today.";
        }

        private string ApplyRevival(Player player)
        {
            player.ActiveUtilityBuffId = "revival_draught";
            player.ActiveUtilityBuffExpiry = DateTime.UtcNow.AddHours(24);
            return "✅ **Revival Draught active!** You will survive one killing blow in your next dungeon run.";
        }

        // =========================
        // LEVEL UP PROCESSING
        // =========================
        private int ProcessLevelUps(Player player)
        {
            int levelUps = 0;

            while (player.AlchemistLevel < 99)
            {
                int xpNeeded = GetTotalXpForLevel(player.AlchemistLevel + 1);
                if (player.AlchemistXP >= xpNeeded)
                {
                    player.AlchemistLevel++;
                    levelUps++;
                }
                else break;
            }

            return levelUps;
        }

        // =========================
        // XP CURVE — N² × 50
        // =========================
        public static int GetTotalXpForLevel(int level)
        {
            return level * level * 50;
        }
    }
}

using Hogs.RPG.Core.Entities.EnhancementObjects;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.GameData.Enhancement;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;

namespace Hogs.RPG.Services.EnhancementServices
{
    // =========================
    // 🔨 ENHANCEMENT SERVICE
    // Runs enhance attempts and the Upgrade Piece + Infuse Crystal ->
    // Concentrated Blackstone craft.
    //
    // This service never touches Attack/Defense/Health — it only ever
    // changes the level int on Player. StatService (Phase 5) derives the
    // actual stat bonus live from that level, so there's nothing here to
    // keep in sync.
    //
    // NOT thread-locked per user internally — concurrent-attempt
    // protection (stopping someone from double-clicking /enhance and
    // double-spending) is handled by a precondition at the command layer
    // in Phase 6, same pattern as GearSwapLock/TradeLock/BossLock. This
    // service assumes calls for a given user are serialized.
    // =========================
    public class EnhancementService
    {
        private static readonly Random _random = new();

        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;

        private const string BlackstoneId = "blackstone";
        private const string CronStoneId = "cron_stone";
        private const string InfuseCrystalId = "infuse_crystal";

        public EnhancementService(PlayerRepository playerRepository, InventoryService inventoryService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
        }

        // =========================
        // PREVIEW (no materials spent)
        // =========================
        public async Task<EnhancePreview> GetAttemptPreviewAsync(ulong discordId, EquipmentSlot slot, int cronStonesRequested)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(discordId);
            int currentLevel = player != null ? EnhancementSlotMap.GetEnhancementLevel(player, slot) : 0;
            int targetLevel = currentLevel + 1;
            bool isMaxLevel = currentLevel >= EnhancementRates.MaxLevel;

            int blackstoneCost = isMaxLevel ? 0 : EnhancementCosts.GetBlackstoneCost(targetLevel);
            int blackstonesOwned = await _inventoryService.GetItemAmountAsync(discordId, BlackstoneId);

            bool requiresConcentrated = !isMaxLevel && EnhancementCosts.RequiresConcentratedBlackstone(targetLevel);
            int concentratedOwned = requiresConcentrated
                ? await _inventoryService.GetItemAmountAsync(discordId, EnhancementSlotMap.GetConcentratedBlackstoneItemId(slot))
                : 0;

            int cronStonesOwned = await _inventoryService.GetItemAmountAsync(discordId, CronStoneId);
            int cronStonesToUse = Math.Max(0, cronStonesRequested);
            cronStonesToUse = Math.Min(cronStonesToUse, cronStonesOwned);
            cronStonesToUse = Math.Min(cronStonesToUse, EnhancementCosts.MaxUsefulCronStones);

            double baseRate = isMaxLevel ? 0.0 : EnhancementRates.GetBaseSuccessPercent(targetLevel);
            double bonusRate = EnhancementCosts.GetCronStoneBonusPercent(cronStonesToUse);
            double effectiveRate = Math.Min(100.0, baseRate + bonusRate);

            string? blockedReason = null;
            bool canAttempt = true;

            if (player == null)
            {
                canAttempt = false;
                blockedReason = "Player not found.";
            }
            else if (isMaxLevel)
            {
                canAttempt = false;
                blockedReason = "This slot is already at PEN — there's nowhere further to go.";
            }
            else if (blackstonesOwned < blackstoneCost)
            {
                canAttempt = false;
                blockedReason = $"Not enough Blackstones — need {blackstoneCost}, you have {blackstonesOwned}.";
            }
            else if (requiresConcentrated && concentratedOwned < 1)
            {
                canAttempt = false;
                blockedReason = "You need a Concentrated Blackstone for this slot to attempt PRI. Craft one with /enhance craft.";
            }

            return new EnhancePreview
            {
                Slot = slot,
                CurrentLevel = currentLevel,
                TargetLevel = targetLevel,
                IsMaxLevel = isMaxLevel,
                BlackstoneCost = blackstoneCost,
                BlackstonesOwned = blackstonesOwned,
                RequiresConcentratedBlackstone = requiresConcentrated,
                HasConcentratedBlackstone = concentratedOwned >= 1,
                CronStonesToUse = cronStonesToUse,
                CronStonesOwned = cronStonesOwned,
                BaseSuccessPercent = baseRate,
                BonusSuccessPercent = bonusRate,
                EffectiveSuccessPercent = effectiveRate,
                CanAttempt = canAttempt,
                BlockedReason = blockedReason
            };
        }

        // =========================
        // ATTEMPT (spends materials, rolls, applies or refunds)
        // =========================
        public async Task<EnhanceAttemptResult> AttemptEnhanceAsync(ulong discordId, EquipmentSlot slot, int cronStonesRequested)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(discordId);
            if (player == null)
            {
                return new EnhanceAttemptResult { Success = false, FailureReason = "Player not found." };
            }

            int currentLevel = EnhancementSlotMap.GetEnhancementLevel(player, slot);

            if (currentLevel >= EnhancementRates.MaxLevel)
            {
                return new EnhanceAttemptResult
                {
                    Success = false,
                    FailureReason = "This slot is already at PEN.",
                    PreviousLevel = currentLevel,
                    NewLevel = currentLevel
                };
            }

            int targetLevel = currentLevel + 1;
            int blackstoneCost = EnhancementCosts.GetBlackstoneCost(targetLevel);

            int blackstonesOwned = await _inventoryService.GetItemAmountAsync(discordId, BlackstoneId);
            if (blackstonesOwned < blackstoneCost)
            {
                return new EnhanceAttemptResult
                {
                    Success = false,
                    FailureReason = $"Not enough Blackstones — need {blackstoneCost}, you have {blackstonesOwned}.",
                    PreviousLevel = currentLevel,
                    NewLevel = currentLevel
                };
            }

            bool requiresConcentrated = EnhancementCosts.RequiresConcentratedBlackstone(targetLevel);
            string concentratedId = EnhancementSlotMap.GetConcentratedBlackstoneItemId(slot);

            if (requiresConcentrated)
            {
                int concentratedOwned = await _inventoryService.GetItemAmountAsync(discordId, concentratedId);
                if (concentratedOwned < 1)
                {
                    return new EnhanceAttemptResult
                    {
                        Success = false,
                        FailureReason = "You need a Concentrated Blackstone for this slot. Craft one with /enhance craft.",
                        PreviousLevel = currentLevel,
                        NewLevel = currentLevel
                    };
                }
            }

            // Clamp Cron Stones to what's owned and what's actually useful (25% cap)
            int cronStonesOwned = await _inventoryService.GetItemAmountAsync(discordId, CronStoneId);
            int cronStonesUsed = Math.Max(0, cronStonesRequested);
            cronStonesUsed = Math.Min(cronStonesUsed, cronStonesOwned);
            cronStonesUsed = Math.Min(cronStonesUsed, EnhancementCosts.MaxUsefulCronStones);

            double baseRate = EnhancementRates.GetBaseSuccessPercent(targetLevel);
            double bonusRate = EnhancementCosts.GetCronStoneBonusPercent(cronStonesUsed);
            double effectiveRate = Math.Min(100.0, baseRate + bonusRate);

            // ===== Consume materials — happens regardless of outcome =====
            await _inventoryService.TakeItemAsync(discordId, BlackstoneId, blackstoneCost);

            if (cronStonesUsed > 0)
                await _inventoryService.TakeItemAsync(discordId, CronStoneId, cronStonesUsed);

            if (requiresConcentrated)
                await _inventoryService.TakeItemAsync(discordId, concentratedId, 1);

            // ===== Roll =====
            bool rollSucceeded = _random.NextDouble() * 100.0 < effectiveRate;

            bool upgradePieceRefunded = false;

            if (rollSucceeded)
            {
                EnhancementSlotMap.SetEnhancementLevel(player, slot, targetLevel);
                await _playerRepository.UpdatePlayerAsync(player);
            }
            else if (requiresConcentrated)
            {
                // Fail on a +15 -> PRI attempt: Concentrated Blackstone is gone,
                // but the Upgrade Piece that was consumed to craft it comes back.
                string upgradePieceId = EnhancementSlotMap.GetUpgradePieceItemId(slot);
                await _inventoryService.GiveItemAsync(discordId, upgradePieceId, 1);
                upgradePieceRefunded = true;
            }

            return new EnhanceAttemptResult
            {
                Success = true,
                RollSucceeded = rollSucceeded,
                PreviousLevel = currentLevel,
                NewLevel = rollSucceeded ? targetLevel : currentLevel,
                BlackstonesSpent = blackstoneCost,
                CronStonesSpent = cronStonesUsed,
                ConcentratedBlackstoneConsumed = requiresConcentrated,
                UpgradePieceRefunded = upgradePieceRefunded,
                EffectiveSuccessPercent = effectiveRate
            };
        }

        // =========================
        // CRAFT: Upgrade Piece + Infuse Crystal -> Concentrated Blackstone
        // Deterministic, no RNG — always succeeds if the materials are there.
        // =========================
        public async Task<(bool success, string? failureReason)> CraftConcentratedBlackstoneAsync(ulong discordId, EquipmentSlot slot)
        {
            string upgradePieceId = EnhancementSlotMap.GetUpgradePieceItemId(slot);

            int upgradePiecesOwned = await _inventoryService.GetItemAmountAsync(discordId, upgradePieceId);
            int infuseCrystalsOwned = await _inventoryService.GetItemAmountAsync(discordId, InfuseCrystalId);

            if (upgradePiecesOwned < 1)
                return (false, "You don't have an Upgrade Piece for this slot.");

            if (infuseCrystalsOwned < 1)
                return (false, "You don't have an Infuse Crystal. These drop from the T6 raid.");

            await _inventoryService.TakeItemAsync(discordId, upgradePieceId, 1);
            await _inventoryService.TakeItemAsync(discordId, InfuseCrystalId, 1);

            string concentratedId = EnhancementSlotMap.GetConcentratedBlackstoneItemId(slot);
            await _inventoryService.GiveItemAsync(discordId, concentratedId, 1);

            return (true, null);
        }
    }
}
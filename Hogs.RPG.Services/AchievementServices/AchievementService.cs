using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.AchievementObjects;
using Hogs.RPG.Core.GameData.Achievements;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.GameplayServices;
using System.Text;

namespace Hogs.RPG.Services.AchievementServices
{
    public class AchievementService
    {
        private readonly AchievementRepository _achievementRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly RelicRepository _relicRepository;
        private readonly StatService _statService;
        private readonly GameEventService _gameEventService;

        public AchievementService(
            AchievementRepository achievementRepository,
            PlayerRepository playerRepository,
            RelicRepository relicRepository,
            StatService statService,
            GameEventService gameEventService)
        {
            _achievementRepository = achievementRepository;
            _playerRepository = playerRepository;
            _relicRepository = relicRepository;
            _statService = statService;
            _gameEventService = gameEventService;
        }

        // =========================
        // CHECK AND AWARD
        // Called after any significant action
        // =========================
        public async Task CheckAndAwardAsync(ulong userId)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null) return;

            // Load all already-earned achievement IDs in one query
            var earned = await _achievementRepository.GetAllAsync(userId);
            var earnedIds = earned.Select(a => a.AchievementId).ToHashSet();

            // Build context — gear score + relic ranks
            var ctx = await BuildContextAsync(player);

            var newlyEarned = new List<AchievementDefinition>();

            foreach (var def in AchievementDefinitions.All)
            {
                if (earnedIds.Contains(def.Id)) continue;

                try
                {
                    if (def.Condition(ctx))
                        newlyEarned.Add(def);
                }
                catch
                {
                    // Never let a bad condition crash gameplay
                }
            }

            if (newlyEarned.Count == 0) return;

            // =========================
            // AWARD EACH NEW ACHIEVEMENT
            // =========================
            int goldEarned = 0;

            foreach (var def in newlyEarned)
            {
                await _achievementRepository.AwardAsync(userId, def.Id, isRetroactive: false);
                goldEarned += def.GoldReward;
                earnedIds.Add(def.Id);
            }

            // =========================
            // UPDATE PLAYER COUNT + MILESTONE + GOLD
            // =========================
            player.AchievementCount = earnedIds.Count;
            player.Gold += goldEarned;

            var bonus = AchievementMilestones.GetBonus(player.AchievementCount);
            if (bonus.Title != null)
                player.Title = bonus.Title;

            await _playerRepository.UpdatePlayerAsync(player);

            // =========================
            // POST FEED FOR EACH NEW ACHIEVEMENT
            // =========================
            foreach (var def in newlyEarned)
                await _gameEventService.SendAchievementAsync(player, def, def.GoldReward);

            // =========================
            // POST MILESTONE TITLE UNLOCK IF HIT
            // =========================
            int[] milestones = { 5, 15, 30, 60, 100 };
            foreach (var milestone in milestones)
            {
                int prevCount = player.AchievementCount - newlyEarned.Count;
                if (prevCount < milestone && player.AchievementCount >= milestone)
                {
                    var milestoneBonus = AchievementMilestones.GetBonus(milestone);
                    if (milestoneBonus.Title != null)
                        await _gameEventService.SendMilestoneTitleAsync(player, milestone, milestoneBonus.Title);
                }
            }
        }

        // =========================
        // RETROACTIVE MIGRATION
        // Called once via /admin migrate-achievements
        // Silent — no feed posts, no gold rewards
        // =========================
        public async Task RunRetroactiveMigrationAsync()
        {
            Console.WriteLine("[AchievementService] Starting retroactive migration...");

            var allPlayers = await _playerRepository.GetAllPlayersAsync();
            int totalAwarded = 0;

            foreach (var player in allPlayers)
            {
                try
                {
                    var earned = await _achievementRepository.GetAllAsync(player.DiscordId);
                    var earnedIds = earned.Select(a => a.AchievementId).ToHashSet();

                    var ctx = await BuildContextAsync(player);

                    int newCount = 0;

                    foreach (var def in AchievementDefinitions.All)
                    {
                        if (!def.IsRetroactiveEligible) continue;
                        if (earnedIds.Contains(def.Id)) continue;

                        try
                        {
                            if (def.Condition(ctx))
                            {
                                await _achievementRepository.AwardAsync(player.DiscordId, def.Id, isRetroactive: true);
                                earnedIds.Add(def.Id);
                                newCount++;
                                totalAwarded++;
                            }
                        }
                        catch { }
                    }

                    if (newCount > 0)
                    {
                        // Update count and title silently — no gold, no feed
                        player.AchievementCount = earnedIds.Count;
                        var bonus = AchievementMilestones.GetBonus(player.AchievementCount);
                        if (bonus.Title != null)
                            player.Title = bonus.Title;

                        await _playerRepository.UpdatePlayerAsync(player);

                        Console.WriteLine($"[Retroactive] {player.Username} — awarded {newCount} achievements (total: {player.AchievementCount})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Retroactive] Error for player {player.DiscordId}: {ex.Message}");
                }
            }

            Console.WriteLine($"[AchievementService] Retroactive migration complete. Total awarded: {totalAwarded}");
        }

        // =========================
        // BUILD CONTEXT
        // Computes gear score and relic ranks for condition evaluation
        // =========================
        private async Task<AchievementContext> BuildContextAsync(Core.Entities.PlayerObjects.Player player)
        {
            var (attack, defense, health) = _statService.CalculateStats(player);
            int gearScore = attack + defense + health;

            var relics = await _relicRepository.GetEquippedRelicsAsync(player.DiscordId);
            var slot1 = relics.FirstOrDefault(r => r.SlotIndex == 0);
            var slot2 = relics.FirstOrDefault(r => r.SlotIndex == 1);

            return new AchievementContext
            {
                Player = player,
                GearScore = gearScore,
                Slot1RelicRank = slot1?.Rank ?? 0,
                Slot2RelicRank = slot2?.Rank ?? 0
            };
        }

        // =========================
        // GET MILESTONE BONUS FOR PLAYER
        // Used by StatService, TrailService, EnergyService etc.
        // =========================
        public static AchievementBonus GetBonusForPlayer(int achievementCount)
        {
            return AchievementMilestones.GetBonus(achievementCount);
        }
    }
}
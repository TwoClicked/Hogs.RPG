using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.Achievements;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AchievementServices;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    [Group("achievements", "Achievement commands")]
    [GearSwapLock]
    [BossLock]
    [TradeLock]
    public class AchievementModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AchievementRepository _achievementRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly AchievementService _achievementService;

        public AchievementModule(
            AchievementRepository achievementRepository,
            PlayerRepository playerRepository,
            AchievementService achievementService)
        {
            _achievementRepository = achievementRepository;
            _playerRepository = playerRepository;
            _achievementService = achievementService;
        }

        // =========================
        // /achievements
        // =========================
        [SlashCommand("view", "View your achievements")]
        public async Task View()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("❌ Use /startadventure first.", ephemeral: true);
                return;
            }

            var earned = await _achievementRepository.GetAllAsync(Context.User.Id);
            var earnedIds = earned.Select(a => a.AchievementId).ToHashSet();

            // Group by category
            var categories = AchievementDefinitions.All
                .GroupBy(a => a.Category)
                .OrderBy(g => g.Key);

            var embed = new EmbedBuilder()
                .WithTitle($"🏆 {player.Username}'s Achievements")
                .WithColor(new Color(0xF1C40F))
                .WithFooter($"{player.AchievementCount}/100 achieved · {player.AchievementCount * 250:N0}g earned");

            if (!string.IsNullOrEmpty(player.Title))
                embed.WithDescription($"**Title:** {player.Title}");

            foreach (var group in categories)
            {
                int earnedInCat = group.Count(a => earnedIds.Contains(a.Id));
                int totalInCat = group.Count();

                var sb = new StringBuilder();
                foreach (var def in group.OrderBy(a => a.Id))
                {
                    bool has = earnedIds.Contains(def.Id);
                    var earnedRecord = earned.FirstOrDefault(e => e.AchievementId == def.Id);
                    string badge = has ? "✅" : "⬜";
                    string retroTag = (has && earnedRecord?.IsRetroactive == true) ? " *(retroactive)*" : "";
                    sb.AppendLine($"{badge} **{def.Name}**{retroTag}");
                    if (!has)
                        sb.AppendLine($"  *{def.Description}*");
                }

                embed.AddField(
                    $"{group.First().Icon} {group.Key} — {earnedInCat}/{totalInCat}",
                    sb.ToString().Trim(),
                    inline: false);
            }

            // Milestone progress
            var nextMilestone = AchievementMilestones.GetNextMilestone(player.AchievementCount);
            if (nextMilestone.HasValue)
            {
                var nextBonus = AchievementMilestones.GetBonus(nextMilestone.Value);
                string bonusDesc = nextMilestone.Value switch
                {
                    5 => "+5 ATK, +5 DEF + 🌱 Rookie Adventurer title",
                    15 => "+50 HP",
                    30 => "+1 daily trail + 🏕️ Seasoned Adventurer title",
                    60 => "Energy + Stamina regen 2/min",
                    100 => "Dungeon cooldown 1 hour + 🏆 Master Adventurer title",
                    _ => ""
                };
                embed.AddField(
                    $"⭐ Next Milestone: {nextMilestone.Value} achievements",
                    $"{bonusDesc}\n*{nextMilestone.Value - player.AchievementCount} more needed*",
                    inline: false);
            }

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        // =========================
        // /achievements leaderboard
        // =========================
        [SlashCommand("leaderboard", "View the achievement leaderboard")]
        public async Task Leaderboard()
        {
            await DeferAsync(ephemeral: true);

            var top = await _achievementRepository.GetTopByCountAsync(10);
            var myRank = await _achievementRepository.GetRankAsync(Context.User.Id);
            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);

            var embed = new EmbedBuilder()
                .WithTitle("🏆 Achievement Leaderboard")
                .WithColor(new Color(0xF1C40F));

            var sb = new StringBuilder();
            string[] medals = { "👑", "🥈", "🥉" };

            for (int i = 0; i < top.Count; i++)
            {
                var (discordId, count) = top[i];
                string medal = i < 3 ? medals[i] : $"#{i + 1}";
                var topPlayer = await _playerRepository.GetByDiscordIdAsync(discordId);
                string name = topPlayer?.Username ?? discordId.ToString();
                sb.AppendLine($"{medal} **{name}** — {count} achievements");
            }

            embed.WithDescription(sb.ToString().Trim());

            if (player != null)
            {
                embed.AddField(
                    "📊 Your Rank",
                    $"**#{myRank}** — {player.AchievementCount} achievements",
                    inline: false);
            }

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}
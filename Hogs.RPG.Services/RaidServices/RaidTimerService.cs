using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.RaidServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

namespace Hogs.RPG.Services.RaidServices
{
    public class RaidTimerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;

        private const int RoundTimeoutMinutes = 5;

        public RaidTimerService(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🚀 RaidTimerService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                try
                {
                    await CheckExpiredRoundsAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ RaidTimerService error: {ex.Message}");
                }
            }
        }

        private async Task CheckExpiredRoundsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var raidRepo = scope.ServiceProvider.GetRequiredService<RaidRepository>();
            var raidService = scope.ServiceProvider.GetRequiredService<RaidService>();
            var inventoryRepo = scope.ServiceProvider.GetRequiredService<InventoryRepository>();

            var activeSessions = await raidRepo.GetAllActiveSessionsAsync();

            foreach (var session in activeSessions)
            {
                var elapsed = DateTime.UtcNow - session.RoundStartedAt;
                if (elapsed.TotalMinutes < RoundTimeoutMinutes) continue;

                Console.WriteLine($"⏳ Raid {session.Id} round {session.CurrentRound} timed out — auto-resolving.");

                // Auto-submit default action for players who haven't acted
                foreach (var participant in session.Participants.Where(p => !p.HasActedThisRound))
                {
                    string defaultAction = participant.Role switch
                    {
                        RaidRole.Tank => "hold",
                        RaidRole.Healer => await GetHealerDefaultAsync(inventoryRepo, participant),
                        RaidRole.Dps => "attack",
                        _ => "hold"
                    };

                    participant.HasActedThisRound = true;
                    participant.PendingAction = defaultAction;

                    Console.WriteLine($"⏳ Auto-action for {participant.DiscordId} ({participant.Role}): {defaultAction}");
                }

                await raidRepo.SaveSessionAsync(session);

                // Now resolve the round via RaidService
                var (success, message, roundResult) = await raidService.SubmitActionAsync(
                    session.Participants.First().DiscordId, session.Id, session.Participants.First().PendingAction);

                if (roundResult == null) continue;

                // Post result to thread
                var thread = _client.GetChannel(session.ThreadId) as IThreadChannel;
                if (thread == null) continue;

                if (roundResult.IsWipe)
                {
                    await PostWipeAsync(thread, roundResult);
                    continue;
                }

                if (roundResult.IsVictory)
                {
                    await PostVictoryAsync(thread, roundResult);
                    await PostRaidVictoryFeedAsync(roundResult);  
                    continue;
                }

                await PostTimeoutRoundResultAsync(thread, roundResult, session.Id);
            }
        }

        private async Task<string> GetHealerDefaultAsync(InventoryRepository inventoryRepo, Hogs.RPG.Core.Entities.RaidParticipant participant)
        {
            var inventory = await inventoryRepo.GetInventoryAsync(participant.DiscordId);
            var potion = inventory.FirstOrDefault(i => i.ItemId == "health_potion");
            return potion != null && potion.Quantity > 0 ? "heal" : "empower_attack";
        }

        private async Task PostTimeoutRoundResultAsync(IThreadChannel thread, RaidRoundResult result, int sessionId)
        {
            var session = result.Session;
            var raidDef = RaidRegistry.GetByTier(session.Tier);

            string HpBar(int current, int max, int barLength = 10)
            {
                int filled = max > 0 ? (int)((double)current / max * barLength) : 0;
                return $"[{new string('█', filled)}{new string('░', barLength - filled)}] {current}/{max}";
            }

            var embed = new EmbedBuilder()
                .WithTitle($"⏳ Round {result.Round} — Auto Resolved")
                .WithColor(new Color(0x95A5A6))
                .WithDescription("One or more players did not act in time. Default actions were submitted.")
                .AddField("🛡️ Tank", result.TankText, inline: false)
                .AddField("⚔️ DPS", result.DpsText, inline: false)
                .AddField("💚 Healer", result.HealerText, inline: false)
                .AddField("👹 Boss", result.BossText, inline: false)
                .AddField("━━━━━━━━━━", "**HP Status**", inline: false)
                .AddField("👹 Boss HP", HpBar(session.BossCurrentHp, session.BossMaxHp), inline: false);

            foreach (var p in session.Participants)
            {
                string roleIcon = p.Role switch
                {
                    RaidRole.Tank => "🛡️",
                    RaidRole.Dps => "⚔️",
                    RaidRole.Healer => "💚",
                    _ => "❓"
                };

                bool hasAggro = session.AggroDiscordId == p.DiscordId;
                string aggroTag = hasAggro ? " 🎯" : "";

                embed.AddField(
                    $"{roleIcon} {p.Role}{aggroTag}",
                    HpBar(p.CurrentHp, p.MaxHp, 8),
                    inline: true);
            }

            embed.WithFooter($"Round {session.CurrentRound} — Submit your actions below.");

            await thread.SendMessageAsync(embed: embed.Build());

            foreach (var p in session.Participants)
            {
                var actionComponents = BuildActionButtonsForRole(sessionId, session.CurrentRound, p);
                await thread.SendMessageAsync(
                    $"<@{p.DiscordId}> — Your actions ({p.Role}):",
                    components: actionComponents);
            }
        }

        private async Task PostWipeAsync(IThreadChannel thread, RaidRoundResult result)
        {
            var embed = new EmbedBuilder()
                .WithTitle("💀 Raid Wiped")
                .WithColor(Color.DarkRed)
                .WithDescription(
                    $"{result.WipeReason}\n\n" +
                    $"The party has been defeated.\n" +
                    $"Each player lost **1000 gold** and their raid key.")
                .Build();

            await thread.SendMessageAsync(embed: embed);
            await thread.ModifyAsync(t => t.Archived = true);
        }

        private async Task PostVictoryAsync(IThreadChannel thread, RaidRoundResult result)
        {
            var description = "🏆 **The boss has been defeated!**\n\n";

            foreach (var reward in result.Rewards)
            {
                string roleIcon = reward.Role switch
                {
                    RaidRole.Tank => "🛡️",
                    RaidRole.Dps => "⚔️",
                    RaidRole.Healer => "💚",
                    _ => "❓"
                };

                description += $"{roleIcon} <@{reward.DiscordId}>\n";
                description += $"💰 +{reward.Gold} gold | 📈 +{reward.PlayerXp} XP | 🐾 +{reward.PetXp} Pet XP\n";

                if (reward.ShardDropped)
                    description += $"💎 **Tier {reward.ShardTier} Relic Shard dropped!**\n";

                if (!string.IsNullOrEmpty(reward.LevelUpMessage))
                    description += $"{reward.LevelUpMessage}\n";

                description += "\n";
            }

            var embed = new EmbedBuilder()
                .WithTitle("🏆 Victory!")
                .WithColor(Color.Gold)
                .WithDescription(description)
                .Build();

            await thread.SendMessageAsync(embed: embed);
            await thread.ModifyAsync(t => t.Archived = true);
        }

        // =========================
        // FEED: RAID VICTORY
        // =========================
        private async Task PostRaidVictoryFeedAsync(RaidRoundResult result)
        {
            var feedChannel = _client.GetChannel(1485357755433750549UL) as IMessageChannel;
            if (feedChannel == null) return;

            var session = result.Session;
            var raidDef = RaidRegistry.GetByTier(session?.Tier ?? 0);
            var bossName = raidDef?.Name ?? "Unknown Raid Boss";

            var sb = new StringBuilder();
            sb.AppendLine($"The **{bossName}** has been defeated!\n");

            foreach (var reward in result.Rewards)
            {
                string roleIcon = reward.Role switch
                {
                    RaidRole.Tank => "🛡️",
                    RaidRole.Dps => "⚔️",
                    RaidRole.Healer => "💚",
                    _ => "❓"
                };

                sb.Append($"{roleIcon} <@{reward.DiscordId}> — 💰 +{reward.Gold} gold | 📈 +{reward.PlayerXp} XP");

                if (reward.ShardDropped)
                    sb.Append($"\n  💎 **Tier {reward.ShardTier} Relic Shard dropped!**");

                sb.AppendLine();
            }

            var embed = new EmbedBuilder()
                .WithTitle($"⚔️ Raid Clear — {bossName}")
                .WithColor(Color.Gold)
                .WithDescription(sb.ToString())
                .WithFooter($"Tier {session?.Tier} Raid")
                .Build();

            await feedChannel.SendMessageAsync(embed: embed);
        }


        private MessageComponent BuildActionButtonsForRole(
            int sessionId,
            int round,
            Hogs.RPG.Core.Entities.RaidParticipant participant)
        {
            var builder = new ComponentBuilder();

            switch (participant.Role)
            {
                case RaidRole.Dps:
                    builder.WithButton("⚔️ Attack", $"raid_action:{sessionId}:{round}:attack",
                        ButtonStyle.Danger, row: 0);
                    break;
                case RaidRole.Tank:
                    builder.WithButton("🛡️ Hold", $"raid_action:{sessionId}:{round}:hold",
                        ButtonStyle.Secondary, row: 0);
                    builder.WithButton("📣 Taunt", $"raid_action:{sessionId}:{round}:taunt",
                        ButtonStyle.Secondary, row: 0);
                    builder.WithButton("💥 Shatter", $"raid_action:{sessionId}:{round}:shatter",
                        ButtonStyle.Secondary, row: 0,
                        disabled: participant.ShatterCooldownRoundsRemaining > 0);
                    break;
                case RaidRole.Healer:
                    builder.WithButton("💚 Heal", $"raid_action:{sessionId}:{round}:heal",
                        ButtonStyle.Success, row: 0);
                    builder.WithButton("✨ Empower ATK", $"raid_action:{sessionId}:{round}:empower_attack",
                        ButtonStyle.Success, row: 0);
                    builder.WithButton("✨ Empower DEF", $"raid_action:{sessionId}:{round}:empower_defense",
                        ButtonStyle.Success, row: 0);
                    break;
            }

            return builder.Build();
        }
    }
}
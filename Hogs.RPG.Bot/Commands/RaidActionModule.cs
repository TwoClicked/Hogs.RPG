using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Services.RaidServices;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    public class RaidActionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RaidService _raidService;
        private readonly DiscordSocketClient _client;

        public RaidActionModule(RaidService raidService, DiscordSocketClient client)
        {
            _raidService = raidService;
            _client = client;
        }

        // =========================
        // ACTION BUTTON HANDLER
        // =========================
        [ComponentInteraction("raid_action:*:*:*")]
        public async Task HandleAction(string sessionIdStr, string roundStr, string action)
        {
            await DeferAsync(ephemeral: true);

            int sessionId = int.Parse(sessionIdStr);
            int buttonRound = int.Parse(roundStr);

            var currentSession = await _raidService.GetSessionAsync(sessionId);
            if (currentSession == null)
            {
                await FollowupAsync("❌ Raid not found.", ephemeral: true);
                return;
            }

            if (buttonRound != currentSession.CurrentRound)
            {
                await FollowupAsync("⏩ That action was for a previous round. Use the buttons for the current round.", ephemeral: true);
                return;
            }

            var (success, message, roundResult) = await _raidService.SubmitActionAsync(
                Context.User.Id, sessionId, action);

            if (!success)
            {
                await FollowupAsync(message, ephemeral: true);
                return;
            }

            if (roundResult == null)
            {
                // =========================
                // WAITING FOR OTHER PLAYERS
                // Post a public status board so everyone can see who has acted —
                // prevents the double-press lobby lock bug.
                // =========================
                var thread = Context.Channel as IThreadChannel;
                if (thread != null)
                {
                    var session = await _raidService.GetActiveByThreadAsync(thread.Id);
                    if (session != null)
                    {
                        var statusLines = session.Participants.Select(p =>
                        {
                            string roleIcon = p.Role switch
                            {
                                RaidRole.Tank => "🛡️",
                                RaidRole.Dps => "⚔️",
                                RaidRole.Healer => "💚",
                                _ => "❓"
                            };

                            string actionLabel = p.PendingAction switch
                            {
                                "attack" => "⚔️ Attack",
                                "reckless" => "💀 Reckless",
                                "focus" => "🎯 Focus",
                                "hold" => "🛡️ Hold",
                                "taunt" => "📣 Taunt",
                                "shatter" => "💥 Shatter",
                                "heal" => "💚 Heal",
                                "party_heal" => "🌿 Party Heal",
                                "emergency_heal" => "⚡ Emergency Heal",
                                "empower_attack" => "✨ Empower ATK",
                                "empower_defense" => "✨ Empower DEF",
                                _ => "..."
                            };

                            return p.HasActedThisRound
                                ? $"{roleIcon} **{p.Role}** — {actionLabel} ✅"
                                : $"{roleIcon} **{p.Role}** — ⏳ Waiting...";
                        });

                        // Public — all players in the thread can see who has acted
                        await thread.SendMessageAsync(
                            $"🎯 **<@{Context.User.Id}> chose an action!**\n\n" +
                            string.Join("\n", statusLines));
                    }
                }

                await FollowupAsync("✅ Action locked in!", ephemeral: true);
                return;
            }

            // Round resolved — post results publicly
            await PostRoundResultAsync(roundResult, sessionId);
            await FollowupAsync("✅ Round resolved!", ephemeral: true);
        }

        // =========================
        // POST ROUND RESULT
        // =========================
        private async Task PostRoundResultAsync(RaidRoundResult result, int sessionId)
        {
            var thread = Context.Channel as IThreadChannel;
            if (thread == null) return;

            string HpBar(int current, int max, int barLength = 10)
            {
                int filled = max > 0 ? (int)((double)current / max * barLength) : 0;
                return $"[{new string('█', filled)}{new string('░', barLength - filled)}] {current}/{max}";
            }

            // =========================
            // VICTORY
            // =========================
            if (result.IsVictory)
            {
                var sb = new StringBuilder();
                sb.AppendLine("🎉 **Raid Complete!**\n");

                foreach (var reward in result.Rewards)
                {
                    string roleIcon = reward.Role switch
                    {
                        RaidRole.Tank => "🛡️",
                        RaidRole.Dps => "⚔️",
                        RaidRole.Healer => "💚",
                        _ => "❓"
                    };

                    sb.AppendLine($"{roleIcon} <@{reward.DiscordId}>");
                    sb.AppendLine($"  💰 +{reward.Gold} Gold | ⭐ +{reward.PlayerXp} XP | 🐾 +{reward.PetXp} Pet XP");
                    if (reward.ShardDropped)
                        sb.AppendLine($"  💎 Relic Shard (Tier {reward.ShardTier}) dropped!");
                    if (!string.IsNullOrEmpty(reward.LevelUpMessage))
                        sb.AppendLine($"  🎊 {reward.LevelUpMessage}");
                    sb.AppendLine();
                }

                var raidDef = RaidRegistry.GetByTier(result.Session?.Tier ?? 1);
                string bossName = raidDef?.Name ?? "Boss";

                var victoryEmbed = new EmbedBuilder()
                    .WithTitle($"🏆 {bossName} Defeated!")
                    .WithDescription(sb.ToString().Trim())
                    .WithColor(Color.Gold)
                    .Build();

                await thread.SendMessageAsync(embed: victoryEmbed);

                await PostRaidVictoryFeedAsync(result);
                return;
            }

            // =========================
            // WIPE
            // =========================
            if (result.IsWipe)
            {
                var wipeEmbed = new EmbedBuilder()
                    .WithTitle("💀 Raid Wipe")
                    .WithDescription(
                        string.IsNullOrEmpty(result.WipeReason)
                            ? "Your party was defeated. Better luck next time!"
                            : result.WipeReason)
                    .WithColor(Color.DarkRed)
                    .Build();

                await thread.SendMessageAsync(embed: wipeEmbed);
                return;
            }

            // =========================
            // ROUND RESULT
            // =========================
            var session = result.Session;
            var roundEmbed = new EmbedBuilder()
                .WithTitle($"⚔️ Round {result.Round} Results")
                .WithColor(new Color(0xE74C3C))
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

                roundEmbed.AddField(
                    $"{roleIcon} {p.Role}{aggroTag}",
                    HpBar(p.CurrentHp, p.MaxHp, 8),
                    inline: true);
            }

            roundEmbed.WithFooter($"Round {session.CurrentRound} — Submit your actions below.");

            await thread.SendMessageAsync(embed: roundEmbed.Build());

            // Post fresh action buttons per player for the new round
            var freshSession = await _raidService.GetSessionAsync(sessionId);
            if (freshSession == null) return;

            foreach (var participant in freshSession.Participants)
            {
                var actionComponents = BuildActionButtonsForRole(sessionId, freshSession.CurrentRound, participant);
                await thread.SendMessageAsync(
                    $"<@{participant.DiscordId}> — Round {freshSession.CurrentRound} actions ({participant.Role}):",
                    components: actionComponents);
            }
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
                .WithTitle("⚔️ Raid Victory!")
                .WithDescription(sb.ToString())
                .WithColor(Color.Gold)
                .Build();

            await feedChannel.SendMessageAsync(embed: embed);
        }

        // =========================
        // BUILD ACTION BUTTONS PER ROLE
        // =========================
        private MessageComponent BuildActionButtonsForRole(
            int sessionId, int round,
            Hogs.RPG.Core.Entities.RaidParticipant participant)
        {
            var builder = new ComponentBuilder();
            string prefix = $"raid_action:{sessionId}:{round}";

            switch (participant.Role)
            {
                case RaidRole.Dps:
                    builder
                        .WithButton("⚔️ Attack", $"{prefix}:attack", ButtonStyle.Danger)
                        .WithButton("💀 Reckless", $"{prefix}:reckless", ButtonStyle.Danger,
                            disabled: participant.RecklessCooldownRoundsRemaining > 0)
                        .WithButton("🎯 Focus", $"{prefix}:focus", ButtonStyle.Danger,
                            disabled: participant.FocusStacks >= 2);
                    break;

                case RaidRole.Tank:
                    builder
                        .WithButton("🛡️ Hold", $"{prefix}:hold", ButtonStyle.Secondary)
                        .WithButton("📣 Taunt", $"{prefix}:taunt", ButtonStyle.Secondary)
                        .WithButton("💥 Shatter", $"{prefix}:shatter", ButtonStyle.Secondary,
                            disabled: participant.ShatterCooldownRoundsRemaining > 0);
                    break;

                case RaidRole.Healer:
                    builder
                        .WithButton("💚 Heal", $"{prefix}:heal", ButtonStyle.Success)
                        .WithButton("🌿 Party Heal", $"{prefix}:party_heal", ButtonStyle.Success)
                        .WithButton("⚡ Emergency Heal", $"{prefix}:emergency_heal", ButtonStyle.Success,
                            disabled: participant.EmergencyHealCooldownRoundsRemaining > 0)
                        .WithButton("✨ Empower ATK", $"{prefix}:empower_attack", ButtonStyle.Success, row: 1)
                        .WithButton("✨ Empower DEF", $"{prefix}:empower_defense", ButtonStyle.Success, row: 1);
                    break;
            }

            return builder.Build();
        }
    }
}

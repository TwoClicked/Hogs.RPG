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
            // Keep ephemeral so error messages are private —
            // the public status is posted via thread.SendMessageAsync below.
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
                // Post a PUBLIC status board to the thread so everyone can
                // see who has acted — prevents the double-press lobby lock bug.
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

                        // PUBLIC post — all players in the thread can see who has acted
                        await thread.SendMessageAsync(
                            $"🎯 **<@{Context.User.Id}> chose an action!**\n\n" +
                            string.Join("\n", statusLines));
                    }
                }

                // Private ack — satisfies the interaction acknowledgement
                await FollowupAsync("✅ Action locked in!", ephemeral: true);
                return;
            }

            // Round resolved — round result is already posted publicly by PostRoundResultAsync
            await PostRoundResultAsync(roundResult, sessionId);
            await FollowupAsync("✅ Round resolved!", ephemeral: true);
        }

        // =========================
        // POST ROUND RESULT
        // (unchanged — already posts publicly to thread)
        // =========================
        private async Task PostRoundResultAsync(
            RaidRoundResult result, int sessionId)
        {
            var thread = Context.Channel as IThreadChannel;
            if (thread == null) return;

            string HpBar(int current, int max, int barLength = 10)
            {
                int filled = max > 0 ? (int)((double)current / max * barLength) : 0;
                return $"[{new string('█', filled)}{new string('░', barLength - filled)}] {current}/{max}";
            }

            if (result.IsVictory)
            {
                var sb = new StringBuilder();
                sb.AppendLine("🎉 **Raid Complete!**\n");

                foreach (var reward in result.Rewards)
                {
                    sb.AppendLine($"<@{reward.DiscordId}> ({reward.Role})");
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
                return;
            }

            if (result.IsWipe)
            {
                var wipeEmbed = new EmbedBuilder()
                    .WithTitle("💀 Raid Wipe")
                    .WithDescription("Your party was defeated. Better luck next time!")
                    .WithColor(Color.DarkRed)
                    .Build();

                await thread.SendMessageAsync(embed: wipeEmbed);
                return;
            }

            // =========================
            // ONGOING ROUND — post result embed + new action buttons
            // =========================
            var session = result.Session;
            if (session == null) return;

            var roundRaidDef = RaidRegistry.GetByTier(session.Tier);

            var roundSb = new StringBuilder();

            foreach (var line in result.EventLog)
                roundSb.AppendLine(line);

            roundSb.AppendLine();
            roundSb.AppendLine($"👹 **{roundRaidDef?.Name ?? "Boss"}** — {HpBar(result.BossHpAfter, result.BossMaxHp)}");

            foreach (var p in session.Participants)
                roundSb.AppendLine($"  {(p.Role == RaidRole.Tank ? "🛡️" : p.Role == RaidRole.Dps ? "⚔️" : "💚")} **{p.Role}** — {HpBar(p.CurrentHp, p.MaxHp)}");

            var roundEmbed = new EmbedBuilder()
                .WithTitle($"⚔️ Round {session.CurrentRound - 1} Result")
                .WithDescription(roundSb.ToString().Trim())
                .WithColor(new Color(0xE67E22))
                .Build();

            await thread.SendMessageAsync(embed: roundEmbed);

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
                        .WithButton("💀 Reckless", $"{prefix}:reckless", ButtonStyle.Danger)
                        .WithButton("🎯 Focus", $"{prefix}:focus", ButtonStyle.Primary);
                    break;

                case RaidRole.Tank:
                    builder
                        .WithButton("🛡️ Hold", $"{prefix}:hold", ButtonStyle.Primary)
                        .WithButton("📣 Taunt", $"{prefix}:taunt", ButtonStyle.Primary)
                        .WithButton("💥 Shatter", $"{prefix}:shatter", ButtonStyle.Danger);
                    break;

                case RaidRole.Healer:
                    builder
                        .WithButton("💚 Heal", $"{prefix}:heal", ButtonStyle.Success)
                        .WithButton("🌿 Party Heal", $"{prefix}:party_heal", ButtonStyle.Success)
                        .WithButton("⚡ Emergency Heal", $"{prefix}:emergency_heal", ButtonStyle.Danger)
                        .WithButton("✨ Empower ATK", $"{prefix}:empower_attack", ButtonStyle.Primary, row: 1)
                        .WithButton("✨ Empower DEF", $"{prefix}:empower_defense", ButtonStyle.Primary, row: 1);
                    break;
            }

            return builder.Build();
        }
    }
}

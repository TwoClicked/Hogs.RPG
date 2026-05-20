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
                // Already acted — silently acknowledge
                await FollowupAsync("✅ Action already registered!", ephemeral: true);
                return;
            }

            if (roundResult == null)
            {
                var statusThread = Context.Channel as IThreadChannel;
                if (statusThread != null)
                {
                    var session = await _raidService.GetActiveByThreadAsync(statusThread.Id);
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
                                "emergency_menu" => "⚡ Emergency...",
                                "emergency_heal_tank" => "⚡ Emergency (Tank)",
                                "emergency_heal_dps" => "⚡ Emergency (DPS)",
                                "emergency_heal_healer" => "⚡ Emergency (Self)",
                                "empower_attack" => "✨ Empower ATK",
                                "empower_defense" => "✨ Empower DEF",
                                _ => "..."
                            };

                            string status = p.HasActedThisRound
                                ? $"✅ {actionLabel}"
                                : "⏳ Waiting...";

                            return $"{roleIcon} <@{p.DiscordId}> — {status}";
                        });

                        string statusText = $"**Round {session.CurrentRound} Actions:**\n{string.Join("\n", statusLines)}";

                        if (session.RoundStatusMessageId != 0)
                        {
                            try
                            {
                                var existing = await statusThread.GetMessageAsync(session.RoundStatusMessageId) as IUserMessage;
                                if (existing != null)
                                    await existing.ModifyAsync(m => m.Content = statusText);
                                else
                                {
                                    var newMsg = await statusThread.SendMessageAsync(statusText);
                                    await _raidService.UpdateRoundStatusMessageIdAsync(session.Id, newMsg.Id);
                                }
                            }
                            catch
                            {
                                var newMsg = await statusThread.SendMessageAsync(statusText);
                                await _raidService.UpdateRoundStatusMessageIdAsync(session.Id, newMsg.Id);
                            }
                        }
                        else
                        {
                            var newMsg = await statusThread.SendMessageAsync(statusText);
                            await _raidService.UpdateRoundStatusMessageIdAsync(session.Id, newMsg.Id);
                        }
                    }
                }

                await FollowupAsync("✅ Action submitted!", ephemeral: true);
                return;
            }

            var thread = Context.Channel as IThreadChannel;
            if (thread == null)
            {
                await FollowupAsync("❌ Could not find raid thread.", ephemeral: true);
                return;
            }

            if (roundResult.IsWipe)
            {
                await PostWipeResultAsync(thread, roundResult);
                await FollowupAsync("💀 The raid ended in a wipe.", ephemeral: true);
                return;
            }

            if (roundResult.IsVictory)
            {
                await PostVictoryResultAsync(thread, roundResult);
                await PostRaidVictoryFeedAsync(roundResult);
                await FollowupAsync("🏆 Victory!", ephemeral: true);
                return;
            }

            await PostRoundResultAsync(thread, roundResult, sessionId);
            await FollowupAsync("✅ Round resolved!", ephemeral: true);
        }

        // =========================
        // EMERGENCY HEAL TARGET MENU
        // =========================
        [ComponentInteraction("raid_action:*:*:emergency_menu")]
        public async Task EmergencyHealMenu(string sessionIdStr, string roundStr)
        {
            await DeferAsync(ephemeral: true);

            int sessionId = int.Parse(sessionIdStr);
            int round = int.Parse(roundStr);

            // Check they haven't already acted this round
            var session = await _raidService.GetSessionAsync(sessionId);
            if (session != null)
            {
                var participant = session.Participants.FirstOrDefault(p => p.DiscordId == Context.User.Id);
                if (participant != null && participant.HasActedThisRound)
                {
                    await FollowupAsync("✅ Action already registered!", ephemeral: true);
                    return;
                }
            }

            var components = new ComponentBuilder()
                .WithButton("🛡️ Heal Tank", $"raid_action:{sessionId}:{round}:emergency_heal_tank", ButtonStyle.Success)
                .WithButton("⚔️ Heal DPS", $"raid_action:{sessionId}:{round}:emergency_heal_dps", ButtonStyle.Success)
                .WithButton("💚 Heal Myself", $"raid_action:{sessionId}:{round}:emergency_heal_healer", ButtonStyle.Success)
                .Build();

            await FollowupAsync("⚡ **Emergency Heal** — Choose your target (costs 3 potions, 10 round cooldown):", components: components, ephemeral: true);
        }

        // =========================
        // POST ROUND RESULT
        // =========================
        private async Task PostRoundResultAsync(
            IThreadChannel thread,
            RaidRoundResult result,
            int sessionId)
        {
            var session = result.Session;
            var raidDef = RaidRegistry.GetByTier(session.Tier);

            string HpBar(int current, int max, int barLength = 10)
            {
                int filled = max > 0 ? (int)((double)current / max * barLength) : 0;
                return $"[{new string('█', filled)}{new string('░', barLength - filled)}] {current}/{max}";
            }

            var embed = new EmbedBuilder()
                .WithTitle($"⚔️ Round {result.Round} Complete")
                .WithColor(new Color(0xE67E22))
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

        // =========================
        // POST WIPE RESULT
        // =========================
        private async Task PostWipeResultAsync(IThreadChannel thread, RaidRoundResult result)
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

        // =========================
        // POST VICTORY RESULT
        // =========================
        private async Task PostVictoryResultAsync(IThreadChannel thread, RaidRoundResult result)
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

        // =========================
        // ACTION BUTTONS BUILDER — PER ROLE
        // =========================
        private MessageComponent BuildActionButtonsForRole(int sessionId, int round, Hogs.RPG.Core.Entities.RaidParticipant participant)
        {
            var builder = new ComponentBuilder();

            switch (participant.Role)
            {
                case RaidRole.Dps:
                    builder.WithButton("⚔️ Attack", $"raid_action:{sessionId}:{round}:attack",
                        ButtonStyle.Danger, row: 0);
                    builder.WithButton("💀 Reckless", $"raid_action:{sessionId}:{round}:reckless",
                        ButtonStyle.Danger, row: 0,
                        disabled: participant.RecklessCooldownRoundsRemaining > 0);
                    builder.WithButton("🎯 Focus", $"raid_action:{sessionId}:{round}:focus",
                        ButtonStyle.Danger, row: 0,
                        disabled: participant.FocusCooldownRoundsRemaining > 0);
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
                    builder.WithButton("🌿 Party Heal", $"raid_action:{sessionId}:{round}:party_heal",
                        ButtonStyle.Success, row: 0);
                    builder.WithButton("⚡ Emergency", $"raid_action:{sessionId}:{round}:emergency_menu",
                        ButtonStyle.Success, row: 0,
                        disabled: participant.EmergencyHealCooldownRoundsRemaining > 0);
                    builder.WithButton("✨ Empower ATK", $"raid_action:{sessionId}:{round}:empower_attack",
                        ButtonStyle.Success, row: 1);
                    builder.WithButton("✨ Empower DEF", $"raid_action:{sessionId}:{round}:empower_defense",
                        ButtonStyle.Success, row: 1);
                    break;
            }

            return builder.Build();
        }
    }
}
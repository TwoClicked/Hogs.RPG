using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Services.RaidServices;

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
        [ComponentInteraction("raid_action:*:*")]
        public async Task HandleAction(string sessionIdStr, string action)
        {
            await DeferAsync(ephemeral: true);

            int sessionId = int.Parse(sessionIdStr);

            var (success, message, roundResult) = await _raidService.SubmitActionAsync(
                Context.User.Id, sessionId, action);

            if (!success)
            {
                await FollowupAsync(message, ephemeral: true);
                return;
            }

            // If round not yet resolved just acknowledge
            if (roundResult == null)
            {
                await FollowupAsync("✅ Action submitted! Waiting for the other party members.", ephemeral: true);
                return;
            }

            // Round resolved — post result to thread
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
                await FollowupAsync("🏆 Victory!", ephemeral: true);
                return;
            }

            // Post round result and next round buttons
            await PostRoundResultAsync(thread, roundResult, sessionId);
            await FollowupAsync("✅ Round resolved!", ephemeral: true);
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

            // Next round action buttons
            var components = BuildActionButtons(sessionId, session);

            await thread.SendMessageAsync(
                embed: embed.Build(),
                components: components);
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
        // ACTION BUTTONS BUILDER
        // =========================
        private MessageComponent BuildActionButtons(
            int sessionId,
            Hogs.RPG.Core.Entities.RaidSession session)
        {
            var builder = new ComponentBuilder();

            foreach (var p in session.Participants)
            {
                switch (p.Role)
                {
                    case RaidRole.Dps:
                        builder.WithButton("⚔️ Attack", $"raid_action:{sessionId}:attack",
                            ButtonStyle.Danger, row: 0);
                        break;
                    case RaidRole.Tank:
                        builder.WithButton("🛡️ Hold", $"raid_action:{sessionId}:hold",
                            ButtonStyle.Primary, row: 1);
                        builder.WithButton("📣 Taunt", $"raid_action:{sessionId}:taunt",
                            ButtonStyle.Secondary, row: 1);
                        builder.WithButton("💥 Shatter", $"raid_action:{sessionId}:shatter",
                            ButtonStyle.Danger, row: 1,
                            disabled: p.ShatterCooldownRoundsRemaining > 0);
                        break;
                    case RaidRole.Healer:
                        builder.WithButton("💚 Heal", $"raid_action:{sessionId}:heal",
                            ButtonStyle.Success, row: 2);
                        builder.WithButton("✨ Empower ATK", $"raid_action:{sessionId}:empower_attack",
                            ButtonStyle.Secondary, row: 2);
                        builder.WithButton("✨ Empower DEF", $"raid_action:{sessionId}:empower_defense",
                            ButtonStyle.Secondary, row: 2);
                        break;
                }
            }

            return builder.Build();
        }
    }
}
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.RaidServices;

namespace Hogs.RPG.Bot.Commands
{
    [BossLock]
    public class RaidModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RaidService _raidService;
        private readonly PlayerRepository _playerRepository;
        private readonly DiscordSocketClient _client;
        private readonly InventoryRepository _inventoryRepo;

        private const ulong RAID_CHANNEL_ID = 1504160665231691796;

        public RaidModule(
            RaidService raidService,
            PlayerRepository playerRepository,
            DiscordSocketClient client,
            InventoryRepository inventoryRepo)
        {
            _raidService = raidService;
            _playerRepository = playerRepository;
            _client = client;
            _inventoryRepo = inventoryRepo;
        }

        // =========================
        // CHANNEL GUARD
        // =========================
        private async Task<bool> EnsureRaidChannelAsync()
        {
            if (Context.Channel.Id != RAID_CHANNEL_ID)
            {
                await RespondAsync(
                    $"❌ Raid commands can only be used in <#{RAID_CHANNEL_ID}>.",
                    ephemeral: true);
                return false;
            }
            return true;
        }

        private async Task<bool> EnsurePlayerAsync()
        {
            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await RespondAsync("⚠️ You need to start your adventure first with `/startadventure`.", ephemeral: true);
                return false;
            }
            return true;
        }

        // =========================
        // /raidkeys — View your raid keys
        // =========================
        [SlashCommand("raidkeys", "View your current raid keys")]
        public async Task RaidKeys()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("⚠️ You need to start your adventure first with `/startadventure`.", ephemeral: true);
                return;
            }

            var keyIds = new[]
            {
                 ("raid_key_t1", "T1 — Lair Key"),
                 ("raid_key_t2", "T2 — Stronghold Key"),
                 ("raid_key_t3", "T3 — Fortress Key"),
                 ("raid_key_t4", "T4 — Citadel Key"),
                 ("raid_key_t5", "T5 — World Boss Key")
            };

            var embed = new EmbedBuilder()
                .WithTitle("🗝️ Your Raid Keys")
                .WithColor(new Color(0xE67E22));

            foreach (var (itemId, label) in keyIds)
            {
                var item = (await _inventoryRepo.GetInventoryAsync(Context.User.Id))
                    .FirstOrDefault(i => i.ItemId == itemId);

                int qty = item?.Quantity ?? 0;
                embed.AddField(label, $"`{qty}x`", inline: true);
            }

            embed.WithFooter("Craft keys with /craft using hunt materials.");

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        // =========================
        // /raid-start
        // =========================
        [SlashCommand("raid-start", "Open a raid lobby and choose your role")]
        public async Task RaidStart(
            [Summary("tier", "Raid tier (1-5)")] int tier)
        {
            if (!await EnsureRaidChannelAsync()) return;
            await DeferAsync();

            if (!await EnsurePlayerAsync()) return;

            if (tier < 1 || tier > 5)
            {
                await FollowupAsync("❌ Invalid tier. Choose between 1 and 5.", ephemeral: true);
                return;
            }

            // Role selection buttons
            var components = new ComponentBuilder()
                .WithButton("🛡️ Tank", $"raid_role:{tier}:Tank", ButtonStyle.Primary)
                .WithButton("⚔️ DPS", $"raid_role:{tier}:DPS", ButtonStyle.Danger)
                .WithButton("💚 Healer", $"raid_role:{tier}:Healer", ButtonStyle.Success)
                .Build();

            await FollowupAsync(
                $"⚔️ **Tier {tier} Raid** — Choose your role to open the lobby:",
                components: components,
                ephemeral: true);
        }

        // =========================
        // ROLE SELECTION BUTTON
        // =========================
        [ComponentInteraction("raid_role:*:*")]
        public async Task SelectRole(string tierStr, string roleStr)
        {
            await DeferAsync(ephemeral: true);

            int tier = int.Parse(tierStr);
            var role = Enum.Parse<RaidRole>(roleStr, ignoreCase: true);

            var raidChannel = _client.GetChannel(RAID_CHANNEL_ID) as ITextChannel;
            if (raidChannel == null)
            {
                await FollowupAsync("❌ Raid channel not found.", ephemeral: true);
                return;
            }

            var (success, message, session) = await _raidService.CreateLobbyAsync(
                Context.User.Id,
                Context.User.Username,
                tier,
                role,
                RAID_CHANNEL_ID);

            if (!success)
            {
                await FollowupAsync(message, ephemeral: true);
                return;
            }

            var raidDef = RaidRegistry.GetByTier(tier);

            // Build lobby embed
            var embed = BuildLobbyEmbed(session, raidDef.Name, tier);

            // Join and Start buttons
            var components = new ComponentBuilder()
                .WithButton("🛡️ Join as Tank", $"raid_join:{session.Id}:Tank", ButtonStyle.Primary,
                    disabled: session.Participants.Any(p => p.Role == RaidRole.Tank))
                .WithButton("⚔️ Join as DPS", $"raid_join:{session.Id}:DPS", ButtonStyle.Danger,
                    disabled: session.Participants.Any(p => p.Role == RaidRole.Dps))
                .WithButton("💚 Join as Healer", $"raid_join:{session.Id}:Healer", ButtonStyle.Success,
                    disabled: session.Participants.Any(p => p.Role == RaidRole.Healer))
                .WithButton("▶️ Start Raid", $"raid_start:{session.Id}", ButtonStyle.Secondary,
                    disabled: true)
                .Build();

            var lobbyMessage = await raidChannel.SendMessageAsync(
                embed: embed,
                components: components);

            // Store lobby message ID
            session.LobbyMessageId = lobbyMessage.Id;
            await _raidService.UpdateLobbyMessageIdAsync(session.Id, lobbyMessage.Id);

            await FollowupAsync($"✅ Lobby opened! Others can join in <#{RAID_CHANNEL_ID}>.", ephemeral: true);
        }

        // =========================
        // JOIN LOBBY BUTTON
        // =========================
        [ComponentInteraction("raid_join:*:*")]
        public async Task JoinLobby(string sessionIdStr, string roleStr)
        {
            await DeferAsync(ephemeral: true);

            int sessionId = int.Parse(sessionIdStr);
            var role = Enum.Parse<RaidRole>(roleStr, ignoreCase: true);

            var (success, message) = await _raidService.JoinLobbyAsync(
                Context.User.Id,
                Context.User.Username,
                sessionId,
                role);

            if (!success)
            {
                await FollowupAsync(message, ephemeral: true);
                return;
            }

            // Refresh lobby embed
            await RefreshLobbyEmbedAsync(sessionId);

            await FollowupAsync(message, ephemeral: true);
        }

        // =========================
        // START RAID BUTTON
        // =========================
        [ComponentInteraction("raid_start:*")]
        public async Task StartRaid(string sessionIdStr)
        {
            await DeferAsync(ephemeral: true);

            int sessionId = int.Parse(sessionIdStr);

            var session = await _raidService.GetSessionAsync(sessionId);
            if (session == null)
            {
                await FollowupAsync("❌ Lobby not found.", ephemeral: true);
                return;
            }

            if (session.LeaderDiscordId != Context.User.Id)
            {
                await FollowupAsync("❌ Only the raid leader can start the raid.", ephemeral: true);
                return;
            }

            var raidChannel = _client.GetChannel(RAID_CHANNEL_ID) as ITextChannel;
            if (raidChannel == null)
            {
                await FollowupAsync("❌ Raid channel not found.", ephemeral: true);
                return;
            }

            var (success, result) = await _raidService.StartRaidAsync(
                Context.User.Id, sessionId, raidChannel);

            if (!success)
            {
                await FollowupAsync(result, ephemeral: true);
                return;
            }

            // result is thread ID on success
            ulong threadId = ulong.Parse(result);
            var thread = _client.GetChannel(threadId) as IThreadChannel;

            if (thread != null)
            {
                var raidDef = RaidRegistry.GetByTier(session.Tier);
                var freshSession = await _raidService.GetSessionAsync(sessionId);

                // Post public raid status embed in thread
                var embed = BuildRaidEmbed(freshSession, raidDef, "⚔️ The raid has begun! Each player has their action buttons below.");
                await thread.SendMessageAsync(
                    string.Join(" ", freshSession.Participants.Select(p => $"<@{p.DiscordId}>")),
                    embed: embed);

                // Post individual action message per player in thread.
                // Store the message ID so re-selection can edit it in place.
                foreach (var participant in freshSession.Participants)
                {
                    var actionComponents = BuildActionButtonsForRole(sessionId, freshSession.CurrentRound, participant);
                    var msg = await thread.SendMessageAsync(
                        $"<@{participant.DiscordId}> — Your actions ({participant.Role}):",
                        components: actionComponents);

                    await _raidService.UpdateParticipantActionMessageIdAsync(sessionId, participant.DiscordId, msg.Id);
                }
            }

            // Disable lobby buttons
            await DisableLobbyButtonsAsync(sessionId);

            await FollowupAsync($"✅ Raid started! Head to the thread.", ephemeral: true);
        }

        // =========================
        // /raid-leave
        // =========================
        [SlashCommand("raid-leave", "Leave your current raid lobby")]
        public async Task RaidLeave()
        {
            if (!await EnsureRaidChannelAsync()) return;
            await DeferAsync(ephemeral: true);

            var session = await _raidService.GetPlayerActiveSessionAsync(Context.User.Id);
            if (session == null)
            {
                await FollowupAsync("❌ You are not in any raid lobby.", ephemeral: true);
                return;
            }

            if (session.Status != Core.Enums.RaidStatus.Lobby)
            {
                await FollowupAsync("❌ You cannot leave a raid that has already started.", ephemeral: true);
                return;
            }

            // If leader is leaving and others are in — transfer or disband
            if (session.LeaderDiscordId == Context.User.Id && session.Participants.Count > 1)
            {
                await FollowupAsync("❌ You are the raid leader. Use `/raid-disband` to close the lobby.", ephemeral: true);
                return;
            }

            var participant = session.Participants.First(p => p.DiscordId == Context.User.Id);
            await _raidService.RemoveParticipantDirectAsync(participant.Id);
            await RefreshLobbyEmbedAsync(session.Id);

            await FollowupAsync("✅ You left the raid lobby.", ephemeral: true);
        }

        // =========================
        // /raid-disband
        // =========================
        [SlashCommand("raid-disband", "Disband your raid lobby (leader only)")]
        public async Task RaidDisband()
        {
            if (!await EnsureRaidChannelAsync()) return;
            await DeferAsync(ephemeral: true);

            var session = await _raidService.GetPlayerActiveSessionAsync(Context.User.Id);
            if (session == null)
            {
                await FollowupAsync("❌ You are not in any raid lobby.", ephemeral: true);
                return;
            }

            if (session.LeaderDiscordId != Context.User.Id)
            {
                await FollowupAsync("❌ Only the raid leader can disband the lobby.", ephemeral: true);
                return;
            }

            if (session.Status != Core.Enums.RaidStatus.Lobby)
            {
                await FollowupAsync("❌ You cannot disband a raid that has already started.", ephemeral: true);
                return;
            }

            await DisableLobbyButtonsAsync(session.Id);
            await _raidService.DeleteSessionDirectAsync(session.Id);

            await FollowupAsync("✅ Raid lobby disbanded.", ephemeral: true);
        }

        // =========================
        // HELPERS — Embed builders
        // =========================
        private Embed BuildLobbyEmbed(
            Hogs.RPG.Core.Entities.RaidSession session,
            string raidName, int tier)
        {
            var participants = session.Participants;

            string GetSlot(RaidRole role)
            {
                var p = participants.FirstOrDefault(x => x.Role == role);
                return p != null ? $"<@{p.DiscordId}>" : "*Open*";
            }

            return new EmbedBuilder()
                .WithTitle($"⚔️ Raid Lobby — Tier {tier}: {raidName}")
                .WithColor(new Color(0xE67E22))
                .AddField("🛡️ Tank", GetSlot(RaidRole.Tank), inline: true)
                .AddField("⚔️ DPS", GetSlot(RaidRole.Dps), inline: true)
                .AddField("💚 Healer", GetSlot(RaidRole.Healer), inline: true)
                .AddField("Status", $"{participants.Count}/3 players", inline: false)
                .WithFooter("All 3 players need a Raid Key to start.")
                .Build();
        }

        private Embed BuildRaidEmbed(
            Hogs.RPG.Core.Entities.RaidSession session,
            Hogs.RPG.Core.Entities.RaidDefinition raidDef,
            string description)
        {
            string HpBar(int current, int max, int barLength = 10)
            {
                int filled = max > 0 ? (int)((double)current / max * barLength) : 0;
                return $"[{new string('█', filled)}{new string('░', barLength - filled)}] {current}/{max}";
            }

            var embed = new EmbedBuilder()
                .WithTitle($"⚔️ {raidDef.Name} — Round {session.CurrentRound}")
                .WithDescription(description)
                .WithColor(new Color(0xE74C3C))
                .WithThumbnailUrl(raidDef.ImageUrl)
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

            return embed.Build();
        }

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
                    // Emergency Heal — single tap, auto-targets lowest HP member
                    builder.WithButton("⚡ Emergency", $"raid_action:{sessionId}:{round}:emergency_heal",
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

        // =========================
        // HELPERS — Lobby refresh
        // =========================
        private async Task RefreshLobbyEmbedAsync(int sessionId)
        {
            var session = await _raidService.GetSessionAsync(sessionId);
            if (session == null) return;

            var raidDef = RaidRegistry.GetByTier(session.Tier);
            var channel = _client.GetChannel(RAID_CHANNEL_ID) as ITextChannel;
            if (channel == null) return;

            var message = await channel.GetMessageAsync(session.LobbyMessageId) as IUserMessage;
            if (message == null) return;

            bool isFull = session.Participants.Count >= 3;

            var embed = BuildLobbyEmbed(session, raidDef.Name, session.Tier);

            var components = new ComponentBuilder()
                .WithButton("🛡️ Join as Tank", $"raid_join:{session.Id}:Tank", ButtonStyle.Primary,
                    disabled: session.Participants.Any(p => p.Role == RaidRole.Tank))
                .WithButton("⚔️ Join as DPS", $"raid_join:{session.Id}:DPS", ButtonStyle.Danger,
                    disabled: session.Participants.Any(p => p.Role == RaidRole.Dps))
                .WithButton("💚 Join as Healer", $"raid_join:{session.Id}:Healer", ButtonStyle.Success,
                    disabled: session.Participants.Any(p => p.Role == RaidRole.Healer))
                .WithButton("▶️ Start Raid", $"raid_start:{session.Id}", ButtonStyle.Secondary,
                    disabled: !isFull)
                .Build();

            await message.ModifyAsync(m =>
            {
                m.Embed = embed;
                m.Components = components;
            });

            // Ping all members when lobby is full
            if (isFull)
            {
                await channel.SendMessageAsync(
                    $"🔔 Party is full! {string.Join(", ", session.Participants.Select(p => $"<@{p.DiscordId}>"))} — <@{session.LeaderDiscordId}> can now start the raid!");
            }
        }

        private async Task DisableLobbyButtonsAsync(int sessionId)
        {
            var session = await _raidService.GetSessionAsync(sessionId);
            if (session == null) return;

            var channel = _client.GetChannel(RAID_CHANNEL_ID) as ITextChannel;
            if (channel == null) return;

            var message = await channel.GetMessageAsync(session.LobbyMessageId) as IUserMessage;
            if (message == null) return;

            await message.ModifyAsync(m =>
            {
                m.Components = new ComponentBuilder().Build();
            });
        }
    }
}

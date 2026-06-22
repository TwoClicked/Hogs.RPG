using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities.TowerObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Tower;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.TowerServices;

// =========================
// SLASH COMMANDS  (/tower solo, /tower duo, /tower buffs)
// =========================
[Group("tower", "Tower of Doom")]
public class TowerModule : InteractionModuleBase<SocketInteractionContext>
{
    private const ulong TOWER_CHANNEL_ID = 1517665507631956151;

    private readonly TowerService _towerService;
    private readonly SigilRepository _sigilRepo;
    private readonly PlayerRepository _playerRepo;

    public TowerModule(TowerService towerService, SigilRepository sigilRepo, PlayerRepository playerRepo)
    {
        _towerService = towerService;
        _sigilRepo = sigilRepo;
        _playerRepo = playerRepo;
    }

    // =========================
    // /tower solo
    // =========================
    [SlashCommand("solo", "Start a solo Tower of Doom run")]
    public async Task Solo()
    {
        if (Context.Channel.Id != TOWER_CHANNEL_ID)
        {
            await RespondAsync($"❌ Tower runs can only be started in <#{TOWER_CHANNEL_ID}>.", ephemeral: true);
            return;
        }

        await DeferAsync();

        var userId = Context.User.Id;

        if (_towerService.IsInSession(userId))
        {
            await FollowupAsync("❌ You are already in a Tower run.", ephemeral: true);
            return;
        }

        var player = await _playerRepo.GetByDiscordIdAsync(userId);
        if (player == null)
        {
            await FollowupAsync("❌ You haven't started your adventure yet.", ephemeral: true);
            return;
        }

        if (player.LastSoloTowerRun.HasValue && player.LastSoloTowerRun.Value.Date == DateTime.UtcNow.Date)
        {
            await FollowupAsync("❌ You have already done your solo Tower run today. Come back tomorrow.", ephemeral: true);
            return;
        }

        var session = _towerService.CreateLobby(userId, Context.User.GlobalName ?? Context.User.Username, TowerMode.Solo, TOWER_CHANNEL_ID);
        if (session == null)
        {
            await FollowupAsync("❌ Could not create a lobby.", ephemeral: true);
            return;
        }

        var embed = TowerModuleHelpers.BuildLobbyEmbed(session);
        var components = new ComponentBuilder()
            .WithButton("✅ Ready", $"tower_ready:{session.SessionId}", ButtonStyle.Success)
            .WithButton("▶️ Start", $"tower_start:{session.SessionId}", ButtonStyle.Primary, disabled: true)
            .Build();

        var msg = await FollowupAsync(embed: embed, components: components);
        session.LobbyMessageId = msg.Id;
    }

    // =========================
    // /tower duo
    // =========================
    [SlashCommand("duo", "Create a duo Tower of Doom lobby")]
    public async Task Duo()
    {
        if (Context.Channel.Id != TOWER_CHANNEL_ID)
        {
            await RespondAsync($"❌ Tower runs can only be started in <#{TOWER_CHANNEL_ID}>.", ephemeral: true);
            return;
        }

        await DeferAsync();

        var userId = Context.User.Id;

        if (_towerService.IsInSession(userId))
        {
            await FollowupAsync("❌ You are already in a Tower run.", ephemeral: true);
            return;
        }

        var player = await _playerRepo.GetByDiscordIdAsync(userId);
        if (player == null)
        {
            await FollowupAsync("❌ You haven't started your adventure yet.", ephemeral: true);
            return;
        }

        if (player.LastDuoTowerRun.HasValue && player.LastDuoTowerRun.Value.Date == DateTime.UtcNow.Date)
        {
            await FollowupAsync("❌ You have already done your duo Tower run today. Come back tomorrow.", ephemeral: true);
            return;
        }

        var session = _towerService.CreateLobby(userId, Context.User.GlobalName ?? Context.User.Username, TowerMode.Duo, TOWER_CHANNEL_ID);
        if (session == null)
        {
            await FollowupAsync("❌ Could not create a lobby.", ephemeral: true);
            return;
        }

        var embed = TowerModuleHelpers.BuildLobbyEmbed(session);
        var components = new ComponentBuilder()
            .WithButton("🚪 Join",   $"tower_join:{session.SessionId}",  ButtonStyle.Secondary)
            .WithButton("✅ Ready",  $"tower_ready:{session.SessionId}", ButtonStyle.Success)
            .WithButton("▶️ Start", $"tower_start:{session.SessionId}", ButtonStyle.Primary, disabled: true)
            .Build();

        var msg = await FollowupAsync(embed: embed, components: components);
        session.LobbyMessageId = msg.Id;
    }

    // =========================
    // /tower buffs (sigils)
    // =========================
    [SlashCommand("buffs", "View your permanent Tower Sigils")]
    public async Task Buffs()
    {
        await DeferAsync(ephemeral: true);

        var userId = Context.User.Id;
        var sigils = await _sigilRepo.GetSigilsAsync(userId);

        var embed = new EmbedBuilder()
            .WithTitle("✨ Your Tower Sigils")
            .WithColor(Color.Gold)
            .WithDescription("Sigils are permanent bonuses earned by defeating bosses in the Tower of Doom.\nEach sigil stacks up to **3 times**.");

        foreach (var def in SigilRegistry.All)
        {
            var owned = sigils.FirstOrDefault(s => s.SigilId == def.Id);
            int count = owned?.Count ?? 0;

            string stacks = count > 0
                ? string.Concat(Enumerable.Repeat("🔷", count)) + string.Concat(Enumerable.Repeat("⬛", SigilRegistry.MaxStacks - count))
                : string.Concat(Enumerable.Repeat("⬛", SigilRegistry.MaxStacks));

            string title = count > 0 ? $"{def.Emoji} {def.Name}" : $"🔒 {def.Name}";
            string body = count > 0
                ? $"{stacks} {count}/{SigilRegistry.MaxStacks} stacks\n*{def.LoreText}*"
                : $"{stacks} 0/{SigilRegistry.MaxStacks} — *Defeat boss floors in the Tower of Doom*";

            embed.AddField(title, body, false);
        }

        embed.WithFooter("5% drop chance from Tower boss floors (25, 50, 75...)");

        await FollowupAsync(embed: embed.Build(), ephemeral: true);
    }
}

// =========================
// BUTTON HANDLERS (separate class — [Group] modules don't handle ComponentInteractions)
// =========================
public class TowerButtonModule : InteractionModuleBase<SocketInteractionContext>
{
    private const ulong TOWER_CHANNEL_ID = 1517665507631956151;

    private readonly TowerService _towerService;
    private readonly PlayerRepository _playerRepo;
    private readonly DiscordSocketClient _client;

    public TowerButtonModule(TowerService towerService, PlayerRepository playerRepo, DiscordSocketClient client)
    {
        _towerService = towerService;
        _playerRepo = playerRepo;
        _client = client;
    }

    // =========================
    // LOBBY BUTTONS
    // =========================
    [ComponentInteraction("tower_join:*")]
    public async Task HandleJoin(string sessionId)
    {
        await DeferAsync(ephemeral: true);

        var userId = Context.User.Id;
        var player = await _playerRepo.GetByDiscordIdAsync(userId);

        if (player == null)
        {
            await FollowupAsync("❌ You haven't started your adventure yet.", ephemeral: true);
            return;
        }

        if (player.LastDuoTowerRun.HasValue && player.LastDuoTowerRun.Value.Date == DateTime.UtcNow.Date)
        {
            await FollowupAsync("❌ You have already done your duo Tower run today.", ephemeral: true);
            return;
        }

        var (success, error) = _towerService.TryJoin(sessionId, userId, Context.User.GlobalName ?? Context.User.Username);
        if (!success)
        {
            await FollowupAsync($"❌ {error}", ephemeral: true);
            return;
        }

        await FollowupAsync("✅ You joined the lobby!", ephemeral: true);
        await RefreshLobbyMessage(sessionId);
    }

    [ComponentInteraction("tower_ready:*")]
    public async Task HandleReady(string sessionId)
    {
        await DeferAsync(ephemeral: true);

        var session = _towerService.GetSession(sessionId);
        if (session == null)
        {
            await FollowupAsync("❌ Lobby not found.", ephemeral: true);
            return;
        }

        var userId = Context.User.Id;
        if (session.Participants.All(p => p.DiscordId != userId))
        {
            await FollowupAsync("❌ You are not in this lobby.", ephemeral: true);
            return;
        }

        _towerService.ToggleReady(sessionId, userId);
        await FollowupAsync("✅ Ready status toggled.", ephemeral: true);
        await RefreshLobbyMessage(sessionId);
    }

    [ComponentInteraction("tower_start:*")]
    public async Task HandleStart(string sessionId)
    {
        await DeferAsync(ephemeral: true);

        var session = _towerService.GetSession(sessionId);
        if (session == null)
        {
            await FollowupAsync("❌ Lobby not found.", ephemeral: true);
            return;
        }

        if (session.Participants[0].DiscordId != Context.User.Id)
        {
            await FollowupAsync("❌ Only the lobby creator can start the run.", ephemeral: true);
            return;
        }

        if (!_towerService.AllReady(sessionId))
        {
            await FollowupAsync("❌ Not all players are ready.", ephemeral: true);
            return;
        }

        var (success, error) = await _towerService.StartRunAsync(sessionId);
        if (!success)
        {
            await FollowupAsync($"❌ {error}", ephemeral: true);
            return;
        }

        // Remove lobby buttons from the lobby message
        var towerChannel = _client.GetChannel(TOWER_CHANNEL_ID) as ITextChannel;
        if (towerChannel != null)
        {
            try
            {
                var lobbyMsg = await towerChannel.GetMessageAsync(session.LobbyMessageId) as IUserMessage;
                if (lobbyMsg != null)
                    await lobbyMsg.ModifyAsync(m => { m.Components = new ComponentBuilder().Build(); });
            }
            catch { /* best effort */ }
        }

        // Mention participants in the thread
        var thread = _client.GetChannel(session.ThreadId) as IThreadChannel;
        if (thread != null)
        {
            var mentions = string.Join(" ", session.Participants.Select(p => $"<@{p.DiscordId}>"));
            await thread.SendMessageAsync($"{mentions}\n🗼 **Your Tower run has begun! Good luck climbing.**");
        }

        await FollowupAsync($"✅ Run started! Head to <#{session.ThreadId}>.", ephemeral: true);
    }

    // =========================
    // CHECKPOINT BUTTONS
    // =========================
    [ComponentInteraction("tower_cp:*:*:*")]
    public async Task HandleCheckpoint(string sessionId, string playerIdStr, string choice)
    {
        await DeferAsync(ephemeral: true);

        ulong playerId = ulong.Parse(playerIdStr);

        if (Context.User.Id != playerId)
        {
            await FollowupAsync("❌ This button is not for you.", ephemeral: true);
            return;
        }

        var session = _towerService.GetSession(sessionId);
        if (session == null)
        {
            await FollowupAsync("❌ Session not found.", ephemeral: true);
            return;
        }

        if (choice == "buff")
        {
            var p = session.Participants.FirstOrDefault(x => x.DiscordId == playerId);
            if (p?.PendingBuffChoices == null)
            {
                await FollowupAsync("❌ No buff choices available.", ephemeral: true);
                return;
            }

            var components = _towerService.BuildBuffPickComponents(sessionId, playerId, p.PendingBuffChoices);
            var desc = string.Join("\n", p.PendingBuffChoices.Select(b =>
            {
                var def = TowerBuffPool.Get(b);
                return $"{def.Emoji} **{def.Name}** — {def.Description}";
            }));

            await FollowupAsync(embed: new EmbedBuilder()
                .WithTitle("✨ Choose a Buff")
                .WithDescription(desc)
                .WithColor(Color.Blue)
                .Build(),
                components: components,
                ephemeral: true);
            return;
        }

        if (choice == "removedebuff")
        {
            var session2 = _towerService.GetSession(sessionId);
            var p2 = session2?.Participants.FirstOrDefault(x => x.DiscordId == playerId);
            if (p2 != null && p2.DebuffRemovesRemaining <= 0)
            {
                await FollowupAsync("❌ You have used all 5 Remove Debuff charges for this run.", ephemeral: true);
                return;
            }
            if (p2 != null && p2.Debuffs.Count > 1)
            {
                var components2 = _towerService.BuildDebuffPickComponents(sessionId, playerId, p2.Debuffs);
                var desc2 = string.Join("\n", p2.Debuffs.Select(d =>
                {
                    var def = TowerDebuffPool.Get(d.Type);
                    return $"{def.Emoji} **{def.Name}**";
                }));
                await FollowupAsync(embed: new EmbedBuilder()
                    .WithTitle("🗑️ Choose a Debuff to Remove")
                    .WithDescription(desc2)
                    .WithColor(Color.DarkRed)
                    .Build(),
                    components: components2,
                    ephemeral: true);
                return;
            }
        }

        var (success, message) = await _towerService.HandleCheckpointChoiceAsync(sessionId, playerId, choice);
        await FollowupAsync(message, ephemeral: true);
    }

    [ComponentInteraction("tower_rmdebuff:*:*:*")]
    public async Task HandleDebuffRemove(string sessionId, string playerIdStr, string indexStr)
    {
        await DeferAsync(ephemeral: true);

        ulong playerId = ulong.Parse(playerIdStr);
        int index = int.Parse(indexStr);

        if (Context.User.Id != playerId)
        {
            await FollowupAsync("❌ This button is not for you.", ephemeral: true);
            return;
        }

        var (success, message) = await _towerService.HandleDebuffRemoveAsync(sessionId, playerId, index);
        await FollowupAsync(message, ephemeral: true);
    }

    [ComponentInteraction("tower_buff:*:*:*")]
    public async Task HandleBuffPick(string sessionId, string playerIdStr, string indexStr)
    {
        await DeferAsync(ephemeral: true);

        ulong playerId = ulong.Parse(playerIdStr);
        int index = int.Parse(indexStr);

        if (Context.User.Id != playerId)
        {
            await FollowupAsync("❌ This button is not for you.", ephemeral: true);
            return;
        }

        var (success, message) = await _towerService.HandleBuffPickAsync(sessionId, playerId, index);
        await FollowupAsync(message, ephemeral: true);
    }

    // =========================
    // MERCHANT SHOP BUTTONS
    // =========================
    [ComponentInteraction("tower_shop:*:*:*")]
    public async Task HandleMerchantPurchase(string sessionId, string playerIdStr, string itemKey)
    {
        ulong playerId = ulong.Parse(playerIdStr);

        if (Context.User.Id != playerId)
        {
            await RespondAsync("❌ This shop is not for you.", ephemeral: true);
            return;
        }

        if (Context.Interaction is not SocketMessageComponent component)
            return;

        var (success, message) = await _towerService.HandleMerchantPurchaseAsync(sessionId, playerId, itemKey);

        if (!success)
        {
            await RespondAsync(message, ephemeral: true);
            return;
        }

        var session = _towerService.GetSession(sessionId);
        var p = session?.Participants.FirstOrDefault(x => x.DiscordId == playerId);

        if (p != null)
        {
            var refreshedComponents = _towerService.BuildMerchantComponents(sessionId, p);
            await component.UpdateAsync(msg => msg.Components = refreshedComponents);
        }
        else
        {
            await component.DeferAsync();
        }

        await component.FollowupAsync(message, ephemeral: true);
    }

    // =========================
    // PRE-BOSS CHOICE BUTTONS
    // =========================
    [ComponentInteraction("tower_preboss:*:*:*")]
    public async Task HandlePreBossChoice(string sessionId, string playerIdStr, string choice)
    {
        await DeferAsync(ephemeral: true);

        ulong playerId = ulong.Parse(playerIdStr);
        if (Context.User.Id != playerId)
        {
            await FollowupAsync("❌ This button is not for you.", ephemeral: true);
            return;
        }

        if (choice == "removedebuff")
        {
            var session = _towerService.GetSession(sessionId);
            var p = session?.Participants.FirstOrDefault(x => x.DiscordId == playerId);
            if (p != null && p.Debuffs.Count > 1)
            {
                var components = _towerService.BuildPreBossDebuffPickComponents(sessionId, playerId, p.Debuffs);
                var desc = string.Join("\n", p.Debuffs.Select(d =>
                {
                    var def = TowerDebuffPool.Get(d.Type);
                    return $"{def.Emoji} **{def.Name}** (x{d.Stacks})";
                }));
                await FollowupAsync(embed: new EmbedBuilder()
                    .WithTitle("🗑️ Choose a Debuff to Reduce")
                    .WithDescription(desc)
                    .WithColor(Color.DarkRed)
                    .Build(),
                    components: components,
                    ephemeral: true);
                return;
            }
        }

        var (success, message) = await _towerService.HandlePreBossChoiceAsync(sessionId, playerId, choice);
        if (message == "preboss_removedebuff_pick") return;
        await FollowupAsync(message, ephemeral: true);
    }

    [ComponentInteraction("tower_preboss_rm:*:*:*")]
    public async Task HandlePreBossDebuffPick(string sessionId, string playerIdStr, string indexStr)
    {
        await DeferAsync(ephemeral: true);

        ulong playerId = ulong.Parse(playerIdStr);
        int index = int.Parse(indexStr);

        if (Context.User.Id != playerId)
        {
            await FollowupAsync("❌ This button is not for you.", ephemeral: true);
            return;
        }

        var (success, message) = await _towerService.HandlePreBossDebuffPickAsync(sessionId, playerId, index);
        await FollowupAsync(message, ephemeral: true);
    }

    // =========================
    // HELPERS
    // =========================
    private async Task RefreshLobbyMessage(string sessionId)
    {
        var session = _towerService.GetSession(sessionId);
        if (session == null) return;

        bool allReady = _towerService.AllReady(sessionId);
        bool hasTwoPlayers = session.Participants.Count == 2;

        var components = new ComponentBuilder();

        if (session.Mode == TowerMode.Duo)
            components.WithButton("🚪 Join",   $"tower_join:{sessionId}",  ButtonStyle.Secondary, disabled: hasTwoPlayers);

        components.WithButton("✅ Ready",  $"tower_ready:{sessionId}", ButtonStyle.Success);
        components.WithButton("▶️ Start", $"tower_start:{sessionId}", ButtonStyle.Primary, disabled: !allReady || (session.Mode == TowerMode.Duo && !hasTwoPlayers));

        var towerChannel = _client.GetChannel(TOWER_CHANNEL_ID) as ITextChannel;
        if (towerChannel == null) return;

        try
        {
            var msg = await towerChannel.GetMessageAsync(session.LobbyMessageId) as IUserMessage;
            if (msg != null)
                await msg.ModifyAsync(m =>
                {
                    m.Embed = TowerModuleHelpers.BuildLobbyEmbed(session);
                    m.Components = components.Build();
                });
        }
        catch { /* best effort */ }
    }
}

// =========================
// SHARED HELPERS
// =========================
public static class TowerModuleHelpers
{
    public static Embed BuildLobbyEmbed(TowerSession session)
    {
        var builder = new EmbedBuilder()
            .WithTitle($"🗼 Tower of Doom — {session.Mode} Run")
            .WithColor(Color.DarkRed)
            .WithDescription(session.Mode == TowerMode.Solo
                ? "Climb alone. The tower shows no mercy."
                : "Find a partner. Two climb higher than one.\n\nBoth players must ready up before starting.");

        foreach (var p in session.Participants)
            builder.AddField(p.Username, p.ReadyToStart ? "✅ Ready" : "❌ Not Ready", true);

        if (session.Mode == TowerMode.Duo && session.Participants.Count < 2)
            builder.AddField("Player 2", "*Waiting...*", true);

        builder.WithFooter("Rewards: 15 gold/floor · 2500 XP · 250 Pet XP on run end");

        return builder.Build();
    }
}

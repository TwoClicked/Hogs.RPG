// Hogs.RPG.Services/TrailServices/TrailService.cs

using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.GameData.Trails;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Hogs.RPG.Services.TrailServices
{
    public class TrailService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;
        private readonly Random _random = new();

        private readonly Dictionary<ulong, TrailState> _activeTrails = new();

        private const int MaxDailyTrails = 3;
        private const ulong FeedChannelId = 1485357755433750549;

        private static readonly string[] AllRareMaterials = InventoryItemDefinitions.All.Values
            .Where(i => i.Type == "Material" && i.SubCategory == "Rare")
            .Select(i => i.Id)
            .ToArray();

        private static readonly string[] CommonRareMaterials = InventoryItemDefinitions.All.Values
            .Where(i => i.Type == "Material" && i.SubCategory == "Rare")
            .Select(i => i.Id)
            .Take(5)
            .ToArray();

        private static readonly string[] SnareMaterials = InventoryItemDefinitions.All.Values
            .Where(i => i.Type == "Material" && i.SubCategory == "Craft" && i.Tier <= 2)
            .Select(i => i.Id)
            .ToArray();

        public TrailService(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;
        }

        // =========================
        // START TRAIL
        // =========================
        public async Task<(string? error, Embed? embed, MessageComponent? components)> StartTrailAsync(
            ulong userId, string zoneId)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();

            var player = await playerRepo.GetByDiscordIdAsync(userId);
            if (player == null)
                return ("You need to start your adventure first.", null, null);

            if (_activeTrails.ContainsKey(userId))
                return ("You are already on a trail. Check your DMs to continue.", null, null);

            if (!TrailZoneRegistry.All.TryGetValue(zoneId, out var zone))
                return ("Unknown trail zone.", null, null);

            if (player.Level < zone.RequiredLevel)
                return ($"You must be level {zone.RequiredLevel} to run this trail.", null, null);

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (player.LastTrailDate != today)
            {
                player.TrailsToday = 0;
                player.LastTrailDate = today;
            }

            if (player.TrailsToday >= MaxDailyTrails)
                return ($"You've used all {MaxDailyTrails} trails for today. Resets at midnight UTC.", null, null);

            player.TrailsToday++;
            await playerRepo.UpdatePlayerAsync(player);

            var state = new TrailState
            {
                UserId = userId,
                Zone = zone,
                Events = RollEvents(zone),
                BaseTokens = _random.Next(zone.MinTokens, zone.MaxTokens + 1)
            };

            _activeTrails[userId] = state;

            await ProcessAutoEventsAsync(state, scope.ServiceProvider);

            if (state.CurrentEventIndex >= state.Events.Count)
            {
                await FinalizeTrailAsync(state, scope.ServiceProvider);
                _activeTrails.Remove(userId);
            }

            var (embed, components) = BuildTrailMessage(state);
            return (null, embed, components);
        }

        // =========================
        // STORE DM MESSAGE REFERENCE
        // =========================
        public void SetTrailMessage(ulong userId, ulong messageId, ulong channelId)
        {
            if (_activeTrails.TryGetValue(userId, out var state))
            {
                state.DmMessageId = messageId;
                state.DmChannelId = channelId;
            }
        }

        // =========================
        // HANDLE DECISION
        // =========================
        public async Task<(Embed embed, MessageComponent? components)?> HandleDecisionAsync(
            ulong userId, string choice)
        {
            if (!_activeTrails.TryGetValue(userId, out var state))
                return null;

            using var scope = _scopeFactory.CreateScope();
            var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();

            var decisionEvent = state.Events[state.CurrentEventIndex];
            string logEntry = $"{decisionEvent.Icon} **{decisionEvent.Name}**";

            bool boosted = state.NextEventBoosted;
            state.NextEventBoosted = false;

            switch (decisionEvent.Type)
            {
                case TrailEventType.AmbushEncounter:
                    if (choice == "ambush")
                    {
                        bool success = _random.NextDouble() < 0.60;
                        if (success)
                        {
                            int bonus = boosted ? 15 : 10;
                            state.TokensEarned += bonus;
                            logEntry += $" — You struck true! **+{bonus} tokens**";
                        }
                        else
                        {
                            int penalty = (int)(state.TokensEarned * 0.20);
                            state.TokensEarned = Math.Max(0, state.TokensEarned - penalty);
                            logEntry += $" — The beast fought back hard. **-{penalty} tokens**";
                        }
                    }
                    else
                    {
                        state.TokensEarned += 2;
                        logEntry += " — You slipped past quietly. **+2 tokens**";
                    }
                    break;

                case TrailEventType.TrackersGamble:
                    if (choice == "pressluck")
                    {
                        bool success = _random.NextDouble() < 0.50;
                        if (success)
                        {
                            int before = state.TokensEarned;
                            state.TokensEarned = boosted
                                ? (int)(state.TokensEarned * 2.5)
                                : state.TokensEarned * 2;
                            logEntry += $" — Fortune favours the bold! **+{state.TokensEarned - before} tokens**";
                        }
                        else
                        {
                            int penalty = (int)(state.TokensEarned * 0.40);
                            state.TokensEarned = Math.Max(0, state.TokensEarned - penalty);
                            logEntry += $" — Another hunter got there first. **-{penalty} tokens**";
                        }
                    }
                    else
                    {
                        int safe = boosted ? 8 : 5;
                        state.TokensEarned += safe;
                        logEntry += $" — You took the safe option. **+{safe} tokens**";
                    }
                    break;

                case TrailEventType.RareSighting:
                    if (choice == "investigate")
                    {
                        bool success = _random.NextDouble() < 0.40;
                        if (success)
                        {
                            var rareMat = AllRareMaterials[_random.Next(AllRareMaterials.Length)];
                            await inventoryService.GiveItemAsync(userId, rareMat, 1);
                            state.TokensEarned += 1;
                            string matName = GetItemName(rareMat);
                            logEntry += $" — You secured a rare find! **+1x {matName}**";
                            state.NotableDrops.Add($"1x {matName}");
                        }
                        else
                        {
                            int penalty = (int)(state.TokensEarned * 0.20);
                            state.TokensEarned = Math.Max(0, state.TokensEarned - penalty);
                            logEntry += $" — It spooked and vanished, costing you time. **-{penalty} tokens**";
                        }
                    }
                    else
                    {
                        state.TokensEarned += 1;
                        logEntry += " — You moved on carefully. **+1 token**";
                    }
                    break;

                case TrailEventType.LegendaryEncounter:
                    if (choice == "approach")
                    {
                        var player = await playerRepo.GetByDiscordIdAsync(userId);
                        bool petDrop = _random.NextDouble() < 0.03;

                        if (petDrop && !player.HasHuntingPet)
                        {
                            player.HasHuntingPet = true;
                            await playerRepo.UpdatePlayerAsync(player);
                            state.TokensEarned += 5;
                            state.PetDropped = true;
                            logEntry += " — **The creature approached you calmly. A hunting companion has joined your side! 🐾 +5 tokens**";
                        }
                        else if (petDrop && player.HasHuntingPet)
                        {
                            state.TokensEarned += 15;
                            logEntry += " — You already have a companion, but the creature left a gift. **+15 tokens**";
                        }
                        else
                        {
                            state.TokensEarned += 5;
                            logEntry += " — The creature regarded you calmly, then disappeared into the dark. **+5 tokens**";
                        }
                    }
                    else
                    {
                        state.TokensEarned += 3;
                        logEntry += " — You retreated quietly into the trees. **+3 tokens**";
                    }
                    break;
            }

            state.EventLog.Add(logEntry);
            state.CurrentEventIndex++;

            await ProcessAutoEventsAsync(state, scope.ServiceProvider);

            if (state.CurrentEventIndex >= state.Events.Count)
            {
                await FinalizeTrailAsync(state, scope.ServiceProvider);
                _activeTrails.Remove(userId);
            }

            var (embed, components) = BuildTrailMessage(state);
            return (embed, components);
        }

        // =========================
        // PROCESS AUTO EVENTS
        // =========================
        private async Task ProcessAutoEventsAsync(TrailState state, IServiceProvider services)
        {
            var inventoryService = services.GetRequiredService<InventoryService>();

            while (state.CurrentEventIndex < state.Events.Count)
            {
                var evt = state.Events[state.CurrentEventIndex];

                if (IsDecisionEvent(evt.Type))
                    break;

                int modifier = state.NextEventBoosted
                    ? (int)(evt.TokenModifier * 1.5)
                    : evt.TokenModifier;

                state.NextEventBoosted = false;

                string logEntry = $"{evt.Icon} **{evt.Name}**";

                switch (evt.Type)
                {
                    case TrailEventType.FreshTracks:
                        state.TokensEarned += modifier;
                        state.NextEventBoosted = true;
                        logEntry += $" — Promising signs ahead. **+{modifier} tokens** *(next event boosted)*";
                        break;

                    case TrailEventType.SnareSet:
                        state.TokensEarned += modifier;
                        var snareMat = SnareMaterials[_random.Next(SnareMaterials.Length)];
                        int snareQty = _random.Next(2, 6);
                        await inventoryService.GiveItemAsync(state.UserId, snareMat, snareQty);
                        string snareMatName = GetItemName(snareMat);
                        logEntry += $" — **+{modifier} tokens** + {snareQty}x {snareMatName}";
                        state.NotableDrops.Add($"{snareQty}x {snareMatName}");
                        break;

                    case TrailEventType.RoughTerrain:
                        int terrainPenalty = Math.Abs(modifier);
                        state.TokensEarned = Math.Max(0, state.TokensEarned - terrainPenalty);
                        logEntry += $" — Rough going. **-{terrainPenalty} tokens**";
                        break;

                    case TrailEventType.HiddenCache:
                        state.TokensEarned += modifier;
                        var cacheMat = CommonRareMaterials[_random.Next(CommonRareMaterials.Length)];
                        await inventoryService.GiveItemAsync(state.UserId, cacheMat, 1);
                        string cacheMatName = GetItemName(cacheMat);
                        logEntry += $" — **+{modifier} tokens** + 1x {cacheMatName}";
                        state.NotableDrops.Add($"1x {cacheMatName}");
                        break;

                    case TrailEventType.ClearPath:
                        state.TokensEarned += modifier;
                        logEntry += $" — Smooth sailing. **+{modifier} tokens**";
                        break;
                }

                state.EventLog.Add(logEntry);
                state.CurrentEventIndex++;
            }
        }

        // =========================
        // FINALIZE TRAIL
        // =========================
        private async Task FinalizeTrailAsync(TrailState state, IServiceProvider services)
        {
            var playerRepo = services.GetRequiredService<PlayerRepository>();
            var player = await playerRepo.GetByDiscordIdAsync(state.UserId);

            int total = Math.Max(1, state.TokensEarned + state.BaseTokens);

            player.TrackerTokens += total;
            player.TrailsCompleted++;

            await playerRepo.UpdatePlayerAsync(player);

            state.TotalTokensAwarded = total;
            state.PlayerTokenBalance = player.TrackerTokens;
            state.IsComplete = true;

            await PostFeedAsync(state, player);
        }

        // =========================
        // UPDATE DM MESSAGE (legacy — unused by decision flow)
        // =========================
        public async Task UpdateDmMessageAsync(TrailState state)
        {
            if (state.DmChannelId == 0 || state.DmMessageId == 0)
                return;

            if (_client.GetChannel(state.DmChannelId) is not IMessageChannel channel)
                return;

            var (embed, components) = BuildTrailMessage(state);

            await channel.ModifyMessageAsync(state.DmMessageId, msg =>
            {
                msg.Embed = embed;
                msg.Components = components ?? new ComponentBuilder().Build();
            });
        }

        // =========================
        // BUILD TRAIL MESSAGE
        // =========================
        private (Embed embed, MessageComponent? components) BuildTrailMessage(TrailState state)
        {
            var sb = new StringBuilder();

            foreach (var log in state.EventLog)
                sb.AppendLine($"✅ {log}");

            MessageComponent? components = null;

            if (!state.IsComplete && state.CurrentEventIndex < state.Events.Count)
            {
                var current = state.Events[state.CurrentEventIndex];
                sb.AppendLine();
                sb.AppendLine($"{current.Icon} **{current.Name}**");
                sb.AppendLine(current.Description);
                components = BuildDecisionComponents(current.Type);
            }

            sb.AppendLine();
            sb.AppendLine($"🪙 **Tokens earned so far:** {state.TokensEarned}");

            string title = state.IsComplete
                ? $"{state.Zone.Icon} {state.Zone.Name} — Complete!"
                : $"{state.Zone.Icon} {state.Zone.Name} — Event {Math.Min(state.CurrentEventIndex + 1, 5)}/5";

            var color = state.IsComplete ? Color.Gold : new Color(0x2E8B57);

            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(color)
                .WithDescription(sb.ToString().Trim());

            if (state.IsComplete)
            {
                embed.AddField(
                    "🏅 Tokens Awarded",
                    $"**{state.TotalTokensAwarded}** *(includes {state.BaseTokens} base trail tokens)*",
                    inline: false);

                embed.AddField(
                    "💰 Your Balance",
                    $"**{state.PlayerTokenBalance} Tracker Tokens** — use `/trail shop` to spend them",
                    inline: false);

                if (state.PetDropped)
                    embed.AddField(
                        "🐾 Hunting Companion Found!",
                        "A rare creature has joined your side. It will aid you on future hunts.",
                        inline: false);
            }

            return (embed.Build(), components);
        }

        // =========================
        // BUILD DECISION COMPONENTS
        // =========================
        private static MessageComponent? BuildDecisionComponents(TrailEventType type)
        {
            var builder = new ComponentBuilder();

            switch (type)
            {
                case TrailEventType.AmbushEncounter:
                    builder.WithButton("🏹 Ambush", "trail_decision_ambush", ButtonStyle.Danger);
                    builder.WithButton("🌿 Pass", "trail_decision_pass", ButtonStyle.Secondary);
                    break;

                case TrailEventType.TrackersGamble:
                    builder.WithButton("🎲 Press Luck", "trail_decision_pressluck", ButtonStyle.Primary);
                    builder.WithButton("✅ Take It Safe", "trail_decision_takesafe", ButtonStyle.Secondary);
                    break;

                case TrailEventType.RareSighting:
                    builder.WithButton("🔍 Investigate", "trail_decision_investigate", ButtonStyle.Primary);
                    builder.WithButton("➡️ Move On", "trail_decision_moveon", ButtonStyle.Secondary);
                    break;

                case TrailEventType.LegendaryEncounter:
                    builder.WithButton("🤝 Approach", "trail_decision_approach", ButtonStyle.Success);
                    builder.WithButton("🏃 Retreat", "trail_decision_retreat", ButtonStyle.Secondary);
                    break;

                default:
                    return null;
            }

            return builder.Build();
        }

        // =========================
        // ROLL EVENTS
        // =========================
        private List<TrailEventDefinition> RollEvents(TrailZoneDefinition zone)
        {
            var pool = zone.EventPool
                .Select(kvp => (Event: TrailEventRegistry.All[kvp.Key], Weight: kvp.Value))
                .ToList();

            int totalWeight = pool.Sum(x => x.Weight);
            var rolled = new List<TrailEventDefinition>();
            int legendaryCount = 0;

            for (int i = 0; i < 5; i++)
            {
                int attempts = 0;
                TrailEventDefinition? picked = null;

                while (picked == null && attempts < 20)
                {
                    attempts++;
                    int roll = _random.Next(totalWeight);
                    int cumulative = 0;

                    foreach (var (evt, weight) in pool)
                    {
                        cumulative += weight;
                        if (roll < cumulative)
                        {
                            if (evt.Type == TrailEventType.LegendaryEncounter && legendaryCount >= 1)
                                break;

                            picked = evt;

                            if (evt.Type == TrailEventType.LegendaryEncounter)
                                legendaryCount++;

                            break;
                        }
                    }
                }

                rolled.Add(picked ?? AllTrailEvents.ClearPath);
            }

            return rolled;
        }

        // =========================
        // POST TO RPG FEED
        // =========================
        private async Task PostFeedAsync(TrailState state, Player player)
        {
            if (_client.GetChannel(FeedChannelId) is not IMessageChannel channel)
                return;

            var sb = new StringBuilder();
            sb.AppendLine($"<@{state.UserId}> completed the **{state.Zone.Name}** and earned **{state.TotalTokensAwarded} Tracker Tokens**!");

            if (state.PetDropped)
                sb.AppendLine("🐾 They found a **Hunting Companion** on the trail!");

            if (state.NotableDrops.Count > 0)
                sb.AppendLine($"✨ Notable finds: {string.Join(", ", state.NotableDrops.Distinct())}");

            sb.AppendLine($"🏅 Lifetime Trails: **{player.TrailsCompleted}**");

            var embed = new EmbedBuilder()
                .WithTitle($"{state.Zone.Icon} Trail Complete")
                .WithColor(Color.Gold)
                .WithDescription(sb.ToString().Trim())
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        // =========================
        // PURCHASE
        // =========================
        public async Task<string> PurchaseAsync(ulong userId, string itemId, string category)
        {
            itemId = itemId.Replace("-", "_");

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();
            var player = await playerRepo.GetByDiscordIdAsync(userId);
            if (player == null) return "❌ Player not found.";

            int cost;
            int quantity;

            switch (category)
            {
                case "gear":
                    // All 9 Hunter's gear pieces cost 200 tokens each
                    cost = 200;
                    quantity = 1;
                    break;

                case "craft":
                    // Level 30 required — prevents tier skipping
                    if (player.Level < 30)
                        return "❌ Craft materials are locked until **Level 30**. Keep hunting and grinding!";
                    cost = 50;
                    quantity = 100;
                    break;

                case "rare":
                    // Level 30 required — prevents tier skipping
                    if (player.Level < 30)
                        return "❌ Rare materials are locked until **Level 30**. Keep hunting and grinding!";
                    cost = 75;
                    quantity = 5;
                    break;

                case "snack":
                    cost = 8;
                    quantity = 1;
                    itemId = "trail_pet_snack";
                    break;

                default:
                    return "❌ Unknown shop category.";
            }

            if (player.TrackerTokens < cost)
                return $"❌ You need **{cost} tokens** but only have **{player.TrackerTokens}**.";

            player.TrackerTokens -= cost;
            await playerRepo.UpdatePlayerAsync(player);

            if (category == "snack")
            {
                var petService = scope.ServiceProvider.GetRequiredService<PetServices.PetService>();
                var pet = await petService.GetEquippedPetAsync(userId);

                if (pet == null)
                {
                    player.TrackerTokens += cost;
                    await playerRepo.UpdatePlayerAsync(player);
                    return "❌ You don't have a pet equipped.";
                }

                await petService.AddXPAsync(userId, 25);
                return $"🐾 Your pet received **+25 XP**! You have **{player.TrackerTokens} tokens** remaining.";
            }

            await inventoryService.GiveItemAsync(userId, itemId, quantity);

            string itemName = EquipmentRegistry.All.TryGetValue(itemId, out var gearDef)
                ? gearDef.Name
                : InventoryItemDefinitions.All.TryGetValue(itemId, out var matDef)
                    ? matDef.Name
                    : itemId;

            return $"✅ Purchased **{quantity}x {itemName}** for **{cost} tokens**. You have **{player.TrackerTokens} tokens** remaining.";
        }

        // =========================
        // GET TOKEN BALANCE
        // =========================
        public async Task<int?> GetTokenBalanceAsync(ulong userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var player = await playerRepo.GetByDiscordIdAsync(userId);
            return player?.TrackerTokens;
        }

        // =========================
        // HELPERS
        // =========================

        private static bool IsDecisionEvent(TrailEventType type) =>
            type == TrailEventType.AmbushEncounter ||
            type == TrailEventType.TrackersGamble ||
            type == TrailEventType.RareSighting ||
            type == TrailEventType.LegendaryEncounter;

        private static string GetItemName(string itemId) =>
            InventoryItemDefinitions.All.TryGetValue(itemId, out var def) ? def.Name : itemId;

        public bool HasActiveTrail(ulong userId) => _activeTrails.ContainsKey(userId);

        public TrailState? GetActiveTrail(ulong userId) =>
            _activeTrails.TryGetValue(userId, out var state) ? state : null;
    }
}
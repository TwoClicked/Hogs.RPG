// Hogs.RPG.Bot/Commands/TrailModule.cs

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.GameData.Trails;
using Hogs.RPG.Services.TrailServices;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    [Group("trail", "The Ashwood Trail — hunt for tokens, gear and your companion")]
    public class TrailModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TrailService _trailService;

        public TrailModule(TrailService trailService)
        {
            _trailService = trailService;
        }

        // =========================
        // /trail run
        // =========================
        [SlashCommand("run", "Run the Ashwood Trail and earn Tracker Tokens")]
        public async Task RunTrail()
        {
            await DeferAsync(ephemeral: true);

            var (error, embed, components) = await _trailService.StartTrailAsync(
                Context.User.Id,
                AllTrailZones.AshwoodTrail.Id);

            if (error != null)
            {
                await FollowupAsync(error, ephemeral: true);
                return;
            }

            try
            {
                var dm = await Context.User.CreateDMChannelAsync();

                var message = await dm.SendMessageAsync(
                    embed: embed,
                    components: components ?? new ComponentBuilder().Build());

                _trailService.SetTrailMessage(Context.User.Id, message.Id, dm.Id);

                await FollowupAsync("📬 Check your DMs to run your trail!", ephemeral: true);
            }
            catch
            {
                await FollowupAsync(
                    "❌ I couldn't DM you. Please enable DMs from server members and try again.",
                    ephemeral: true);
            }
        }

        // =========================
        // /trail tokens
        // =========================
        [SlashCommand("tokens", "Check your Tracker Token balance")]
        public async Task CheckTokens()
        {
            await DeferAsync(ephemeral: true);

            var balance = await _trailService.GetTokenBalanceAsync(Context.User.Id);

            if (balance == null)
            {
                await FollowupAsync("You need to start your adventure first.", ephemeral: true);
                return;
            }

            await FollowupAsync(
                $"🪙 You have **{balance} Tracker Tokens**.\nUse `/trail shop` to spend them.",
                ephemeral: true);
        }

        // =========================
        // /trail shop
        // =========================
        [SlashCommand("shop", "Browse the Tracker's Camp shop")]
        public async Task OpenShop()
        {
            await DeferAsync(ephemeral: true);

            var embed = new EmbedBuilder()
                .WithTitle("🏕️ Tracker's Camp")
                .WithColor(new Color(0x8B4513))
                .WithDescription(
                    "Welcome to the Tracker's Camp. Spend your hard-earned tokens here.\n\n" +
                    "**Categories**\n" +
                    "⚔️ **Gear** — Hunter's gear set pieces (shop exclusive)\n" +
                    "🧱 **Craft** — Hunt craft materials in bulk\n" +
                    "★ **Rare** — Rare hunt materials\n" +
                    "🐾 **Snacks** — Pet snack for your companion");

            var components = new ComponentBuilder()
                .WithButton("⚔️ Gear", "trail_shop_gear_0", ButtonStyle.Primary, row: 0)
                .WithButton("🧱 Craft", "trail_shop_craft_0", ButtonStyle.Secondary, row: 0)
                .WithButton("★ Rare", "trail_shop_rare_0", ButtonStyle.Secondary, row: 0)
                .WithButton("🐾 Snacks", "trail_shop_snack_0", ButtonStyle.Secondary, row: 0);

            await FollowupAsync(embed: embed.Build(), components: components.Build(), ephemeral: true);
        }
    }

    // =========================
    // TRAIL INTERACTION MODULE
    // Component interactions must NOT be in a [Group] module
    // =========================
    public class TrailInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TrailService _trailService;

        public TrailInteractionModule(TrailService trailService)
        {
            _trailService = trailService;
        }

        // =========================
        // DECISION BUTTONS
        // =========================
        [ComponentInteraction("trail_decision_ambush")]
        public async Task DecisionAmbush() => await HandleDecision("ambush");

        [ComponentInteraction("trail_decision_pass")]
        public async Task DecisionPass() => await HandleDecision("pass");

        [ComponentInteraction("trail_decision_pressluck")]
        public async Task DecisionPressLuck() => await HandleDecision("pressluck");

        [ComponentInteraction("trail_decision_takesafe")]
        public async Task DecisionTakeSafe() => await HandleDecision("takesafe");

        [ComponentInteraction("trail_decision_investigate")]
        public async Task DecisionInvestigate() => await HandleDecision("investigate");

        [ComponentInteraction("trail_decision_moveon")]
        public async Task DecisionMoveOn() => await HandleDecision("moveon");

        [ComponentInteraction("trail_decision_approach")]
        public async Task DecisionApproach() => await HandleDecision("approach");

        [ComponentInteraction("trail_decision_retreat")]
        public async Task DecisionRetreat() => await HandleDecision("retreat");

        // =========================
        // SHOP NAV BUTTONS
        // =========================
        [ComponentInteraction("trail_shop_*_*")]
        public async Task ShopNav(string category, int page)
        {
            if (Context.Interaction is not SocketMessageComponent component) return;
            await component.DeferAsync();
            await ShowShop(category, page, component);
        }

        [ComponentInteraction("trail_buy_*_*_*")]
        public async Task ShopBuy(string itemId, string category, int page)
        {
            if (Context.Interaction is not SocketMessageComponent component) return;
            await component.DeferAsync();

            itemId = itemId.Replace("-", "_");
            var result = await _trailService.PurchaseAsync(Context.User.Id, itemId, category);

            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = result;
                msg.Embed = null;
                msg.Components = new ComponentBuilder()
                    .WithButton("← Back to Shop", $"trail_shop_{category}_{page}", ButtonStyle.Secondary)
                    .Build();
            });
        }

        // =========================
        // DECISION HANDLER
        // =========================
        private async Task HandleDecision(string choice)
        {
            if (Context.Interaction is not SocketMessageComponent component) return;

            await component.DeferAsync();

            if (!_trailService.HasActiveTrail(Context.User.Id))
            {
                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "❌ No active trail found. It may have expired — use `/trail run` to start a new one.";
                    msg.Components = new ComponentBuilder().Build();
                });
                return;
            }

            await _trailService.HandleDecisionAsync(Context.User.Id, choice);
        }

        // =========================
        // SHOW SHOP
        // =========================
        private async Task ShowShop(string category, int page, SocketMessageComponent component)
        {
            var embed = new EmbedBuilder()
                .WithTitle("🏕️ Tracker's Camp")
                .WithColor(new Color(0x8B4513));

            var components = new ComponentBuilder();

            switch (category)
            {
                case "main":
                    embed.WithDescription(
                        "Welcome to the Tracker's Camp. Spend your hard-earned tokens here.\n\n" +
                        "**Categories**\n" +
                        "⚔️ **Gear** — Hunter's gear set pieces (shop exclusive)\n" +
                        "🧱 **Craft** — Hunt craft materials in bulk\n" +
                        "★ **Rare** — Rare hunt materials\n" +
                        "🐾 **Snacks** — Pet snack for your companion");

                    components
                        .WithButton("⚔️ Gear", "trail_shop_gear_0", ButtonStyle.Primary, row: 0)
                        .WithButton("🧱 Craft", "trail_shop_craft_0", ButtonStyle.Secondary, row: 0)
                        .WithButton("★ Rare", "trail_shop_rare_0", ButtonStyle.Secondary, row: 0)
                        .WithButton("🐾 Snacks", "trail_shop_snack_0", ButtonStyle.Secondary, row: 0);
                    break;

                case "gear":
                    BuildGearShop(embed, components, page);
                    break;

                case "craft":
                    BuildCraftShop(embed, components, page);
                    break;

                case "rare":
                    BuildRareShop(embed, components, page);
                    break;

                case "snack":
                    BuildSnackShop(embed, components);
                    break;
            }

            if (category != "main")
                components.WithButton("← Back", "trail_shop_main_0", ButtonStyle.Secondary, row: 4);

            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed.Build();
                msg.Components = components.Build();
            });
        }

        private void BuildGearShop(EmbedBuilder embed, ComponentBuilder components, int page)
        {
            embed.WithTitle("🏕️ Tracker's Camp — ⚔️ Hunter's Gear");

            var items = new[]
            {
                ("hunter-vest",      "Hunter's Vest",      130),
                ("hunter-bow",       "Hunter's Bow",       130),
                ("hunter-helm",      "Hunter's Helm",      110),
                ("hunter-leggings",  "Hunter's Leggings",  110),
                ("hunter-quiver",    "Hunter's Quiver",    110),
                ("hunter-boots",     "Hunter's Boots",      80),
                ("hunter-gloves",    "Hunter's Gloves",     80),
                ("hunter-ring",      "Hunter's Ring",       80),
                ("hunter-amulet",    "Hunter's Amulet",     80),
            };

            const int perPage = 4;
            int totalPages = (int)Math.Ceiling(items.Length / (double)perPage);
            page = Math.Clamp(page, 0, totalPages - 1);

            var sb = new StringBuilder();
            sb.AppendLine("Each piece gives **+0.5% XP** and **+0.5% materials** on `/hunt`.");
            sb.AppendLine("Equip all 9 for a **+5% rare drop rate** set bonus.\n");

            var pageItems = items.Skip(page * perPage).Take(perPage).ToList();
            foreach (var (id, name, cost) in pageItems)
                sb.AppendLine($"⚔️ **{name}** — {cost} tokens");

            embed.WithDescription(sb.ToString().Trim());
            embed.WithFooter($"Page {page + 1}/{totalPages} · Full set: ~950 tokens over ~1 month");

            for (int i = 0; i < pageItems.Count; i++)
            {
                var (id, name, cost) = pageItems[i];
                components.WithButton($"Buy {name} ({cost}🪙)", $"trail_buy_{id}_gear_{page}", ButtonStyle.Success, row: i);
            }

            if (totalPages > 1)
            {
                var navRow = new ActionRowBuilder();
                if (page > 0) navRow.WithButton("⬅️", $"trail_shop_gear_{page - 1}", ButtonStyle.Secondary);
                if (page < totalPages - 1) navRow.WithButton("➡️", $"trail_shop_gear_{page + 1}", ButtonStyle.Secondary);
                components.AddRow(navRow);
            }
        }

        private void BuildCraftShop(EmbedBuilder embed, ComponentBuilder components, int page)
        {
            embed.WithTitle("🏕️ Tracker's Camp — 🧱 Craft Materials");

            var items = new[]
            {
                ("fur",            "Fur",             100, 5),
                ("leather",        "Leather",         100, 5),
                ("bone",           "Bone",            100, 5),
                ("feather",        "Feather",         100, 5),
                ("claws",          "Claws",           100, 5),
                ("monster-blood",  "Monster Blood",   100, 5),
                ("hide",           "Hide",            100, 5),
                ("fang",           "Fang",            100, 5),
                ("talon",          "Talon",           100, 5),
                ("horn",           "Horn",            100, 5),
                ("sharp-claw",     "Sharp Claw",      100, 5),
                ("saber-fang",      "Saber Fang",      100, 5),
                ("griffin-feather", "Griffin Feather", 100, 5),
                ("giant-antler",    "Giant Antler",    100, 5),
                ("thick-hide",      "Thick Hide",      100, 5),
                ("dark-feather",    "Dark Feather",    100, 5),
                ("storm-feather",   "Storm Feather",   100, 5),
                ("ancient-hide",    "Ancient Hide",    100, 5),
                ("shadow-claw",     "Shadow Claw",     100, 5),
                ("titan-antler",    "Titan Antler",    100, 5),
                ("void-feather",    "Void Feather",    100, 5),
                ("mythic-hide",     "Mythic Hide",     100, 5),
                ("sky-talon",       "Sky Talon",       100, 5),
                ("abyss-claw",      "Abyss Claw",      100, 5),
                ("colossus-antler", "Colossus Antler", 100, 5),
                ("death-feather",   "Death Feather",   100, 5),
            };

            const int perPage = 4;
            int totalPages = (int)Math.Ceiling(items.Length / (double)perPage);
            page = Math.Clamp(page, 0, totalPages - 1);

            var pageItems = items.Skip(page * perPage).Take(perPage).ToList();

            var sb = new StringBuilder();
            foreach (var (id, name, qty, cost) in pageItems)
                sb.AppendLine($"🧱 **{name}** — {qty}x for **{cost} tokens**");

            embed.WithDescription(sb.ToString().Trim());
            embed.WithFooter($"Page {page + 1}/{totalPages}");

            for (int i = 0; i < pageItems.Count; i++)
            {
                var (id, name, qty, cost) = pageItems[i];
                components.WithButton($"Buy {qty}x {name} ({cost}🪙)", $"trail_buy_{id}_craft_{page}", ButtonStyle.Success, row: i);
            }

            if (totalPages > 1)
            {
                var navRow = new ActionRowBuilder();
                if (page > 0) navRow.WithButton("⬅️", $"trail_shop_craft_{page - 1}", ButtonStyle.Secondary);
                if (page < totalPages - 1) navRow.WithButton("➡️", $"trail_shop_craft_{page + 1}", ButtonStyle.Secondary);
                components.AddRow(navRow);
            }
        }

        private void BuildRareShop(EmbedBuilder embed, ComponentBuilder components, int page)
        {
            embed.WithTitle("🏕️ Tracker's Camp — ★ Rare Materials");

            var items = new[]
            {
                ("wolf-trophy",     "Wolf Trophy"),
                ("boar-tusk",       "Boar Tusk"),
                ("stag-antler",     "Stag Antler"),
                ("ancient-feather", "Ancient Feather"),
                ("bear-heart",      "Bear Heart"),
                ("alpha-fang",      "Alpha Fang"),
                ("storm-talon",     "Storm Talon"),
                ("saber-relic",     "Saber Relic"),
                ("griffin-core",    "Griffin Core"),
                ("storm-relic",     "Storm Relic"),
                ("ancient-core",    "Ancient Core"),
                ("mythic-heart",    "Mythic Heart"),
                ("sky-relic",       "Sky Relic"),
            };

            const int perPage = 4;
            int totalPages = (int)Math.Ceiling(items.Length / (double)perPage);
            page = Math.Clamp(page, 0, totalPages - 1);

            var pageItems = items.Skip(page * perPage).Take(perPage).ToList();

            var sb = new StringBuilder();
            foreach (var (id, name) in pageItems)
                sb.AppendLine($"★ **{name}** — 5x for **10 tokens**");

            embed.WithDescription(sb.ToString().Trim());
            embed.WithFooter($"Page {page + 1}/{totalPages}");

            for (int i = 0; i < pageItems.Count; i++)
            {
                var (id, name) = pageItems[i];
                components.WithButton($"Buy 5x {name} (10🪙)", $"trail_buy_{id}_rare_{page}", ButtonStyle.Success, row: i);
            }

            if (totalPages > 1)
            {
                var navRow = new ActionRowBuilder();
                if (page > 0) navRow.WithButton("⬅️", $"trail_shop_rare_{page - 1}", ButtonStyle.Secondary);
                if (page < totalPages - 1) navRow.WithButton("➡️", $"trail_shop_rare_{page + 1}", ButtonStyle.Secondary);
                components.AddRow(navRow);
            }
        }

        private void BuildSnackShop(EmbedBuilder embed, ComponentBuilder components)
        {
            embed.WithTitle("🏕️ Tracker's Camp — 🐾 Pet Snacks");
            embed.WithDescription(
                "🐾 **Trail Pet Snack** — +25 XP to your equipped pet\n" +
                "**Cost: 8 tokens**\n\n" +
                "*The gold shop version gives +50 XP — this is the trail flavour.*");

            components.WithButton("Buy Trail Pet Snack (8🪙)", "trail_buy_trail-pet-snack_snack_0", ButtonStyle.Success, row: 0);
        }
    }
}
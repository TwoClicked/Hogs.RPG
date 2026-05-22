// Hogs.RPG.Bot/Commands/TrailModule.cs

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Registries;
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
        // Starts a trail in DMs — auto-resolves passive events,
        // pauses on decision events for player input
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

            // Attempt to send the trail to DMs
            try
            {
                var dm = await Context.User.CreateDMChannelAsync();

                var message = await dm.SendMessageAsync(
                    embed: embed,
                    components: components ?? new ComponentBuilder().Build());

                // Store DM reference so TrailService can edit it as events resolve
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
        // Quick balance check
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
        // Tracker's Camp token shop — paginated by category
        // =========================
        [SlashCommand("shop", "Browse the Tracker's Camp shop")]
        public async Task OpenShop()
        {
            await DeferAsync(ephemeral: true);
            await ShowShop("main", 0);
        }

        // =========================
        // DECISION BUTTON HANDLERS
        // All trail decisions route through HandleDecisionAsync
        // =========================

        [ComponentInteraction("trail_decision_ambush")]
        public async Task DecisionAmbush()
            => await HandleDecision("ambush");

        [ComponentInteraction("trail_decision_pass")]
        public async Task DecisionPass()
            => await HandleDecision("pass");

        [ComponentInteraction("trail_decision_pressluck")]
        public async Task DecisionPressLuck()
            => await HandleDecision("pressluck");

        [ComponentInteraction("trail_decision_takesafe")]
        public async Task DecisionTakeSafe()
            => await HandleDecision("takesafe");

        [ComponentInteraction("trail_decision_investigate")]
        public async Task DecisionInvestigate()
            => await HandleDecision("investigate");

        [ComponentInteraction("trail_decision_moveon")]
        public async Task DecisionMoveOn()
            => await HandleDecision("moveon");

        [ComponentInteraction("trail_decision_approach")]
        public async Task DecisionApproach()
            => await HandleDecision("approach");

        [ComponentInteraction("trail_decision_retreat")]
        public async Task DecisionRetreat()
            => await HandleDecision("retreat");

        // =========================
        // SHOP NAV BUTTONS
        // =========================

        [ComponentInteraction("trail_shop_*_*")]
        public async Task ShopNav(string category, int page)
        {
            if (Context.Interaction is not SocketMessageComponent component) return;
            await component.DeferAsync();
            await ShowShop(category, page);
        }

        [ComponentInteraction("trail_buy_*_*_*")]
        public async Task ShopBuy(string itemId, string category, int page)
        {
            if (Context.Interaction is not SocketMessageComponent component) return;
            await component.DeferAsync();
            await ProcessPurchase(itemId, category, page);
        }

        // =========================
        // DECISION HANDLER
        // Defers the button, delegates to TrailService, updates DM
        // =========================
        private async Task HandleDecision(string choice)
        {
            if (Context.Interaction is not SocketMessageComponent component) return;

            // Acknowledge the button immediately
            await component.DeferAsync();

            if (!_trailService.HasActiveTrail(Context.User.Id))
            {
                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "❌ No active trail found.";
                    msg.Components = new ComponentBuilder().Build();
                });
                return;
            }

            await _trailService.HandleDecisionAsync(Context.User.Id, choice);
        }

        // =========================
        // SHOW SHOP
        // Tracker's Camp — sorted by category then tier
        // Categories: gear | craft | rare | snack
        // =========================
        private async Task ShowShop(string category, int page)
        {
            // Resolve player token balance
            // We pass through the service scope via DI — TrailService handles DB access
            var state = _trailService.GetActiveTrail(Context.User.Id);

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

            // Back to main button on all sub-pages
            if (category != "main")
                components.WithButton("← Back", "trail_shop_main_0", ButtonStyle.Secondary, row: 4);

            if (Context.Interaction is SocketMessageComponent comp)
            {
                await comp.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embed = embed.Build();
                    msg.Components = components.Build();
                });
            }
            else
            {
                await FollowupAsync(embed: embed.Build(), components: components.Build(), ephemeral: true);
            }
        }

        // =========================
        // GEAR SHOP
        // 9 Hunter's gear pieces sorted by slot value
        // =========================
        private void BuildGearShop(EmbedBuilder embed, ComponentBuilder components, int page)
        {
            embed.WithTitle("🏕️ Tracker's Camp — ⚔️ Hunter's Gear");

            var items = new[]
            {
                ("hunter_vest",      "Hunter's Vest",      130),
                ("hunter_bow",       "Hunter's Bow",       130),
                ("hunter_helm",      "Hunter's Helm",      110),
                ("hunter_leggings",  "Hunter's Leggings",  110),
                ("hunter_quiver",    "Hunter's Quiver",    110),
                ("hunter_boots",     "Hunter's Boots",      80),
                ("hunter_gloves",    "Hunter's Gloves",     80),
                ("hunter_ring",      "Hunter's Ring",       80),
                ("hunter_amulet",    "Hunter's Amulet",     80),
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

            // Buy buttons — one per item on this page
            for (int i = 0; i < pageItems.Count; i++)
            {
                var (id, name, cost) = pageItems[i];
                components.WithButton(
                    $"Buy {name} ({cost}🪙)",
                    $"trail_buy_{id}_gear_{page}",
                    ButtonStyle.Success,
                    row: i);
            }

            // Pagination
            if (totalPages > 1)
            {
                var navRow = new ActionRowBuilder();
                if (page > 0)
                    navRow.WithButton("⬅️", $"trail_shop_gear_{page - 1}", ButtonStyle.Secondary);
                if (page < totalPages - 1)
                    navRow.WithButton("➡️", $"trail_shop_gear_{page + 1}", ButtonStyle.Secondary);
                components.AddRow(navRow);
            }
        }

        // =========================
        // CRAFT SHOP
        // All craft materials in bundles, sorted by tier
        // =========================
        private void BuildCraftShop(EmbedBuilder embed, ComponentBuilder components, int page)
        {
            embed.WithTitle("🏕️ Tracker's Camp — 🧱 Craft Materials");

            // All craft materials grouped by tier, bundle sizes per pricing
            var items = new[]
            {
                // T1 — 100x for 5 tokens
                ("fur",           "Fur",            100, 5),
                ("leather",       "Leather",        100, 5),
                ("bone",          "Bone",           100, 5),
                ("feather",       "Feather",        100, 5),
                ("claws",         "Claws",          100, 5),
                ("monster_blood", "Monster Blood",  100, 5),
                // T2
                ("hide",        "Hide",       100, 5),
                ("fang",        "Fang",       100, 5),
                ("talon",       "Talon",      100, 5),
                ("horn",        "Horn",       100, 5),
                ("sharp_claw",  "Sharp Claw", 100, 5),
                // T3
                ("saber_fang",      "Saber Fang",      100, 5),
                ("griffin_feather", "Griffin Feather", 100, 5),
                ("giant_antler",    "Giant Antler",    100, 5),
                ("thick_hide",      "Thick Hide",      100, 5),
                ("dark_feather",    "Dark Feather",    100, 5),
                // T4
                ("storm_feather", "Storm Feather", 100, 5),
                ("ancient_hide",  "Ancient Hide",  100, 5),
                ("shadow_claw",   "Shadow Claw",   100, 5),
                ("titan_antler",  "Titan Antler",  100, 5),
                ("void_feather",  "Void Feather",  100, 5),
                // T5
                ("mythic_hide",      "Mythic Hide",      100, 5),
                ("sky_talon",        "Sky Talon",        100, 5),
                ("abyss_claw",       "Abyss Claw",       100, 5),
                ("colossus_antler",  "Colossus Antler",  100, 5),
                ("death_feather",    "Death Feather",    100, 5),
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
                components.WithButton(
                    $"Buy {qty}x {name} ({cost}🪙)",
                    $"trail_buy_{id}_craft_{page}",
                    ButtonStyle.Success,
                    row: i);
            }

            if (totalPages > 1)
            {
                var navRow = new ActionRowBuilder();
                if (page > 0)
                    navRow.WithButton("⬅️", $"trail_shop_craft_{page - 1}", ButtonStyle.Secondary);
                if (page < totalPages - 1)
                    navRow.WithButton("➡️", $"trail_shop_craft_{page + 1}", ButtonStyle.Secondary);
                components.AddRow(navRow);
            }
        }

        // =========================
        // RARE SHOP
        // All rare materials — 5x for 10 tokens
        // =========================
        private void BuildRareShop(EmbedBuilder embed, ComponentBuilder components, int page)
        {
            embed.WithTitle("🏕️ Tracker's Camp — ★ Rare Materials");

            var items = new[]
            {
                ("wolf_trophy",    "Wolf Trophy"),
                ("boar_tusk",      "Boar Tusk"),
                ("stag_antler",    "Stag Antler"),
                ("ancient_feather","Ancient Feather"),
                ("bear_heart",     "Bear Heart"),
                ("alpha_fang",     "Alpha Fang"),
                ("storm_talon",    "Storm Talon"),
                ("saber_relic",    "Saber Relic"),
                ("griffin_core",   "Griffin Core"),
                ("storm_relic",    "Storm Relic"),
                ("ancient_core",   "Ancient Core"),
                ("mythic_heart",   "Mythic Heart"),
                ("sky_relic",      "Sky Relic"),
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
                components.WithButton(
                    $"Buy 5x {name} (10🪙)",
                    $"trail_buy_{id}_rare_{page}",
                    ButtonStyle.Success,
                    row: i);
            }

            if (totalPages > 1)
            {
                var navRow = new ActionRowBuilder();
                if (page > 0)
                    navRow.WithButton("⬅️", $"trail_shop_rare_{page - 1}", ButtonStyle.Secondary);
                if (page < totalPages - 1)
                    navRow.WithButton("➡️", $"trail_shop_rare_{page + 1}", ButtonStyle.Secondary);
                components.AddRow(navRow);
            }
        }

        // =========================
        // SNACK SHOP
        // Trail pet snack — cheaper than gold shop
        // =========================
        private void BuildSnackShop(EmbedBuilder embed, ComponentBuilder components)
        {
            embed.WithTitle("🏕️ Tracker's Camp — 🐾 Pet Snacks");
            embed.WithDescription(
                "🐾 **Trail Pet Snack** — +25 XP to your equipped pet\n" +
                "**Cost: 8 tokens**\n\n" +
                "*The gold shop version gives +50 XP — this is the trail flavour.*");

            components.WithButton(
                "Buy Trail Pet Snack (8🪙)",
                "trail_buy_trail_pet_snack_snack_0",
                ButtonStyle.Success,
                row: 0);
        }

        // =========================
        // PROCESS PURCHASE
        // Resolves the buy button — deducts tokens, gives items
        // =========================
        private async Task ProcessPurchase(string itemId, string category, int page)
        {
            var result = await _trailService.PurchaseAsync(Context.User.Id, itemId, category);

            if (Context.Interaction is SocketMessageComponent component)
            {
                // Show purchase result briefly then refresh the shop page
                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = result;
                    msg.Embed = null;
                    msg.Components = new ComponentBuilder()
                        .WithButton("← Back to Shop", $"trail_shop_{category}_{page}", ButtonStyle.Secondary)
                        .Build();
                });
            }
        }
    }
}
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Services.InventoryServices;
using System.Text;

public class InventoryModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InventoryService _inventoryService;

    private const int ItemsPerPage = 8;

    private readonly Dictionary<int, string> _tierNames = new()
    {
        { 1, "🟤 Tier 1 — Hunter" },
        { 2, "⚪ Tier 2 — Raider" },
        { 3, "🟢 Tier 3 — Warlord" },
        { 4, "🔵 Tier 4 — Champion" },
        { 5, "🟣 Tier 5 — Mythic" }
    };

    public InventoryModule(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    // =========================
    // INVENTORY ROOT
    // =========================
    [SlashCommand("inventory", "View your inventory")]
    public async Task Inventory()
    {
        await DeferAsync(ephemeral: true);
        await ShowInventory("Equipment", null, 0);
    }

    // =========================
    // TAB BUTTONS
    // =========================
    [ComponentInteraction("inv_tab_*_*_*")]
    public async Task SwitchTab(string category, string sub, int page)
    {
        // Sub comes in as the raw value e.g. "Craft", "Rare", "Alchemy", "none"
        string? subCategory = sub == "none"
            ? (category == "Material" ? "Craft" : null)
            : sub;

        await ShowInventory(category, subCategory, 0);
    }

    // =========================
    // NAV BUTTONS
    // =========================
    [ComponentInteraction("inv_prev_*_*_*")]
    public async Task PrevPage(string category, string sub, int page)
    {
        // Sub comes in lowercase from pagination buttons e.g. "craft", "rare", "alchemy", "none"
        string? subCategory = sub == "none" ? null : char.ToUpper(sub[0]) + sub.Substring(1);
        await ShowInventory(category, subCategory, page - 1);
    }

    [ComponentInteraction("inv_next_*_*_*")]
    public async Task NextPage(string category, string sub, int page)
    {
        string? subCategory = sub == "none" ? null : char.ToUpper(sub[0]) + sub.Substring(1);
        await ShowInventory(category, subCategory, page + 1);
    }

    // =========================
    // MAIN RENDER
    // =========================
    private async Task ShowInventory(string category, string? subCategory, int page)
    {
        var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);

        var allItems = inventory
            .Where(i => i.Quantity > 0)
            .Where(i => InventoryItemDefinitions.All.ContainsKey(i.ItemId))
            .Select(i => new
            {
                Item = InventoryItemDefinitions.All[i.ItemId],
                Amount = i.Quantity
            })
            .Where(i => i.Item.Type == category)
            .ToList();

        var filtered = allItems
            .Where(i => subCategory == null || i.Item.SubCategory == subCategory)
            .OrderBy(i => i.Item.Tier ?? 99)
            .ThenBy(i => i.Item.Name)
            .ToList();

        string title = (category, subCategory) switch
        {
            ("Equipment", _) => "⚔️ Gear",
            ("Potion", _) => "🧪 Potions",
            ("Material", "Craft") => "🧱 Craft Materials",
            ("Material", "Rare") => "★ Rare Materials",
            ("Material", "Alchemy") => "⚗️ Alchemy Materials",
            ("Material", _) => "🧱 Materials",
            _ => "📦 Other"
        };

        // =========================
        // EMPTY STATE
        // =========================
        if (filtered.Count == 0)
        {
            var emptyEmbed = new EmbedBuilder()
                .WithColor(Color.DarkGrey)
                .WithTitle(title)
                .WithDescription("Nothing here.")
                .Build();

            await UpdateOrFollowup(emptyEmbed, BuildComponents(category, subCategory, 0, 0));
            return;
        }

        // =========================
        // PAGINATION
        // =========================
        int totalPages = (int)Math.Ceiling(filtered.Count / (double)ItemsPerPage);
        page = Math.Clamp(page, 0, totalPages - 1);

        var pageItems = filtered
            .Skip(page * ItemsPerPage)
            .Take(ItemsPerPage);

        var sb = new StringBuilder();
        int? lastTier = null;

        foreach (var entry in pageItems)
        {
            if (subCategory == "Craft" && entry.Item.Tier.HasValue && entry.Item.Tier != lastTier)
            {
                lastTier = entry.Item.Tier;
                var tierLabel = _tierNames.TryGetValue(entry.Item.Tier.Value, out var tn)
                    ? tn
                    : $"Tier {entry.Item.Tier}";
                sb.AppendLine($"\n**{tierLabel}**");
            }

            sb.AppendLine($"{entry.Item.Icon} **{entry.Item.Name}** x{entry.Amount}");
        }

        var color = subCategory switch
        {
            "Rare" => Color.Gold,
            "Alchemy" => Color.Purple,
            _ => Color.DarkBlue
        };

        var embed = new EmbedBuilder()
            .WithColor(color)
            .WithTitle(title)
            .WithDescription(sb.ToString().Trim())
            .WithFooter($"Page {page + 1}/{Math.Max(1, totalPages)}")
            .Build();

        await UpdateOrFollowup(embed, BuildComponents(category, subCategory, page, totalPages));
    }

    // =========================
    // UPDATE / FOLLOWUP HANDLER
    // =========================
    private async Task UpdateOrFollowup(Embed embed, MessageComponent components)
    {
        if (Context.Interaction is SocketMessageComponent interaction)
        {
            await interaction.UpdateAsync(msg =>
            {
                msg.Content = null;
                msg.Embed = embed;
                msg.Components = components;
            });
        }
        else
        {
            await FollowupAsync(embed: embed, components: components, ephemeral: true);
        }
    }

    // =========================
    // COMPONENT BUILDER
    // =========================
    private MessageComponent BuildComponents(string category, string? subCategory, int page, int totalPages)
    {
        var builder = new ComponentBuilder();

        // Pagination uses lowercase sub to keep it a single word with no underscores
        // e.g. "craft", "rare", "alchemy", "none"
        // Tab buttons use the original cased value e.g. "Craft", "Rare", "Alchemy"
        string paginationSub = subCategory == null ? "none" : subCategory.ToLower();

        // Row 0 — Main category tabs
        builder.WithButton("⚔️ Gear", "inv_tab_Equipment_none_0", ButtonStyle.Primary, disabled: category == "Equipment", row: 0);
        builder.WithButton("🧪 Potions", "inv_tab_Potion_none_0", ButtonStyle.Primary, disabled: category == "Potion", row: 0);
        builder.WithButton("🧱 Materials", "inv_tab_Material_none_0", ButtonStyle.Primary, disabled: category == "Material", row: 0);

        // Row 1 — Material subcategory tabs
        // These use the original _ format and map directly to handler wildcards
        if (category == "Material")
        {
            builder.WithButton("🧱 Craft", "inv_tab_Material_Craft_0", ButtonStyle.Secondary, disabled: subCategory == "Craft", row: 1);
            builder.WithButton("★ Rare", "inv_tab_Material_Rare_0", ButtonStyle.Secondary, disabled: subCategory == "Rare", row: 1);
            builder.WithButton("⚗️ Alchemy", "inv_tab_Material_Alchemy_0", ButtonStyle.Secondary, disabled: subCategory == "Alchemy", row: 1);
        }

        // Row 2 — Pagination
        builder.WithButton("⬅️", $"inv_prev_{category}_{paginationSub}_{page}", ButtonStyle.Secondary, disabled: page == 0, row: 2);
        builder.WithButton($"Page {page + 1}/{Math.Max(1, totalPages)}", "inv_page", ButtonStyle.Secondary, disabled: true, row: 2);
        builder.WithButton("➡️", $"inv_next_{category}_{paginationSub}_{page}", ButtonStyle.Secondary, disabled: page >= totalPages - 1, row: 2);

        return builder.Build();
    }
}
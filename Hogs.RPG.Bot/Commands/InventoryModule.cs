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
        await ShowInventory("Equipment", 0);
    }

    // =========================
    // TAB BUTTONS
    // =========================
    [ComponentInteraction("inv_tab_*_*")]
    public async Task SwitchTab(string category, int page)
    {
        await ShowInventory(category, 0);
    }

    // =========================
    // NAV BUTTONS
    // =========================
    [ComponentInteraction("inv_prev_*_*")]
    public async Task PrevPage(string category, int page)
    {
        await ShowInventory(category, page - 1);
    }

    [ComponentInteraction("inv_next_*_*")]
    public async Task NextPage(string category, int page)
    {
        await ShowInventory(category, page + 1);
    }

    // =========================
    // MAIN RENDER
    // =========================
    private async Task ShowInventory(string category, int page)
    {
        var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);

        var filtered = inventory
            .Where(i => i.Quantity > 0)
            .Select(i => new
            {
                Item = InventoryItemDefinitions.All[i.ItemId],
                Amount = i.Quantity
            })
            .Where(i => i.Item.Type == category)
            .OrderBy(i => i.Item.Name)
            .ToList();

        // =========================
        // EMPTY STATE
        // =========================
        if (filtered.Count == 0)
        {
            var emptyComponents = BuildComponents(category, 0, 0);

            var emptyEmbed = new EmbedBuilder()
                .WithColor(Color.DarkGrey)
                .WithTitle(category switch
                {
                    "Material" => "🧱 Materials",
                    "Potion" => "🧪 Potions",
                    "Equipment" => "⚔️ Gear",
                    _ => "📦 Other"
                })
                .WithDescription("Nothing here.")
                .Build();

            await UpdateOrFollowup(emptyEmbed, emptyComponents);
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

        foreach (var item in pageItems)
        {
            sb.AppendLine($"{item.Item.Icon} **{item.Item.Name}** x{item.Amount}");
        }

        var inventoryEmbed = new EmbedBuilder()
            .WithColor(Color.DarkBlue)
            .WithTitle(category switch
            {
                "Material" => "🧱 Materials",
                "Potion" => "🧪 Potions",
                "Equipment" => "⚔️ Gear",
                _ => "📦 Other"
            })
            .WithDescription(sb.ToString())
            .WithFooter($"Page {page + 1}/{Math.Max(1, totalPages)}")
            .Build();

        var components = BuildComponents(category, page, totalPages);

        await UpdateOrFollowup(inventoryEmbed, components);
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
            await FollowupAsync(
                embed: embed,
                components: components,
                ephemeral: true
            );
        }
    }

    // =========================
    // COMPONENT BUILDER
    // =========================
    private MessageComponent BuildComponents(string category, int page, int totalPages)
    {
        var builder = new ComponentBuilder();

        // 🔹 Tabs
        builder.WithButton("⚔️ Gear", $"inv_tab_Equipment_0", ButtonStyle.Primary, disabled: category == "Equipment");
        builder.WithButton("🧪 Potions", $"inv_tab_Potion_0", ButtonStyle.Primary, disabled: category == "Potion");
        builder.WithButton("🧱 Materials", $"inv_tab_Material_0", ButtonStyle.Primary, disabled: category == "Material");
        builder.WithButton("📦 Other", $"inv_tab_Other_0", ButtonStyle.Primary, disabled: category == "Other");

        // 🔹 Navigation
        builder.WithButton("⬅️", $"inv_prev_{category}_{page}", ButtonStyle.Secondary, disabled: page == 0);
        builder.WithButton($"Page {page + 1}/{Math.Max(1, totalPages)}", "inv_page", ButtonStyle.Secondary, disabled: true);
        builder.WithButton("➡️", $"inv_next_{category}_{page}", ButtonStyle.Secondary, disabled: page >= totalPages - 1);

        return builder.Build();
    }
}
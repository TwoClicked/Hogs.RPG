using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;

public class SmithCraftAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var playerRepo = services.GetService(typeof(PlayerRepository)) as PlayerRepository;
        var inventoryService = services.GetService(typeof(InventoryService)) as InventoryService;

        if (playerRepo == null || inventoryService == null)
            return AutocompletionResult.FromSuccess();

        var player = await AutocompleteCache<Player>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => playerRepo.GetByDiscordIdAsync(context.User.Id)
        );

        if (player == null)
            return AutocompletionResult.FromSuccess();

        var inventory = await AutocompleteCache<List<InventoryItem>>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => inventoryService.GetInventoryAsync(context.User.Id)
        );

        var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);
        var input = interaction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var results = SmithingItemRegistry.All.Values
            .Where(i => player.SmithingLevel >= i.RequiredSmithingLevel)
            .Where(i => string.IsNullOrEmpty(input) || i.Name.ToLower().Contains(input))
            .Select(i =>
            {
                // Calculate how many the player can forge from current bars/materials
                int canForge = int.MaxValue;
                foreach (var (matId, needed) in i.BarRequirements)
                {
                    invLookup.TryGetValue(matId, out int owned);
                    canForge = Math.Min(canForge, owned / needed);
                }
                if (canForge == int.MaxValue) canForge = 0;

                string label = canForge > 0
                    ? $"{i.Icon} {i.Name} — can forge {canForge} · {i.NpcGoldPrice}g each (Lv {i.RequiredSmithingLevel})"
                    : $"{i.Icon} {i.Name} — not enough materials (Lv {i.RequiredSmithingLevel})";

                return (i, canForge, label);
            })
            .OrderByDescending(x => x.canForge > 0 ? x.i.RequiredSmithingLevel : -1)
            .Take(25)
            .Select(x => new AutocompleteResult(x.label, x.i.Id));

        return AutocompletionResult.FromSuccess(results);
    }
}
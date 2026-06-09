using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;

public class MarketListItemAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var inventoryService = services.GetService(typeof(InventoryService)) as InventoryService;
        if (inventoryService == null)
            return AutocompletionResult.FromSuccess();

        var inventory = await AutocompleteCache<List<InventoryItem>>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => inventoryService.GetInventoryAsync(context.User.Id)
        );

        var input = interaction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        // Only show items that exist in InventoryItemDefinitions and have quantity > 0
        var results = inventory
            .Where(i => i.Quantity > 0)
            .Where(i => InventoryItemDefinitions.All.ContainsKey(i.ItemId))
            .Select(i =>
            {
                InventoryItemDefinitions.All.TryGetValue(i.ItemId, out var def);
                return (i, def);
            })
            .Where(x => string.IsNullOrEmpty(input) ||
                        x.def?.Name.ToLower().Contains(input) == true ||
                        x.i.ItemId.Contains(input))
            .OrderBy(x => x.def?.Name)
            .Take(25)
            .Select(x =>
            {
                var label = $"{x.def?.Icon ?? "📦"} {x.def?.Name ?? x.i.ItemId} × {x.i.Quantity}";
                return new AutocompleteResult(label, x.i.ItemId);
            });

        return AutocompletionResult.FromSuccess(results);
    }
}
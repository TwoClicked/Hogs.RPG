using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;

public class AlchemyDrinkAutocompleteHandler : AutocompleteHandler
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

        var results = inventory
            .Where(i => i.Quantity > 0)
            .Where(i => AlchemyPotionRegistry.All.ContainsKey(i.ItemId))
            .Select(i =>
            {
                AlchemyPotionRegistry.All.TryGetValue(i.ItemId, out var potion);
                var label = $"{potion?.Icon ?? "🧪"} {potion?.Name ?? i.ItemId} × {i.Quantity} — {potion?.Description ?? ""}";
                return (label, i.ItemId);
            })
            .Where(x => string.IsNullOrEmpty(input) || x.label.ToLower().Contains(input))
            .Take(25)
            .Select(x => new AutocompleteResult(x.label, x.ItemId));

        return AutocompletionResult.FromSuccess(results);
    }
}
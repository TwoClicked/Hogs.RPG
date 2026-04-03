using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Core.GameData.InventoryItems;
using System;
using System.Linq;
using System.Threading.Tasks;

public class TradeItemAutocompleteHandler : AutocompleteHandler
{
    private readonly InventoryService _inventoryService;

    public TradeItemAutocompleteHandler(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var inventory = await _inventoryService.GetInventoryAsync(context.User.Id);

        var results = inventory
            .Where(x => x.Quantity > 0)
            .Take(25)
            .Select(item =>
            {
                var name = InventoryItemDefinitions.All.TryGetValue(item.ItemId, out var def)
                    ? def.Name
                    : item.ItemId;

                return new AutocompleteResult(
                    $"{name} (Max: {item.Quantity})",
                    item.ItemId
                );
            });

        return AutocompletionResult.FromSuccess(results);
    }
}
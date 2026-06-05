using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Raids;
using Hogs.RPG.Services.InventoryServices;

namespace Hogs.RPG.Bot.Autocomplete
{
    public class ConvertMaterialAutocompleteHandler : AutocompleteHandler
    {
        private readonly InventoryService _inventoryService;

        public ConvertMaterialAutocompleteHandler(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction interaction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            var inventory = await AutocompleteCache<List<InventoryItem>>.GetOrCreateAsync(
                context.User.Id,
                TimeSpan.FromSeconds(15),
                () => _inventoryService.GetInventoryAsync(context.User.Id)
            );

            var input = interaction.Data.Current.Value?.ToString()?.ToLower() ?? "";

            var qtyLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            var results = MaterialConversionData.MaterialToKeyTier.Keys
                .Select(matId =>
                {
                    qtyLookup.TryGetValue(matId, out int qty);
                    InventoryItemDefinitions.All.TryGetValue(matId, out var def);
                    string name = def?.Name ?? matId;
                    int tier = MaterialConversionData.MaterialToKeyTier[matId];
                    int maxKeys = qty / MaterialConversionData.MaterialsPerKey;
                    return (matId, name, qty, tier, maxKeys);
                })
                .Where(x => x.qty > 0)
                .Where(x => string.IsNullOrEmpty(input) || x.name.ToLower().Contains(input))
                .OrderByDescending(x => x.qty)
                .Take(25)
                .Select(x => new AutocompleteResult(
                    $"T{x.tier} · {x.name} — {x.qty:N0} held ({x.maxKeys} key{(x.maxKeys == 1 ? "" : "s")} possible)",
                    x.matId));

            return AutocompletionResult.FromSuccess(results);
        }
    }
}
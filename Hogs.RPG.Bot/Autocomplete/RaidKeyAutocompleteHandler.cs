using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Services.InventoryServices;

public class RaidKeyAutocompleteHandler : AutocompleteHandler
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

        var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);
        var input = interaction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var raidKeyIds = new[]
        {
            "raid_key_t1", "raid_key_t2", "raid_key_t3",
            "raid_key_t4", "raid_key_t5"
        };

        var results = raidKeyIds
            .Where(id => RecipeRegistry.All.ContainsKey(id))
            .Select(id =>
            {
                var recipe = RecipeRegistry.All[id];
                InventoryItemDefinitions.All.TryGetValue(id, out var keyDef);

                int canCraft = int.MaxValue;
                foreach (var (matId, needed) in recipe.Materials)
                {
                    invLookup.TryGetValue(matId, out int owned);
                    canCraft = Math.Min(canCraft, owned / needed);
                }
                if (canCraft == int.MaxValue) canCraft = 0;

                var name = keyDef?.Name ?? id;
                var label = canCraft > 0
                    ? $"🗝️ {name} — can craft {canCraft}"
                    : $"🗝️ {name} — not enough materials";

                return new AutocompleteResult(label, id);
            })
            .Where(r => string.IsNullOrEmpty(input) || r.Name.ToLower().Contains(input))
            .Take(25);

        return AutocompletionResult.FromSuccess(results);
    }
}
using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Core.GameData.Registries;

public class CraftAutocompleteHandler : AutocompleteHandler
{
    private static readonly Dictionary<int, string> TierLabels = new()
    {
        { 1, "T1" },
        { 2, "T2" },
        { 3, "T3" },
        { 4, "T4" },
        { 5, "T5" },
    };

    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var value = autocompleteInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var results = RecipeRegistry.All.Values
            .Where(r => r.Name.ToLower().Contains(value))
            .Take(25)
            .Select(r =>
            {
                // Derive tier from the highest tier material in the recipe
                int? tier = r.Materials.Keys
                    .Select(id => InventoryItemDefinitions.All.TryGetValue(id, out var def) ? def.Tier : null)
                    .Where(t => t.HasValue)
                    .Select(t => t!.Value)
                    .DefaultIfEmpty(0)
                    .Max() is int t and > 0 ? t : (int?)null;

                string tierLabel = tier.HasValue && TierLabels.TryGetValue(tier.Value, out var label)
                    ? $"[{label}] "
                    : "";

                return new AutocompleteResult($"{tierLabel}{r.Name}", r.Id);
            })
            .ToList();

        return Task.FromResult(AutocompletionResult.FromSuccess(results));
    }
}
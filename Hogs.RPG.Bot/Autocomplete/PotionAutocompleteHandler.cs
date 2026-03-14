using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.GameData.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PotionAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var value = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

        var potions = RecipeRegistry.All.Values
            .Where(r => r.ResultItem.Contains("potion"))
            .Where(r => r.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
            .Take(25)
            .Select(r => new AutocompleteResult(r.Name, r.Id))
            .ToList();

        return await Task.FromResult(AutocompletionResult.FromSuccess(potions));
    }
}
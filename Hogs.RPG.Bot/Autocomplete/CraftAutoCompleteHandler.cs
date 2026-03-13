using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.GameData.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CraftAutocompleteHandler : AutocompleteHandler
{
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
            .Select(r => new AutocompleteResult(r.Name, r.Id))
            .ToList();

        return Task.FromResult(AutocompletionResult.FromSuccess(results));
    }
}
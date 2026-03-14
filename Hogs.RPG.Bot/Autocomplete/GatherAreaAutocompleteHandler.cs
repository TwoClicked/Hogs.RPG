using Discord;
using Discord.Interactions;
using Hogs.RPG.GameData.Gathering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GatherAreaAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var value = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

        var results = GatherAreaRegistry.All
            .Where(a => a.Key.Contains(value, StringComparison.OrdinalIgnoreCase))
            .Take(25)
            .Select(a => new AutocompleteResult(a.Value.Name, a.Key));

        return await Task.FromResult(AutocompletionResult.FromSuccess(results));
    }
}
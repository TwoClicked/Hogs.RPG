using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CraftAmountAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var results = new List<AutocompleteResult>
        {
            new AutocompleteResult("1", "1"),
            new AutocompleteResult("10", "10"),
            new AutocompleteResult("25", "25"),
            new AutocompleteResult("50", "50"),
            new AutocompleteResult("Max", "max")
        };

        return await Task.FromResult(AutocompletionResult.FromSuccess(results));
    }
}
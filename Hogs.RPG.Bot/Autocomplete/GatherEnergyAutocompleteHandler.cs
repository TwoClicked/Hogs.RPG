using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GatherEnergyAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var input = interaction.Data.Current.Value?.ToString() ?? "";

        var options = new List<string>
        {
            "10",
            "25",
            "50",
            "100",
            "250",
            "Max"
        };

        var results = options
            .Where(o => o.Contains(input, StringComparison.OrdinalIgnoreCase))
            .Take(25)
            .Select(o => new AutocompleteResult(o, o))
            .ToList();

        return Task.FromResult(AutocompletionResult.FromSuccess(results));
    }
}
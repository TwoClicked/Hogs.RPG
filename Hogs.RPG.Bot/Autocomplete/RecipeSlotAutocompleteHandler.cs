using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RecipeSlotAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var input = autocompleteInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var slots = EquipmentRegistry.All
            .Values
            .Select(x => x.Slot.ToString())
            .Distinct()
            .ToList();

        var results = slots
            .Where(s => s.ToLower().Contains(input))
            .Take(25)
            .Select(s => new AutocompleteResult(s, s));

        return Task.FromResult(AutocompletionResult.FromSuccess(results));
    }
}
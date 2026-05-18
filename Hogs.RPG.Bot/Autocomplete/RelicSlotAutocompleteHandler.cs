using Discord;
using Discord.Interactions;

public class RelicSlotAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var results = new List<AutocompleteResult>
        {
            new AutocompleteResult("Slot 1", "1"),
            new AutocompleteResult("Slot 2", "2")
        };

        return Task.FromResult(AutocompletionResult.FromSuccess(results));
    }
}
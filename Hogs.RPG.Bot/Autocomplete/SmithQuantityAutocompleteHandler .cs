using Discord;
using Discord.Interactions;

public class SmithQuantityAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var results = new List<AutocompleteResult>
        {
            new("1",   "1"),
            new("5",   "5"),
            new("10",  "10"),
            new("25",  "25"),
            new("50",  "50"),
            new("Max", "max")
        };

        return Task.FromResult(AutocompletionResult.FromSuccess(results));
    }
}
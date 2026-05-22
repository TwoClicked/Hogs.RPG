// Hogs.RPG.Bot/Autocomplete/GearSetAutocompleteHandler.cs

using Discord;
using Discord.Interactions;
using Hogs.RPG.Data.Repositories;

public class GearSetAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var repo = services.GetService(typeof(GearSetRepository)) as GearSetRepository;

        var sets = await repo.GetSetsAsync(context.User.Id);

        if (sets == null || sets.Count == 0)
            return AutocompletionResult.FromSuccess(Enumerable.Empty<AutocompleteResult>());

        var results = sets
            .OrderBy(s => s.SetIndex)
            .Select(s => new AutocompleteResult($"Set {s.SetIndex} — {s.SetName}", s.SetIndex));

        return AutocompletionResult.FromSuccess(results);
    }
}
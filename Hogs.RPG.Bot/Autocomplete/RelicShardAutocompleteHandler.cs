using Discord;
using Discord.Interactions;
using Hogs.RPG.Data.Repositories;

public class RelicShardAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var relicRepo = services.GetService(typeof(RelicRepository)) as RelicRepository;
        if (relicRepo == null)
            return AutocompletionResult.FromSuccess();

        var shards = await relicRepo.GetShardsAsync(context.User.Id);

        var results = shards
            .Where(s => s.Quantity > 0)
            .OrderBy(s => s.Tier)
            .Select(s => new AutocompleteResult(
                $"Tier {s.Tier} Shard ({s.Quantity}x)",
                s.Tier.ToString()))
            .Take(25);

        return AutocompletionResult.FromSuccess(results);
    }
}
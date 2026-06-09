using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;

public class MarketListRelicAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var relicRepo = services.GetService(typeof(RelicRepository)) as RelicRepository;
        if (relicRepo == null)
            return AutocompletionResult.FromSuccess();

        var relics = await relicRepo.GetRelicsAsync(context.User.Id);

        var input = interaction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var results = relics
            .Where(r => !r.IsEquipped && !r.IsListed)
            .Select(r =>
            {
                RelicRegistry.All.TryGetValue(r.RelicId, out var def);
                string name = def?.Name ?? r.RelicId;
                string label = $"💎 {name} — Rank {r.Rank} · {def?.Affinity} [#{r.Id}]";
                return (label, r.Id);
            })
            .Where(x => string.IsNullOrEmpty(input) || x.label.ToLower().Contains(input))
            .Take(25)
            .Select(x => new AutocompleteResult(x.label, x.Id));

        return AutocompletionResult.FromSuccess(results);
    }
}
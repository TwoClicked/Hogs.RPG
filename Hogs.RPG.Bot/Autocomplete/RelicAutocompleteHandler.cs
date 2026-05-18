using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.RelicServices;

public class RelicAutocompleteHandler : AutocompleteHandler
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

        var relics = await relicRepo.GetRelicsAsync(context.User.Id);

        var input = autocompleteInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var results = relics
            .Where(r => !r.IsEquipped)
            .Select(r =>
            {
                var def = RelicRegistry.Get(r.RelicId);
                var label = $"[ID:{r.Id}] {def.Name} (Rank {r.Rank}) — {def.Affinity}";
                return new AutocompleteResult(label, r.Id.ToString());
            })
            .Where(r => r.Name.ToLower().Contains(input))
            .Take(25);

        return AutocompletionResult.FromSuccess(results);
    }
}
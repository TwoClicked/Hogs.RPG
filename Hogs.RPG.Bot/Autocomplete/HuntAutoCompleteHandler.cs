using Discord;
using Discord.Interactions;
using Hogs.RPG.GameData.Hunts;
using Hogs.RPG.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HuntAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var playerRepo = services.GetService(typeof(PlayerRepository)) as PlayerRepository;

        if (playerRepo == null)
            return AutocompletionResult.FromSuccess();

        var player = await playerRepo.GetByDiscordIdAsync(context.User.Id);

        if (player == null)
            return AutocompletionResult.FromSuccess();

        var input = autocompleteInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var hunts = HuntTargetRegistry.All.Values
            .Where(h => player.Level >= h.RequiredLevel)
            .Where(h =>
                string.IsNullOrEmpty(input) ||
                h.Name.ToLower().Contains(input) ||
                h.Id.ToLower().Contains(input))
            .OrderBy(h => h.RequiredLevel)
            .Take(25)
            .Select(h => new AutocompleteResult($"{h.Icon} {h.Name}", h.Id))
            .ToList();

        // Fallback (if nothing matched)
        if (hunts.Count == 0)
        {
            hunts = HuntTargetRegistry.All.Values
                .Where(h => player.Level >= h.RequiredLevel)
                .OrderBy(h => h.RequiredLevel)
                .Take(5)
                .Select(h => new AutocompleteResult($"{h.Icon} {h.Name}", h.Id))
                .ToList();
        }

        return AutocompletionResult.FromSuccess(hunts);
    }
}
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

        var value = autocompleteInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var hunts = HuntTargetRegistry.All.Values
            .Where(h => player.Level >= h.RequiredLevel)
            .Where(h => h.Name.ToLower().Contains(value) || h.Id.Contains(value))
            .Take(25)
            .Select(h => new AutocompleteResult($"{h.Icon} {h.Name}", h.Id))
            .ToList();

        return AutocompletionResult.FromSuccess(hunts);
    }
}
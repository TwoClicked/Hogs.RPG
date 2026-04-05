using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
public class HuntAmountAutocompleteHandler : AutocompleteHandler
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

        var player = await AutocompleteCache<Player>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => playerRepo.GetByDiscordIdAsync(context.User.Id)
        );

        if (player == null)
            return AutocompletionResult.FromSuccess();

        var input = autocompleteInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";
        int max = player.HunterStamina;
        int maxCap = 100;
        var results = new List<AutocompleteResult>();

        results.Add(new AutocompleteResult($"🔥 Max ({max}/{maxCap})", "max"));

        var presets = new[] { 1, 5, 10, 25, 50 };
        foreach (var val in presets)
        {
            if (val > max) continue;
            int percent = (int)Math.Round((double)val / maxCap * 100);
            results.Add(new AutocompleteResult($"{val} ({percent}%)", val.ToString()));
        }

        results = results
            .Where(r => string.IsNullOrEmpty(input) || r.Name.ToLower().Contains(input))
            .Take(25)
            .ToList();

        return AutocompletionResult.FromSuccess(results);
    }
}
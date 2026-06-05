using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities.PlayerObjects;
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

        // Check if stamina boost is active
        bool staminaBoostActive = player.StaminaBoostExpiry.HasValue &&
                                  player.StaminaBoostExpiry.Value > DateTime.UtcNow;

        int maxCap = staminaBoostActive ? 150 : 100;
        int max = player.HunterStamina;

        var results = new List<AutocompleteResult>();

        string boostTag = staminaBoostActive ? " ⚡ Boosted" : "";
        results.Add(new AutocompleteResult($"🔥 Max ({max}/{maxCap}){boostTag}", "max"));

        var presets = new[] { 1, 5, 10, 25, 50, 100 };
        foreach (var val in presets)
        {
            if (val > max) continue;
            int percent = (int)Math.Round((double)val / maxCap * 100);
            results.Add(new AutocompleteResult($"{val} ({percent}%)", val.ToString()));
        }

        // Add 150 preset only if boost is active
        if (staminaBoostActive && max >= 150)
            results.Add(new AutocompleteResult($"150 (100%) ⚡", "150"));

        results = results
            .Where(r => string.IsNullOrEmpty(input) || r.Name.ToLower().Contains(input))
            .Take(25)
            .ToList();

        return AutocompletionResult.FromSuccess(results);
    }
}
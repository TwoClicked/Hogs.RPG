using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GatheringServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
public class GatherEnergyAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var playerRepo = services.GetService(typeof(PlayerRepository)) as PlayerRepository;
        var energyService = services.GetService(typeof(EnergyService)) as EnergyService;

        if (playerRepo == null || energyService == null)
            return AutocompletionResult.FromSuccess();

        var player = await AutocompleteCache<Player>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => playerRepo.GetByDiscordIdAsync(context.User.Id)
        );

        if (player == null)
        {
            return AutocompletionResult.FromSuccess(new[]
            {
                new AutocompleteResult("❌ Create a profile first with /startadventure", "0")
            });
        }

        energyService.RegenerateEnergy(player);

        var input = interaction.Data.Current.Value?.ToString() ?? "";
        int currentEnergy = player.Energy;
        int maxEnergy = energyService.GetMaxEnergy(player);
        bool boosted = maxEnergy > 100;

        var options = new List<string> { "10", "25", "50", "100", "Max" };

        var results = options
            .Where(o =>
            {
                if (o.Equals("Max", StringComparison.OrdinalIgnoreCase)) return currentEnergy > 0;
                if (int.TryParse(o, out int val)) return val <= currentEnergy;
                return false;
            })
            .Where(o => o.Contains(input, StringComparison.OrdinalIgnoreCase))
            .Take(25)
            .Select(o =>
            {
                if (o.Equals("Max", StringComparison.OrdinalIgnoreCase))
                    return new AutocompleteResult($"⚡ Max ({currentEnergy}/{maxEnergy}){(boosted ? " ⚡ Boosted" : "")}", currentEnergy.ToString());
                return new AutocompleteResult($"⚡ {o} (Energy: {currentEnergy}/{maxEnergy})", o);
            })
            .ToList();

        // Add 150 preset only if boost is active and player has enough energy
        if (boosted && currentEnergy >= 150)
            results.Add(new AutocompleteResult($"150 (100%) ⚡", "150"));

        if (results.Count == 0)
            results.Add(new AutocompleteResult($"⚡ Current Energy: {currentEnergy}/{maxEnergy}", currentEnergy.ToString()));

        return AutocompletionResult.FromSuccess(results);
    }
}
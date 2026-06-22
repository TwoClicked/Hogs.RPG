// Hogs.RPG.Bot/Autocomplete/TradeAddPetAutocompleteHandler.cs

using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Services.PetServices;
using Microsoft.Extensions.DependencyInjection;

public class TradeAddPetAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var petService = services.GetRequiredService<PetService>();

        var pets = await petService.GetPetsAsync(context.User.Id);

        if (pets == null || pets.Count == 0)
            return AutocompletionResult.FromSuccess();

        var input = interaction.Data.Current.Value?.ToString() ?? "";

        var results = pets
            .Where(p => !p.IsEquipped && !p.IsListed)
            .Where(p => PetRegistry.All.ContainsKey(p.PetId))
            .Select(p =>
            {
                PetRegistry.All.TryGetValue(p.PetId, out var def);
                string displayName = p.CustomName ?? def!.Name;
                string label = $"{def!.Icon} {displayName} (Lv. {p.Level}) — ID: {p.Id}";
                return new { DbId = p.Id, Label = label };
            })
            .Where(p => p.Label.Contains(input, StringComparison.OrdinalIgnoreCase))
            .Take(25)
            .Select(p => new AutocompleteResult(p.Label, p.DbId))
            .ToList();

        return AutocompletionResult.FromSuccess(results);
    }
}

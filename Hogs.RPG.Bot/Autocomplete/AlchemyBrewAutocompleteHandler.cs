using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;

public class AlchemyBrewAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var playerRepo = services.GetService(typeof(PlayerRepository)) as PlayerRepository;
        var inventoryService = services.GetService(typeof(InventoryService)) as InventoryService;

        if (playerRepo == null || inventoryService == null)
            return AutocompletionResult.FromSuccess();

        var player = await AutocompleteCache<Player>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => playerRepo.GetByDiscordIdAsync(context.User.Id)
        );

        if (player == null)
            return AutocompletionResult.FromSuccess();

        var inventory = await AutocompleteCache<List<InventoryItem>>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => inventoryService.GetInventoryAsync(context.User.Id)
        );

        var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);
        var input = interaction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var results = AlchemyPotionRegistry.All.Values
            .Where(p => player.AlchemistLevel >= p.RequiredAlchemistLevel)
            .Where(p => string.IsNullOrEmpty(input) || p.Name.ToLower().Contains(input))
            .Select(p =>
            {
                int canBrew = int.MaxValue;
                foreach (var (ingId, needed) in p.IngredientRequirements)
                {
                    invLookup.TryGetValue(ingId, out int owned);
                    canBrew = Math.Min(canBrew, owned / needed);
                }
                if (canBrew == int.MaxValue) canBrew = 0;

                string label = canBrew > 0
                    ? $"{p.Icon} {p.Name} — can brew {canBrew} · {p.AlchemyXpReward} XP (Lv {p.RequiredAlchemistLevel})"
                    : $"{p.Icon} {p.Name} — not enough ingredients (Lv {p.RequiredAlchemistLevel})";

                return (p, canBrew, label);
            })
            .OrderByDescending(x => x.canBrew > 0 ? x.p.RequiredAlchemistLevel : -1)
            .Take(25)
            .Select(x => new AutocompleteResult(x.label, x.p.Id));

        return AutocompletionResult.FromSuccess(results);
    }
}
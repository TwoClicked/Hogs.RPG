using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;

public class SmeltBarAutocompleteHandler : AutocompleteHandler
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

        var results = SmeltingRegistry.All.Values
            .Where(r => player.SmithingLevel >= r.RequiredSmithingLevel)
            .Where(r => string.IsNullOrEmpty(input) || r.BarName.ToLower().Contains(input))
            .Select(r =>
            {
                // Calculate how many bars the player can make from current ore
                int canMake = int.MaxValue;
                foreach (var (oreId, needed) in r.OreRequirements)
                {
                    invLookup.TryGetValue(oreId, out int owned);
                    canMake = Math.Min(canMake, owned / needed);
                }
                if (canMake == int.MaxValue) canMake = 0;

                var barIcon = InventoryItemDefinitions.All.TryGetValue(r.BarId, out var barDef)
                    ? barDef.Icon : "🔩";

                string label = canMake > 0
                    ? $"{barIcon} {r.BarName} — can make {canMake} (Lv {r.RequiredSmithingLevel})"
                    : $"{barIcon} {r.BarName} — not enough ore (Lv {r.RequiredSmithingLevel})";

                return new AutocompleteResult(label, r.BarId);
            })
            .OrderByDescending(r => r.Name.Contains("not enough") ? 0 : 1)
            .Take(25);

        return AutocompletionResult.FromSuccess(results);
    }
}
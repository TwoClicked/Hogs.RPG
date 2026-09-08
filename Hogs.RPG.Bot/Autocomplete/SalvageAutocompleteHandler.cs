using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.Salvage;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SalvageGearAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var inventoryService = services.GetService(typeof(InventoryService)) as InventoryService;
        var equipmentService = services.GetService(typeof(EquipmentService)) as EquipmentService;
        var playerRepository = services.GetService(typeof(PlayerRepository)) as PlayerRepository;

        if (inventoryService == null || equipmentService == null || playerRepository == null)
            return AutocompletionResult.FromSuccess();

        var player = await AutocompleteCache<Player>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => playerRepository.GetByDiscordIdAsync(context.User.Id)
        );

        if (player == null)
            return AutocompletionResult.FromSuccess();

        var inventory = await AutocompleteCache<List<InventoryItem>>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => inventoryService.GetInventoryAsync(context.User.Id)
        );

        var input = autocompleteInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var results = inventory
            .Where(i => i.Quantity > 0)
            .Where(i => SalvageRegistry.BlackstoneValue.ContainsKey(i.ItemId))
            .Select(i =>
            {
                var item = equipmentService.GetEquipment(i.ItemId);
                int value = SalvageRegistry.BlackstoneValue[i.ItemId];
                return (inv: i, item, value);
            })
            .Where(x => x.item != null)
            .Where(x => string.IsNullOrEmpty(input) || x.item.Name.ToLower().Contains(input))
            .OrderByDescending(x => x.value)
            .ThenBy(x => x.item.Name)
            .Select(x => new AutocompleteResult(
                $"{x.item.Name} — own {x.inv.Quantity} — {x.value} Blackstones each",
                x.inv.ItemId))
            .Take(25);

        return AutocompletionResult.FromSuccess(results);
    }
}
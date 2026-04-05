using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
public class EquipBySlotAutocompleteHandler : AutocompleteHandler
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

        var player = await AutocompleteCache<Player>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => playerRepository.GetByDiscordIdAsync(context.User.Id)
        );

        var inventory = await AutocompleteCache<List<InventoryItem>>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => inventoryService.GetInventoryAsync(context.User.Id)
        );

        var slotOption = autocompleteInteraction.Data.Options
            .FirstOrDefault(x => x.Name == "slot");
        if (slotOption == null)
            return AutocompletionResult.FromSuccess();

        var slot = slotOption.Value.ToString();

        var equipped = new HashSet<string>
        {
            player.MainHand, player.OffHand, player.Helmet, player.Body,
            player.Legs, player.Gloves, player.Boots, player.Ring, player.Amulet
        };

        var results = new List<AutocompleteResult>();
        foreach (var inv in inventory.Where(i => i.Quantity > 0))
        {
            var item = equipmentService.GetEquipment(inv.ItemId);
            if (item == null) continue;
            if (!item.Slot.ToString().Equals(slot, StringComparison.OrdinalIgnoreCase)) continue;
            if (equipped.Contains(inv.ItemId)) continue;

            var stats = new List<string>();
            if (item.Attack > 0) stats.Add($"+{item.Attack} ATK");
            if (item.Defense > 0) stats.Add($"+{item.Defense} DEF");
            if (item.Health > 0) stats.Add($"+{item.Health} HP");
            var statText = stats.Count > 0 ? $" ({string.Join(", ", stats)})" : "";

            results.Add(new AutocompleteResult($"{item.Name}{statText}", item.Id));
        }

        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
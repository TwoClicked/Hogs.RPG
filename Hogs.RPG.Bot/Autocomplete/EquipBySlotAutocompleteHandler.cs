using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Data.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EquipBySlotAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var inventoryService = (InventoryService)services.GetService(typeof(InventoryService));
        var equipmentService = (EquipmentService)services.GetService(typeof(EquipmentService));
        var playerRepository = (PlayerRepository)services.GetService(typeof(PlayerRepository));

        var userId = context.User.Id;

        var player = await playerRepository.GetByDiscordIdAsync(userId);
        var inventory = await inventoryService.GetInventoryAsync(userId);

        // 👇 Get selected slot from command input
        var slotOption = autocompleteInteraction.Data.Options
            .FirstOrDefault(x => x.Name == "slot");

        if (slotOption == null)
            return AutocompletionResult.FromSuccess();

        var slot = slotOption.Value.ToString();

        // 👇 Collect equipped items
        var equipped = new HashSet<string>
        {
            player.MainHand,
            player.OffHand,
            player.Helmet,
            player.Body,
            player.Legs,
            player.Gloves,
            player.Boots,
            player.Ring,
            player.Amulet
        };

        var results = new List<AutocompleteResult>();

        foreach (var inv in inventory.Where(i => i.Quantity > 0))
        {
            var item = equipmentService.GetEquipment(inv.ItemId);
            if (item == null) continue;

            // Filter by slot
            if (!item.Slot.ToString().Equals(slot, StringComparison.OrdinalIgnoreCase))
                continue;

            // Skip already equipped
            if (equipped.Contains(inv.ItemId))
                continue;

            // Build stat text
            var stats = new List<string>();

            if (item.Attack > 0) stats.Add($"+{item.Attack} ATK");
            if (item.Defense > 0) stats.Add($"+{item.Defense} DEF");
            if (item.Health > 0) stats.Add($"+{item.Health} HP");

            var statText = stats.Count > 0
                ? $" ({string.Join(", ", stats)})"
                : "";

            results.Add(new AutocompleteResult(
                $"{item.Name}{statText}",
                item.Id
            ));
        }

        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
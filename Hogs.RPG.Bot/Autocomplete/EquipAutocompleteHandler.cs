using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class EquipAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var inventoryService = services.GetService(typeof(InventoryService)) as InventoryService;
        var equipmentService = services.GetService(typeof(EquipmentService)) as EquipmentService;

        if (inventoryService == null || equipmentService == null)
            return AutocompletionResult.FromSuccess();

        var inventory = await inventoryService.GetInventoryAsync(context.User.Id);

        var results = new List<AutocompleteResult>();

        foreach (var item in inventory)
        {
            var equipItem = equipmentService.GetEquipment(item.ItemId);

            if (equipItem == null)
                continue;

            var icon = equipItem.Slot switch
            {
                EquipmentSlot.MainHand => "🗡",
                EquipmentSlot.OffHand => "🏹",
                EquipmentSlot.Body => "🛡",
                EquipmentSlot.Helmet => "🪖",
                EquipmentSlot.Boots => "🥾",
                EquipmentSlot.Gloves => "🧤",
                EquipmentSlot.Legs => "👖",
                EquipmentSlot.Ring => "💍",
                EquipmentSlot.Amulet => "📿",
                _ => "⚔"
            };

            results.Add(new AutocompleteResult(
                $"{icon} {equipItem.Name}",
                equipItem.Id
            ));
        }

        // Discord allows max 25 suggestions
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
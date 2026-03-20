using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EquipSlotAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var slots = new List<AutocompleteResult>
        {
            new("🗡 Main Hand", "MainHand"),
            new("🏹 Off Hand", "OffHand"),
            new("🪖 Helmet", "Helmet"),
            new("🛡 Body", "Body"),
            new("👖 Legs", "Legs"),
            new("🧤 Gloves", "Gloves"),
            new("🥾 Boots", "Boots"),
            new("💍 Ring", "Ring"),
            new("📿 Amulet", "Amulet")
        };

        return Task.FromResult(AutocompletionResult.FromSuccess(slots));
    }
}
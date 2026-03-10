using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UnequipAutocompleteHandler : AutocompleteHandler
{
    private static readonly List<AutocompleteResult> Slots = new()
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

    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        return Task.FromResult(AutocompletionResult.FromSuccess(Slots));
    }
}
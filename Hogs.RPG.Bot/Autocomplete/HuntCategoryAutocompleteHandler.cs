using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Autocomplete
{
    public class HuntCategoryAutocompleteHandler : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction interaction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            var results = new List<AutocompleteResult>
        {
            new("⚔ Normal Hunts", "Normal"),
            new("🧪 Alchemy Hunts", "Alchemy")
        };

            return Task.FromResult(AutocompletionResult.FromSuccess(results));
        }
    }
}

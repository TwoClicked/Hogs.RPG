using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;

public class PetDungeonAutocompleteHandler : AutocompleteHandler
{
    private readonly PlayerRepository _playerRepository;

    public PetDungeonAutocompleteHandler(PlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var player = await AutocompleteCache<Player>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => _playerRepository.GetByDiscordIdAsync(context.User.Id)
        );

        if (player == null)
            return AutocompletionResult.FromSuccess();

        var input = interaction.Data.Current.Value?.ToString() ?? "";

        // Each pet dungeon tells the player which pet it can drop
        var petIcons = new Dictionary<string, string>
        {
            { "blazewings_gorge",  "⚔️ Attack Pet"  },
            { "stonehall_depths",  "🛡️ Defense Pet" },
            { "drowned_archives",  "❤️ Health Pet"  },
        };

        var results = PetDungeonRegistry.All.Values
            .Where(d => d.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
            .OrderBy(d => d.RequiredLevel)
            .Take(25)
            .Select(d =>
            {
                petIcons.TryGetValue(d.Id, out var petLabel);
                petLabel ??= "🐾 Pet";

                string label = player.Level >= d.RequiredLevel
                    ? $"{d.Name} (Lv {d.RequiredLevel}) — {petLabel}"
                    : $"🔒 {d.Name} (Lv {d.RequiredLevel}) — {petLabel}";

                return new AutocompleteResult(label, d.Id);
            })
            .ToList();

        return AutocompletionResult.FromSuccess(results);
    }
}

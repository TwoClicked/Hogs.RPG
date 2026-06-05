using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
public class DungeonAutocompleteHandler : AutocompleteHandler
{
    private readonly PlayerRepository _playerRepository;
    public DungeonAutocompleteHandler(PlayerRepository playerRepository)
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

        var results = DungeonRegistry.All.Values
            .Where(d => d.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
            .OrderBy(d => d.RequiredLevel)
            .Take(25)
            .Select(d => new AutocompleteResult(
                player.Level >= d.RequiredLevel
                    ? $"{d.Name} (Lv {d.RequiredLevel})"
                    : $"🔒 {d.Name} (Lv {d.RequiredLevel})",
                d.Id))
            .ToList();

        return AutocompletionResult.FromSuccess(results);
    }
}
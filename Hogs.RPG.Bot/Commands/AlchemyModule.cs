using Discord.Interactions;
using Hogs.RPG.Services.AlchemyServices;

public class AlchemyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly AlchemyService _alchemyService;

    public AlchemyModule(AlchemyService alchemyService)
    {
        _alchemyService = alchemyService;
    }

    [SlashCommand("alchemy", "Craft potions")]
    public async Task CraftPotion(
        [Autocomplete(typeof(PotionAutocompleteHandler))]
        string potion,
        int amount = 1)
    {
        var result = await _alchemyService.CraftPotionAsync(
            Context.User.Id,
            potion,
            amount
        );

        await RespondAsync(result);
    }
}
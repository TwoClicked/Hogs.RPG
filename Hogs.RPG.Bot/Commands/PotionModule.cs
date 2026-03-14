using Discord.Interactions;
using Hogs.RPG.Services.GameplayServices;
using System.Threading.Tasks;

public class PotionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly PotionService _potionService;

    public PotionModule(PotionService potionService)
    {
        _potionService = potionService;
    }

    [SlashCommand("xppotion", "Toggle auto-use of XP potions")]
    public async Task ToggleXpPotion()
    {
        var result = await _potionService.ToggleXpPotionAsync(Context.User.Id);

        await RespondAsync(result,ephemeral:true);
    }

    [SlashCommand("potions", "View your potion inventory")]
    public async Task Potions()
    {
        var result = await _potionService.GetPotionStatusAsync(Context.User.Id);

        await RespondAsync(result, ephemeral:true);
    }
}
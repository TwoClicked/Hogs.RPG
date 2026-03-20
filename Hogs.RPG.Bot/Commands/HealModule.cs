using Discord.Interactions;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Data.Repositories;
using System.Threading.Tasks;

public class HealModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly HealService _healService;

    public HealModule(HealService healService)
    {
        _healService = healService;
    }

    [SlashCommand("heal", "Use a health potion to fully heal")]
    public async Task Heal()
    {
        await DeferAsync();

        var result = await _healService.HealAsync(Context.User.Id);

        if (!result.IsSuccess)
        {
            await FollowupAsync(result.Message);
            return;
        }

        await FollowupAsync(
            $"❤️ You healed to full! ({result.RemainingPotions} potions left.)"
        );
    }
}
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using System.Threading.Tasks;

[BossLock]
[GearSwapLock]
[TradeLock]
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
        await DeferAsync(ephemeral: true);

        var result = await _healService.HealAsync(Context.User.Id);

        if (!result.IsSuccess)
        {
            await FollowupAsync(result.Message, ephemeral: true);
            return;
        }

        await FollowupAsync(
            $"❤️ You healed to full! ({result.RemainingPotions} potions left.)",
            ephemeral: true
        );
    }
}
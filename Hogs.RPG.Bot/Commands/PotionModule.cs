using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using System.Threading.Tasks;

[BossLock]
[GearSwapLock]
[TradeLock]
public class PotionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly PotionService _potionService;
    private readonly PlayerRepository _playerRepository;

    public PotionModule(PotionService potionService, PlayerRepository playerRepository)
    {
        _potionService = potionService;
        _playerRepository = playerRepository;
    }

    private async Task<bool> EnsurePlayerAsync()
    {
        var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);

        if (player == null)
        {
            await RespondAsync("⚠️ You need to start your adventure first with `/startadventure`.", ephemeral: true);
            return false;
        }

        return true;
    }

    [SlashCommand("xppotion", "Toggle auto-use of XP potions")]
    public async Task ToggleXpPotion()
    {
        if (!await EnsurePlayerAsync()) return;

        var result = await _potionService.ToggleXpPotionAsync(Context.User.Id);

        await RespondAsync(result, ephemeral: true);
    }

    [SlashCommand("potions", "View your potion inventory")]
    public async Task Potions()
    {
        if (!await EnsurePlayerAsync()) return;

        var result = await _potionService.GetPotionStatusAsync(Context.User.Id);

        await RespondAsync(result, ephemeral: true);
    }
}
using Discord.Interactions;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Data.Repositories;
using System.Threading.Tasks;

public class HealModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InventoryService _inventoryService;
    private readonly PlayerRepository _playerRepository;

    public HealModule(
        InventoryService inventoryService,
        PlayerRepository playerRepository)
    {
        _inventoryService = inventoryService;
        _playerRepository = playerRepository;
    }

    [SlashCommand("heal", "Use a health potion to fully heal")]
    public async Task Heal()
    {
        var userId = Context.User.Id;

        var player = await _playerRepository.GetByDiscordIdAsync(userId);

        if (player == null)
        {
            await RespondAsync("You need to start your adventure first.");
            return;
        }

        // Already full HP
        if (player.Health >= player.MaxHealth)
        {
            await RespondAsync("❤️ You're already at full health.");
            return;
        }

        var inventory = await _inventoryService.GetInventoryAsync(userId);
        var potion = inventory.FirstOrDefault(i => i.ItemId == "health_potion");

        if (potion == null || potion.Quantity <= 0)
        {
            await RespondAsync("❌ You don't have any health potions.");
            return;
        }

        // Apply heal
        player.Health = player.MaxHealth;

        // Remove potion
        await _inventoryService.TakeItemAsync(userId, "health_potion", 1);

        // Save player
        await _playerRepository.UpdatePlayerAsync(player);

        var remaining = potion.Quantity - 1;

        await RespondAsync($"❤️ You healed to full! ({remaining} potions left.)");
    }
}
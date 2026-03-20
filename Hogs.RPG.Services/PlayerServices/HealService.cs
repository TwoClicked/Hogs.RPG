using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;

public class HealService
{
    private readonly InventoryService _inventoryService;
    private readonly PlayerRepository _playerRepository;

    public HealService(
        InventoryService inventoryService,
        PlayerRepository playerRepository)
    {
        _inventoryService = inventoryService;
        _playerRepository = playerRepository;
    }

    public async Task<HealResult> HealAsync(ulong userId)
    {
        var player = await _playerRepository.GetByDiscordIdAsync(userId);

        if (player == null)
            return HealResult.Fail("You need to start your adventure first.");

        if (player.Health >= player.MaxHealth)
            return HealResult.Fail("❤️ You're already at full health.");

        var inventory = await _inventoryService.GetInventoryAsync(userId);
        var potion = inventory.FirstOrDefault(i => i.ItemId == "health_potion");

        if (potion == null || potion.Quantity <= 0)
            return HealResult.Fail("❌ You don't have any health potions.");

        player.Health = player.MaxHealth;

        await _inventoryService.TakeItemAsync(userId, "health_potion", 1);
        await _playerRepository.UpdatePlayerAsync(player);

        return HealResult.Success(player.Health, player.MaxHealth, potion.Quantity - 1);
    }
}
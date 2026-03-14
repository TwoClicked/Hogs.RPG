using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.GameplayServices
{
    public class PotionService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;

        public PotionService(PlayerRepository playerRepository, InventoryService inventoryService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
        }

        public async Task<string> ToggleXpPotionAsync(ulong userId)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to start your adventure first.";

            player.AutoUseXpPotions = !player.AutoUseXpPotions;

            await _playerRepository.UpdatePlayerAsync(player);

            return player.AutoUseXpPotions
                ? "🧪 XP Potion auto-use **enabled**."
                : "🧪 XP Potion auto-use **disabled**.";
        }

        public async Task<string> GetPotionStatusAsync(ulong userId)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to start your adventure first.";

            var inventory = await _inventoryService.GetInventoryAsync(userId);

            int xpPotions = inventory
                .FirstOrDefault(i => i.ItemId == "xp_potion")?.Quantity ?? 0;

            int healthPotions = inventory
                .FirstOrDefault(i => i.ItemId == "health_potion")?.Quantity ?? 0;

            string autoXp = player.AutoUseXpPotions ? "Enabled" : "Disabled";

            return
                             $@"🧪 **Potion Inventory**
                                         
                             XP Potions: {xpPotions}
                             Health Potions: {healthPotions}
                             
                             Auto XP Potion: {autoXp}";
        }
    }
}
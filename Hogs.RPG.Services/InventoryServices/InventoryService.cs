using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.InventoryServices
{
    public class InventoryService
    {
        private readonly InventoryRepository _inventoryRepository;

        public InventoryService(InventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }
  

        public async Task<List<InventoryItem>> GetInventoryAsync(ulong discordId)
        {
            return await _inventoryRepository.GetInventoryAsync(discordId);
        }

        public async Task GiveItemAsync(ulong discordId, string itemId, int amount)
        {
            await _inventoryRepository.AddItemAsync(discordId, itemId, amount);
        }

        public async Task TakeItemAsync(ulong discordId, string itemId, int amount)
        {
            await _inventoryRepository.RemoveItemAsync(discordId, itemId, amount);
        }

        public async Task<int> GetItemAmountAsync(ulong discordId, string itemId)
        {
            var inventory = await _inventoryRepository.GetInventoryAsync(discordId);

            var item = inventory.FirstOrDefault(x => x.ItemId == itemId);

            if (item == null)
                return 0;

            return item.Quantity;
        }
    }


}

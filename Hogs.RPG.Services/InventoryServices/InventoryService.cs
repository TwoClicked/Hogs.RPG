using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.InventoryServices
{
    public class InventoryService
    {
        private readonly InventoryRepository _inventoryRepository;

        // 🔒 Global lock to prevent concurrent sheet writes
        private static readonly SemaphoreSlim _lock = new(1, 1);

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
            await _lock.WaitAsync();
            try
            {
                await _inventoryRepository.AddItemAsync(discordId, itemId, amount);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task TakeItemAsync(ulong discordId, string itemId, int amount)
        {
            await _lock.WaitAsync();
            try
            {
                await _inventoryRepository.RemoveItemAsync(discordId, itemId, amount);
            }
            finally
            {
                _lock.Release();
            }
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
using Hogs.RPG.Core.Entities.EquipmentObjects;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class InventoryRepository
    {
        private readonly GameDbContext _context;

        public InventoryRepository(GameDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET INVENTORY
        // =========================
        public async Task<List<InventoryItem>> GetInventoryAsync(ulong discordId)
        {
            return await _context.InventoryItems
                .Where(x => x.DiscordId == discordId)
                .AsNoTracking()
                .ToListAsync();
        }

        // =========================
        // ADD ITEM
        // =========================
        public async Task AddItemAsync(ulong discordId, string itemId, int amount)
        {
            if (amount <= 0)
                return;

            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(x => x.DiscordId == discordId && x.ItemId == itemId);

            if (item == null)
            {
                _context.InventoryItems.Add(new InventoryItem
                {
                    DiscordId = discordId,
                    ItemId = itemId,
                    Quantity = amount
                });
            }
            else
            {
                item.Quantity += amount;
            }

            await _context.SaveChangesAsync();
        }

        // =========================
        // REMOVE ITEM
        // =========================
        public async Task RemoveItemAsync(ulong discordId, string itemId, int amount)
        {
            if (amount <= 0)
                return;

            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(x => x.DiscordId == discordId && x.ItemId == itemId);

            if (item == null)
                return;

            item.Quantity -= amount;

            if (item.Quantity <= 0)
                _context.InventoryItems.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}
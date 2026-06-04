using Hogs.RPG.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class SmithingShopRepository
    {
        private readonly GameDbContext _context;

        public SmithingShopRepository(GameDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET LISTINGS FOR ONE PLAYER
        // =========================
        public async Task<List<SmithingShopListing>> GetListingsAsync(ulong discordId)
        {
            return await _context.SmithingShopListings
                .Where(x => x.DiscordId == discordId)
                .ToListAsync();
        }

        // =========================
        // GET ALL LISTINGS (used by NPC daily job)
        // =========================
        public async Task<List<SmithingShopListing>> GetAllListingsAsync()
        {
            return await _context.SmithingShopListings
                .Where(x => x.Quantity > 0)
                .ToListAsync();
        }

        // =========================
        // GET ALL LISTINGS GROUPED BY PLAYER (used by NPC daily job)
        // =========================
        public async Task<List<IGrouping<ulong, SmithingShopListing>>> GetAllListingsGroupedAsync()
        {
            var all = await _context.SmithingShopListings
                .Where(x => x.Quantity > 0)
                .ToListAsync();

            return all.GroupBy(x => x.DiscordId).ToList();
        }

        // =========================
        // ADD OR INCREMENT LISTING
        // Called when a player crafts an item — auto-lists it
        // =========================
        public async Task AddOrIncrementAsync(ulong discordId, string itemId, int quantity = 1)
        {
            var existing = await _context.SmithingShopListings
                .FirstOrDefaultAsync(x => x.DiscordId == discordId && x.ItemId == itemId);

            if (existing == null)
            {
                _context.SmithingShopListings.Add(new SmithingShopListing
                {
                    DiscordId = discordId,
                    ItemId = itemId,
                    Quantity = quantity
                });
            }
            else
            {
                existing.Quantity += quantity;
            }

            await _context.SaveChangesAsync();
        }

        // =========================
        // DEDUCT SOLD QUANTITY
        // Called by NPC daily job after purchase
        // =========================
        public async Task DeductAsync(ulong discordId, string itemId, int quantity)
        {
            var listing = await _context.SmithingShopListings
                .FirstOrDefaultAsync(x => x.DiscordId == discordId && x.ItemId == itemId);

            if (listing == null) return;

            listing.Quantity -= quantity;

            if (listing.Quantity <= 0)
                _context.SmithingShopListings.Remove(listing);

            await _context.SaveChangesAsync();
        }

        // =========================
        // SAVE CHANGES (batch use)
        // =========================
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
using Hogs.RPG.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class ShopRepository
    {
        private readonly GameDbContext _context;

        public ShopRepository(GameDbContext context)
        {
            _context = context;
        }

        // =========================
        // PURCHASES
        // =========================
        public async Task AddPurchaseAsync(ShopPurchase purchase)
        {
            _context.ShopPurchases.Add(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ShopPurchase>> GetPendingAsync()
        {
            return await _context.ShopPurchases
                .Where(p => !p.IsFulfilled)
                .OrderBy(p => p.PurchasedAt)
                .ToListAsync();
        }

        public async Task<bool> FulfillAsync(int purchaseId)
        {
            var purchase = await _context.ShopPurchases
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null || purchase.IsFulfilled)
                return false;

            purchase.IsFulfilled = true;
            purchase.FulfilledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // AUCTIONS
        // =========================
        public async Task AddAuctionAsync(ActiveAuction auction)
        {
            _context.ActiveAuctions.Add(auction);
            await _context.SaveChangesAsync();
        }

        public async Task<ActiveAuction?> GetAuctionByIdAsync(int id)
        {
            return await _context.ActiveAuctions
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<ActiveAuction?> GetActiveAuctionByItemAsync(string itemId)
        {
            return await _context.ActiveAuctions
                .FirstOrDefaultAsync(a => a.ItemId == itemId && !a.IsEnded);
        }

        public async Task<List<ActiveAuction>> GetActiveAuctionsAsync()
        {
            return await _context.ActiveAuctions
                .Where(a => !a.IsEnded)
                .OrderBy(a => a.StartedAt)
                .ToListAsync();
        }

        public async Task UpdateAuctionAsync(ActiveAuction auction)
        {
            _context.ActiveAuctions.Update(auction);
            await _context.SaveChangesAsync();
        }

        public async Task SetAuctionMessageIdAsync(int auctionId, ulong messageId)
        {
            var auction = await _context.ActiveAuctions
                .FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction == null) return;

            auction.MessageId = messageId;
            await _context.SaveChangesAsync();
        }
    }
}
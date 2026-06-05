using Hogs.RPG.Core.Entities.PlayerObjects;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class PlayerAuctionRepository
    {
        private readonly GameDbContext _context;

        public PlayerAuctionRepository(GameDbContext context)
        {
            _context = context;
        }

        // =========================
        // ADD LISTING
        // =========================
        public async Task AddListingAsync(PlayerAuctionListing listing)
        {
            _context.PlayerAuctionListings.Add(listing);
            await _context.SaveChangesAsync();
        }

        // =========================
        // GET BY ID
        // =========================
        public async Task<PlayerAuctionListing?> GetByIdAsync(int id)
        {
            return await _context.PlayerAuctionListings
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        // =========================
        // GET ALL ACTIVE LISTINGS
        // =========================
        public async Task<List<PlayerAuctionListing>> GetActiveListingsAsync()
        {
            return await _context.PlayerAuctionListings
                .Where(l => !l.IsEnded && !l.IsCancelled)
                .OrderBy(l => l.ListedAt)
                .ToListAsync();
        }

        // =========================
        // GET ACTIVE LISTINGS BY SELLER
        // =========================
        public async Task<List<PlayerAuctionListing>> GetActiveBySellerAsync(ulong sellerId)
        {
            return await _context.PlayerAuctionListings
                .Where(l => l.SellerDiscordId == sellerId && !l.IsEnded && !l.IsCancelled)
                .OrderBy(l => l.ListedAt)
                .ToListAsync();
        }

        // =========================
        // UPDATE LISTING
        // =========================
        public async Task UpdateListingAsync(PlayerAuctionListing listing)
        {
            _context.PlayerAuctionListings.Update(listing);
            await _context.SaveChangesAsync();
        }

        // =========================
        // SET MESSAGE ID (after posting to channel)
        // =========================
        public async Task SetMessageIdAsync(int listingId, ulong messageId)
        {
            var listing = await _context.PlayerAuctionListings
                .FirstOrDefaultAsync(l => l.Id == listingId);

            if (listing == null) return;

            listing.MessageId = messageId;
            await _context.SaveChangesAsync();
        }
    }
}

using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class PlayerAuctionListing
    {
        public int Id { get; set; }

        // =========================
        // WHO LISTED IT
        // =========================
        public ulong SellerDiscordId { get; set; }

        // =========================
        // WHAT IS LISTED
        // Only one of ItemId / PetId / RelicId will be set, depending on Type.
        // =========================
        public ListingType Type { get; set; }

        public string? ItemId { get; set; }   // for Type == Item
        public int? PetId { get; set; }    // for Type == Pet (PlayerPet.Id)
        public int? RelicId { get; set; }  // for Type == Relic (PlayerRelic.Id)

        // Cached display name so embeds can render without extra lookups
        public string ListingName { get; set; } = "";

        // =========================
        // PRICING
        // =========================
        public int BasePrice { get; set; }
        public int? BuyoutPrice { get; set; } // null = no buyout

        public int CurrentBid { get; set; }
        public ulong? CurrentBidderDiscordId { get; set; }

        // =========================
        // STATE
        // =========================
        public bool IsEnded { get; set; } = false;
        public bool IsCancelled { get; set; } = false;

        public DateTime ListedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }

        // =========================
        // DISCORD UI
        // Tracks the public listing message so bids can update the embed.
        // =========================
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
    }
}

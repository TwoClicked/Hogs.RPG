using Hogs.RPG.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data
{
    public class GameDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<BossSpawnState> BossSpawnStates { get; set; }
        public DbSet<PlayerPet> PlayerPets { get; set; }
        public DbSet<ShopPurchase> ShopPurchases { get; set; }
        public DbSet<ActiveAuction> ActiveAuctions { get; set; }
        public DbSet<PlayerRelic> PlayerRelics { get; set; }
        public DbSet<PlayerRelicShard> PlayerRelicShards { get; set; }
        public DbSet<RaidSession> RaidSessions { get; set; }
        public DbSet<RaidParticipant> RaidParticipants { get; set; }
        public DbSet<GearSet> GearSets { get; set; }

        // Player-to-player marketplace listings
        public DbSet<PlayerAuctionListing> PlayerAuctionListings { get; set; }

        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent duplicate inventory entries
            modelBuilder.Entity<InventoryItem>()
                .HasIndex(x => new { x.DiscordId, x.ItemId })
                .IsUnique();

            // Serialized as string, not a table
            modelBuilder.Ignore<ActiveBuff>();
            modelBuilder.Ignore<ActiveRaidEffect>();

            modelBuilder.Entity<PlayerPet>()
                .HasOne(p => p.Player)
                .WithMany()
                .HasForeignKey(p => p.DiscordId)
                .HasPrincipalKey(p => p.DiscordId);

            modelBuilder.Entity<PlayerRelic>()
                .HasOne(r => r.Player)
                .WithMany()
                .HasForeignKey(r => r.DiscordId)
                .HasPrincipalKey(p => p.DiscordId);

            modelBuilder.Entity<PlayerRelicShard>()
                .HasOne(s => s.Player)
                .WithMany()
                .HasForeignKey(s => s.DiscordId)
                .HasPrincipalKey(p => p.DiscordId);

            modelBuilder.Entity<RaidParticipant>()
                .HasOne(p => p.RaidSession)
                .WithMany(s => s.Participants)
                .HasForeignKey(p => p.RaidSessionId);

            modelBuilder.Entity<GearSet>()
                .HasOne(g => g.Player)
                .WithMany()
                .HasForeignKey(g => g.DiscordId)
                .HasPrincipalKey(p => p.DiscordId);
        }
    }
}

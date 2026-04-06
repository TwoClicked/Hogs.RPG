using Hogs.RPG.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Data
{
    public class GameDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<BossSpawnState> BossSpawnStates { get; set; }
        public DbSet<PlayerPet> PlayerPets { get; set; }

        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔥 Prevent duplicate inventory entries
            modelBuilder.Entity<InventoryItem>()
                .HasIndex(x => new { x.DiscordId, x.ItemId })
                .IsUnique();

            // We save as a string for now, Will move to 1 - many later when more active buffs come
            modelBuilder.Ignore<ActiveBuff>();

            modelBuilder.Entity<PlayerPet>()
                .HasOne(p => p.Player)
                .WithMany()
                .HasForeignKey(p => p.DiscordId)
                .HasPrincipalKey(p => p.DiscordId);
        }
    }
}

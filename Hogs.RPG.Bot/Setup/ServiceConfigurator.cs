using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Data;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services;
using Hogs.RPG.Services.AlchemyServices;
using Hogs.RPG.Services.AuctionServices;
using Hogs.RPG.Services.DungeonServices;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.GatheringServices;
using Hogs.RPG.Services.HuntServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.RaidServices;
using Hogs.RPG.Services.RelicServices;
using Hogs.RPG.Services.ShopServices;
using Hogs.RPG.Services.SmithingServices;
using Hogs.RPG.Services.TradeServices;
using Hogs.RPG.Services.TrailServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hogs.RPG.Bot.Setup
{
    public static class ServiceConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            // =========================
            // 🗄️ DATABASE
            // =========================
            services.AddDbContext<GameDbContext>(options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

                if (!string.IsNullOrEmpty(connectionString))
                {
                    var uri = new Uri(connectionString);
                    var userInfo = uri.UserInfo.Split(':');

                    var npgsqlConnection =
                        $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                        $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";

                    options.UseNpgsql(npgsqlConnection);
                }
                else
                {
                    options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=HogsRPG;Trusted_Connection=True;");
                }
            });

            // =========================
            // 📦 REPOSITORIES (Scoped)
            // =========================
            services.AddScoped<PlayerRepository>();
            services.AddScoped<PetRepository>();
            services.AddScoped<InventoryRepository>();
            services.AddScoped<BossStateRepository>();
            services.AddScoped<ShopRepository>();
            services.AddScoped<RelicRepository>();
            services.AddScoped<RaidRepository>();
            services.AddScoped<GearSetRepository>();
            services.AddScoped<PlayerAuctionRepository>();

            // =========================
            // ⚙️ CORE GAME SERVICES (Scoped)
            // =========================
            services.AddScoped<PlayerService>();
            services.AddScoped<HealService>();
            services.AddScoped<StatService>();
            services.AddScoped<InventoryService>();
            services.AddScoped<HuntService>();
            services.AddScoped<HunterStaminaService>();
            services.AddScoped<LevelService>();
            services.AddScoped<ItemService>();
            services.AddScoped<CraftingService>();
            services.AddScoped<EquipService>();
            services.AddScoped<EquipmentService>();
            services.AddScoped<BuffService>();
            services.AddScoped<PotionService>();
            services.AddScoped<EnergyService>();
            services.AddScoped<GatherService>();
            services.AddScoped<AlchemyService>();
            services.AddScoped<EvolvePetService>();
            services.AddScoped<PetService>();
            services.AddScoped<PetPassiveService>();
            services.AddScoped<RelicService>();
            services.AddScoped<RaidService>();
            services.AddScoped<GearSetService>();
            services.AddSingleton<PlayerAuctionService>();
            services.AddScoped<MaterialConversionService>();
            services.AddScoped<SmithingService>();
            services.AddScoped<SmithingShopRepository>();
            services.AddScoped<AlchemyBrewService>();

            // =========================
            // 🏆 LEADERBOARDS
            // =========================
            services.AddScoped<LeaderboardService>();
            services.AddSingleton<LeaderboardUpdater>();

            // =========================
            // 🔒 SINGLETONS (Schedulers / Systems)
            // =========================
            services.AddSingleton<PetDungeonService>();
            services.AddSingleton<BossService>();
            services.AddSingleton<BossScheduler>();
            services.AddSingleton<ShopService>();
            services.AddSingleton<TradeService>();
            services.AddSingleton<TradeCleanupService>();
            services.AddSingleton<RaidTimerService>();
            services.AddSingleton<TrailService>();
            services.AddSingleton<NpcShopService>();
            services.AddSingleton<GameEventService>();

            // DungeonService keeps in-memory state → must stay singleton
            services.AddSingleton<DungeonService>();
        }
    }
}
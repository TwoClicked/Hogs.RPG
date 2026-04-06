using Hogs.RPG.Core.GameData.Recipes;
using System;
using Hogs.RPG.Data;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AlchemyServices;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.GatheringServices;
using Hogs.RPG.Services.HuntServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.TradeServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
                    // Convert Railway's postgres URL to Npgsql connection string
                    var uri = new Uri(connectionString);
                    var userInfo = uri.UserInfo.Split(':');
                    var npgsqlConnection = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
                    options.UseNpgsql(npgsqlConnection);
                }
                else
                    options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=HogsRPG;Trusted_Connection=True;");
            });

            // =========================
            // 📦 REPOSITORIES (Scoped)
            // =========================
            services.AddScoped<PlayerRepository>();
            services.AddScoped<PetRepository>();
            services.AddScoped<InventoryRepository>();
            services.AddScoped<BossStateRepository>();

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
            services.AddScoped<TradeService>();
            services.AddScoped<PetService>();
            services.AddScoped<PetPassiveService>();

            // =========================
            // 🔒 SINGLETONS (no DB injection)
            // =========================
            services.AddSingleton<BossService>();
            services.AddSingleton<BossScheduler>();
            services.AddSingleton<TradeCleanupService>();

            // DungeonService is Singleton because it holds in-memory session state
            // (_active, _lastDungeonRun, etc.) that must persist across requests.
            // It resolves scoped DB dependencies via IServiceScopeFactory internally.
            services.AddSingleton<DungeonService>();
        }
    }
}

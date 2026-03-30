using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Data;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AlchemyServices;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.GatheringServices;
using Hogs.RPG.Services.HuntServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PlayerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hogs.RPG.Bot.Setup
{
    public static class ServiceConfigurator
    {
        public static void Configure(IServiceCollection services)
        {

            //EF connect
            services.AddDbContext<GameDbContext>(options =>
                                                 options.UseSqlServer(
                                                 "Server=(localdb)\\MSSQLLocalDB;Database=HogsRPG;Trusted_Connection=True;"));

            // registering Services
            services.AddSingleton<PlayerService>();
            services.AddSingleton<HealService>();
            services.AddSingleton<StatService>();

            services.AddSingleton<InventoryService>();

            services.AddSingleton<HuntService>();
            services.AddSingleton<HunterStaminaService>();
            services.AddSingleton<LevelService>();

            services.AddSingleton<ItemService>();

            services.AddSingleton<CraftingService>();

            services.AddSingleton<EquipService>();
            services.AddSingleton<EquipmentService>();

            services.AddSingleton<BuffService>();
            services.AddSingleton<PotionService>();

            services.AddSingleton<EnergyService>();
            services.AddSingleton<GatherService>();

            services.AddSingleton<AlchemyService>();

            services.AddSingleton<BossService>();

            services.AddSingleton<BossScheduler>();
            services.AddHostedService<BossScheduler>();

            services.AddSingleton<DungeonService>();

            services.AddScoped<PlayerRepository>();
            services.AddScoped<InventoryRepository>();
            services.AddScoped<BossStateRepository>();
        }
    }
}
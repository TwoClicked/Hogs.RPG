using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Data.GoogleSheets;
using Hogs.RPG.Data.Interfaces;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AlchemyServices;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.GatheringServices;
using Hogs.RPG.Services.HuntServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PlayerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Hogs.RPG.Bot.Setup
{
    public static class ServiceConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton<IGoogleSheetsService>(
                new GoogleSheetsService(
                    @"C:\Users\diego\source\repos\Hogs.RPG\credentials.json",
                    "1pA9mu28YmNe7GSchqJyxmdw8ErAjta0Xvm3LUKjpr50"
                )
            );
            // registering Services
            services.AddSingleton<PlayerRepository>();
            services.AddSingleton<PlayerService>();
            services.AddSingleton<HealService>();
            services.AddSingleton<StatService>();

            services.AddSingleton<InventoryRepository>();
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

            services.AddSingleton<BossRepository>();
            services.AddSingleton<BossService>();

            services.AddSingleton<BossScheduler>();
            services.AddHostedService<BossScheduler>();

            services.AddSingleton<DungeonService>();
        }
    }
}
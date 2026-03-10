using Hogs.RPG.Data.GoogleSheets;
using Hogs.RPG.Data.Interfaces;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
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

            services.AddSingleton<InventoryRepository>();
            services.AddSingleton<InventoryService>();

            services.AddSingleton<HuntService>();
            services.AddSingleton<LevelService>();

            services.AddSingleton<ItemService>();

            services.AddSingleton<CraftingService>();

            services.AddSingleton<EquipService>();
            services.AddSingleton<EquipmentService>();
        }
    }
}
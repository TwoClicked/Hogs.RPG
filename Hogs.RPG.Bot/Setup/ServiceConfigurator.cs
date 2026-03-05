using Hogs.RPG.Data.GoogleSheets;
using Hogs.RPG.Data.Interfaces;
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
        }
    }
}
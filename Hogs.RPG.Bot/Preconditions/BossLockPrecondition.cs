using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.Game;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Preconditions
{
    public class BossLockAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(
            IInteractionContext context,
            ICommandInfo commandInfo,
            IServiceProvider services)
        {
            var bossService = services.GetRequiredService<BossService>();

            if (bossService.IsAnyBossActive())
            {
                return Task.FromResult(PreconditionResult.FromError(
                    "⚔️ A global boss is active! All actions are locked until it's defeated or flees."));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
// Hogs.RPG.Bot/Preconditions/GearSwapLockPrecondition.cs

using Discord;
using Discord.Interactions;
using Hogs.RPG.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Hogs.RPG.Bot.Preconditions
{
    public class GearSwapLockAttribute : PreconditionAttribute
    {
        private const int CooldownSeconds = 10;

        public override async Task<PreconditionResult> CheckRequirementsAsync(
            IInteractionContext context,
            ICommandInfo commandInfo,
            IServiceProvider services)
        {
            var playerRepo = services.GetRequiredService<PlayerRepository>();
            var player = await playerRepo.GetByDiscordIdAsync(context.User.Id);

            if (player?.LastGearSwapAt.HasValue == true)
            {
                var elapsed = DateTime.UtcNow - player.LastGearSwapAt.Value;
                if (elapsed.TotalSeconds < CooldownSeconds)
                {
                    var remaining = Math.Ceiling(CooldownSeconds - elapsed.TotalSeconds);
                    return PreconditionResult.FromError(
                        $"⚙️ Gear swap in progress — please wait **{remaining}s** before using commands.");
                }
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
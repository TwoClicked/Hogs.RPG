// Hogs.RPG.Bot/Preconditions/TradeLockAttribute.cs

using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.TradeServices;
using Microsoft.Extensions.DependencyInjection;

namespace Hogs.RPG.Bot.Preconditions
{
    // =========================
    // TRADE LOCK
    // Mirrors BossLockAttribute's shape, but the check is per-player
    // rather than global — only the two players actually inside an
    // active/pending trade are locked out of everything else.
    //
    // Apply this to every gameplay module that currently carries
    // [BossLock] (inventory, hunt, craft, alchemy, market, raids,
    // dungeons, pets, achievements, shop, etc). Do NOT apply it to
    // TradeModule itself — a trading player still needs to be able
    // to use /tradeadd, /tradeconfirm, /tradecancel, /tradeview, etc.
    // =========================
    public class TradeLockAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(
            IInteractionContext context,
            ICommandInfo commandInfo,
            IServiceProvider services)
        {
            var tradeService = services.GetRequiredService<TradeService>();

            if (tradeService.HasActiveTrade(context.User.Id))
            {
                return Task.FromResult(PreconditionResult.FromError(
                    "📦 You're in an active trade! Finish it with `/tradeconfirm` or back out with `/tradecancel` before doing anything else."));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
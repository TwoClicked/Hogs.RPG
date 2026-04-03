using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.TradeServices
{
    public class TradeCleanupService : BackgroundService
    {
        private readonly TradeService _tradeService;
        private readonly DiscordSocketClient _client;


        public TradeCleanupService(TradeService tradeService, DiscordSocketClient client)
        {
            _tradeService = tradeService;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                var now = DateTime.UtcNow;

                var trades = _tradeService.GetAllTrades();

                foreach (var trade in trades)
                {
                    var elapsed = now - trade.LastUpdatedAt;

                    var channel = _client.GetChannel(1489405758603923477) as IMessageChannel;

                    if (channel == null) continue;

                    // ⚠️ WARNING at 4 minutes
                    if (!trade.WarningSent && elapsed > TimeSpan.FromMinutes(4))
                    {
                        await channel.SendMessageAsync(
                            $"⚠️ Trade between <@{trade.Player1Id}> and <@{trade.Player2Id}> will expire in **1 minute** due to inactivity!");

                        trade.WarningSent = true;
                    }

                    // ⏳ EXPIRE at 5 minutes
                    if (elapsed > TimeSpan.FromMinutes(5))
                    {
                        await channel.SendMessageAsync(
                            $"⏳ Trade between <@{trade.Player1Id}> and <@{trade.Player2Id}> expired due to inactivity.");

                        _tradeService.RemoveTrade(trade);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
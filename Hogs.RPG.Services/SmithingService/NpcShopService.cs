using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities.JobObjects;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AchievementServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

namespace Hogs.RPG.Services.SmithingServices
{
    public class NpcShopService : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceScopeFactory _scopeFactory;

        private const int DailyGoldCap = 5000;
        private const int NpcResetHourUtc = 12;

        private readonly Random _random = new();
        private DateTime _lastRun = DateTime.MinValue;

        public NpcShopService(
            DiscordSocketClient client,
            IServiceScopeFactory scopeFactory)
        {
            _client = client;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🛒 NpcShopService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                if (now.Hour == NpcResetHourUtc &&
                    now.Minute == 0 &&
                    _lastRun.Date != now.Date)
                {
                    _lastRun = now;
                    Console.WriteLine($"[NpcShopService] Running daily NPC purchases at {now:HH:mm} UTC");

                    try
                    {
                        await RunNpcPurchasesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ NpcShopService error: {ex}");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        // =========================
        // MAIN NPC PURCHASE LOOP
        // =========================
        private async Task RunNpcPurchasesAsync()
        {
            using var scope = _scopeFactory.CreateScope();

            var shopRepo = scope.ServiceProvider.GetRequiredService<SmithingShopRepository>();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();

            var groupedListings = await shopRepo.GetAllListingsGroupedAsync();

            Console.WriteLine($"[NpcShopService] Processing {groupedListings.Count} player shop(s)");

            foreach (var playerGroup in groupedListings)
            {
                var discordId = playerGroup.Key;

                try
                {
                    await ProcessPlayerShopAsync(discordId, playerGroup.ToList(), shopRepo, playerRepo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ NpcShopService error for player {discordId}: {ex.Message}");
                }
            }
        }

        // =========================
        // PROCESS ONE PLAYER'S SHOP
        // =========================
        private async Task ProcessPlayerShopAsync(
            ulong discordId,
            List<SmithingShopListing> listings,
            SmithingShopRepository shopRepo,
            PlayerRepository playerRepo)
        {
            var player = await playerRepo.GetByDiscordIdAsync(discordId);
            if (player == null) return;

            var todayKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (player.SmithingLastReset != todayKey)
            {
                player.SmithingEarnedToday = 0;
                player.SmithingLastReset = todayKey;
            }

            int remainingCap = DailyGoldCap - player.SmithingEarnedToday;
            if (remainingCap <= 0) return;

            var salesLog = new List<(string Name, int Qty, int Total)>();
            int totalEarned = 0;

            foreach (var listing in listings)
            {
                if (remainingCap <= 0) break;

                if (!SmithingItemRegistry.All.TryGetValue(listing.ItemId, out var itemDef))
                    continue;

                if (listing.Quantity <= 0) continue;

                var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
                bool elixirActive = player.BlacksmithElixirActiveDate == yesterday;

                int npcDemand = elixirActive
                    ? itemDef.MaxNpcBuysPerDay
                    : _random.Next(1, itemDef.MaxNpcBuysPerDay + 1);

                if (npcDemand == 0) continue;

                int unitsSold = Math.Min(npcDemand, listing.Quantity);

                int potentialEarnings = unitsSold * itemDef.NpcGoldPrice;
                if (potentialEarnings > remainingCap)
                {
                    unitsSold = remainingCap / itemDef.NpcGoldPrice;
                    if (unitsSold <= 0) continue;
                    potentialEarnings = unitsSold * itemDef.NpcGoldPrice;
                }

                await shopRepo.DeductAsync(discordId, listing.ItemId, unitsSold);

                totalEarned += potentialEarnings;
                remainingCap -= potentialEarnings;

                salesLog.Add((itemDef.Name, unitsSold, potentialEarnings));
            }

            if (totalEarned <= 0) return;

            // =========================
            // 📊 ACHIEVEMENT COUNTER
            // =========================
            player.Gold += totalEarned;
            player.SmithingEarnedToday += totalEarned;
            player.TotalNpcGoldEarned += totalEarned;

            await playerRepo.UpdatePlayerAsync(player);

            // DM receipt
            await SendReceiptAsync(discordId, salesLog, totalEarned, player.SmithingEarnedToday);

            // =========================
            // 🏆 ACHIEVEMENT CHECK
            // =========================
            using var achScope = _scopeFactory.CreateScope();
            var achievementService = achScope.ServiceProvider.GetRequiredService<AchievementService>();
            await achievementService.CheckAndAwardAsync(discordId);
        }

        // =========================
        // DM RECEIPT
        // =========================
        private async Task SendReceiptAsync(
            ulong discordId,
            List<(string Name, int Qty, int Total)> sales,
            int totalEarned,
            int earnedToday)
        {
            try
            {
                var user = await _client.GetUserAsync(discordId);
                if (user == null) return;

                var sb = new StringBuilder();
                sb.AppendLine("🛒 **NPC Market Report — Daily Sales**");
                sb.AppendLine("─────────────────────────");

                foreach (var (name, qty, total) in sales)
                    sb.AppendLine($"⚒️ {name} × {qty} — **{total}g**");

                sb.AppendLine("─────────────────────────");
                sb.AppendLine($"💰 **Total Earned: {totalEarned}g**");
                sb.AppendLine($"📊 Today's Total: {earnedToday}g / {DailyGoldCap}g cap");

                await user.SendMessageAsync(sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to DM receipt to {discordId}: {ex.Message}");
            }
        }
    }
}

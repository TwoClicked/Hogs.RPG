using Discord;
using Discord.WebSocket;
using Hogs.RPG.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class LeaderboardUpdater : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DiscordSocketClient _client;

    private const ulong CHANNEL_ID = 1490715637075546192;

    private ulong _mainMsgId;

    public LeaderboardUpdater(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
    {
        _scopeFactory = scopeFactory;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("🏆 LeaderboardUpdater started");

        // Wait for Discord to actually be ready (VERY IMPORTANT)
        while (_client.ConnectionState != ConnectionState.Connected)
        {
            Console.WriteLine("⏳ Waiting for Discord connection...");
            await Task.Delay(1000);
        }

        Console.WriteLine("✅ Discord connected, starting leaderboard loop");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine("🏆 Updating leaderboards...");
                await UpdateLeaderboard();
                Console.WriteLine("✅ Leaderboards updated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Leaderboard error: {ex}");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // TEMP for debugging
        }
    }

    private async Task UpdateLeaderboard()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<LeaderboardService>();

        var channel = _client.GetChannel(CHANNEL_ID) as IMessageChannel;
        if (channel == null)
        {
            Console.WriteLine($"❌ Channel not found: {CHANNEL_ID}");
            return;
        }

        Console.WriteLine($"✅ Channel found: {channel.Id}");

        var gold = await service.GetTopGold(5);
        var xp = await service.GetTopXP(5);
        var gear = await service.GetTopGearScore(5);
        var dungeons = await service.GetTopDungeonRuns(5);
        var petLevel = await service.GetTopPetLevel(5);
        var petGear = await service.GetTopPetGearScore(5);

        var embed = new EmbedBuilder()
            .WithTitle("🏆 HOGS RPG — LEADERBOARDS")
            .WithColor(new Color(255, 215, 0))
            .WithDescription("━━━━━━━━━━━━━━━━━━━━━━━")
            .WithFooter($"Last updated: {DateTime.UtcNow:HH:mm} UTC");

        embed.AddField("💰 Gold", FormatGold(gold), true);
        embed.AddField("📈 Level", FormatXP(xp), true);
        embed.AddField("⚔ Gear Score", FormatGear(gear), true);

        embed.AddField("🏰 Dungeons", FormatDungeons(dungeons), true);
        embed.AddField("🐾 Pet Level", FormatPetLevel(petLevel), true);
        embed.AddField("🐾 Pet Power", FormatPetGear(petGear), true);

        _mainMsgId = await SendOrUpdate(channel, embed.Build(), _mainMsgId);
    }

    // =========================
    // 🏅 FORMATTING
    // =========================

    private string GetMedal(int rank)
    {
        return rank switch
        {
            1 => "👑",
            2 => "🥈",
            3 => "🥉",
            _ => $"#{rank}"
        };
    }

    private string FormatGold(System.Collections.Generic.List<Hogs.RPG.Core.Entities.Player> players)
    {
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {p.Username} — **{p.Gold:N0}**"));
    }

    private string FormatXP(System.Collections.Generic.List<Hogs.RPG.Core.Entities.Player> players)
    {
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {p.Username} — **Lv.{p.Level}** ({p.XP:N0})"));
    }

    private string FormatGear(System.Collections.Generic.List<(Hogs.RPG.Core.Entities.Player player, int score)> data)
    {
        return string.Join("\n", data.Select((x, i) =>
            $"{GetMedal(i + 1)} {x.player.Username} — **{x.score:N0}**"));
    }

    private string FormatDungeons(System.Collections.Generic.List<Hogs.RPG.Core.Entities.Player> players)
    {
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {p.Username} — **{p.DungeonRunsCompleted}**"));
    }

    private string FormatPetLevel(System.Collections.Generic.List<(Hogs.RPG.Core.Entities.Player player, Hogs.RPG.Core.Entities.PlayerPet pet)> data)
    {
        return string.Join("\n", data.Select((x, i) =>
            $"{GetMedal(i + 1)} {x.player.Username} — **Lv.{x.pet.Level}**"));
    }

    private string FormatPetGear(System.Collections.Generic.List<(Hogs.RPG.Core.Entities.Player player, Hogs.RPG.Core.Entities.PlayerPet pet, int score)> data)
    {
        return string.Join("\n", data.Select((x, i) =>
            $"{GetMedal(i + 1)} {x.player.Username} — **{x.score}**"));
    }

    // =========================
    // 🔄 MESSAGE HANDLER
    // =========================

    private async Task<ulong> SendOrUpdate(IMessageChannel channel, Embed embed, ulong messageId)
    {
        if (messageId == 0)
        {
            var msg = await channel.SendMessageAsync(embed: embed);
            return msg.Id;
        }
        else
        {
            var msg = await channel.GetMessageAsync(messageId) as IUserMessage;

            if (msg != null)
            {
                await msg.ModifyAsync(m => m.Embed = embed);
                return messageId;
            }
            else
            {
                var newMsg = await channel.SendMessageAsync(embed: embed);
                return newMsg.Id;
            }
        }
    }
}
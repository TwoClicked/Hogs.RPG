using Discord;
using Discord.WebSocket;
using Hogs.RPG.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Entities.PetObjects;

public class LeaderboardUpdater
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DiscordSocketClient _client;

    private const ulong CHANNEL_ID = 1490715637075546192;
    private const ulong GUILD_ID = 1109193500664287336;

    private ulong _mainMsgId;

    public LeaderboardUpdater(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
    {
        _scopeFactory = scopeFactory;
        _client = client;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("🏆 LeaderboardUpdater started");

        while (_client.ConnectionState != ConnectionState.Connected)
        {
            Console.WriteLine("⏳ Waiting for Discord connection...");
            await Task.Delay(1000);
        }

        Console.WriteLine("✅ Discord connected, starting leaderboard loop");

        while (true)
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

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }

    // =========================
    // 🏷️ DISPLAY NAME HELPER
    // =========================

    private string GetDisplayName(ulong userId, string fallback)
    {
        var guild = _client.GetGuild(GUILD_ID);
        var member = guild?.GetUser(userId);
        return member?.DisplayName ?? member?.GlobalName ?? member?.Username ?? fallback;
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

        if (_mainMsgId == 0)
            _mainMsgId = await FindExistingMessage(channel);

        var gold = await service.GetTopGold(5);
        var xp = await service.GetTopXP(5);
        var gear = await service.GetTopGearScore(5);
        var dungeons = await service.GetTopDungeonRuns(5);
        var raids = await service.GetTopRaidsCompleted(5);
        var bossDmg = await service.GetTopBossDamage(5);
        var petGear = await service.GetTopPetGearScore(5);
        var deaths = await service.GetTopDeaths(5);
        var trails = await service.GetTopTrails(5);
        var smithing = await service.GetTopSmithingLevel(5);
        var achievements = await service.GetTopAchievements(5);


        List<Hogs.RPG.Core.Entities.PlayerObjects.Player> alchemist = await service.GetTopAlchemistLevel(5);

        var embed = new EmbedBuilder()
            .WithTitle("🏆 HOGS RPG — LEADERBOARDS")
            .WithColor(new Color(255, 215, 0))
            .WithDescription("━━━━━━━━━━━━━━━━━━━━━━━")
            .WithFooter($"Last updated: {DateTime.UtcNow:HH:mm} UTC");

        // Row 1
        embed.AddField("💰 Gold", FormatGold(gold), true);
        embed.AddField("📈 Level", FormatXP(xp), true);
        embed.AddField("⚔️ Gear Score", FormatGear(gear), true);

        // Row 2
        embed.AddField("🏰 Dungeons", FormatDungeons(dungeons), true);
        embed.AddField("⚔️ Raids", FormatRaids(raids), true);
        embed.AddField("💥 Boss Damage", FormatBossDmg(bossDmg), true);

        // Row 3
        embed.AddField("🐾 Pet Power", FormatPetGear(petGear), true);
        embed.AddField("💀 Deaths", FormatDeaths(deaths), true);
        embed.AddField("🏕️ Trails", FormatTrails(trails), true);

        // Row 4
        embed.AddField("⚒️ Smithing Level", FormatSmithing(smithing), true);
        embed.AddField("🧪 Alchemist Level", FormatAlchemist(alchemist), true);
        embed.AddField("🏆 Achievements", FormatAchievements(achievements), true);

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

    private string FormatAchievements(List<(ulong DiscordId, int Count)> entries)
    {
        if (!entries.Any()) return "*No data yet*";
        return string.Join("\n", entries.Select((e, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(e.DiscordId, "Unknown")} — **{e.Count}** achievements"));
    }
    private string FormatAlchemist(List<Hogs.RPG.Core.Entities.PlayerObjects.Player> players)
    {
        if (!players.Any()) return "*No data yet*";
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(p.DiscordId, p.Username)} — **Lv. {p.AlchemistLevel}**"));
    }
    private string FormatGold(List<Player> players)
    {
        if (!players.Any()) return "*No data yet*";
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(p.DiscordId, p.Username)} — **{p.Gold:N0}**"));
    }

    private string FormatXP(List<Player> players)
    {
        if (!players.Any()) return "*No data yet*";
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(p.DiscordId, p.Username)} — **Lv.{p.Level}**"));
    }

    private string FormatGear(List<(Player player, int score)> data)
    {
        if (!data.Any()) return "*No data yet*";
        return string.Join("\n", data.Select((x, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(x.player.DiscordId, x.player.Username)} — **{x.score:N0}**"));
    }

    private string FormatSmithing(List<Player> players)
    {
        if (!players.Any()) return "*No data yet*";
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(p.DiscordId, p.Username)} — **Lv. {p.SmithingLevel}**"));
    }

    private string FormatDungeons(List<Player> players)
    {
        if (!players.Any()) return "*No data yet*";
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(p.DiscordId, p.Username)} — **{p.DungeonRunsCompleted}**"));
    }

    private string FormatRaids(List<Player> players)
    {
        if (!players.Any()) return "*No data yet*";
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(p.DiscordId, p.Username)} — **{p.RaidsCompleted}**"));
    }

    private string FormatBossDmg(List<Player> players)
    {
        if (!players.Any()) return "*No data yet*";
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(p.DiscordId, p.Username)} — **{p.TotalBossDamage:N0}**"));
    }

    private string FormatPetGear(List<(Player player, Hogs.RPG.Core.Entities.PlayerPet pet, int score)> data)
    {
        if (!data.Any()) return "*No data yet*";
        return string.Join("\n", data.Select((x, i) =>
        {
            PetDefinition petDef = null;
            Hogs.RPG.Core.GameData.Registries.PetRegistry.All.TryGetValue(x.pet.PetId, out petDef);
            var petName = x.pet.CustomName ?? petDef?.Name ?? "Unknown";
            return $"{GetMedal(i + 1)} {GetDisplayName(x.player.DiscordId, x.player.Username)} ({petName}) — **{x.score}**";
        }));
    }

    private string FormatDeaths(List<Player> players)
    {
        if (!players.Any()) return "*No data yet*";
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(p.DiscordId, p.Username)} — **{p.Deaths}**"));
    }

    private string FormatTrails(List<Player> players)
    {
        if (!players.Any()) return "*No data yet*";
        return string.Join("\n", players.Select((p, i) =>
            $"{GetMedal(i + 1)} {GetDisplayName(p.DiscordId, p.Username)} — **{p.TrailsCompleted}**"));
    }

    // =========================
    // 🔄 MESSAGE HANDLER
    // =========================

    private async Task<ulong> FindExistingMessage(IMessageChannel channel)
    {
        var messages = await channel.GetMessagesAsync(10).FlattenAsync();
        var existing = messages.FirstOrDefault(m =>
            m.Author.Id == _client.CurrentUser.Id &&
            m is IUserMessage);

        return existing?.Id ?? 0;
    }

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
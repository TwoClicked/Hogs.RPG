using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.PlayerServices;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hogs.RPG.Core.Entities.BossDefinition;

namespace Hogs.RPG.Bot.Commands
{
    public class BossModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BossService _bossService;
        private readonly PlayerService _playerService;
        private readonly PlayerRepository _playerRepository;

        private const int ATTACK_COOLDOWN_SECONDS = 5;

        public BossModule(
            BossService bossService,
            PlayerService playerService,
            PlayerRepository playerRepository)
        {
            _bossService = bossService;
            _playerService = playerService;
            _playerRepository = playerRepository;
        }

        // =========================
        // VIEW BOSSES
        // =========================

        [SlashCommand("bosses", "View all active bosses")]
        public async Task GetActiveBosses()
        {
            var bosses = _bossService.GetAllActiveBosses();

            if (bosses.Count == 0)
            {
                await RespondAsync("❌ There are currently no active bosses.");
                return;
            }

            if (bosses.Count == 1)
            {
                var boss = bosses.First();
                var embed = BuildBossEmbed(boss);

                await RespondAsync(embed: embed);
                return;
            }

            var builder = new StringBuilder();

            builder.AppendLine("⚔ **Active Bosses:**\n");

            foreach (var boss in bosses)
            {
                builder.AppendLine($"🔥 **{boss.Definition.Name}** ({boss.Definition.Type})");
                builder.AppendLine($"❤️ {boss.CurrentHealth}/{boss.Definition.MaxHealth}");
                builder.AppendLine();
            }

            await RespondAsync(builder.ToString());
        }

        // =========================
        // ATTACK BOSS
        // =========================

        [SlashCommand("attackboss", "Attack the active boss")]
        public async Task AttackBoss()
        {
            var bosses = _bossService.GetAllActiveBosses();

            if (bosses.Count == 0)
            {
                await RespondAsync("❌ There are no active bosses.");
                return;
            }

            var boss = bosses.First();

            var player = await _playerService.GetOrCreatePlayerAsync(
                Context.User.Id,
                Context.User.GlobalName
            );

            // ⏱️ COOLDOWN
            if (!string.IsNullOrEmpty(player.LastBossAttack))
            {
                if (DateTime.TryParse(player.LastBossAttack, null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var lastAttack))
                {
                    var timeSince = DateTime.UtcNow - lastAttack;

                    if (timeSince.TotalSeconds < ATTACK_COOLDOWN_SECONDS)
                    {
                        int remaining = (int)(ATTACK_COOLDOWN_SECONDS - timeSince.TotalSeconds);
                        await RespondAsync($"⏳ Wait {remaining}s before attacking again.");
                        return;
                    }
                }
            }

            player.LastBossAttack = DateTime.UtcNow.ToString("o");

            var result = _bossService.AttackBoss(
                boss.Definition.Id,
                Context.User.Id,
                player.Attack,
                player.MaxHealth
            );

            // SELF DAMAGE
            if (result.selfDamage > 0)
            {
                player.Health -= result.selfDamage;

                if (player.Health < 0)
                    player.Health = 0;

                if (player.Health <= 0)
                {
                    player.Gold = Math.Max(0, player.Gold - 250);
                    player.XP = 0;
                    player.Health = 1;

                    await _playerRepository.UpdatePlayerAsync(player);

                    await RespondAsync($"""
💀 You were defeated!

-250 Gold
XP reset

💰 {player.Gold} | 📊 {player.XP}
❤️ {player.Health}/{player.MaxHealth}
""");
                    return;
                }
            }

            await _playerRepository.UpdatePlayerAsync(player);

            // 🏆 BOSS DEATH (delegated to service)
            if (boss.CurrentHealth <= 0)
            {
                var message = await _bossService.HandleBossDeathAsync(boss);
                _bossService.RemoveBoss(boss.Definition.Id);

                await RespondAsync(message);
                return;
            }

            string hpBar = GetHealthBar(boss.CurrentHealth, boss.Definition.MaxHealth);

            await RespondAsync($"""
🎯 Attacking **{boss.Definition.Name}** ({boss.Definition.Type})

{result.resultText}

❤️ {player.Health}/{player.MaxHealth}

Boss HP:
{hpBar} {boss.CurrentHealth}/{boss.Definition.MaxHealth}
""");
        }

        // =========================
        // EMBED
        // =========================

        private Embed BuildBossEmbed(ActiveBoss boss)
        {
            var def = boss.Definition;

            string typeLabel = def.Type == BossType.Weekly
                ? "👑 Weekly Boss"
                : "⚔ Daily Boss";

            return new EmbedBuilder()
                .WithTitle($"🔥 {def.Name} has appeared!")
                .WithDescription($"{typeLabel}\n\n{def.Description}")

                .AddField("❤️ Health",
                    $"{GetHealthBar(boss.CurrentHealth, def.MaxHealth)}\n{boss.CurrentHealth}/{def.MaxHealth}",
                    true)

                .AddField("🛡 Defense", def.Defense, true)
                .AddField("💰 Reward", $"{def.RewardGold} Gold", true)

                .AddField("⚔ Abilities",
                    string.IsNullOrWhiteSpace(def.AbilitiesText)
                        ? "Unknown..."
                        : def.AbilitiesText)

                .WithImageUrl(def.ImageUrl)
                .WithColor(def.Type == BossType.Weekly ? Color.Gold : Color.DarkRed)
                .WithFooter("⏳ Defeat the boss before it escapes!")
                .WithCurrentTimestamp()
                .Build();
        }

        private string GetHealthBar(int current, int max)
        {
            int bars = 10;
            double percent = (double)current / max;
            int filled = (int)(percent * bars);

            return new string('█', filled) + new string('░', bars - filled);
        }
    }
}
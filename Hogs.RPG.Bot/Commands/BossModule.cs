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

        public BossModule(
            BossService bossService,
            PlayerService playerService,
            PlayerRepository playerRepository)
        {
            _bossService = bossService;
            _playerService = playerService;
            _playerRepository = playerRepository;
        }

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
                var embed = _bossService.BuildBossEmbed(boss);

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

        [SlashCommand("attackboss", "Attack the active boss")]
        public async Task AttackBoss()
        {
            await DeferAsync(ephemeral: true);

            var boss = _bossService.GetAllActiveBosses().FirstOrDefault();

            if (boss == null)
            {
                await ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "❌ There are no active bosses.";
                });
                return;
            }

            var result = await _bossService.AttackBossAsync(
                boss.Definition.Id,
                Context.User.Id
            );

            // Reload updated player
            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);

            // =========================
            // PLAYER DEATH
            // =========================
            if (player.Health <= 0)
            {
                player.Gold = Math.Max(0, player.Gold - 250);
                player.XP = 0;
                player.Health = 1;

                await _playerRepository.UpdatePlayerAsync(player);

                await ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = $"""
💀 You were defeated!

-250 Gold
XP reset

💰 {player.Gold} | 📊 {player.XP}
❤️ {player.Health}/{player.MaxHealth}
""";
                });

                return;
            }

            // Update boss UI
            await _bossService.TryUpdateBossMessageAsync(boss, Context.Client);

            // Boss death
            if (boss.CurrentHealth <= 0)
            {
                var message = await _bossService.HandleBossDeathAsync(boss);
                _bossService.RemoveBoss(boss.Definition.Id);

                await ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = message;
                });

                return;
            }

            // Normal response
            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = $"""
🎯 {result.text}

❤️ {player.Health}/{player.MaxHealth}
""";
            });
        }
    }
}
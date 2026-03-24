using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Data.Repositories;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // =========================
        // VIEW BOSSES
        // =========================
        [SlashCommand("bosses", "View all active bosses")]
        public async Task GetActiveBosses()
        {
            var bosses = _bossService.GetAllActiveBosses();

            if (bosses.Count == 0)
            {
                await RespondAsync("❌ There are currently no active bosses.", ephemeral: true);
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

        // =========================
        // ATTACK BOSS (NO FEEDBACK)
        // =========================
        [SlashCommand("attackboss", "Attack the active boss")]
        public async Task AttackBoss()
        {
            // Required by Discord to prevent timeout
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

            // 🔥 Instant attack (no queue, no response spam)
            await _bossService.AttackAsync(boss.Definition.Id, Context.User.Id);

            // ✅ No follow-up message
        }
    }
}
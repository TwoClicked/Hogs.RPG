using Discord.Interactions;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.PlayerServices;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.InteractionModels
{
    public class BossInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BossService _bossService;
        private readonly PlayerService _playerService;
        private readonly PlayerRepository _playerRepository;
        private readonly HealService _healService;

        public BossInteractionModule(
            BossService bossService,
            PlayerService playerService,
            PlayerRepository playerRepository,
            HealService healService)
        {
            _bossService = bossService;
            _playerService = playerService;
            _playerRepository = playerRepository;
            _healService = healService;
        }

        [ComponentInteraction("boss_attack:*")]
        public async Task Attack(string bossId)
        {
            await DeferAsync(ephemeral: true);

            var boss = _bossService.GetActiveBoss(bossId);

            if (boss == null)
            {
                await FollowupAsync(BuildPanel(
                    "❌ Boss not found",
                    "The boss has already disappeared."
                ), ephemeral: true);
                return;
            }

            var result = await _bossService.AttackBossAsync(
                bossId,
                Context.User.Id
            );

            // Reload updated player
            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);

            // =========================
            // PLAYER DEATH
            // =========================
            if (player.Health <= 0)
            {
                player.Gold = System.Math.Max(0, player.Gold - 250);
                player.XP = 0;
                player.Health = 1;

                await _playerRepository.UpdatePlayerAsync(player);

                await FollowupAsync(BuildPanel(
                    "💀 You were defeated",
                    $"""
-250 Gold
XP reset

❤️ {player.Health}/{player.MaxHealth}
💰 {player.Gold} | 📊 {player.XP}
"""
                ), ephemeral: true);

                return;
            }

            // Update boss UI
            await _bossService.TryUpdateBossMessageAsync(boss, Context.Client);

            // Boss death
            if (boss.CurrentHealth <= 0)
            {
                var message = await _bossService.HandleBossDeathAsync(boss);
                _bossService.RemoveBoss(bossId);

                await FollowupAsync(message, ephemeral: false);
                return;
            }

            // Normal response
            await FollowupAsync(BuildPanel(
                "🎯 Attack Result",
                $"""
{result.text}

❤️ {player.Health}/{player.MaxHealth}
"""
            ), ephemeral: true);
        }

        [ComponentInteraction("boss_heal:*")]
        public async Task Heal(string bossId)
        {
            await DeferAsync(ephemeral: true);

            var result = await _healService.HealAsync(Context.User.Id);

            if (!result.IsSuccess)
            {
                await FollowupAsync(BuildPanel(
                    "❌ Heal Failed",
                    result.Message
                ), ephemeral: true);
                return;
            }

            await FollowupAsync(BuildPanel(
                "🧪 Healing Successful",
                $"""
❤️ {result.Health}/{result.MaxHealth}
🧪 {result.RemainingPotions} potions left
"""
            ), ephemeral: true);
        }

        private string BuildPanel(string title, string content)
        {
            return $"""
━━━━━━━━━━━━━━━━━━
**{title}**

{content}
━━━━━━━━━━━━━━━━━━
""";
        }
    }
}
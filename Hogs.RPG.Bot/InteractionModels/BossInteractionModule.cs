using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.InteractionModels
{
    public class BossInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BossService _bossService;
        private readonly PlayerService _playerService;
        private readonly PlayerRepository _playerRepository;
        private readonly HealService _healService;
        private readonly StatService _statService;
        private readonly InventoryService _inventoryService;

        public BossInteractionModule(
            BossService bossService,
            PlayerService playerService,
            PlayerRepository playerRepository,
            HealService healService,
            StatService statService,
            InventoryService inventoryService)
        {
            _bossService = bossService;
            _playerService = playerService;
            _playerRepository = playerRepository;
            _healService = healService;
            _statService = statService;
            _inventoryService = inventoryService;
        }

        // =========================
        // ATTACK (NO FEEDBACK)
        // =========================
        [ComponentInteraction("boss_attack:*")]
        public async Task Attack(string bossId)
        {
            var component = (SocketMessageComponent)Context.Interaction;

            // Required to avoid timeout
            await component.DeferAsync(ephemeral: true);

            var boss = _bossService.GetActiveBoss(bossId);

            if (boss == null)
            {
                await FollowupAsync(BuildPanel(
                    "❌ Boss not found",
                    "The boss has already disappeared."
                ), ephemeral: true);
                return;
            }

            // 🔥 Instant attack (no queue, no spam)
            await _bossService.AttackAsync(bossId, Context.User.Id);

            // ✅ No feedback
        }

        // =========================
        // UI PANEL
        // =========================
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
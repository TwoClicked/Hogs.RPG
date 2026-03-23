using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System.Linq;
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

        private static readonly Dictionary<ulong, DateTime> _lastFeedback = new();

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

        [ComponentInteraction("boss_attack:*")]
        public async Task Attack(string bossId)
        {
            var component = (SocketMessageComponent)Context.Interaction;

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

            var (damage, selfDamage, text, isDead) =
                await _bossService.AttackBossAsync(bossId, Context.User.Id);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);

            if (player == null)
            {
                await FollowupAsync(BuildPanel(
                    "❌ No character found",
                    "You need to start your adventure first."
                ), ephemeral: true);
                return;
            }

            var (_, _, maxHealth) = _statService.CalculateStats(player);

            // =========================
            // PLAYER DEATH / AUTO SAVE
            // =========================
            if (player.Health <= 0)
            {
                var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);
                var potion = inventory.FirstOrDefault(i => i.ItemId == "health_potion");

                if (potion != null && potion.Quantity >= 2)
                {
                    var healResult = await _healService.HealAsync(Context.User.Id);
                    await _inventoryService.TakeItemAsync(Context.User.Id, "health_potion", 1);

                    await FollowupAsync(BuildPanel(
                        "🧪 Last Second Save!",
                        $"""
You were about to die...

🧪 You consumed **2 Health Potions** to survive!

❤️ {healResult.Health}/{healResult.MaxHealth}
🧪 {healResult.RemainingPotions - 1} potions left
"""
                    ), ephemeral: true);

                    return;
                }

                player.Gold = Math.Max(0, player.Gold - 250);
                player.XP = 0;
                player.Health = 1;

                await _playerRepository.UpdatePlayerAsync(player);

                var (_, _, maxHp) = _statService.CalculateStats(player);

                await FollowupAsync(BuildPanel(
                    "💀 You were defeated",
                    $"""
-250 Gold
XP reset

❤️ {player.Health}/{maxHp}
💰 {player.Gold} | 📊 {player.XP}
"""
                ), ephemeral: true);

                return;
            }

            var userId = Context.User.Id;

            bool shouldSendFeedback = !_lastFeedback.TryGetValue(userId, out var last)
                || (DateTime.UtcNow - last).TotalSeconds >= 2;

            if (shouldSendFeedback)
            {
                _lastFeedback[userId] = DateTime.UtcNow;

                await FollowupAsync(BuildPanel(
                                "⚔️ Combat",
                                $"""
            {text ?? "You attack the boss."}
            
            ❤️ {player.Health}/{maxHealth}
            """
                            ), ephemeral: true);
            }
        }

        [ComponentInteraction("boss_heal:*")]
        public async Task Heal(string bossId)
        {
            var component = (SocketMessageComponent)Context.Interaction;

            await component.DeferAsync(ephemeral: true);

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
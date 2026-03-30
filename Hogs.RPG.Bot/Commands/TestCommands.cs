using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PlayerServices;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hogs.RPG.Core.Entities.BossDefinition;

namespace Hogs.RPG.Bot.Commands
{
    public class TestCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private const ulong ADMIN_ROLE_ID = 1483528182106685691;

        private readonly PlayerService _playerService;
        private readonly BossService _bossService;
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;

        public TestCommands(
            BossService bossService,
            PlayerService playerService,
            PlayerRepository playerRepository,
            InventoryService inventoryService)
        {
            _bossService = bossService;
            _playerService = playerService;
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
        }

        // =========================
        // ADMIN CHECK (INLINE)
        // =========================
        private async Task<bool> EnsureAdminAsync()
        {
            if (Context.User is not SocketGuildUser user ||
                !user.Roles.Any(r => r.Id == ADMIN_ROLE_ID))
            {
                await RespondAsync("🔒 These are admin-only commands.", ephemeral: true);
                return false;
            }

            return true;
        }

        // =========================
        // TEST LOAD
        // =========================

        [SlashCommand("testbosses", "Test loading bosses (Admin Only)")]
        public async Task TestBosses()
        {
            if (!await EnsureAdminAsync()) return;

            await DeferAsync(ephemeral: true);

            var bosses = GlobalBossRegistry.GetByType(BossType.Daily);

            if (bosses.Count == 0)
            {
                await FollowupAsync("No bosses found.", ephemeral: true);
                return;
            }

            var builder = new StringBuilder();

            foreach (var boss in bosses)
            {
                builder.AppendLine($"{boss.Name} | HP: {boss.MaxHealth} | Type: {boss.Type}");
            }

            await FollowupAsync(builder.ToString(), ephemeral: true);
        }


        // =========================
        // FORCE DAILY
        // =========================

        [SlashCommand("forcedaily", "Force spawn a daily boss (Admin Only)")]
        public async Task ForceDailyBoss(string bossId)
        {
            if (!await EnsureAdminAsync()) return;

            await DeferAsync();

            try
            {
                var boss = await _bossService.SpawnBoss(bossId);

                if (boss == null)
                {
                    await FollowupAsync($"❌ Boss with ID `{bossId}` not found.", ephemeral: true);
                    return;
                }

                var embed = _bossService.BuildBossEmbed(boss);

                var components = new ComponentBuilder()
                    .WithButton("⚔ Attack", $"boss_attack:{boss.Definition.Id}", ButtonStyle.Danger)
                    .WithButton("🧪 Heal", $"boss_heal:{boss.Definition.Id}", ButtonStyle.Success)
                    .Build();

                var msg = await FollowupAsync(
                    $"⚔ A Daily Boss has been spawned!\n**ID:** `{bossId}`",
                    embed: embed,
                    components: components
                );

                // 🔥 IMPORTANT
                boss.ChannelId = Context.Channel.Id;
                boss.MessageId = msg.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 COMMAND CRASH: {ex}");
                await FollowupAsync("💥 Something went wrong spawning the boss.", ephemeral: true);
            }
        }


        // =========================
        // GIVE ITEM
        // =========================

        [SlashCommand("giveitem", "Give yourself an item (Admin Only)")]
        public async Task GiveItem(string itemId, int amount)
        {
            if (!await EnsureAdminAsync()) return;

            var userId = Context.User.Id;

            await _inventoryService.GiveItemAsync(userId, itemId, amount);

            await RespondAsync($"You received {amount}x {itemId}", ephemeral: true);
        }

        // =========================
        // EMBED
        // =========================

        private Embed BuildBossEmbed(ActiveBoss boss)
        {
            var def = boss.Definition;

            string typeLabel = def.Type.ToString() == "Weekly"
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
                .WithColor(def.Type.ToString() == "Weekly" ? Color.Gold : Color.DarkRed)
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
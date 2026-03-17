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

namespace Hogs.RPG.Bot.Commands
{
    [RequireRole(1483528182106685691)]
    public class TestCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BossRepository _bossRepository;
        private readonly PlayerService _playerService;
        private readonly BossService _bossService;
        private readonly PlayerRepository _playerRepository;

        public TestCommands(
            BossRepository bossRepository,
            BossService bossService,
            PlayerService playerService,
            PlayerRepository playerRepository)
        {
            _bossRepository = bossRepository;
            _bossService = bossService;
            _playerService = playerService;
            _playerRepository = playerRepository;
        }

        // =========================
        // TEST LOAD
        // =========================

        [SlashCommand("testbosses", "Test loading bosses")]
        public async Task TestBosses()
        {
            await DeferAsync();

            var bosses = await _bossRepository.GetAllAsync();

            if (bosses.Count == 0)
            {
                await FollowupAsync("No bosses found.");
                return;
            }

            var builder = new StringBuilder();

            foreach (var boss in bosses)
            {
                builder.AppendLine($"{boss.Name} | HP: {boss.MaxHealth} | Type: {boss.Type}");
            }

            await FollowupAsync(builder.ToString());
        }

        // =========================
        // FORCE WEEKLY
        // =========================

        [SlashCommand("forceweekly", "Force spawn a weekly boss")]
        public async Task ForceWeeklyBoss()
        {
            await DeferAsync();

            var boss = await _bossService.SpawnWeeklyBoss();

            if (boss == null)
            {
                await FollowupAsync("❌ No weekly boss found.");
                return;
            }

            var embed = BuildBossEmbed(boss);

            await FollowupAsync(
                "@BossRaid 🔥 A Weekly Boss has been spawned!",
                embed: embed
            );
        }

        // =========================
        // FORCE DAILY
        // =========================

        [SlashCommand("forcedaily", "Force spawn a daily boss")]
        public async Task ForceDailyBoss(string bossId)
        {
            await DeferAsync();

            try
            {
                Console.WriteLine($"🔥 ForceDaily called with ID: {bossId}");

                var boss = await _bossService.SpawnBoss(bossId);

                Console.WriteLine($"🔥 Boss result: {(boss == null ? "NULL" : boss.Definition.Name)}");

                if (boss == null)
                {
                    await FollowupAsync($"❌ Boss with ID `{bossId}` not found.");
                    return;
                }

                var embed = BuildBossEmbed(boss);

                await FollowupAsync(
                    $"⚔ A Daily Boss has been spawned!\n**ID:** `{bossId}`",
                    embed: embed
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 COMMAND CRASH: {ex}");
                await FollowupAsync("💥 Something went wrong spawning the boss.");
            }
        }

        // =========================
        // CLEAR BOSSES (VERY USEFUL)
        // =========================

        [SlashCommand("clearbosses", "Remove all active bosses")]
        public async Task ClearBosses()
        {
            _bossService.ClearAllBosses();

            await RespondAsync("🧹 All active bosses have been cleared.");
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
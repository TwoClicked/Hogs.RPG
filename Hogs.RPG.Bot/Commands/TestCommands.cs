using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AchievementServices;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.RelicServices;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hogs.RPG.Core.Entities.GlobalBossObjects.BossDefinition;

namespace Hogs.RPG.Bot.Commands
{
    [BossLock]
    [GearSwapLock]
    public class TestCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private const ulong ADMIN_ROLE_ID = 1483528182106685691;

        private readonly PlayerService _playerService;
        private readonly BossService _bossService;
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly PetService _petService;
        private readonly RelicService _relicService;
        private readonly AchievementService _achievementService;

        public TestCommands(
            BossService bossService,
            PlayerService playerService,
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            PetService petService,
            RelicService relicService,
            AchievementService achievementService)
        {
            _bossService = bossService;
            _playerService = playerService;
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _petService = petService;
            _relicService = relicService;
            _achievementService = achievementService;
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
                builder.AppendLine($"{boss.Name} | HP: {boss.MaxHealth} | Type: {boss.Type}");

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

                boss.ChannelId = Context.Channel.Id;
                boss.MessageId = msg.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 COMMAND CRASH: {ex}");
                await FollowupAsync("💥 Something went wrong spawning the boss.", ephemeral: true);
            }
        }


        [SlashCommand("backfill-pets", "Backfill pet counters for all players (Admin Only)")]
        public async Task BackfillPets()
        {
            if (!await EnsureAdminAsync()) return;

            await DeferAsync(ephemeral: true);

            var players = await _playerRepository.GetAllPlayersAsync();
            int updated = 0;

            foreach (var player in players)
            {
                var pets = await _petService.GetPetsAsync(player.DiscordId);

                if (pets.Count == 0) continue;

                player.TotalPetsOwned = pets.Count;
                player.HighestPetLevel = pets.Max(p => p.Level);
                player.HuntingCompanionUnlocked = player.HasHuntingPet;

                await _playerRepository.UpdatePlayerAsync(player);
                updated++;
            }

            await FollowupAsync($"✅ Backfilled pet data for **{updated}** players.", ephemeral: true);
        }

        [SlashCommand("backfill-capytara", "Award the Evolution achievement to existing CapyTara owners (Admin Only)")]
        public async Task BackfillCapytara()
        {
            if (!await EnsureAdminAsync()) return;

            await DeferAsync(ephemeral: true);

            var players = await _playerRepository.GetAllPlayersAsync();
            int fixedCount = 0;

            foreach (var player in players)
            {
                if (player.CapyTaraEvolved) continue;

                var pets = await _petService.GetPetsAsync(player.DiscordId);
                if (!pets.Any(p => p.PetId == "capytara")) continue;

                player.CapyTaraEvolved = true;
                await _playerRepository.UpdatePlayerAsync(player);
                await _achievementService.CheckAndAwardAsync(player.DiscordId);
                fixedCount++;
            }

            await FollowupAsync($"✅ Backfilled CapyTara evolution flag for **{fixedCount}** players.", ephemeral: true);
        }

        // =========================
        // GIVE ITEM
        // =========================
        [SlashCommand("giveitem", "Give yourself an item (Admin Only)")]
        public async Task GiveItem(string itemId, int amount)
        {
            if (!await EnsureAdminAsync()) return;

            await _inventoryService.GiveItemAsync(Context.User.Id, itemId, amount);

            await RespondAsync($"✅ You received {amount}x `{itemId}`", ephemeral: true);
        }

        // =========================
        // GIVE PET
        // =========================
        [SlashCommand("givepet", "Give yourself a pet by ID (Admin Only)")]
        public async Task GivePet(string petId)
        {
            if (!await EnsureAdminAsync()) return;

            await DeferAsync(ephemeral: true);

            if (!PetRegistry.All.TryGetValue(petId, out var petDef))
            {
                await FollowupAsync(
                    $"❌ Pet `{petId}` not found.\n\n**Valid pet IDs:**\n{string.Join("\n", PetRegistry.All.Keys.Select(k => $"`{k}`"))}",
                    ephemeral: true);
                return;
            }

            await _petService.GivePetAsync(Context.User.Id, petId);

            await FollowupAsync($"✅ {petDef.Icon} **{petDef.Name}** added to your pet bag!", ephemeral: true);
        }

        // =========================
        // GIVE RAID KEYS TO ALL PLAYERS
        // =========================
        [SlashCommand("giveallraidkeys", "Give 10 of each raid key to all players (Admin Only)")]
        public async Task GiveAllRaidKeys()
        {
            if (!await EnsureAdminAsync()) return;
            await DeferAsync(ephemeral: true);

            var players = await _playerRepository.GetAllPlayersAsync();
            int count = 0;

            foreach (var player in players)
            {
                for (int tier = 1; tier <= 5; tier++)
                {
                    await _inventoryService.GiveItemAsync(player.DiscordId, $"raid_key_t{tier}", 10);
                }
                count++;
            }

            await FollowupAsync($"✅ Gave 10x of each raid key to **{count}** players.");
        }

        // =========================
        // GIVE RELIC SHARD
        // =========================
        [SlashCommand("giverelicshard", "Give yourself a relic shard (Admin Only)")]
        public async Task GiveRelicShard(int tier)
        {
            if (!await EnsureAdminAsync()) return;
            await DeferAsync(ephemeral: true);

            if (tier < 1 || tier > 5)
            {
                await FollowupAsync("❌ Tier must be between 1 and 5.", ephemeral: true);
                return;
            }

            await _relicService.GiveShardAsync(Context.User.Id, tier);

            await FollowupAsync($"✅ Given 1x Tier {tier} Relic Shard.", ephemeral: true);
        }

        // =========================
        // MIGRATE ACHIEVEMENTS
        // =========================
        [SlashCommand("migrate-achievements", "Run the retroactive achievement migration (Admin Only)")]
        public async Task MigrateAchievements()
        {
            if (!await EnsureAdminAsync()) return;

            await DeferAsync(ephemeral: true);

            await FollowupAsync("⏳ Starting retroactive migration... check Railway logs for progress.", ephemeral: true);

            _ = Task.Run(async () =>
            {
                await _achievementService.RunRetroactiveMigrationAsync();
            });
        }
    }
}

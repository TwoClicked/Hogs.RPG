using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services;
using Hogs.RPG.Services.AchievementServices;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.GatheringServices;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.RelicServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    [BossLock]
    public class PlayerCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PlayerService _playerService;
        private readonly PlayerRepository _playerRepository;
        private readonly EquipService _equipService;
        private readonly StatService _statService;
        private readonly EquipmentService _equipmentService;
        private readonly EnergyService _energyService;
        private readonly HunterStaminaService _hunterStaminaService;
        private readonly DiscordSocketClient _client;
        private readonly PetService _petService;
        private readonly RelicService _relicService;
        private readonly LeaderboardService _leaderboardService;
        private readonly AchievementService _achievementService;
        public PlayerCommands(
            PlayerService playerService,
            PlayerRepository playerRepository,
            EquipService equipService,
            StatService statService,
            EquipmentService equipmentService,
            EnergyService energyService,
            HunterStaminaService hungerStaminaService,
            DiscordSocketClient client,
            PetService petService,
            RelicService relicService,
            LeaderboardService leaderboardService,
            AchievementService achievementService)
        {
            _playerService = playerService;
            _playerRepository = playerRepository;
            _equipService = equipService;
            _statService = statService;
            _equipmentService = equipmentService;
            _energyService = energyService;
            _hunterStaminaService = hungerStaminaService;
            _client = client;
            _petService = petService;
            _relicService = relicService;
            _leaderboardService = leaderboardService;
            _achievementService = achievementService;
        }

        // =========================
        // START ADVENTURE
        // =========================
        [SlashCommand("startadventure", "Begin your adventure")]
        public async Task StartAdventure()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);

            if (player != null)
            {
                await FollowupAsync(
                    "⚔ Your adventure begins!\n\n" +
                    "🐾 You feel a presence... a **Verdant Wisp** has bonded with you.\n\n" +
                    "Use `/hunt` to begin gathering resources.",
                    ephemeral: true);
                return;
            }

            var newPlayer = new Player
            {
                DiscordId = Context.User.Id,
                Username = Context.User.Username,
                Level = 1,
                XP = 0,
                Gold = 0,
                Attack = 5,
                Defense = 5,
                Health = 100,
                MaxHealth = 100,

                HunterStamina = 100,
                LastHunterStaminaUpdate = DateTimeOffset.UtcNow.ToString("o"),

                Energy = 100,
                LastEnergyUpdate = DateTimeOffset.UtcNow.ToString("o")
            };

            await _playerRepository.CreatePlayerAsync(newPlayer);

            // 🐾 Give starter pet
            await _petService.GivePetAsync(Context.User.Id, "verdant_cat");
            await _petService.EquipPetAsync(Context.User.Id, "verdant_cat");

            // 🎭 Assign RPG role
            var guild = _client.GetGuild(Context.Guild.Id);
            var user = guild.GetUser(Context.User.Id);
            var role = guild.GetRole(1485387222822948934);

            if (user != null && role != null)
                await user.AddRoleAsync(role);

            await FollowupAsync(
                "⚔ Your adventure begins!\n\nWelcome to **HOGS RPG**.\nUse `/hunt` to begin gathering resources.",
                ephemeral: true);
        }

        // =========================
        // PROFILE
        // =========================
        [SlashCommand("profile", "View your character")]
        public async Task Profile()
        {
            if (Context.Channel.Id != 1486017679016857752UL)
            {
                await RespondAsync("❌ This command can only be used in <#1486017679016857752>.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            var pet = await _petService.GetEquippedPetAsync(Context.User.Id);

            if (player == null)
            {
                await ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "You haven't started your adventure yet. Use `/startadventure`.";
                });
                return;
            }

            _energyService.RegenerateEnergy(player);
            _hunterStaminaService.Regenerate(player);

            // =========================
            // ACTIVE BOOSTS
            // =========================
            bool profileXpBoost = player.XpBoostExpiry.HasValue && player.XpBoostExpiry.Value > DateTime.UtcNow;
            bool profileStaminaBoost = player.StaminaBoostExpiry.HasValue && player.StaminaBoostExpiry.Value > DateTime.UtcNow;

            string xpBoostText = profileXpBoost
                ? $" Active until {player.XpBoostExpiry!.Value:dd MMM HH:mm} UTC"
                : "❌ Inactive";

            string staminaBoostText = profileStaminaBoost
                ? $" Active until {player.StaminaBoostExpiry!.Value:dd MMM HH:mm} UTC"
                : "❌ Inactive";

            int staminaMax = profileStaminaBoost ? 150 : 100;

            // =========================
            // XP BAR
            // =========================
            int xpRequired = player.Level * player.Level * 100;
            double progress = (double)player.XP / xpRequired;
            int filledBars = (int)(progress * 10);
            string xpBar = new string('█', filledBars) + new string('░', 10 - filledBars);

            var stats = _statService.CalculateStats(player);

            int bonusAttack = stats.attack - player.Attack;
            int bonusDefense = stats.defense - player.Defense;
            int bonusHealth = stats.health - player.MaxHealth;

            // =========================
            // EMBED — fields 1-17 (well under Discord's 25 limit after pet section below)
            // =========================
            var embed = new EmbedBuilder()
                .WithTitle($"⚔ {player.Username}'s Profile")
                .WithColor(Color.DarkRed)

                // Row 1 — economy
                .AddField("Level", player.Level, true)
                .AddField("💰 Gold", player.Gold, true)
                .AddField("🪙 Tracker Tokens", player.TrackerTokens, true)

                // XP
                .AddField("XP", $"{player.XP} / {xpRequired}", false)
                .AddField("Progress", xpBar, false)

                // Row — combat stats
                .AddField("Attack", $"🗡 {stats.attack} (+{bonusAttack})", true)
                .AddField("Defense", $"🛡 {stats.defense} (+{bonusDefense})", true)
                .AddField("Health", $"❤️ {player.Health}/{stats.health} (+{bonusHealth})", true)

                // Row — resources
                .AddField("Energy", $"⚡ {player.Energy}", true)
                .AddField("Hunter Stamina", $"🏹 {player.HunterStamina}/{staminaMax}", true)

                // Row — boosts (blank fills third slot so they sit correctly)
                .AddField("✨ Double XP", xpBoostText, true)
                .AddField("⚡ Stamina Boost", staminaBoostText, true)
                .AddField("\u200b", "\u200b", true)

                // Hunt bonuses
                .AddField("🌟 Hunter Set",
                    player.HasHunterSetBonus
                        ? "✅ Complete — +4.5% XP, +4.5% materials, +5% rare drop on all hunts"
                        : "❌ Incomplete — collect all 9 pieces and use `/hunter-setcomplete`",
                    false)
                .AddField("🐾 Hunting Companion",
                    player.HasHuntingPet
                        ? "✅ Active — +5% XP, +5% materials, +3% rare drop on all hunts"
                        : "❌ Not found — discover it on the Ashwood Trail",
                    false)
                .AddField("⚒️ Smithing Level", $"Level **{player.SmithingLevel}** — {player.SmithingXP:N0} XP", true)
                .AddField("🧪 Alchemist Level", $"Level **{player.AlchemistLevel}** — {player.AlchemistXP:N0} XP", true)
                .AddField("🏆 Achievements", $"**{player.AchievementCount}** earned", true)

                // Gear
                .AddField(
                    "⚒ Equipment",
                    $"Main Hand: {FormatItem(player.MainHand)}\n" +
                    $"Off Hand:  {FormatItem(player.OffHand)}\n" +
                    $"Helmet:    {FormatItem(player.Helmet)}\n" +
                    $"Body:      {FormatItem(player.Body)}\n" +
                    $"Legs:      {FormatItem(player.Legs)}\n" +
                    $"Gloves:    {FormatItem(player.Gloves)}\n" +
                    $"Boots:     {FormatItem(player.Boots)}\n" +
                    $"Ring:      {FormatItem(player.Ring)}\n" +
                    $"Amulet:    {FormatItem(player.Amulet)}",
                    false);



            // =========================
            // RELICS — field 18
            // =========================
            var equippedRelics = await _relicService.GetEquippedRelicsAsync(player.DiscordId);

            var slot1Relic = equippedRelics.FirstOrDefault(r => r.SlotIndex == 0);
            var slot2Relic = equippedRelics.FirstOrDefault(r => r.SlotIndex == 1);

            string relicSlot1 = slot1Relic != null
                ? $"{RelicRegistry.Get(slot1Relic.RelicId).Name} (Rank {slot1Relic.Rank}) — {_relicService.FormatBonus(slot1Relic.BonusType)}"
                : "*Empty*";

            string relicSlot2 = slot2Relic != null
                ? $"{RelicRegistry.Get(slot2Relic.RelicId).Name} (Rank {slot2Relic.Rank}) — {_relicService.FormatBonus(slot2Relic.BonusType)}"
                : "*Empty*";

            embed.AddField("💎 Relics",
                $"Slot 1: {relicSlot1}\nSlot 2: {relicSlot2}",
                false);

            // =========================
            // PET — fields 19-21 (collapsed from 9 to 3 to stay under the 25-field limit)
            // =========================
            if (pet != null && PetRegistry.All.TryGetValue(pet.PetId, out var def))
            {
                var (atk, defStat, hp) = _petService.CalculateStats(pet);

                int xpRequiredPet = 20 + (pet.Level * pet.Level * 15);
                double petProgress = (double)pet.XP / xpRequiredPet;
                int filled = (int)(petProgress * 10);
                string petBar = new string('█', filled) + new string('░', 10 - filled);

                string displayName = pet.CustomName ?? def.Name;

                embed
                    .AddField("🐾 Pet",
                        $"{def.Icon} **{displayName}** — Lv. {pet.Level}\n" +
                        $"{petBar} ({pet.XP} / {xpRequiredPet} XP)",
                        false)
                    .AddField("Stats",
                        $"🗡 {atk} ATK · 🛡 {defStat} DEF · ❤️ {hp} HP",
                        false)
                    .AddField("Passives",
                        $"{PetPassiveFormatter.Format(pet.Passive1)} · {PetPassiveFormatter.Format(pet.Passive2)}",
                        false);
            }
            else
            {
                embed.AddField("🐾 Pet", "No pet equipped", false);
            }

            // Total fields with pet: 21. Without pet: 19. Both safely under Discord's 25-field limit.
            await FollowupAsync(embed: embed.Build());
        }

        // =========================
        // MY STATS
        // =========================
        [SlashCommand("mystats", "See your stats and leaderboard rankings")]
        public async Task MyStats()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await ModifyOriginalResponseAsync(msg =>
                    msg.Content = "⚠️ You haven't started your adventure yet. Use `/startadventure`.");
                return;
            }

            var ranks = await _leaderboardService.GetPlayerRanksAsync(Context.User.Id);

            string Rank(int r) => r <= 3
                ? r switch { 1 => "👑 #1", 2 => "🥈 #2", 3 => "🥉 #3", _ => "" }
                : $"#{r} / {ranks.TotalPlayers}";

            var stats = _statService.CalculateStats(player);
            int gearScore = stats.attack + stats.defense + stats.health;

            // Pet score for display
            var equippedPet = await _petService.GetEquippedPetAsync(Context.User.Id);
            string petPowerValue = equippedPet != null
                ? $"{_petService.GetPetGearScore(equippedPet):N0}\n{Rank(ranks.PetGearScore)}"
                : $"No pet\n{Rank(ranks.PetGearScore)}";

            var embed = new EmbedBuilder()
                .WithTitle($"📊 {player.Username}'s Stats & Rankings")
                .WithColor(new Color(0x5865F2))
                .WithFooter("Only visible to you • Rankings update live")

                // Row 1
                .AddField("💰 Gold", $"{player.Gold:N0}\n{Rank(ranks.Gold)}", true)
                .AddField("📈 Level", $"Lv. {player.Level}\n{Rank(ranks.Level)}", true)
                .AddField("⚔️ Gear Score", $"{gearScore:N0}\n{Rank(ranks.GearScore)}", true)

                // Row 2
                .AddField("🏰 Dungeons", $"{player.DungeonRunsCompleted}\n{Rank(ranks.DungeonRuns)}", true)
                .AddField("⚔️ Raids", $"{player.RaidsCompleted}\n{Rank(ranks.Raids)}", true)
                .AddField("💥 Boss Damage", $"{player.TotalBossDamage:N0}\n{Rank(ranks.BossDamage)}", true)

                // Row 3
                .AddField("💀 Deaths", $"{player.Deaths}\n{Rank(ranks.Deaths)}", true)
                .AddField("🏕️ Trails", $"{player.TrailsCompleted}\n{Rank(ranks.Trails)}", true)
                .AddField("🐾 Pet Power", petPowerValue, true)

                // Row 4
                .AddField("⚒️ Smithing", $"Lv. {player.SmithingLevel}\n{Rank(ranks.SmithingLevel)}", true)
                .AddField("🏆 Achievements", $"{player.AchievementCount}\n{Rank(ranks.AchievementCount)}", true)
                .AddField("\u200b", "\u200b", true);



            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = null;
                msg.Embed = embed.Build();
            });
        }

        [SlashCommand("migrate-achievements", "Run the retroactive achievement migration for all players")]
        [RequireRole("Admin")]
        public async Task MigrateAchievements()
        {
            await DeferAsync(ephemeral: true);

            await FollowupAsync("⏳ Starting retroactive migration... check Railway logs for progress.", ephemeral: true);

            _ = Task.Run(async () =>
            {
                await _achievementService.RunRetroactiveMigrationAsync();
            });
        }


        // =========================
        // FORMAT ITEM HELPER
        // =========================
        private string FormatItem(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "*None*";

            var equip = _equipmentService.GetEquipment(id);
            if (equip == null)
                return id;

            InventoryItemDefinitions.All.TryGetValue(id, out var itemDef);

            var stats = new List<string>();
            if (equip.Attack > 0) stats.Add($"+{equip.Attack} ATK");
            if (equip.Defense > 0) stats.Add($"+{equip.Defense} DEF");
            if (equip.Health > 0) stats.Add($"+{equip.Health} HP");

            var statText = stats.Count > 0 ? $" ({string.Join(", ", stats)})" : "";

            var icon = itemDef != null && !string.IsNullOrWhiteSpace(itemDef.Icon)
                ? $"{itemDef.Icon} "
                : "";

            var name = itemDef?.Name ?? equip.Name;

            return $"{icon}{name}{statText}";
        }

        // =========================
        // EQUIP
        // =========================
        [SlashCommand("equip", "Equip an item")]
        public async Task Equip(
            [Autocomplete(typeof(EquipSlotAutocompleteHandler))] string slot,
            [Autocomplete(typeof(EquipBySlotAutocompleteHandler))] string itemId)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);

            if (player == null)
            {
                await RespondAsync("⚠️ You need to start your adventure first with `/startadventure`.", ephemeral: true);
                return;
            }

            var (preview, validItemId) = await _equipService.GetEquipPreviewAsync(Context.User.Id, itemId);

            if (validItemId == null)
            {
                await RespondAsync(preview, ephemeral: true);
                return;
            }

            var components = new ComponentBuilder()
                .WithButton("✅ Confirm", $"equip_confirm:{itemId}", ButtonStyle.Success)
                .WithButton("❌ Cancel", "equip_cancel", ButtonStyle.Secondary);

            await RespondAsync(preview, components: components.Build(), ephemeral: true);
        }

        // =========================
        // UNEQUIP
        // =========================
        [SlashCommand("unequip", "Unequip a piece of gear")]
        public async Task Unequip(
            [Autocomplete(typeof(UnequipAutocompleteHandler))] string slot)
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);

            if (player == null)
            {
                await RespondAsync("⚠️ You need to start your adventure first with `/startadventure`.", ephemeral: true);
                return;
            }

            var result = await _equipService.UnequipAsync(Context.User.Id, slot);
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // EQUIP CONFIRM / CANCEL
        // =========================
        [ComponentInteraction("equip_confirm:*")]
        public async Task ConfirmEquip(string itemId)
        {
            if (Context.Interaction is SocketMessageComponent component)
            {
                await component.UpdateAsync(msg =>
                {
                    msg.Content = "⏳ Equipping item...";
                    msg.Components = new ComponentBuilder().Build();
                });

                var result = await _equipService.EquipAsync(Context.User.Id, itemId);

                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = result;
                });
            }
        }

        [ComponentInteraction("equip_cancel")]
        public async Task CancelEquip()
        {
            if (Context.Interaction is SocketMessageComponent component)
            {
                await component.UpdateAsync(msg =>
                {
                    msg.Content = "❌ Equip cancelled.";
                    msg.Components = new ComponentBuilder().Build();
                });
            }
        }
    }
}
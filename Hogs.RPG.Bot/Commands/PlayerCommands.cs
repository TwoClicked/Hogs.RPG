using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.GatheringServices;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.PlayerServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
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

        public PlayerCommands(
            PlayerService playerService,
            PlayerRepository playerRepository,
            EquipService equipService,
            StatService statService,
            EquipmentService equipmentService,
            EnergyService energyService,
            HunterStaminaService hungerStaminaService,
            DiscordSocketClient client,
            PetService petService)
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
            await _petService.GivePetAsync(Context.User.Id, "verdant_wisp");
            await _petService.EquipPetAsync(Context.User.Id, "verdant_wisp");

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

            var embed = new EmbedBuilder()
                .WithTitle($"⚔ {player.Username}'s Profile")
                .WithColor(Color.DarkRed)

                .AddField("Level", player.Level, true)
                .AddField("Gold", $"💰 {player.Gold}", true)

                .AddField("XP", $"{player.XP} / {xpRequired}", false)
                .AddField("Progress", xpBar, false)

                .AddField("Attack", $"🗡 {stats.attack} (+{bonusAttack})", true)
                .AddField("Defense", $"🛡 {stats.defense} (+{bonusDefense})", true)
                .AddField("Health", $"❤️ {player.Health}/{stats.health} (+{bonusHealth})", true)

                .AddField("Energy", $"⚡ {player.Energy}", true)
                .AddField("Hunter Stamina", $"🏹 {player.HunterStamina}/{staminaMax}", true)

                .AddField("✨ Double XP", xpBoostText, false)
                .AddField("⚡ Stamina Boost", staminaBoostText, true)

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

            if (pet != null && PetRegistry.All.TryGetValue(pet.PetId, out var def))
            {
                var (atk, defStat, hp) = _petService.CalculateStats(pet);

                int xpRequiredPet = 20 + (pet.Level * pet.Level * 15);
                double petProgress = (double)pet.XP / xpRequiredPet;
                int filled = (int)(petProgress * 10);
                string petBar = new string('█', filled) + new string('░', 10 - filled);

                string displayName = pet.CustomName ?? def.Name;

                embed
                    .AddField("🐾 Pet", $"{def.Icon} {displayName}", false)
                    .AddField("Level", $"Lv. {pet.Level}", true)
                    .AddField("XP", $"{pet.XP} / {xpRequiredPet}", true)
                    .AddField("Progress", petBar, false)
                    .AddField("Attack", $"🗡 {atk}", true)
                    .AddField("Defense", $"🛡 {defStat}", true)
                    .AddField("Health", $"❤️ {hp}", true)
                    .AddField("Passive 1", PetPassiveFormatter.Format(pet.Passive1), true)
                    .AddField("Passive 2", PetPassiveFormatter.Format(pet.Passive2), true);
            }
            else
            {
                embed.AddField("🐾 Pet", "No pet equipped", false);
            }

            await FollowupAsync(embed: embed.Build());
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
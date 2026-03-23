using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.GatheringServices;
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

        public PlayerCommands(
            PlayerService playerService,
            PlayerRepository playerRepository,
            EquipService equipService,
            StatService statService,
            EquipmentService equipmentService,
            EnergyService energyService,
            HunterStaminaService hungerStaminaService)
        {
            _playerService = playerService;
            _playerRepository = playerRepository;
            _equipService = equipService;
            _statService = statService;
            _equipmentService = equipmentService;
            _energyService = energyService;
            _hunterStaminaService = hungerStaminaService;
        }

        [SlashCommand("startadventure", "Begin your adventure")]
        public async Task StartAdventure()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);

            if (player != null)
            {
                await FollowupAsync(
                    "⚔ Your adventure has already begun!\nUse `/profile` to view your character.",
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

                LastHunt = "",

                HunterStamina = 100,
                LastHunterStaminaUpdate = DateTimeOffset.UtcNow.ToString("o"),

                Energy = 100,
                LastEnergyUpdate = DateTimeOffset.UtcNow.ToString("o")
            };

            await _playerRepository.CreatePlayerAsync(newPlayer);

            await FollowupAsync(
                "⚔ Your adventure begins!\n\nWelcome to **HOGS RPG**.\nUse `/hunt` to begin gathering resources.",
                ephemeral: true);
        }

        [SlashCommand("profile", "View your character")]
        public async Task Profile()
        {
            await DeferAsync();

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);



            if (player == null)
            {
                await FollowupAsync("You haven't started your adventure yet. Use `/startadventure`.");
                return;
            }

            _energyService.RegenerateEnergy(player);
            _hunterStaminaService.Regenerate(player);

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
                .AddField("Hunter Stamina", $"⚡ {player.HunterStamina}", true)

                .AddField(
                    "⚒ Equipment",
                    $"Main Hand:{FormatItem(player.MainHand)}\n" +
                    $"Off Hand: {FormatItem(player.OffHand)}\n" +
                    $"Helmet:   {FormatItem(player.Helmet)}\n" +
                    $"Body:     {FormatItem(player.Body)}\n" +
                    $"Legs:     {FormatItem(player.Legs)}\n" +
                    $"Gloves:   {FormatItem(player.Gloves)}\n" +
                    $"Boots:    {FormatItem(player.Boots)}\n" +
                    $"Ring:     {FormatItem(player.Ring)}\n" +
                    $"Amulet:   {FormatItem(player.Amulet)}",
                    false)

                .WithFooter("HOGS RPG")
                .WithTimestamp(DateTime.UtcNow)
                .Build();

            await FollowupAsync(embed: embed);
        }

        private string FormatItem(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "*None*";

            var equip = _equipmentService.GetEquipment(id);
            if (equip == null)
                return id;

            // 🔥 Get ItemDefinition (for icon + name)
            InventoryItemDefinitions.All.TryGetValue(id, out var itemDef);

            var stats = new List<string>();

            if (equip.Attack > 0)
                stats.Add($"+{equip.Attack} ATK");

            if (equip.Defense > 0)
                stats.Add($"+{equip.Defense} DEF");

            if (equip.Health > 0)
                stats.Add($"+{equip.Health} HP");

            var statText = stats.Count > 0
                ? $" ({string.Join(", ", stats)})"
                : "";

            var icon = itemDef != null && !string.IsNullOrWhiteSpace(itemDef.Icon)
                ? $"{itemDef.Icon} "
                : "";

            var name = itemDef?.Name ?? equip.Name;

            return $"{icon}{name}{statText}";
        }

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

        [ComponentInteraction("equip_confirm:*")]
        public async Task ConfirmEquip(string itemId)
        {
            if (Context.Interaction is SocketMessageComponent component)
            {
                // 🔥 Immediately ACK + remove buttons
                await component.UpdateAsync(msg =>
                {
                    msg.Content = "⏳ Equipping item...";
                    msg.Components = new ComponentBuilder().Build(); // removes buttons
                });

                // Now do the actual work
                var result = await _equipService.EquipAsync(Context.User.Id, itemId);

                // Update message again
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
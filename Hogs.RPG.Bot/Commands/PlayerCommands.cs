using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.PlayerServices;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    public class PlayerCommands : InteractionModuleBase<SocketInteractionContext>
    {

        private readonly PlayerService _playerService;
        private readonly PlayerRepository _playerRepository;
        private readonly EquipService _equipService;
        public PlayerCommands(PlayerService playerService, PlayerRepository playerRepository, EquipService equipService)
        {
            _playerService = playerService;
            _playerRepository = playerRepository;
            _equipService = equipService;
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

                // Hunt (legacy + new system)
                LastHunt = "",

                HunterStamina = 100,
                LastHunterStaminaUpdate = DateTimeOffset.UtcNow.ToString("o"),

                // Gathering
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

            int xpRequired = player.Level * player.Level * 100;

            double progress = (double)player.XP / xpRequired;
            int filledBars = (int)(progress * 10);

            string xpBar = new string('█', filledBars) + new string('░', 10 - filledBars);

            var embed = new EmbedBuilder()
                .WithTitle($"⚔ {player.Username}'s Profile")
                .WithColor(Color.DarkRed)
                .AddField("Level", player.Level, true)
                .AddField("Gold", $"💰 {player.Gold}", true)
                .AddField("XP", $"{player.XP} / {xpRequired}", false)
                .AddField("Progress", xpBar, false)
                .AddField("Attack", $"🗡 {player.Attack}", true)
                .AddField("Defense", $"🛡 {player.Defense}", true)
                .AddField("Health", $"❤️ {player.Health}", true)
                .AddField("Energy", $"⚡ {player.Energy}", false)
                .AddField(
                         "⚒ Equipment",
                         $"🗡 Main Hand: {player.MainHand ?? "None"}\n" +
                         $"🏹 Off Hand: {player.OffHand ?? "None"}\n" +
                         $"🪖 Helmet: {player.Helmet ?? "None"}\n" +
                         $"🛡 Body: {player.Body ?? "None"}\n" +
                         $"👖 Legs: {player.Legs ?? "None"}\n" +
                         $"🧤 Gloves: {player.Gloves ?? "None"}\n" +
                         $"🥾 Boots: {player.Boots ?? "None"}\n" +
                         $"💍 Ring: {player.Ring ?? "None"}\n" +
                         $"📿 Amulet: {player.Amulet ?? "None"}",
                         false)
                .WithFooter("HOGS RPG")
                .WithTimestamp(DateTime.UtcNow)
                .Build();

            await FollowupAsync(embed: embed);
        }

        [SlashCommand("equip", "Equip an item")]
        public async Task Equip(
            [Autocomplete(typeof(EquipAutocompleteHandler))] string itemId)
        {
            await DeferAsync(ephemeral: true);

            var result = await _equipService.EquipAsync(Context.User.Id, itemId);

            await FollowupAsync(result, ephemeral: true);
        }

        [SlashCommand("unequip", "Unequip a piece of gear")]
        public async Task Unequip(
            [Autocomplete(typeof(UnequipAutocompleteHandler))] string slot)
        {
            await DeferAsync(ephemeral: true);

            var result = await _equipService.UnequipAsync(Context.User.Id, slot);

            await FollowupAsync(result, ephemeral: true);
        }
    }
}

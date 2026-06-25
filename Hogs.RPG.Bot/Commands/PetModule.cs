using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.PetServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    [BossLock]
    [GearSwapLock]
    [TradeLock]
    public class PetModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PetService _petService;
        private readonly PlayerRepository _playerRepository;
        private readonly RaidRepository _raidRepository;

        public PetModule(PetService petService, PlayerRepository playerRepository, RaidRepository raidRepository)
        {
            _petService = petService;
            _playerRepository = playerRepository;
            _raidRepository = raidRepository;
        }

        // =========================
        // 🐾 PET BAG
        // =========================
        [SlashCommand("petbag", "View all your pets")]
        public async Task PetBag()
        {
            var pets = await _petService.GetPetsAsync(Context.User.Id);

            if (pets.Count == 0)
            {
                await RespondAsync("🐾 You have no pets.", ephemeral: true);
                return;
            }

            var lines = pets.Select(p =>
            {
                if (!PetRegistry.All.TryGetValue(p.PetId, out var def))
                    return p.PetId;

                string displayName = p.CustomName ?? def.Name;
                string equipped = p.IsEquipped ? " 🟢 Equipped" : "";

                var passives = new List<string>();
                if (p.Passive1 != null) passives.Add(PetPassiveFormatter.Format(p.Passive1));
                if (p.Passive2 != null) passives.Add(PetPassiveFormatter.Format(p.Passive2));

                return $"{def.Icon} {displayName} (Lv. {p.Level}){equipped}\n" +
                       string.Join("\n", passives.Select(passive => $"   • {passive}"));
            });

            await RespondAsync(
                $"🐾 **Your Pets**\n\n{string.Join("\n", lines)}",
                ephemeral: true);
        }

        // =========================
        // 🐾 /companion — View all companions
        // =========================
        [SlashCommand("companion", "View all your active companions and their bonuses")]
        public async Task Companion()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("❌ You haven't started your adventure yet.", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("🐾 Your Companions")
                .WithColor(Color.DarkGreen)
                .WithDescription("Companions are passive bonuses that are always active once unlocked.")

                .AddField(
                    player.HasHuntingPet ? "✅ Hunting Companion" : "❌ Hunting Companion — *Locked*",
                    player.HasHuntingPet
                        ? "+5% Hunt XP · +5% Hunt Materials · +3% Rare Drop Rate\n*Found on the Ashwood Trail*"
                        : "*Discover it on the Ashwood Trail via `/trail`*",
                    false)

                .AddField(
                    player.HasAlchemistPet ? "✅ Bandit, the Workshop Assistant" : "❌ Alchemist Companion — *Locked*",
                    player.HasAlchemistPet
                        ? "+10% Alchemy XP on every brew and swamp gather\n*Found in The Abandoned Academy (Lv 27)*"
                        : "*Defeat Bandit in The Abandoned Academy (Lv 27)*",
                    false)

                .AddField(
                    player.HasGatherPet ? "✅ The Ravens of Odin" : "❌ Gather Companion — *Locked*",
                    player.HasGatherPet
                        ? "+15% Gather yield on all zones · +50 Max Energy\n*Found in The Ashen Hollow (Lv 29)*"
                        : "*Defeat the Ravens in The Ashen Hollow (Lv 29)*",
                    false)

                .AddField(
                    player.HasBlacksmithPet ? "✅ Furny da Clanka" : "❌ Blacksmith Companion — *Locked*",
                    player.HasBlacksmithPet
                        ? "+10% Smithing XP on every craft\n*Found in Ember ClankaVille (Lv 31)*"
                        : "*Defeat Furny in Ember ClankaVille (Lv 31)*",
                    false)

                .WithFooter("Companions are permanent — once found, always active.");

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        // =========================
        // 🐾 EQUIP PET
        // =========================
        [SlashCommand("pet-equip", "Equip a pet")]
        public async Task EquipPet(
        [Autocomplete(typeof(PetBagAutocompleteHandler))] string petId)
        {
            await DeferAsync(ephemeral: true);

            // RAID LOCK — stops players from unequipping their pet to soften
            // boss scaling, then re-equipping once the boss stats are locked in.
            if (await _raidRepository.IsPlayerInActiveRaidAsync(Context.User.Id))
            {
                await FollowupAsync("⚔️ You can't change pets while in a raid lobby or an active raid.", ephemeral: true);
                return;
            }

            var pets = await _petService.GetPetsAsync(Context.User.Id);

            if (!pets.Any(p => p.PetId == petId))
            {
                await FollowupAsync("❌ You don't own that pet.", ephemeral: true);
                return;
            }

            await _petService.EquipPetAsync(Context.User.Id, petId);

            if (PetRegistry.All.TryGetValue(petId, out var def))
            {
                var pet = pets.First(p => p.PetId == petId);
                string dispName = pet.CustomName ?? def.Name;
                await FollowupAsync($"🐾 Equipped **{dispName}**!", ephemeral: true);
            }
            else
            {
                await FollowupAsync("🐾 Pet equipped.", ephemeral: true);
            }
        }

        // =========================
        // 🎲 REROLL PASSIVE
        // =========================
        [SlashCommand("pet-reroll", "Sacrifice a T2 pet to reroll a passive on your equipped pet")]
        public async Task RerollPassive(
            [Summary("slot", "Which passive slot to reroll (1 = Level 15, 2 = Level 20)")]
            [Choice("Slot 1", 1), Choice("Slot 2", 2)] int slot,
            [Summary("sacrifice", "The T2 pet to sacrifice (costs 5,000 gold)")]
            [Autocomplete(typeof(T2PetBagAutocompleteHandler))] string sacrificePetId)
        {
            await DeferAsync(ephemeral: true);

            var (success, message) = await _petService.RerollPassiveAsync(Context.User.Id, slot, sacrificePetId);

            await FollowupAsync(message, ephemeral: true);
        }


        // =========================
        // 🐾 UNEQUIP PET
        // =========================
        [SlashCommand("pet-unequip", "Unequip your current pet")]
        public async Task UnequipPet()
        {
            await DeferAsync(ephemeral: true);

            // RAID LOCK — stops players from unequipping their pet to soften
            // boss scaling, then re-equipping once the boss stats are locked in.
            if (await _raidRepository.IsPlayerInActiveRaidAsync(Context.User.Id))
            {
                await FollowupAsync("⚔️ You can't change pets while in a raid lobby or an active raid.", ephemeral: true);
                return;
            }

            await _petService.UnequipPetAsync(Context.User.Id);
            await FollowupAsync("🐾 Pet unequipped.", ephemeral: true);
        }

        // =========================
        // 🐾 PET PROFILE
        // =========================
        [SlashCommand("petprofile", "View your equipped pet")]
        public async Task PetProfile()
        {
            await DeferAsync(ephemeral: true);

            var pet = await _petService.GetEquippedPetAsync(Context.User.Id);

            if (pet == null)
            {
                await FollowupAsync("🐾 No pet equipped.", ephemeral: true);
                return;
            }

            if (!PetRegistry.All.TryGetValue(pet.PetId, out var def))
            {
                await FollowupAsync("❌ Pet not found in registry.", ephemeral: true);
                return;
            }

            string displayName = pet.CustomName ?? def.Name;

            var (atk, defStat, hp) = _petService.CalculateStats(pet);

            int xpRequired = 20 + (pet.Level * pet.Level * 15);
            double progress = (double)pet.XP / xpRequired;
            int filled = Math.Clamp((int)(progress * 10), 0, 10);
            string bar = new string('█', filled) + new string('░', 10 - filled);

            var embed = new EmbedBuilder()
                .WithTitle($"🐾 {displayName}")
                .WithColor(Color.Green)
                .AddField("Level", pet.Level, true)
                .AddField("XP", $"{pet.XP} / {xpRequired}", true)
                .AddField("Progress", bar, false)
                .AddField("Attack", $"🗡 {atk}", true)
                .AddField("Defense", $"🛡 {defStat}", true)
                .AddField("Health", $"❤️ {hp}", true)
                .AddField("Passive 1", PetPassiveFormatter.Format(pet.Passive1), true)
                .AddField("Passive 2", PetPassiveFormatter.Format(pet.Passive2), true)
                .WithFooter("HOGS RPG")
                .WithTimestamp(DateTime.UtcNow);

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}
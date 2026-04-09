using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Services.PetServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    public class PetModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PetService _petService;

        public PetModule(PetService petService)
        {
            _petService = petService;
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
        // 🐾 EQUIP PET
        // =========================
        [SlashCommand("pet-equip", "Equip a pet")]
        public async Task EquipPet(string petId)
        {
            await DeferAsync(ephemeral: true);

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
        // 🐾 UNEQUIP PET
        // =========================
        [SlashCommand("pet-unequip", "Unequip your current pet")]
        public async Task UnequipPet()
        {
            await DeferAsync(ephemeral: true);
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
            int filled = (int)(progress * 10);
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
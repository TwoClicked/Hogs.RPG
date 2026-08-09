using Discord;
using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.GameData.Colosseum;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.Registries;

namespace Hogs.RPG.Bot.InteractionModels
{
    /// <summary>
    /// Pure rendering helpers for the Colosseum DM build flow - builds the
    /// embed + components for each screen (main menu, gear slot/item
    /// pickers, pet/passive picker, buffs). No Discord API calls or DB
    /// access here, just view construction, so both ColosseumModule (first
    /// send) and ColosseumBuildInteractionModule (every button/select
    /// handler) can share the exact same rendering without duplicating it.
    /// </summary>
    public static class ColosseumBuildViews
    {
        private static readonly (EquipmentSlot slot, string label, string emoji)[] Slots =
        {
            (EquipmentSlot.MainHand, "Main Hand", "🗡️"),
            (EquipmentSlot.OffHand, "Off Hand", "🛡️"),
            (EquipmentSlot.Helmet, "Helmet", "🪖"),
            (EquipmentSlot.Body, "Body", "👕"),
            (EquipmentSlot.Legs, "Legs", "👖"),
            (EquipmentSlot.Gloves, "Gloves", "🧤"),
            (EquipmentSlot.Boots, "Boots", "🥾"),
            (EquipmentSlot.Ring, "Ring", "💍"),
            (EquipmentSlot.Amulet, "Amulet", "📿"),
        };

        // =========================
        // MAIN MENU
        // =========================
        public static (Embed embed, MessageComponent components) BuildMainMenu(ColosseumParticipant participant)
        {
            var build = participant.Build!;

            var embed = new EmbedBuilder()
                .WithTitle("🏛️ Colosseum Build")
                .WithDescription(
                    $"**AP spent:** {build.ApSpent} / {build.ApBudget}\n\n" +
                    BuildGearSummary(build) + "\n" +
                    BuildPetSummary(build) + "\n" +
                    BuildBuffSummary(build) +
                    (build.LockedAt.HasValue ? "\n\n🔒 **Build locked in.**" : "\n\nPick a category below to start building."))
                .WithColor(new Color(0xC0392B))
                .Build();

            if (build.LockedAt.HasValue)
            {
                // Locked builds get a read-only view - no components at all.
                return (embed, new ComponentBuilder().Build());
            }

            var components = new ComponentBuilder()
                .WithButton("🛡️ Gear", "colosseum_gear_menu", ButtonStyle.Primary)
                .WithButton("🐾 Pet & Passive", "colosseum_pet_menu", ButtonStyle.Primary)
                .WithButton("✨ Buffs", "colosseum_buffs_menu", ButtonStyle.Primary)
                .WithButton("🔒 Lock In", "colosseum_lock_confirm_ask", ButtonStyle.Danger)
                .Build();

            return (embed, components);
        }

        // =========================
        // GEAR: SLOT PICKER
        // =========================
        public static (Embed embed, MessageComponent components) BuildGearSlotPicker(ColosseumBuild build)
        {
            var embed = new EmbedBuilder()
                .WithTitle("🛡️ Choose a slot to upgrade")
                .WithDescription($"**AP spent:** {build.ApSpent} / {build.ApBudget}\n\n{BuildGearSummary(build)}")
                .WithColor(new Color(0xC0392B))
                .Build();

            var menu = new SelectMenuBuilder()
                .WithCustomId("colosseum_gear_slot_select")
                .WithPlaceholder("Choose a gear slot...");

            foreach (var (slot, label, emoji) in Slots)
                menu.AddOption(label, slot.ToString(), emote: new Emoji(emoji));

            var components = new ComponentBuilder()
                .WithSelectMenu(menu)
                .WithButton("⬅️ Back", "colosseum_main_menu", ButtonStyle.Secondary)
                .Build();

            return (embed, components);
        }

        // =========================
        // GEAR: ITEM PICKER FOR ONE SLOT
        // =========================
        public static (Embed embed, MessageComponent components) BuildGearItemPicker(ColosseumBuild build, EquipmentSlot slot)
        {
            var currentItemId = ColosseumPriceRegistry.ResolveGearId(slot, GetGearField(build, slot));
            var currentItem = EquipmentRegistry.All.TryGetValue(currentItemId, out var cur) ? cur.Name : currentItemId;

            var embed = new EmbedBuilder()
                .WithTitle($"🛡️ {slot} — currently: {currentItem}")
                .WithDescription($"**AP spent:** {build.ApSpent} / {build.ApBudget}")
                .WithColor(new Color(0xC0392B))
                .Build();

            var menu = new SelectMenuBuilder()
                .WithCustomId($"colosseum_gear_item_select:{slot}")
                .WithPlaceholder("Choose an item...");

            // Free T1 baseline option first, so they can revert.
            var baselineId = ColosseumGearPrices.T1BaselineBySlot[slot];
            var baselineItem = EquipmentRegistry.All[baselineId];
            menu.AddOption($"{baselineItem.Name} (T1 baseline, free)", "baseline");

            if (ColosseumPriceRegistry.PurchasableGearOptionsBySlot.TryGetValue(slot, out var options))
            {
                foreach (var itemId in options)
                {
                    var item = EquipmentRegistry.All[itemId];
                    var cost = ColosseumPriceRegistry.GetGearCost(itemId);
                    menu.AddOption($"{item.Name} — {cost} AP", itemId);
                }
            }

            var components = new ComponentBuilder()
                .WithSelectMenu(menu)
                .WithButton("⬅️ Back", "colosseum_gear_menu", ButtonStyle.Secondary)
                .Build();

            return (embed, components);
        }

        // =========================
        // PET & PASSIVE
        // =========================
        public static (Embed embed, MessageComponent components) BuildPetPicker(ColosseumBuild build)
        {
            var embed = new EmbedBuilder()
                .WithTitle("🐾 Pet & Passive")
                .WithDescription($"**AP spent:** {build.ApSpent} / {build.ApBudget}\n\n{BuildPetSummary(build)}")
                .WithColor(new Color(0xC0392B))
                .Build();

            var petMenu = new SelectMenuBuilder()
                .WithCustomId("colosseum_pet_select")
                .WithPlaceholder("Choose a pet...");

            petMenu.AddOption("Verdant Cat (T1 baseline, free)", "baseline");
            foreach (var (petId, cost) in ColosseumPetPrices.ApCostByPetId)
            {
                if (cost == 0) continue;
                var pet = PetRegistry.Get(petId);
                petMenu.AddOption($"{pet.Name} (T{pet.Tier}) — {cost} AP", petId);
            }

            var passiveMenu = new SelectMenuBuilder()
                .WithCustomId("colosseum_passive_select")
                .WithPlaceholder("Choose a passive...");

            passiveMenu.AddOption("None", "none");
            foreach (var (passive, cost) in ColosseumPetPrices.ApCostByPassive)
            {
                var def = PetPassiveRegistry.All[passive];
                passiveMenu.AddOption($"{def.Name} — {cost} AP", passive.ToString(), description: Truncate(def.Description, 100));
            }

            var components = new ComponentBuilder()
                .WithSelectMenu(petMenu)
                .WithSelectMenu(passiveMenu)
                .WithButton("⬅️ Back", "colosseum_main_menu", ButtonStyle.Secondary)
                .Build();

            return (embed, components);
        }

        // =========================
        // BUFFS
        // =========================
        public static (Embed embed, MessageComponent components) BuildBuffsMenu(ColosseumBuild build)
        {
            var embed = new EmbedBuilder()
                .WithTitle("✨ Store Buffs")
                .WithDescription(
                    $"**AP spent:** {build.ApSpent} / {build.ApBudget}\n\n" +
                    $"Each stat capped at {ColosseumBuffShop.MaxPurchasesPerStat} purchases.\n\n" +
                    BuildBuffSummary(build))
                .WithColor(new Color(0xC0392B))
                .Build();

            var components = new ComponentBuilder()
                .WithButton($"➕ Attack ({ColosseumBuffShop.AttackBuffCost} AP)", "colosseum_buff_buy:Attack", ButtonStyle.Success, row: 0)
                .WithButton("➖ Attack", "colosseum_buff_remove:Attack", ButtonStyle.Secondary, row: 0)
                .WithButton($"➕ Defense ({ColosseumBuffShop.DefenseBuffCost} AP)", "colosseum_buff_buy:Defense", ButtonStyle.Success, row: 1)
                .WithButton("➖ Defense", "colosseum_buff_remove:Defense", ButtonStyle.Secondary, row: 1)
                .WithButton($"➕ Health ({ColosseumBuffShop.HealthBuffCost} AP)", "colosseum_buff_buy:Health", ButtonStyle.Success, row: 2)
                .WithButton("➖ Health", "colosseum_buff_remove:Health", ButtonStyle.Secondary, row: 2)
                .WithButton("⬅️ Back", "colosseum_main_menu", ButtonStyle.Secondary, row: 3)
                .Build();

            return (embed, components);
        }

        // =========================
        // LOCK CONFIRMATION
        // =========================
        public static (Embed embed, MessageComponent components) BuildLockConfirm(ColosseumBuild build)
        {
            var embed = new EmbedBuilder()
                .WithTitle("🔒 Lock in this build?")
                .WithDescription(
                    "Once locked, you can't make any more changes before the tournament starts.\n\n" +
                    $"**AP spent:** {build.ApSpent} / {build.ApBudget}\n\n" +
                    BuildGearSummary(build) + "\n" +
                    BuildPetSummary(build) + "\n" +
                    BuildBuffSummary(build))
                .WithColor(new Color(0xE74C3C))
                .Build();

            var components = new ComponentBuilder()
                .WithButton("🔒 Confirm Lock In", "colosseum_lock_confirmed", ButtonStyle.Danger)
                .WithButton("⬅️ Cancel", "colosseum_main_menu", ButtonStyle.Secondary)
                .Build();

            return (embed, components);
        }

        // =========================
        // SUMMARY TEXT HELPERS
        // =========================
        private static string BuildGearSummary(ColosseumBuild build)
        {
            var lines = new List<string> { "**Gear:**" };
            foreach (var (slot, label, emoji) in Slots)
            {
                var itemId = ColosseumPriceRegistry.ResolveGearId(slot, GetGearField(build, slot));
                var name = EquipmentRegistry.All.TryGetValue(itemId, out var item) ? item.Name : itemId;
                lines.Add($"{emoji} {label}: {name}");
            }
            return string.Join("\n", lines);
        }

        private static string BuildPetSummary(ColosseumBuild build)
        {
            var petId = ColosseumPriceRegistry.ResolvePetId(build.PetId);
            var petName = PetRegistry.All.TryGetValue(petId, out var pet) ? pet.Name : petId;
            var passiveName = build.PetPassive.HasValue ? PetPassiveRegistry.All[build.PetPassive.Value].Name : "None";
            return $"**Pet:** {petName} (T{build.PetTier})\n**Passive:** {passiveName}";
        }

        private static string BuildBuffSummary(ColosseumBuild build)
        {
            return $"**Buffs:** ⚔️ Attack x{build.BuffAttackPurchases} · 🛡️ Defense x{build.BuffDefensePurchases} · ❤️ Health x{build.BuffHealthPurchases}";
        }

        private static string? GetGearField(ColosseumBuild build, EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.MainHand => build.GearMainHandId,
            EquipmentSlot.OffHand => build.GearOffHandId,
            EquipmentSlot.Helmet => build.GearHelmetId,
            EquipmentSlot.Body => build.GearBodyId,
            EquipmentSlot.Legs => build.GearLegsId,
            EquipmentSlot.Gloves => build.GearGlovesId,
            EquipmentSlot.Boots => build.GearBootsId,
            EquipmentSlot.Ring => build.GearRingId,
            EquipmentSlot.Amulet => build.GearAmuletId,
            _ => null
        };

        private static string Truncate(string text, int max) => text.Length <= max ? text : text[..(max - 1)] + "…";
    }
}
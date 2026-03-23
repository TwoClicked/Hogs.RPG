using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.GameData.Hunts;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.GatheringServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HuntAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var playerRepo = services.GetService(typeof(PlayerRepository)) as PlayerRepository;

        if (playerRepo == null)
            return AutocompletionResult.FromSuccess();

        var player = await playerRepo.GetByDiscordIdAsync(context.User.Id);

        if (player == null)
        {
            new AutocompleteResult("❌ Create a profile first with /startadventure", "no_profile");
        }

        var staminaService = services.GetService(typeof(HunterStaminaService)) as HunterStaminaService;

        staminaService?.Regenerate(player);


        if (player == null)
            return AutocompletionResult.FromSuccess();

        var input = autocompleteInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        // =========================
        // READ SELECTED TIER
        // =========================
        var tierValue = autocompleteInteraction.Data.Options
            .FirstOrDefault(o => o.Name == "tier")?.Value?.ToString();

        int.TryParse(tierValue, out int tierStart);

        // =========================
        // READ CATEGORY (NEW)
        // =========================
        var categoryValue = autocompleteInteraction.Data.Options
            .FirstOrDefault(o => o.Name == "category")?.Value?.ToString();

        HuntCategory selectedCategory = HuntCategory.Normal;

        if (!string.IsNullOrEmpty(categoryValue) &&
            Enum.TryParse<HuntCategory>(categoryValue, true, out var parsed))
        {
            selectedCategory = parsed;
        }

        // =========================
        // TIER HELPER
        // =========================
        string GetTierLabel(int requiredLevel)
        {
            int start = (requiredLevel / 5) * 5;

            if (start == 0)
                start = 1;

            int end = start + 4;

            return $"[Lv {start}-{end}]";
        }

        // =========================
        // FILTER HUNTS
        // =========================
        var hunts = HuntTargetRegistry.All.Values
            .Where(h => player.Level >= h.RequiredLevel)

            .Where(h => h.Category == selectedCategory)

            .Where(h =>
                tierStart == 0 ||
                (h.RequiredLevel >= tierStart && h.RequiredLevel < tierStart + 5))

            .Where(h =>
                string.IsNullOrEmpty(input) ||
                h.Name.ToLower().Contains(input) ||
                h.Id.ToLower().Contains(input))

            .OrderBy(h => h.RequiredLevel)
            .ThenBy(h => h.Name)

            .Take(25)

            .Select(h =>
            {
                var materialName = InventoryItemDefinitions.All.TryGetValue(h.DropItem, out var item)
                    ? item.Name
                    : h.DropItem;

                // 🧪 Highlight alchemy
                var prefix = h.Category == HuntCategory.Alchemy ? "🧪 " : "";

                return new AutocompleteResult(
                    $"{GetTierLabel(h.RequiredLevel)} {prefix}{h.Icon} {h.Name} → ✦ {materialName}",
                    h.Id);
            })
            .ToList();

        // =========================
        // FALLBACK
        // =========================
        if (hunts.Count == 0)
        {
            hunts = HuntTargetRegistry.All.Values
                .Where(h => player.Level >= h.RequiredLevel)
                .Where(h => h.Category == selectedCategory)
                .OrderBy(h => h.RequiredLevel)
                .ThenBy(h => h.Name)
                .Take(5)
                .Select(h =>
                {
                    var materialName = InventoryItemDefinitions.All.TryGetValue(h.DropItem, out var item)
                        ? item.Name
                        : h.DropItem;

                    var prefix = h.Category == HuntCategory.Alchemy ? "🧪 " : "";

                    return new AutocompleteResult(
                        $"{GetTierLabel(h.RequiredLevel)} {prefix}{h.Icon} {h.Name} → ✦ {materialName}",
                        h.Id);
                })
                .ToList();
        }

        return AutocompletionResult.FromSuccess(hunts);
    }
}
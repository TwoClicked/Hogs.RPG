using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.GameData.Hunts;
using Hogs.RPG.Services.GameplayServices;
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

        var player = await AutocompleteCache<Player>.GetOrCreateAsync(
            context.User.Id,
            TimeSpan.FromSeconds(15),
            () => playerRepo.GetByDiscordIdAsync(context.User.Id)
        );

        if (player == null)
            return AutocompletionResult.FromSuccess();

        var staminaService = services.GetService(typeof(HunterStaminaService)) as HunterStaminaService;
        staminaService?.Regenerate(player);

        var input = autocompleteInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";

        var tierValue = autocompleteInteraction.Data.Options
            .FirstOrDefault(o => o.Name == "tier")?.Value?.ToString();
        int.TryParse(tierValue, out int tierStart);

        var categoryValue = autocompleteInteraction.Data.Options
            .FirstOrDefault(o => o.Name == "category")?.Value?.ToString();

        HuntCategory selectedCategory = HuntCategory.Normal;
        if (!string.IsNullOrEmpty(categoryValue) &&
            Enum.TryParse<HuntCategory>(categoryValue, true, out var parsed))
            selectedCategory = parsed;

        // Tier label matched to actual RequiredLevel thresholds:
        // T1 = 1  (ForestTargets)
        // T2 = 10 (WildTargets)
        // T3 = 15 (DeepTargets)
        // T4 = 20 (StormTargets)
        // T5 = 25 (MythicTargets)
        string GetTierLabel(int requiredLevel)
        {
            var tier = requiredLevel switch
            {
                < 10 => "T1",
                < 15 => "T2",
                < 20 => "T3",
                < 25 => "T4",
                _ => "T5"
            };
            return $"[{tier}]";
        }

        string BuildLabel(HuntTarget h)
        {
            var materialName = InventoryItemDefinitions.All.TryGetValue(h.DropItem, out var item)
                ? item.Name : h.DropItem;

            var prefix = h.Category == HuntCategory.Alchemy ? "🧪 " : "";

            var label = $"{GetTierLabel(h.RequiredLevel)} {prefix}{h.Icon} {h.Name} → ✦ {materialName}";

            if (!string.IsNullOrEmpty(h.RareDropItem) &&
                InventoryItemDefinitions.All.TryGetValue(h.RareDropItem, out var rareDef))
            {
                label += $" | ★ {rareDef.Name}";
            }

            return label;
        }

        var hunts = HuntTargetRegistry.All.Values
            .Where(h => player.Level >= h.RequiredLevel)
            .Where(h => h.Category == selectedCategory)
            .Where(h => tierStart == 0 || (h.RequiredLevel >= tierStart && h.RequiredLevel < tierStart + 5))
            .Where(h => string.IsNullOrEmpty(input) || h.Name.ToLower().Contains(input) || h.Id.ToLower().Contains(input))
            .OrderBy(h => h.RequiredLevel)
            .ThenBy(h => h.Name)
            .Take(25)
            .Select(h => new AutocompleteResult(BuildLabel(h), h.Id))
            .ToList();

        if (hunts.Count == 0)
        {
            hunts = HuntTargetRegistry.All.Values
                .Where(h => player.Level >= h.RequiredLevel)
                .Where(h => h.Category == selectedCategory)
                .OrderBy(h => h.RequiredLevel)
                .ThenBy(h => h.Name)
                .Take(5)
                .Select(h => new AutocompleteResult(BuildLabel(h), h.Id))
                .ToList();
        }

        return AutocompletionResult.FromSuccess(hunts);
    }
}
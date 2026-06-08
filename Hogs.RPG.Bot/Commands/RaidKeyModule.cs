using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Raids;
using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Services.AlchemyServices;
using Hogs.RPG.Services.InventoryServices;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    [Group("raidkey", "Raid key crafting")]
    [BossLock]
    public class RaidKeyModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AlchemyService _alchemyService;
        private readonly InventoryService _inventoryService;

        public RaidKeyModule(AlchemyService alchemyService, InventoryService inventoryService)
        {
            _alchemyService = alchemyService;
            _inventoryService = inventoryService;
        }

        // =========================
        // /raidkey craft
        // =========================
        [SlashCommand("craft", "Craft a raid key using hunt materials")]
        public async Task CraftKey(
            [Autocomplete(typeof(RaidKeyAutocompleteHandler))] string key,
            [Autocomplete(typeof(CraftAmountAutocompleteHandler))] string amount = "1")
        {
            await DeferAsync(ephemeral: true);

            int craftAmount;
            if (amount.ToLower() == "max")
                craftAmount = -1;
            else if (!int.TryParse(amount, out craftAmount))
            {
                await FollowupAsync("❌ Invalid amount.", ephemeral: true);
                return;
            }

            var result = await _alchemyService.CraftPotionAsync(
                Context.User.Id, key, craftAmount);

            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // /raidkey recipes
        // =========================
        [SlashCommand("recipes", "View all raid key recipes and material requirements")]
        public async Task KeyRecipes()
        {
            await DeferAsync(ephemeral: true);

            var raidKeyIds = new[]
            {
                "raid_key_t1", "raid_key_t2", "raid_key_t3",
                "raid_key_t4", "raid_key_t5"
            };

            var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);
            var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            var embed = new EmbedBuilder()
                .WithTitle("🗝️ Raid Key Recipes")
                .WithColor(new Color(0xE67E22))
                .WithFooter("Use /raidkey craft to forge a key");

            foreach (var keyId in raidKeyIds)
            {
                if (!RecipeRegistry.All.TryGetValue(keyId, out var recipe)) continue;
                if (!InventoryItemDefinitions.All.TryGetValue(keyId, out var keyDef)) continue;

                int maxCraftable = int.MaxValue;
                foreach (var mat in recipe.Materials)
                {
                    invLookup.TryGetValue(mat.Key, out int owned);
                    maxCraftable = Math.Min(maxCraftable, owned / mat.Value);
                }
                if (maxCraftable == int.MaxValue) maxCraftable = 0;

                var sb = new StringBuilder();
                foreach (var mat in recipe.Materials)
                {
                    invLookup.TryGetValue(mat.Key, out int owned);
                    var matName = InventoryItemDefinitions.All.TryGetValue(mat.Key, out var matDef)
                        ? matDef.Name : mat.Key;
                    string check = owned >= mat.Value ? "✅" : "❌";
                    sb.AppendLine($"{check} {matName} — {owned}/{mat.Value}");
                }

                sb.AppendLine($"*Can craft: {maxCraftable}*");

                embed.AddField($"{keyDef.Icon} {keyDef.Name}", sb.ToString().Trim(), false);
            }

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}
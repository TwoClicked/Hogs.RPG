using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Autocomplete;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Raids;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.RaidServices;

namespace Hogs.RPG.Bot.Commands
{
    [BossLock]
    [GearSwapLock]
    [TradeLock]
    public class ConvertModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly MaterialConversionService _conversionService;
        private readonly InventoryService _inventoryService;
        private readonly PlayerRepository _playerRepository;

        public ConvertModule(
            MaterialConversionService conversionService,
            InventoryService inventoryService,
            PlayerRepository playerRepository)
        {
            _conversionService = conversionService;
            _inventoryService = inventoryService;
            _playerRepository = playerRepository;
        }

        private async Task<bool> EnsurePlayerAsync()
        {
            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await RespondAsync("⚠️ You need to start your adventure first with `/startadventure`.", ephemeral: true);
                return false;
            }
            return true;
        }

        // =========================
        // /convert-materials
        // =========================
        [SlashCommand("convert-materials", "Exchange 2,500 hunt materials for a Raid Key")]
        public async Task ConvertMaterials(
            [Summary("material", "The material to convert"),
             Autocomplete(typeof(ConvertMaterialAutocompleteHandler))] string materialId,
            [Summary("amount", "Number of keys to produce (default: 1)")] int amount = 1)
        {
            await DeferAsync(ephemeral: true);
            if (!await EnsurePlayerAsync()) return;

            if (!MaterialConversionData.MaterialToKeyTier.TryGetValue(materialId, out int tier))
            {
                await FollowupAsync("❌ That material cannot be converted.", ephemeral: true);
                return;
            }

            if (amount < 1)
            {
                await FollowupAsync("❌ Amount must be at least 1.", ephemeral: true);
                return;
            }

            int cost = amount * MaterialConversionData.MaterialsPerKey;
            int held = await _inventoryService.GetItemAmountAsync(Context.User.Id, materialId);
            int maxKeys = _conversionService.GetMaxKeys(held);

            if (held < MaterialConversionData.MaterialsPerKey)
            {
                await FollowupAsync(
                    $"❌ You need at least **{MaterialConversionData.MaterialsPerKey:N0}** to convert. You only have **{held:N0}**.",
                    ephemeral: true);
                return;
            }

            if (held < cost)
            {
                await FollowupAsync(
                    $"❌ Not enough for {amount} key(s). You can make at most **{maxKeys}** with your **{held:N0}** on hand.",
                    ephemeral: true);
                return;
            }

            InventoryItemDefinitions.All.TryGetValue(materialId, out var matDef);
            string matName = matDef?.Name ?? materialId;
            string matIcon = matDef?.Icon ?? "🧱";
            string keyName = MaterialConversionData.TierKeyNames[tier];

            var embed = new EmbedBuilder()
                .WithTitle("🔄 Material Conversion")
                .WithColor(new Color(0xE67E22))
                .WithDescription(
                    $"Convert the following?\n\n" +
                    $"{matIcon} **{cost:N0}x {matName}**\n" +
                    $"↓\n" +
                    $"🗝️ **{amount}x {keyName}** (T{tier})\n\n" +
                    $"You have **{held:N0}** and will have **{held - cost:N0}** remaining.")
                .WithFooter($"Rate: {MaterialConversionData.MaterialsPerKey:N0} materials per key · Max you can make: {maxKeys}");

            var components = new ComponentBuilder()
                .WithButton("✅ Confirm", $"convert_confirm:{materialId}:{amount}", ButtonStyle.Success)
                .WithButton("❌ Cancel", "convert_cancel", ButtonStyle.Danger)
                .Build();

            await FollowupAsync(embed: embed.Build(), components: components, ephemeral: true);
        }

        // =========================
        // CONFIRM BUTTON
        // =========================
        [ComponentInteraction("convert_confirm:*:*")]
        public async Task ConfirmConversion(string materialId, string amountStr)
        {
            await DeferAsync(ephemeral: true);

            int amount = int.Parse(amountStr);
            var (success, message) = await _conversionService.ConvertAsync(Context.User.Id, materialId, amount);

            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = message;
                msg.Embed = null;
                msg.Components = new ComponentBuilder().Build();
            });
        }

        // =========================
        // CANCEL BUTTON
        // =========================
        [ComponentInteraction("convert_cancel")]
        public async Task CancelConversion()
        {
            await DeferAsync(ephemeral: true);

            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = "❌ Conversion cancelled.";
                msg.Embed = null;
                msg.Components = new ComponentBuilder().Build();
            });
        }
    }
}
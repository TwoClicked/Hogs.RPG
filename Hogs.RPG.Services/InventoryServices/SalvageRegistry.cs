using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Salvage;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.GameplayServices
{
    public class SalvageService
    {
        private static readonly string BlackstoneId = EnhancementItems.Blackstone.Id;

        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly EquipmentService _equipmentService;

        public SalvageService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            EquipmentService equipmentService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _equipmentService = equipmentService;
        }

        // =========================
        // PREVIEW (used for the confirm/cancel flow)
        // quantity == -1 means "max" — resolved here against what the
        // player actually owns (equipped copies never show up in
        // inventory, since EquipService.TakeItemAsync removes them on
        // equip, so there's no risk of salvaging a worn piece).
        // =========================
        public async Task<(string preview, string? validItemId, int resolvedQuantity)> GetSalvagePreviewAsync(
            ulong userId, string itemId, int quantity)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null)
                return ("You need to start your adventure first.", null, 0);

            if (!SalvageRegistry.BlackstoneValue.TryGetValue(itemId, out int valuePerItem))
                return ("❌ That item can't be salvaged. Only Global Boss Gear and Dungeon Boss Gear can be broken down.", null, 0);

            var item = _equipmentService.GetEquipment(itemId);
            if (item == null)
                return ("❌ Unknown item.", null, 0);

            int owned = await _inventoryService.GetItemAmountAsync(userId, itemId);

            if (quantity == -1)
                quantity = owned;

            if (owned <= 0 || quantity <= 0)
                return ($"❌ You don't have any spare **{item.Name}** to salvage.", null, 0);

            if (owned < quantity)
                return ($"❌ You only have **{owned}x {item.Name}** — can't salvage {quantity}.", null, 0);

            int totalPayout = valuePerItem * quantity;

            string preview =
                $"🔨 **Salvage {quantity}x {item.Name}?**\n\n" +
                $"This will permanently destroy {quantity}x **{item.Name}** and cannot be undone.\n\n" +
                $"💰 You will receive: **{totalPayout:N0} Blackstones** ({valuePerItem:N0} each)";

            return (preview, itemId, quantity);
        }

        // =========================
        // SALVAGE
        // =========================
        public async Task<string> SalvageAsync(ulong userId, string itemId, int quantity)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null)
                return "❌ You need to start your adventure first.";

            if (!SalvageRegistry.BlackstoneValue.TryGetValue(itemId, out int valuePerItem))
                return "❌ That item can't be salvaged.";

            var item = _equipmentService.GetEquipment(itemId);
            if (item == null)
                return "❌ Unknown item.";

            int owned = await _inventoryService.GetItemAmountAsync(userId, itemId);
            if (owned < quantity)
                return $"❌ You only have **{owned}x {item.Name}** — can't salvage {quantity}.";

            int totalPayout = valuePerItem * quantity;

            await _inventoryService.TakeItemAsync(userId, itemId, quantity);
            await _inventoryService.GiveItemAsync(userId, BlackstoneId, totalPayout);

            return $"🔨 Salvaged **{quantity}x {item.Name}** → 💰 **+{totalPayout:N0} Blackstones**";
        }
    }
}
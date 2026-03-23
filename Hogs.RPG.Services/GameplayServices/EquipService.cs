using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.GameplayServices
{
    public class EquipService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly EquipmentService _equipmentService;
        private readonly StatService _statService;

        public EquipService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            EquipmentService equipmentService,
            StatService statService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _equipmentService = equipmentService;
            _statService = statService;
        }

        // =========================
        // EQUIP
        // =========================
        public async Task<string> EquipAsync(ulong userId, string itemId)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to start your adventure first.";

            var item = _equipmentService.GetEquipment(itemId);

            if (item == null)
                return "That item cannot be equipped.";

            var inventory = await _inventoryService.GetInventoryAsync(userId);
            var ownedItem = inventory.Find(i => i.ItemId == itemId);

            if (ownedItem == null || ownedItem.Quantity <= 0)
                return "You do not have that item.";

            // =========================
            // CURRENT ITEM
            // =========================
            string currentItemId = item.Slot switch
            {
                EquipmentSlot.MainHand => player.MainHand,
                EquipmentSlot.OffHand => player.OffHand,
                EquipmentSlot.Helmet => player.Helmet,
                EquipmentSlot.Body => player.Body,
                EquipmentSlot.Legs => player.Legs,
                EquipmentSlot.Gloves => player.Gloves,
                EquipmentSlot.Boots => player.Boots,
                EquipmentSlot.Ring => player.Ring,
                EquipmentSlot.Amulet => player.Amulet,
                _ => ""
            };

            var currentItem = string.IsNullOrEmpty(currentItemId)
                ? null
                : _equipmentService.GetEquipment(currentItemId);

            // =========================
            // DIFF
            // =========================
            int atkDiff = item.Attack - (currentItem?.Attack ?? 0);
            int defDiff = item.Defense - (currentItem?.Defense ?? 0);
            int hpDiff = item.Health - (currentItem?.Health ?? 0);

            string FormatDiff(int value, string label)
            {
                if (value == 0) return null;
                return value > 0
                    ? $"⬆ +{value} {label}"
                    : $"⬇ {value} {label}";
            }

            var diffLines = new List<string>();

            var atk = FormatDiff(atkDiff, "ATK");
            var def = FormatDiff(defDiff, "DEF");
            var hp = FormatDiff(hpDiff, "HP");

            if (atk != null) diffLines.Add(atk);
            if (def != null) diffLines.Add(def);
            if (hp != null) diffLines.Add(hp);

            var diffText = diffLines.Count > 0
                ? string.Join("\n", diffLines)
                : "No stat change";

            // =========================
            // APPLY EQUIP
            // =========================
            string previousItem = currentItemId;

            switch (item.Slot)
            {
                case EquipmentSlot.MainHand: player.MainHand = itemId; break;
                case EquipmentSlot.OffHand: player.OffHand = itemId; break;
                case EquipmentSlot.Helmet: player.Helmet = itemId; break;
                case EquipmentSlot.Body: player.Body = itemId; break;
                case EquipmentSlot.Legs: player.Legs = itemId; break;
                case EquipmentSlot.Gloves: player.Gloves = itemId; break;
                case EquipmentSlot.Boots: player.Boots = itemId; break;
                case EquipmentSlot.Ring: player.Ring = itemId; break;
                case EquipmentSlot.Amulet: player.Amulet = itemId; break;
            }

            await _inventoryService.TakeItemAsync(userId, itemId, 1);

            if (!string.IsNullOrEmpty(previousItem))
                await _inventoryService.GiveItemAsync(userId, previousItem, 1);

            // =========================
            // CLAMP HEALTH
            // =========================
            ClampHealth(player);

            await _playerRepository.UpdatePlayerAsync(player);

            return
                $"⚔ Equipped **{item.Name}**\n\n" +
                $"📊 **Stat Changes**\n{diffText}";
        }

        // =========================
        // UNEQUIP
        // =========================
        public async Task<string> UnequipAsync(ulong userId, string slot)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to start your adventure first.";

            string itemId = "";

            switch (slot.ToLower())
            {
                case "mainhand":
                    itemId = player.MainHand;
                    player.MainHand = "";
                    break;

                case "offhand":
                    itemId = player.OffHand;
                    player.OffHand = "";
                    break;

                case "helmet":
                    itemId = player.Helmet;
                    player.Helmet = "";
                    break;

                case "body":
                    itemId = player.Body;
                    player.Body = "";
                    break;

                case "legs":
                    itemId = player.Legs;
                    player.Legs = "";
                    break;

                case "gloves":
                    itemId = player.Gloves;
                    player.Gloves = "";
                    break;

                case "boots":
                    itemId = player.Boots;
                    player.Boots = "";
                    break;

                case "ring":
                    itemId = player.Ring;
                    player.Ring = "";
                    break;

                case "amulet":
                    itemId = player.Amulet;
                    player.Amulet = "";
                    break;

                default:
                    return "Invalid slot.";
            }

            if (string.IsNullOrEmpty(itemId))
                return "Nothing is equipped in that slot.";

            var item = _equipmentService.GetEquipment(itemId);

            await _inventoryService.GiveItemAsync(userId, itemId, 1);

            // =========================
            // CLAMP HEALTH
            // =========================
            ClampHealth(player);

            await _playerRepository.UpdatePlayerAsync(player);

            return $"You unequipped **{item.Name}**.";
        }

        // =========================
        // PREVIEW
        // =========================
        public async Task<(string previewText, string itemId)> GetEquipPreviewAsync(ulong userId, string itemId)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return ("You need to start your adventure first.", null);

            var item = _equipmentService.GetEquipment(itemId);

            if (item == null)
                return ("That item cannot be equipped.", null);

            var inventory = await _inventoryService.GetInventoryAsync(userId);
            var ownedItem = inventory.Find(i => i.ItemId == itemId);

            if (ownedItem == null || ownedItem.Quantity <= 0)
                return ("You do not have that item.", null);

            string currentItemId = item.Slot switch
            {
                EquipmentSlot.MainHand => player.MainHand,
                EquipmentSlot.OffHand => player.OffHand,
                EquipmentSlot.Helmet => player.Helmet,
                EquipmentSlot.Body => player.Body,
                EquipmentSlot.Legs => player.Legs,
                EquipmentSlot.Gloves => player.Gloves,
                EquipmentSlot.Boots => player.Boots,
                EquipmentSlot.Ring => player.Ring,
                EquipmentSlot.Amulet => player.Amulet,
                _ => ""
            };

            var currentItem = string.IsNullOrEmpty(currentItemId)
                ? null
                : _equipmentService.GetEquipment(currentItemId);

            int atkDiff = item.Attack - (currentItem?.Attack ?? 0);
            int defDiff = item.Defense - (currentItem?.Defense ?? 0);
            int hpDiff = item.Health - (currentItem?.Health ?? 0);

            string FormatDiff(int value, string label)
            {
                if (value == 0) return null;
                return value > 0
                    ? $"⬆ +{value} {label}"
                    : $"⬇ {value} {label}";
            }

            var diffs = new List<string>();

            if (FormatDiff(atkDiff, "ATK") != null) diffs.Add(FormatDiff(atkDiff, "ATK"));
            if (FormatDiff(defDiff, "DEF") != null) diffs.Add(FormatDiff(defDiff, "DEF"));
            if (FormatDiff(hpDiff, "HP") != null) diffs.Add(FormatDiff(hpDiff, "HP"));

            var diffText = diffs.Count > 0 ? string.Join("\n", diffs) : "No stat change";

            var currentName = currentItem?.Name ?? "None";

            var preview =
                $"⚔ Equip **{item.Name}**?\n\n" +
                $"Current: **{currentName}**\n" +
                $"New: **{item.Name}**\n\n" +
                $"📊 **Result**\n{diffText}";

            return (preview, itemId);
        }

        // =========================
        // HELPER
        // =========================
        private void ClampHealth(Player player)
        {
            var (_, _, maxHealth) = _statService.CalculateStats(player);

            if (player.Health > maxHealth)
                player.Health = maxHealth;
        }
    }
}
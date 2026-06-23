using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.GameplayServices
{
    public class EquipService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly EquipmentService _equipmentService;
        private readonly StatService _statService;
        private readonly RaidRepository _raidRepository;

        public EquipService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            EquipmentService equipmentService,
            StatService statService,
            RaidRepository raidRepository)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _equipmentService = equipmentService;
            _statService = statService;
            _raidRepository = raidRepository;
        }

        // =========================
        // EQUIP
        // =========================
        public async Task<string> EquipAsync(ulong userId, string itemId)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to start your adventure first.";

            // =========================
            // RAID LOCK
            // Blocks gear changes while queued or fighting in a raid — stops
            // players from stripping gear to soften boss scaling, then
            // re-gearing once the boss's stats are locked in.
            // =========================
            if (await _raidRepository.IsPlayerInActiveRaidAsync(userId))
                return "⚔️ You can't change gear while in a raid lobby or an active raid.";

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
                return value > 0 ? $"⬆ +{value} {label}" : $"⬇ {value} {label}";
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

            ClampHealth(player);
            await _playerRepository.UpdatePlayerAsync(player);

            // Player slots changed — invalidate the player cache used by equip-by-slot autocomplete
            AutocompleteCache<Player>.Invalidate(userId);
            // Note: inventory cache is already invalidated inside GiveItemAsync / TakeItemAsync above

            return
                $"⚔ Equipped **{item.Name}**\n\n" +
                $"📊 **Stat Changes**\n{diffText}";
        }

        // =========================
        // GET EQUIP PREVIEW (used for the confirm/cancel flow)
        // =========================
        public async Task<(string preview, string? validItemId)> GetEquipPreviewAsync(ulong userId, string itemId)
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
                return value > 0 ? $"⬆ +{value} {label}" : $"⬇ {value} {label}";
            }

            var diffLines = new List<string>();
            var atk = FormatDiff(atkDiff, "ATK");
            var def = FormatDiff(defDiff, "DEF");
            var hp = FormatDiff(hpDiff, "HP");
            if (atk != null) diffLines.Add(atk);
            if (def != null) diffLines.Add(def);
            if (hp != null) diffLines.Add(hp);

            string diffText = diffLines.Count > 0 ? string.Join("\n", diffLines) : "No stat change";
            string currentName = currentItem != null ? $"**{currentItem.Name}**" : "*nothing*";

            string preview =
                $"⚔ **Equip {item.Name}?**\n\n" +
                $"Slot: **{item.Slot}**\n" +
                $"Replacing: {currentName}\n\n" +
                $"📊 **Stat Changes**\n{diffText}";

            return (preview, itemId);
        }

        // =========================
        // UNEQUIP
        // =========================
        public async Task<string> UnequipAsync(ulong userId, string slot)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You need to start your adventure first.";

            // =========================
            // RAID LOCK
            // Blocks gear changes while queued or fighting in a raid — stops
            // players from stripping gear to soften boss scaling, then
            // re-gearing once the boss's stats are locked in.
            // =========================
            if (await _raidRepository.IsPlayerInActiveRaidAsync(userId))
                return "⚔️ You can't change gear while in a raid lobby or an active raid.";

            string? currentItemId = slot switch
            {
                "MainHand" => player.MainHand,
                "OffHand" => player.OffHand,
                "Helmet" => player.Helmet,
                "Body" => player.Body,
                "Legs" => player.Legs,
                "Gloves" => player.Gloves,
                "Boots" => player.Boots,
                "Ring" => player.Ring,
                "Amulet" => player.Amulet,
                _ => null
            };

            if (string.IsNullOrEmpty(currentItemId))
                return $"❌ Nothing equipped in **{slot}**.";

            var item = _equipmentService.GetEquipment(currentItemId);

            switch (slot)
            {
                case "MainHand": player.MainHand = null; break;
                case "OffHand": player.OffHand = null; break;
                case "Helmet": player.Helmet = null; break;
                case "Body": player.Body = null; break;
                case "Legs": player.Legs = null; break;
                case "Gloves": player.Gloves = null; break;
                case "Boots": player.Boots = null; break;
                case "Ring": player.Ring = null; break;
                case "Amulet": player.Amulet = null; break;
            }

            await _inventoryService.GiveItemAsync(userId, currentItemId, 1);

            ClampHealth(player);
            await _playerRepository.UpdatePlayerAsync(player);

            // Player slots changed — invalidate player cache
            AutocompleteCache<Player>.Invalidate(userId);
            // Inventory cache already invalidated inside GiveItemAsync

            string itemName = item?.Name ?? currentItemId;
            return $"✅ **{itemName}** unequipped and returned to your inventory.";
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

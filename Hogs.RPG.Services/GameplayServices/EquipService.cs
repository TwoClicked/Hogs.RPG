using Hogs.RPG.Core.Entities;
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

        public EquipService(PlayerRepository playerRepository,
                            InventoryService inventoryService,
                            EquipmentService equipmentService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _equipmentService = equipmentService;
        }

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

            string previousItem = "";

            switch (item.Slot)
            {
                case EquipmentSlot.MainHand:
                    previousItem = player.MainHand;
                    player.MainHand = itemId;
                    break;

                case EquipmentSlot.OffHand:
                    previousItem = player.OffHand;
                    player.OffHand = itemId;
                    break;

                case EquipmentSlot.Helmet:
                    previousItem = player.Helmet;
                    player.Helmet = itemId;
                    break;

                case EquipmentSlot.Body:
                    previousItem = player.Body;
                    player.Body = itemId;
                    break;

                case EquipmentSlot.Legs:
                    previousItem = player.Legs;
                    player.Legs = itemId;
                    break;

                case EquipmentSlot.Gloves:
                    previousItem = player.Gloves;
                    player.Gloves = itemId;
                    break;

                case EquipmentSlot.Boots:
                    previousItem = player.Boots;
                    player.Boots = itemId;
                    break;

                case EquipmentSlot.Ring:
                    previousItem = player.Ring;
                    player.Ring = itemId;
                    break;

                case EquipmentSlot.Amulet:
                    previousItem = player.Amulet;
                    player.Amulet = itemId;
                    break;
            }

            // Remove new item from inventory
            await _inventoryService.TakeItemAsync(userId, itemId, 1);

            // If there was old gear → remove stats and return to inventory
            if (!string.IsNullOrEmpty(previousItem))
            {
                var oldItem = _equipmentService.GetEquipment(previousItem);

                if (oldItem != null)
                {
                    player.Attack -= oldItem.Attack;
                    player.Defense -= oldItem.Defense;
                    player.Health -= oldItem.Health;
                }

                await _inventoryService.GiveItemAsync(userId, previousItem, 1);
            }

            // Apply new stats
            player.Attack += item.Attack;
            player.Defense += item.Defense;
            player.Health += item.Health;

            await _playerRepository.UpdatePlayerAsync(player);

            return $"⚔ You equipped **{item.Name}**.";
        }

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

            if (item != null)
            {
                player.Attack -= item.Attack;
                player.Defense -= item.Defense;
                player.Health -= item.Health;
            }

            await _inventoryService.GiveItemAsync(userId, itemId, 1);

            await _playerRepository.UpdatePlayerAsync(player);

            return $"You unequipped **{item.Name}**.";
        }
    }
}
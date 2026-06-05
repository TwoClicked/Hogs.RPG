using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using System.Collections.Generic;

namespace Hogs.RPG.Services.GameplayServices
{
    public class ItemService
    {
        public ItemDefinition GetItem(string id)
        {
            if (InventoryItemDefinitions.All.TryGetValue(id, out var item))
                return item;

            return null;
        }

        public IReadOnlyDictionary<string, ItemDefinition> GetAllItems()
        {
            return InventoryItemDefinitions.All;
        }
    }
}
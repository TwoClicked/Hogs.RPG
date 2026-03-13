using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Equipment;
using System.Collections.Generic;

namespace Hogs.RPG.Services.GameplayServices
{
    public class EquipmentService
    {
        public EquipmentDefinition GetEquipment(string id)
        {
            return EquipmentRegistry.All.TryGetValue(id, out var item)
                ? item
                : null;
        }
    }
}
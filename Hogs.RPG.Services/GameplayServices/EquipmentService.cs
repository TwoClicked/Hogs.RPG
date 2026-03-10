using Hogs.RPG.Core.Entities;
using System.Collections.Generic;

namespace Hogs.RPG.Services.GameplayServices
{
    public class EquipmentService
    {
        private readonly Dictionary<string, EquipmentDefinition> _equipment = new()
        {


            {
                "bone_dagger",
                new EquipmentDefinition
                {
                    Id = "bone_dagger",
                    Name = "Bone Dagger",
                    Slot = "MainHand",
                    Attack = 3
                }
            },
            {
                "hunter_bow",
                new EquipmentDefinition
                {
                    Id = "hunter_bow",
                    Name = "Hunter Bow",
                    Slot = "OffHand",
                    Attack = 2
                }
            },
            {
                "leather_armor",
                new EquipmentDefinition
                {
                    Id = "leather_armor",
                    Name = "Leather Armor",
                    Slot = "Body",
                    Defense = 4
                }
            }
        };

        public EquipmentDefinition GetEquipment(string id)
        {
            if (_equipment.TryGetValue(id, out var item))
                return item;

            return null;
        }
    }
}
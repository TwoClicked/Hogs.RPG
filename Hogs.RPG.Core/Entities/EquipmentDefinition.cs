using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class EquipmentDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public EquipmentSlot Slot { get; set; }

        public int Attack { get; set; } = 0;

        public int Defense { get; set; } = 0;

        public int Health { get; set; } = 0;
    }
}
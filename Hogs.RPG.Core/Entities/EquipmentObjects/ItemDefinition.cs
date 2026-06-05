using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.EquipmentObjects
{
    public class ItemDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int? Tier { get; set; }
        public string? SubCategory { get; set; } // "Craft", "Alchemy", "Rare"
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class EquipmentDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Slot { get; set; }

        public int Attack { get; set; }

        public int Defense { get; set; }

        public int Health { get; set; }
    }
}

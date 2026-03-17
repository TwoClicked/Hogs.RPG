using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class BossLoot
    {

        public string ItemId { get; set; } // Links to the item definitons 

        public double DropChance { get; set; }

        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }


    }
}

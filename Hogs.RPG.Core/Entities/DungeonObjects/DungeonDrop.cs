using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.DungeonObjects
{
    public class DungeonDrop
    {
        public string ItemId { get; set; }
        public int ChancePercent { get; set; } // e.g. 1 = 1%
    }
}

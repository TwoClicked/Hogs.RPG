using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class HuntTarget
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }

        public string DropItem { get; set; }

        public int MinXP { get; set; }
        public int MaxXP { get; set; }

        public int MinGold { get; set; }
        public int MaxGold { get; set; }

        public int MinDrop { get; set; }
        public int MaxDrop { get; set; }
    }
}

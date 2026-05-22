using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class GearSet
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }

        public Player Player { get; set; }

        public int SetIndex { get; set; }   // 1, 2, or 3
        public string SetName { get; set; } = "Unnamed Set";

        // Equipment slots — nullable (empty slot = not saved)
        public string? MainHand { get; set; }
        public string? OffHand { get; set; }
        public string? Helmet { get; set; }
        public string? Body { get; set; }
        public string? Legs { get; set; }
        public string? Gloves { get; set; }
        public string? Boots { get; set; }
        public string? Ring { get; set; }
        public string? Amulet { get; set; }
    }
}






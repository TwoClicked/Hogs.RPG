using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class PlayerRelic
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public Player Player { get; set; }
        public string RelicId { get; set; }
        public int Rank { get; set; } = 1;
        public RelicBonusType BonusType { get; set; }

        public bool IsEquipped { get; set; } = false;
        public int slotindex { get; set; } = 0; 
    }
}

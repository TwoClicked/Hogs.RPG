using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class PlayerPet
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public string PetId { get; set; }

        public int Level { get; set; } = 1;
        public int XP { get; set; } = 0;

        public bool IsEquipped { get; set; } = false;

        // ✅ NEW
        public PetPassive? Passive1 { get; set; }
        public PetPassive? Passive2 { get; set; }
    }
}

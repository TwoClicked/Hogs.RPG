using System.Collections.Generic;

namespace Hogs.RPG.Core.Entities
{
    public class Player
    {
        public int PlayerId { get; set; }
        public ulong DiscordId { get; set; }
        public string Username { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public int Gold { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Health { get; set; }
        public string LastHunt { get; set; }

        // Equipped gear

        public string MainHand { get; set; }
        public string OffHand { get; set; }
        public string Helmet { get; set; }
        public string Body { get; set; }
        public string Legs { get; set; }
        public string Gloves { get; set; }
        public string Boots { get; set; }
        public string Ring { get; set; }
        public string Amulet { get; set; }

        // Active buffs

        public List<ActiveBuff> ActiveBuffs { get; set; } = new();
        public bool AutoUseXpPotions { get; set; }
    }
}
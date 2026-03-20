using System;
using System.Collections.Generic;

namespace Hogs.RPG.Core.Entities
{
    public class ActiveBoss
    {
        public BossDefinition Definition { get; set; }

        public int CurrentHealth { get; set; }

        public DateTime ExpireAt { get; set; }

        public Dictionary<ulong, int> DamageDealt { get; set; } = new();

        // 👇 REQUIRED for raid UI system
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }

        // 👇 REQUIRED for rate-limit safe updates
        public DateTime LastUiUpdate { get; set; } = DateTime.MinValue;
    }
}
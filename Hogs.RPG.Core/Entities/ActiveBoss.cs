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
    }
}
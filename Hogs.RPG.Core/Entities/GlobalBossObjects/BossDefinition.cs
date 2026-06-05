using Hogs.RPG.Core.Entities.DungeonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.GlobalBossObjects
{
    public class BossDefinition
    {

        public string Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public int MaxHealth { get; set; }
        public int Defense { get; set; }

        public int RewardGold { get; set; }
        public List<BossLoot> LootTable { get; set; } = new();

        public BossType Type { get; set; }

        // Later use case 

        public string AbilitiesText { get; set; }


        public enum BossType
        {
            Daily,
            Weekly
        }

    }
}

using Hogs.RPG.Core.Entities.PlayerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.AchievementObjects
{
    public class AchievementContext
    {
        public Player Player { get; set; }
        public int GearScore { get; set; }
        public int Slot1RelicRank { get; set; }
        public int Slot2RelicRank { get; set; }
    }
}
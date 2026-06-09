using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.AchievementObjects
{
    public class AchievementBonus
    {
        // Stat bonuses — applied in StatService
        public int BonusAttack { get; set; } = 0;
        public int BonusDefense { get; set; } = 0;
        public int BonusHealth { get; set; } = 0;

        // Extra daily trails — applied in TrailService
        public int ExtraTrails { get; set; } = 0;

        // Regen rate per minute — 1 default, 2 at milestone 60
        public int RegenRate { get; set; } = 1;

        // Dungeon cooldown in minutes — 120 default, 60 at milestone 100
        public int DungeonCooldownMinutes { get; set; } = 120;

        // Highest earned title — null if below milestone 5
        public string? Title { get; set; }
    }
}
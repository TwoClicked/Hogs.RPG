using Hogs.RPG.Core.Entities.AchievementObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.Achievements
{
    public static class AchievementMilestones
    {
        public static AchievementBonus GetBonus(int achievementCount)
        {
            var bonus = new AchievementBonus();

            // 5 — +5 ATK, +5 DEF, Rookie Adventurer title
            if (achievementCount >= 5)
            {
                bonus.BonusAttack = 5;
                bonus.BonusDefense = 5;
                bonus.Title = "🌱 Rookie Adventurer";
            }

            // 15 — +50 HP
            if (achievementCount >= 15)
                bonus.BonusHealth = 50;

            // 30 — +1 daily trail, Seasoned Adventurer title
            if (achievementCount >= 30)
            {
                bonus.ExtraTrails = 1;
                bonus.Title = "🏕️ Seasoned Adventurer";
            }

            // 60 — regen 2 per minute
            if (achievementCount >= 60)
                bonus.RegenRate = 2;

            // 100 — dungeon cooldown 1 hour, Master Adventurer title
            if (achievementCount >= 100)
            {
                bonus.DungeonCooldownMinutes = 60;
                bonus.Title = "🏆 Master Adventurer";
            }

            return bonus;
        }

        // Returns the next milestone the player hasn't hit yet
        public static int? GetNextMilestone(int achievementCount)
        {
            if (achievementCount < 5) return 5;
            if (achievementCount < 15) return 15;
            if (achievementCount < 30) return 30;
            if (achievementCount < 60) return 60;
            if (achievementCount < 100) return 100;
            return null;
        }
    }
}

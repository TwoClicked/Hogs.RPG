using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.AchievementObjects
{
    public class PlayerAchievement
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public string AchievementId { get; set; }
        public DateTime EarnedAt { get; set; }

        // True if awarded by the retroactive migration — no feed post, no gold
        public bool IsRetroactive { get; set; } = false;
    }
}
using Hogs.RPG.Core.Entities.RaidObjects;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Services.RaidServices
{
    public class RaidRoundResult
    {
        public int Round { get; set; }
        public string TankText { get; set; } = "";
        public string DpsText { get; set; } = "";
        public string HealerText { get; set; } = "";
        public string BossText { get; set; } = "";
        public bool IsVictory { get; set; } = false;
        public bool IsWipe { get; set; } = false;
        public string WipeReason { get; set; } = "";
        public RaidSession? Session { get; set; }
        public List<RaidReward> Rewards { get; set; } = new();
    }
}
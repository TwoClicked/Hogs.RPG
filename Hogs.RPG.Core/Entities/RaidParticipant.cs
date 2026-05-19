using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class RaidParticipant
    {
        public int Id { get; set; }

        public int RaidSessionId { get; set; }
        public RaidSession RaidSession { get; set; }

        public ulong DiscordId { get; set; }

        public RaidRole Role { get; set; }

        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }

        public bool HasActedThisRound { get; set; } = false;
        public string? PendingAction { get; set; } = null;

        public int ShatterCooldownRoundsRemaining { get; set; } = 0;
        public int RecklessCooldownRoundsRemaining { get; set; } = 0;
        public int FocusCooldownRoundsRemaining { get; set; } = 0;
        public int FocusStacks { get; set; } = 0;
        public int EmergencyHealCooldownRoundsRemaining { get; set; } = 0;
    }
}
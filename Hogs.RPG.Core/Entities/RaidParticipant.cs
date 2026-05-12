using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class RaidParticipant
    {
        public int Id { get; set; }

        public int RaidSessionId { get; set; }
        public RaidSession RaidSession { get; set; }

        public ulong DiscordId { get; set; }
        public Player Player { get; set; }

        public RaidRole Role { get; set; }

        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }

        public bool HasActedThisRound { get; set; } = false;
        public string? PendingAction { get; set; } = null;

        public int ShatterCooldownRoundsRemaining { get; set; } = 0;
    }
}
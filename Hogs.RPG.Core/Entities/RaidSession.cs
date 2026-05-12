using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class RaidSession
    {
        public int Id { get; set; }

        public int Tier { get; set; }
        public RaidStatus Status { get; set; } = RaidStatus.Lobby;

        public ulong LeaderDiscordId { get; set; }

        public ulong LobbyChannelId { get; set; }
        public ulong LobbyMessageId { get; set; }

        public ulong ThreadId { get; set; } = 0;

        public int BossCurrentHp { get; set; }
        public int BossMaxHp { get; set; }
        public int BossAttack { get; set; }
        public int BossDefense { get; set; }

        public int CurrentRound { get; set; } = 0;

        public ulong AggroDiscordId { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<RaidParticipant> Participants { get; set; } = new();
        public List<ActiveRaidEffect> ActiveEffects { get; set; } = new();
    }
}
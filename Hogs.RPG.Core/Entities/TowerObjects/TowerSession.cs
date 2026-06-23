using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Tower;

namespace Hogs.RPG.Core.Entities.TowerObjects
{
    public class TowerSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public TowerMode Mode { get; set; }
        public TowerStatus Status { get; set; } = TowerStatus.Lobby;
        public int Floor { get; set; } = 0;
        public List<TowerParticipant> Participants { get; set; } = new();
        public List<TowerParticipant> FallenParticipants { get; set; } = new();
        public ulong ChannelId { get; set; }
        public ulong ThreadId { get; set; }
        public ulong LobbyMessageId { get; set; }
        public DateTime? NextFloorAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Boss fight state
        public TowerBossDefinition? ActiveBoss { get; set; }
        public int BossCurrentHp { get; set; }
        public int BossMaxHp { get; set; }
        public int BossRound { get; set; }
        public DateTime? NextBossRoundAt { get; set; }

        // Merchant shop — appears at most once per run. 0 means it hasn't appeared yet.
        // Purchases are only valid while Floor == MerchantFloor.
        public int MerchantFloor { get; set; } = 0;
    }
}

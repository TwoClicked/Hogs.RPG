using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities.TowerObjects
{
    public class TowerSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public TowerMode Mode { get; set; }
        public TowerStatus Status { get; set; } = TowerStatus.Lobby;
        public int Floor { get; set; } = 0;
        public List<TowerParticipant> Participants { get; set; } = new();
        public ulong ChannelId { get; set; }
        public ulong ThreadId { get; set; }
        public ulong LobbyMessageId { get; set; }
        public DateTime? NextFloorAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using Hogs.RPG.Core.Entities;

public class ActiveBoss
{
    public BossDefinition Definition { get; set; }

    public int CurrentHealth { get; set; }

    public DateTime ExpireAt { get; set; }
    public DateTime SpawnedAt { get; set; } = DateTime.UtcNow;

    public Dictionary<ulong, int> DamageDealt { get; set; } = new();
    public HashSet<ulong> Participants { get; set; } = new();

    public bool IsDead { get; set; } = false;

    // UI tracking
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }

    public DateTime LastUiUpdate { get; set; } = DateTime.MinValue;
}
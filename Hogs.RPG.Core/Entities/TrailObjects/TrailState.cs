// =========================
// TRAIL STATE
// Runtime-only in-memory object — never persisted to DB
// =========================
using Hogs.RPG.Core.Entities.TrailObjects;

public class TrailState
{
    public ulong UserId { get; set; }
    public TrailZoneDefinition Zone { get; set; }
    public List<TrailEventDefinition> Events { get; set; } = new();
    public int CurrentEventIndex { get; set; } = 0;

    // Tokens accumulated from events this trail
    public int TokensEarned { get; set; } = 0;

    // Base tokens rolled at trail start — added at finalization
    public int BaseTokens { get; set; } = 0;

    // Set by FreshTracks — applies 1.5x modifier to the very next event
    public bool NextEventBoosted { get; set; } = false;

    // Human-readable log of resolved events shown in the DM embed
    public List<string> EventLog { get; set; } = new();

    // Notable material drops — shown in the feed post
    public List<string> NotableDrops { get; set; } = new();

    // DM message reference — used to edit in place as events resolve
    public ulong DmMessageId { get; set; } = 0;
    public ulong DmChannelId { get; set; } = 0;

    // Set after FinalizeTrailAsync completes
    public bool IsComplete { get; set; } = false;
    public bool PetDropped { get; set; } = false;
    public int TotalTokensAwarded { get; set; } = 0;
    public int PlayerTokenBalance { get; set; } = 0;
}
namespace Hogs.RPG.Core.Entities.TowerObjects
{
    // A Tower thread that's finished (run ended or aborted) and is waiting for the daily
    // cleanup job to delete it from Discord. Persisted so a bot restart between "run ended"
    // and "next 3am cleanup" doesn't silently lose track of it.
    public class TowerCompletedThread
    {
        public ulong ThreadId { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
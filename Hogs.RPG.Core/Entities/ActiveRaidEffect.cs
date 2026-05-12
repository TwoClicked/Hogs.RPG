using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class ActiveRaidEffect
    {
        public ActiveEffectType EffectType { get; set; }
        public ulong? TargetDiscordId { get; set; }
        public int RoundsRemaining { get; set; }
        public double Value { get; set; }
    }
}
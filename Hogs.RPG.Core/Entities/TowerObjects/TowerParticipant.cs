using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities.TowerObjects
{
    public class TowerParticipant
    {
        public ulong DiscordId { get; set; }
        public string Username { get; set; } = "";
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public bool ReadyToStart { get; set; }
        public bool CheckpointDone { get; set; }
        public int AccumulatedGold { get; set; }
        public int FrenzyStacks { get; set; } = 0;
        public bool TookDamageThisFloor { get; set; }
        public List<TowerBuffType>? PendingBuffChoices { get; set; }
        public List<TowerBuff> Buffs { get; set; } = new();
        public List<TowerDebuff> Debuffs { get; set; } = new();
        public bool HasBeenShackled { get; set; } = false;
    }

    public class TowerBuff
    {
        public TowerBuffType Type { get; set; }
        public int Stacks { get; set; } = 1;
        public int DisabledForFloors { get; set; } = 0;
    }

    public class TowerDebuff
    {
        public TowerDebuffType Type { get; set; }
        public int FloorsRemaining { get; set; } = -1;
        public int? AffectedBuffIndex { get; set; }
    }
}

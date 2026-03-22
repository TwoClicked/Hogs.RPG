namespace Hogs.RPG.Core.Entities
{
    public class ActiveDungeon
    {
        public ulong PlayerId { get; set; }

        public string DungeonId { get; set; }

        public int Floor { get; set; }

        public int PlayerHealth { get; set; }
        public string CurrentImageUrl { get; set; }

        public int MaxHealth { get; set; }

        public int EnemyHealth { get; set; }
        public int EnemyMaxHealth { get; set; }

        public bool IsBoss { get; set; }

        public bool IsActive { get; set; } = true;

        public bool RageTriggered { get; set; }

        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
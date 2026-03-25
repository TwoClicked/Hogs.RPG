namespace Hogs.RPG.Core.Entities
{
    public class ActiveDungeon
    {
        public ulong PlayerId { get; set; }

        public string DungeonId { get; set; }

        public int Floor { get; set; }

        // 🔥 ADD THESE (critical)
        public int Attack { get; set; }
        public int Defense { get; set; }

        public int PlayerHealth { get; set; }
        public int MaxHealth { get; set; }

        public int EnemyHealth { get; set; }
        public int EnemyMaxHealth { get; set; }

        public string CurrentImageUrl { get; set; }

        public bool IsBoss { get; set; }

        public bool IsActive { get; set; } = true;

        

        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }



        //Dungeon boss abilities

        public bool RageTriggered { get; set; }
        public bool CloudActive { get; set; }
    }
}
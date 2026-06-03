using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Services.RaidServices
{
    public class RaidReward
    {
        public ulong DiscordId { get; set; }
        public RaidRole Role { get; set; }
        public int Gold { get; set; }
        public int PlayerXp { get; set; }
        public int PetXp { get; set; }
        public bool ShardDropped { get; set; }
        public int ShardTier { get; set; }
        public string LevelUpMessage { get; set; } = "";

        // Potion settlement at raid end
        public int PotionsPaid { get; set; } = 0;
        public int PotionDebt { get; set; } = 0;
        public int GoldChargedForPotions { get; set; } = 0;
    }
}

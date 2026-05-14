namespace Hogs.RPG.Services.RelicServices
{
    public class RelicBonuses
    {
        // Stat bonuses (applied everywhere)
        public float AttackPercent { get; set; } = 0f;
        public float DefensePercent { get; set; } = 0f;
        public float MaxHpPercent { get; set; } = 0f;

        // Reward bonuses
        public float BonusGoldPercent { get; set; } = 0f;
        public float BonusPlayerXpPercent { get; set; } = 0f;
        public float BonusPetXpPercent { get; set; } = 0f;
        public float BonusLootRollPercent { get; set; } = 0f;

        // DPS bonuses
        public float LifeStealPercent { get; set; } = 0f;
        public float ExecutionerBonusPercent { get; set; } = 0f;
        public float ConsecutiveHitBonusPercent { get; set; } = 0f;

        // Healer bonuses
        public float IncreasedHealPercent { get; set; } = 0f;
        public float ChanceToSavePotion { get; set; } = 0f;
        public int EmpowerExtraRounds { get; set; } = 0;
        public int WiderEmpowerThreshold { get; set; } = 0;

        // Tank bonuses
        public int ShatterExtraRounds { get; set; } = 0;
        public int TauntCooldownReduction { get; set; } = 0;
    }
}
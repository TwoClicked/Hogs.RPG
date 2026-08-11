namespace Hogs.RPG.Core.GameData.Colosseum
{
    public static class ColosseumBuffShop
    {
        public const int AttackBuffCost = 30;
        public const int AttackBuffAmount = 20;

        public const int DefenseBuffCost = 30;
        public const int DefenseBuffAmount = 20;

        public const int HealthBuffCost = 30;
        public const int HealthBuffAmount = 50;

        // Total buff purchases allowed across ALL stats combined - not
        // per-stat. A build could previously buy up to this many of EACH
        // stat (e.g. 2 Attack + 2 Defense + 2 Health = 6 total), which let
        // buff-stacking scale further than intended. Now it's a shared pool:
        // once this many total buffs are bought, no more of any kind can be
        // purchased, regardless of which stats they'd apply to.
        public const int MaxTotalBuffPurchases = 2;
    }
}
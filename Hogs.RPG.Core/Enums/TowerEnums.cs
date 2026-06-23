namespace Hogs.RPG.Core.Enums
{
    public enum TowerMode { Solo, Duo }

    public enum TowerStatus { Lobby, Running, Checkpoint, PreBoss, StartPick, Shopping, Dead }

    public enum TowerFloorEventType
    {
        Combat,
        Elite,
        TreasureRoom,
        CursedFloor,
        RestSite,
        Boss,
        Merchant
    }

    public enum TowerBuffType
    {
        Bloodthirst,
        Executioner,
        DoubleStrike,
        IronSkin,
        Thorns,
        Precision,
        Evasion,
        Frenzy
    }

    public enum TowerDebuffType
    {
        Bleeding,
        Weakened,
        Shackled,
        Cursed
    }
}

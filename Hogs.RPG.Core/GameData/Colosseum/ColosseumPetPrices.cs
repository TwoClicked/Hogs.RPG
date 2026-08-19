using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;

namespace Hogs.RPG.Core.GameData.Colosseum
{
    public static class ColosseumPetPrices
    {
        public const int Tier1Cost = 0;
        public const int Tier2Cost = 80;
        public const int Tier3Cost = 180;

        public static readonly Dictionary<string, int> ApCostByPetId = new()
        {
            { "verdant_cat",       Tier1Cost },

            { "armored_capybara",  Tier2Cost },
            { "el_tata_de_frog",   Tier2Cost },
            { "ice_wolf",          Tier2Cost },

            { "capytara",          Tier3Cost },
        };

        public const string T1BaselinePetId = "verdant_cat";

        public const int PassiveTier1Cost = 40;
        public const int PassiveTier2Cost = 90;
        public const int PassiveTier3Cost = 160;

        public static readonly Dictionary<PetPassive, int> ApCostByPassive = new()
        {
            { PetPassive.Executioner,     PassiveTier1Cost },

            { PetPassive.Lifesteal,       PassiveTier2Cost },
            { PetPassive.GuardianShield,  PassiveTier2Cost },

            { PetPassive.DoubleStrike,    PassiveTier3Cost },
            { PetPassive.Thorns,          PassiveTier3Cost },
        };
    }
}
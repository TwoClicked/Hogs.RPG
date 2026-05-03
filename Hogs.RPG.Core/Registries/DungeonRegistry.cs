using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Dungeons;

namespace Hogs.RPG.Core.GameData.Registries
{
    // =========================
    // GEAR DUNGEONS
    // =========================
    public static class DungeonRegistry
    {
        public static readonly Dictionary<string, DungeonDefinition> All = new()
        {
            { AllDungeons.RotwoodHollow.Id,       AllDungeons.RotwoodHollow       },  // Lv  5
            { AllDungeons.CryptOfFanculo.Id,      AllDungeons.CryptOfFanculo      },  // Lv 10
            { AllDungeons.ForsakenCatacombs.Id,   AllDungeons.ForsakenCatacombs   },  // Lv 15
            { AllDungeons.SunkenCathedral.Id,     AllDungeons.SunkenCathedral     },  // Lv 17
            { AllDungeons.StarchyWastes.Id,       AllDungeons.StarchyWastes       },  // Lv 18
            { AllDungeons.SpiritForest.Id,        AllDungeons.SpiritForest        },  // Lv 20
            { AllDungeons.CarnivalOfCarnage.Id,   AllDungeons.CarnivalOfCarnage   },  // Lv 22
            { AllDungeons.GildedVault.Id,         AllDungeons.GildedVault         },  // Lv 23
            { AllDungeons.TempleOfRuin.Id,        AllDungeons.TempleOfRuin        },  // Lv 25
            { AllDungeons.SparkiteMines.Id,       AllDungeons.SparkiteMines       },  // Lv 27
        };
    }

    // =========================
    // PET DUNGEONS
    // =========================
    public static class PetDungeonRegistry
    {
        public static readonly Dictionary<string, DungeonDefinition> All = new()
        {
            { PetDungeons.BlazewingsGorge.Id,   PetDungeons.BlazewingsGorge   },  // Lv 15 — Attack pet
            { PetDungeons.StonehallDepths.Id,   PetDungeons.StonehallDepths   },  // Lv 20 — Defense pet
            { PetDungeons.DrownedArchives.Id,   PetDungeons.DrownedArchives   },  // Lv 25 — Health pet
        };
    }
}

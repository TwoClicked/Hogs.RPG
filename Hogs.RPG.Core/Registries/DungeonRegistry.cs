using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Dungeons;

namespace Hogs.RPG.Core.GameData.Registries
{
    public static class DungeonRegistry
    {
        public static readonly Dictionary<string, DungeonDefinition> All = new()
        {
            { AllDungeons.CryptOfFanculo.Id, AllDungeons.CryptOfFanculo },
            { AllDungeons.ForsakenCatacombs.Id, AllDungeons.ForsakenCatacombs },
            { AllDungeons.SpiritForest.Id, AllDungeons.SpiritForest },
            { AllDungeons.TempleOfRuin.Id, AllDungeons.TempleOfRuin }
        };
    }
}
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Dungeons;

namespace Hogs.RPG.Core.GameData.Registries
{
    public static class DungeonRegistry
    {
        public static readonly Dictionary<string, DungeonDefinition> All = new()
        {
            { Level25Dungeon.CryptOfFanculo.Id, Level25Dungeon.CryptOfFanculo }
        };
    }
}
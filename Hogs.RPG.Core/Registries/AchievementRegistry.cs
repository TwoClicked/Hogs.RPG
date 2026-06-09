using Hogs.RPG.Core.Entities.AchievementObjects;
using Hogs.RPG.Core.GameData.Achievements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Registries
{
    public static class AchievementRegistry
    {
        public static readonly Dictionary<string, AchievementDefinition> All =
            AchievementDefinitions.All.ToDictionary(a => a.Id);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities;

public static class HuntTargetRegistry
{
    public static readonly Dictionary<string, HuntTarget> All = new()
    {
        { ForestTargets.Wolf.Id, ForestTargets.Wolf },
        { ForestTargets.Boar.Id, ForestTargets.Boar },
        { ForestTargets.Stag.Id, ForestTargets.Stag },
        { ForestTargets.Raven.Id, ForestTargets.Raven },
        { ForestTargets.Fox.Id, ForestTargets.Fox }
    };
}

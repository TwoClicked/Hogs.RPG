// Hogs.RPG.Core/Registries/TrailZoneRegistry.cs

using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Trails;

namespace Hogs.RPG.Core.GameData.Registries
{
    public static class TrailZoneRegistry
    {
        public static readonly Dictionary<string, TrailZoneDefinition> All = new()
        {
            { AllTrailZones.AshwoodTrail.Id, AllTrailZones.AshwoodTrail }
        };
    }
}
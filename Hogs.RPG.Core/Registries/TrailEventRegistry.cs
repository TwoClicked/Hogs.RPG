// Hogs.RPG.Core/Registries/TrailEventRegistry.cs

using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Trails;

namespace Hogs.RPG.Core.GameData.Registries
{
    public static class TrailEventRegistry
    {
        public static readonly Dictionary<string, TrailEventDefinition> All = new()
        {
            { AllTrailEvents.FreshTracks.Id,        AllTrailEvents.FreshTracks        },
            { AllTrailEvents.SnareSet.Id,            AllTrailEvents.SnareSet           },
            { AllTrailEvents.RoughTerrain.Id,        AllTrailEvents.RoughTerrain       },
            { AllTrailEvents.HiddenCache.Id,         AllTrailEvents.HiddenCache        },
            { AllTrailEvents.ClearPath.Id,           AllTrailEvents.ClearPath          },
            { AllTrailEvents.AmbushEncounter.Id,     AllTrailEvents.AmbushEncounter    },
            { AllTrailEvents.TrackersGamble.Id,      AllTrailEvents.TrackersGamble     },
            { AllTrailEvents.RareSighting.Id,        AllTrailEvents.RareSighting       },
            { AllTrailEvents.LegendaryEncounter.Id,  AllTrailEvents.LegendaryEncounter },
        };
    }
}
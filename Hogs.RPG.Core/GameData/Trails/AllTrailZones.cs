using Hogs.RPG.Core.Entities.TrailObjects;
using Hogs.RPG.Core.GameData.Trails;

public static class AllTrailZones
{
    // =========================
    // ASHWOOD TRAIL
    // Single zone for launch — expand later by adding entries here
    // =========================
    public static readonly TrailZoneDefinition AshwoodTrail = new()
    {
        Id = "ashwood_trail",
        Name = "Ashwood Trail",
        Icon = "🌲",
        Description = "A dense forest trail known for rich wildlife and the occasional legendary sighting. Experienced hunters speak of it with reverence.",
        RequiredLevel = 1,
        MinTokens = 8,
        MaxTokens = 15,
        EventPool = new Dictionary<string, int>
            {
                { AllTrailEvents.FreshTracks.Id,        AllTrailEvents.FreshTracks.Weight        },
                { AllTrailEvents.SnareSet.Id,            AllTrailEvents.SnareSet.Weight           },
                { AllTrailEvents.RoughTerrain.Id,        AllTrailEvents.RoughTerrain.Weight       },
                { AllTrailEvents.HiddenCache.Id,         AllTrailEvents.HiddenCache.Weight        },
                { AllTrailEvents.ClearPath.Id,           AllTrailEvents.ClearPath.Weight          },
                { AllTrailEvents.AmbushEncounter.Id,     AllTrailEvents.AmbushEncounter.Weight    },
                { AllTrailEvents.TrackersGamble.Id,      AllTrailEvents.TrackersGamble.Weight     },
                { AllTrailEvents.RareSighting.Id,        AllTrailEvents.RareSighting.Weight       },
                { AllTrailEvents.LegendaryEncounter.Id,  AllTrailEvents.LegendaryEncounter.Weight },
            }
    };
}
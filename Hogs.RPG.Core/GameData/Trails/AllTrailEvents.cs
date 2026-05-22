using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Trails
{
    public static class AllTrailEvents
    {
        // =========================
        // AUTO EVENTS
        // =========================

        public static readonly TrailEventDefinition FreshTracks = new()
        {
            Id = "fresh_tracks",
            Name = "Fresh Tracks",
            Icon = "🐾",
            Description = "You spot fresh animal tracks in the mud. The trail ahead looks promising.",
            Type = TrailEventType.FreshTracks,
            TokenModifier = 3,
            Weight = 15
        };

        public static readonly TrailEventDefinition SnareSet = new()
        {
            Id = "snare_set",
            Name = "Snare Opportunity",
            Icon = "🪤",
            Description = "You find a perfect spot to set a snare. A short while later it pays off.",
            Type = TrailEventType.SnareSet,
            TokenModifier = 2,
            Weight = 15
        };

        public static readonly TrailEventDefinition RoughTerrain = new()
        {
            Id = "rough_terrain",
            Name = "Rough Terrain",
            Icon = "🌧️",
            Description = "Muddy ground and dense brush slow your progress. Harder to track anything out here.",
            Type = TrailEventType.RoughTerrain,
            TokenModifier = -3,
            Weight = 12
        };

        public static readonly TrailEventDefinition HiddenCache = new()
        {
            Id = "hidden_cache",
            Name = "Hidden Cache",
            Icon = "🌿",
            Description = "Tucked beneath a fallen log you find a stash of rare materials left by another hunter.",
            Type = TrailEventType.HiddenCache,
            TokenModifier = 1,
            Weight = 10
        };

        public static readonly TrailEventDefinition ClearPath = new()
        {
            Id = "clear_path",
            Name = "Clear Path",
            Icon = "🧭",
            Description = "The trail opens up and you make excellent time. Everything goes smoothly.",
            Type = TrailEventType.ClearPath,
            TokenModifier = 4,
            Weight = 15
        };

        // =========================
        // DECISION EVENTS
        // =========================

        public static readonly TrailEventDefinition AmbushEncounter = new()
        {
            Id = "ambush_encounter",
            Name = "Ambush Encounter",
            Icon = "⚔️",
            Description = "A large beast crosses your path. You could ambush it for a big reward — but it won't go down without a fight.",
            Type = TrailEventType.AmbushEncounter,
            TokenModifier = 0,
            Weight = 12
        };

        public static readonly TrailEventDefinition TrackersGamble = new()
        {
            Id = "trackers_gamble",
            Name = "Tracker's Gamble",
            Icon = "💰",
            Description = "You find a contested cache. Another hunter's mark is on it. Press your luck for double tokens — or play it safe.",
            Type = TrailEventType.TrackersGamble,
            TokenModifier = 0,
            Weight = 10
        };

        public static readonly TrailEventDefinition RareSighting = new()
        {
            Id = "rare_sighting",
            Name = "Rare Sighting",
            Icon = "🌟",
            Description = "Something valuable stirs in the brush ahead. Worth investigating — but disturbing it could cost you.",
            Type = TrailEventType.RareSighting,
            TokenModifier = 0,
            Weight = 8
        };

        public static readonly TrailEventDefinition LegendaryEncounter = new()
        {
            Id = "legendary_encounter",
            Name = "Legendary Encounter",
            Icon = "⭐",
            Description = "A rare creature emerges from the shadows. You've never seen anything like it. This could be a once in a lifetime moment.",
            Type = TrailEventType.LegendaryEncounter,
            TokenModifier = 0,
            Weight = 3
        };
    }
}
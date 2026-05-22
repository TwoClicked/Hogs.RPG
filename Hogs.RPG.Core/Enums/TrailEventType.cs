public enum TrailEventType
{
    // =========================
    // AUTO EVENTS
    // =========================

    // Found fresh tracks — buffs token yield of the next event
    FreshTracks,

    // Set a snare — small guaranteed material drop
    SnareSet,

    // Rough terrain — slight token penalty this trail
    RoughTerrain,

    // Found a hidden stash — random rare material drop
    HiddenCache,

    // Clear path — small flat token bonus, no drama
    ClearPath,

    // =========================
    // DECISION EVENTS
    // =========================

    // Large beast spotted — ambush for bonus tokens or pass safely
    AmbushEncounter,

    // Contested cache — press luck for double tokens or lose some
    TrackersGamble,

    // Something valuable in the brush — investigate for rare material
    // but risk losing some rewards earned so far
    RareSighting,

    // Legendary creature appears — chance to obtain hunting companion pet
    // (or tokens if already owned)
    LegendaryEncounter
}
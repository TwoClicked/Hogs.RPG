using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Recipes;

public static class Tier1HunterRecipes
{
    public static readonly Recipe ClawDagger = new()
    {
        Id = "claw_dagger",
        Name = "Claw Dagger",
        ResultItem = "claw_dagger",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "claws", 20 },
            { "bone", 10 }
        }
    };

    public static readonly Recipe BoneBuckler = new()
    {
        Id = "bone_buckler",
        Name = "Bone Buckler",
        ResultItem = "bone_buckler",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "bone", 25 },
            { "leather", 15 }
        }
    };

    public static readonly Recipe LeatherVest = new()
    {
        Id = "leather_vest",
        Name = "Leather Vest",
        ResultItem = "leather_vest",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "leather", 30 },
            { "fur", 20 }
        }
    };

    public static readonly Recipe BoneHelm = new()
    {
        Id = "bone_helm",
        Name = "Bone Helm",
        ResultItem = "bone_helm",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "bone", 15 },
            { "leather", 5 }
        }
    };

    public static readonly Recipe HunterLeggings = new()
    {
        Id = "hunter_leggings",
        Name = "Hunter Leggings",
        ResultItem = "hunter_leggings",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "leather", 25 }
        }
    };

    public static readonly Recipe HideBoots = new()
    {
        Id = "hide_boots",
        Name = "Hide Boots",
        ResultItem = "hide_boots",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "fur", 15 },
            { "leather", 10 }
        }
    };

    public static readonly Recipe FurGloves = new()
    {
        Id = "fur_gloves",
        Name = "Fur Gloves",
        ResultItem = "fur_gloves",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "fur", 15 }
        }
    };

    public static readonly Recipe FeatherBand = new()
    {
        Id = "feather_band",
        Name = "Feather Band",
        ResultItem = "feather_band",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "feather", 20 }
        }
    };

    public static readonly Recipe RavenCharm = new()
    {
        Id = "raven_charm",
        Name = "Raven Charm",
        ResultItem = "raven_charm",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "feather", 25 },
            { "bone", 10 }
        }
    };
}
using Hogs.RPG.Core.Entities.GlobalBossObjects;
using Hogs.RPG.Core.GameData.GlobalBosses;
using static Hogs.RPG.Core.Entities.GlobalBossObjects.BossDefinition;

namespace Hogs.RPG.Core.Registries
{
    public static class GlobalBossRegistry
    {
        // ============================================================
        // MASTER REGISTRY
        // ============================================================

        public static readonly IReadOnlyDictionary<string, BossDefinition> All =
            new Dictionary<string, BossDefinition>
            {
                { AllBosses.Aurelius.Id, AllBosses.Aurelius },
                { AllBosses.Gravelmaw.Id, AllBosses.Gravelmaw },
                { AllBosses.PrimordialSerpent.Id, AllBosses.PrimordialSerpent },
                { AllBosses.Xerathul.Id, AllBosses.Xerathul },
                { AllBosses.TwoTierTyr.Id, AllBosses.TwoTierTyr },
                { AllBosses.KingThorlak.Id, AllBosses.KingThorlak },
                { AllBosses.ClickPunisher.Id, AllBosses.ClickPunisher },
                { AllBosses.GullveigHuld.Id, AllBosses.GullveigHuld }
            };

        // ============================================================
        // LOOKUPS
        // ============================================================

        public static List<BossDefinition> GetByType(BossType type)
            => All.Values.Where(b => b.Type == type).ToList();

        public static BossDefinition? GetById(string id)
            => id != null && All.TryGetValue(id, out var boss) ? boss : null;
    }
}

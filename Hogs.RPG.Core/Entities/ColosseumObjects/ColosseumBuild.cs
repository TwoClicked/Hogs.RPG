using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;

namespace Hogs.RPG.Core.Entities.ColosseumObjects
{
    /// <summary>
    /// The sandboxed loadout a participant builds for one Colosseum
    /// tournament. Fully separate from the player's real inventory - buying
    /// a piece of gear here just sets a slot to that EquipmentDefinition's
    /// Id, it doesn't touch anything the player actually owns.
    ///
    /// Every participant starts on the free T1 baseline (T1 gear in every
    /// slot, a T1 pet, no passive, no buffs) and spends ApBudget upgrading
    /// from there. The 9 gear slot fields are nullable so "still on the free
    /// T1 baseline" can be represented as null rather than duplicating the
    /// T1 item ids here - ColosseumService resolves null slots against the
    /// T1 baseline at build/combat time.
    /// </summary>
    public class ColosseumBuild
    {
        public int Id { get; set; }

        public int ColosseumParticipantId { get; set; }
        public ColosseumParticipant ColosseumParticipant { get; set; } = null!;

        // =========================
        // BUDGET
        // =========================
        // Copied from ColosseumTournament.BuildBudgetAP when this build is
        // created, so a tournament's budget can be tuned run-to-run without
        // retroactively changing builds already in progress.
        public int ApBudget { get; set; } = 1000;

        // Running total of AP spent across gear + pet + passive + buffs.
        // Every purchase in ColosseumService must validate ApSpent + cost
        // <= ApBudget before applying.
        public int ApSpent { get; set; } = 0;

        // =========================
        // GEAR (9 slots)
        // Null = still on the free T1 baseline for that slot.
        // Non-null = EquipmentDefinition.Id of the purchased upgrade.
        // =========================
        public string? GearMainHandId { get; set; }
        public string? GearOffHandId { get; set; }
        public string? GearHelmetId { get; set; }
        public string? GearBodyId { get; set; }
        public string? GearLegsId { get; set; }
        public string? GearGlovesId { get; set; }
        public string? GearBootsId { get; set; }
        public string? GearRingId { get; set; }
        public string? GearAmuletId { get; set; }

        // =========================
        // PET
        // =========================
        // Null = still on the free T1 baseline pet. Non-null = PetDefinition.Id
        // of the purchased pet tier upgrade.
        public string? PetId { get; set; }

        // Cached copy of the equipped pet's tier (1-3) for quick reference
        // without a registry lookup. Kept in sync by ColosseumService whenever
        // PetId changes.
        public int PetTier { get; set; } = 1;

        // Single passive slot for Colosseum builds (the live game allows two
        // passive slots on a real pet, but we're keeping this to one to match
        // the pricing model we designed and to keep bracket-scale combat logs
        // readable). Null = no passive purchased.
        public PetPassive? PetPassive { get; set; }

        // =========================
        // STORE BUFFS
        // Flat combat stat boosts, Tower-shop style. Each capped at 3
        // purchases so nobody dumps their whole budget into one stat.
        // =========================
        public int BuffAttackPurchases { get; set; } = 0;
        public int BuffDefensePurchases { get; set; } = 0;
        public int BuffHealthPurchases { get; set; } = 0;

        // =========================
        // LOCK STATE
        // =========================
        // Set when the participant hits "lock in" in the DM build UI, or
        // when the registration-lock scheduler randomizes an unfinished
        // build. Once set, ColosseumService should reject further purchases
        // against this build.
        public DateTime? LockedAt { get; set; }
    }
}
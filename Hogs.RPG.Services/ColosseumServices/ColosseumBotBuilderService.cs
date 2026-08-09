using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.GameData.Colosseum;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.Registries;

namespace Hogs.RPG.Services.ColosseumServices
{
    /// <summary>
    /// Generates fully randomized Colosseum builds. Used for two cases that
    /// share the exact same logic per the design decision: bot participants
    /// padding the bracket out to 32, and real players who signed up but
    /// never locked a build before the registration deadline.
    ///
    /// Spending strategy: every possible purchase (each gear slot upgrade
    /// option, each pet tier+id combo, each passive, each buff purchase) is
    /// thrown into one flat pool, shuffled, then bought greedily in that
    /// random order as long as it still fits the remaining budget and its
    /// category isn't already spoken for. A single pass over a shuffled pool
    /// naturally uses up most of the budget without needing multiple retry
    /// passes - cheap options later in the shuffle still get a chance even
    /// after an earlier expensive one was skipped for being unaffordable.
    /// </summary>
    public class ColosseumBotBuilderService
    {
        private static readonly Random _random = new();

        // Flavor names for bot participants - real players use their Discord
        // display name instead, so this pool only needs to cover bots.
        private static readonly string[] _botNamePool =
        {
            "Skull Splitter", "Iron Tusk", "Frost Reaver", "Ash Wolf", "Grim Anchor",
            "Rust Fang", "Bone Chanter", "Storm Yoke", "Wretch Hollow", "Ember Tusk",
            "Salt Beard", "Crow Marsh", "Hollow Tide", "Cinder Wolf", "Gale Ravager",
            "Thorn Wick", "Muck Warden", "Pale Antler", "Drift Ember", "Wraith Anchor"
        };

        /// <summary>
        /// Creates a bot ColosseumParticipant (unsaved - caller persists via
        /// ColosseumRepository) with a randomized flavor name and a fully
        /// randomized build against the tournament's AP budget.
        /// </summary>
        public ColosseumParticipant GenerateBotParticipant(ColosseumTournament tournament)
        {
            var participant = new ColosseumParticipant
            {
                ColosseumTournamentId = tournament.Id,
                DiscordId = 0,
                IsBot = true,
                BotDisplayName = $"{_botNamePool[_random.Next(_botNamePool.Length)]} (Bot)",
                BuildLocked = true,
                BuildWasRandomized = true
            };

            participant.Build = GenerateRandomBuild(tournament.BuildBudgetAP);
            return participant;
        }

        /// <summary>
        /// Produces a randomized, budget-respecting build. Also used for real
        /// players who didn't lock in before the registration deadline -
        /// ColosseumService should call this and set BuildWasRandomized=true
        /// on their participant when that happens.
        /// </summary>
        public ColosseumBuild GenerateRandomBuild(int apBudget)
        {
            var build = new ColosseumBuild
            {
                ApBudget = apBudget,
                ApSpent = 0,
                LockedAt = DateTime.UtcNow
            };

            var pool = BuildCandidatePool();
            Shuffle(pool);

            var slotsFilled = new HashSet<EquipmentSlot>();
            var petChosen = false;
            var passiveChosen = false;
            var buffCounts = new Dictionary<BuffStat, int>
            {
                { BuffStat.Attack, 0 },
                { BuffStat.Defense, 0 },
                { BuffStat.Health, 0 }
            };

            foreach (var option in pool)
            {
                if (build.ApSpent + option.Cost > apBudget)
                    continue;

                switch (option.Kind)
                {
                    case CandidateKind.Gear:
                        if (slotsFilled.Contains(option.Slot!.Value))
                            continue;
                        ApplyGear(build, option);
                        slotsFilled.Add(option.Slot.Value);
                        break;

                    case CandidateKind.Pet:
                        if (petChosen)
                            continue;
                        ApplyPet(build, option);
                        petChosen = true;
                        break;

                    case CandidateKind.Passive:
                        if (passiveChosen)
                            continue;
                        build.PetPassive = option.Passive;
                        passiveChosen = true;
                        break;

                    case CandidateKind.Buff:
                        if (buffCounts[option.BuffStat!.Value] >= ColosseumBuffShop.MaxPurchasesPerStat)
                            continue;
                        ApplyBuff(build, option);
                        buffCounts[option.BuffStat.Value]++;
                        break;
                }

                build.ApSpent += option.Cost;
            }

            return build;
        }

        // =========================
        // CANDIDATE POOL
        // =========================
        private List<Candidate> BuildCandidatePool()
        {
            var pool = new List<Candidate>();

            // Gear: every purchasable item in every slot is its own candidate.
            // Once one is bought for a slot, the rest for that slot are
            // skipped in the main loop (slotsFilled check).
            foreach (var (slot, itemIds) in ColosseumPriceRegistry.PurchasableGearOptionsBySlot)
            {
                foreach (var itemId in itemIds)
                {
                    pool.Add(new Candidate
                    {
                        Kind = CandidateKind.Gear,
                        Slot = slot,
                        GearItemId = itemId,
                        Cost = ColosseumPriceRegistry.GetGearCost(itemId)
                    });
                }
            }

            // Pet: every non-baseline pet id is a candidate.
            foreach (var (petId, cost) in ColosseumPetPrices.ApCostByPetId)
            {
                if (cost == 0) continue; // T1 baseline, not a purchase
                pool.Add(new Candidate { Kind = CandidateKind.Pet, PetId = petId, Cost = cost });
            }

            // Passive: all 5 are candidates.
            foreach (var (passive, cost) in ColosseumPetPrices.ApCostByPassive)
            {
                pool.Add(new Candidate { Kind = CandidateKind.Passive, Passive = passive, Cost = cost });
            }

            // Buffs: add MaxPurchasesPerStat separate candidates per stat so
            // the random shuffle can naturally buy 0-3 of each independently.
            foreach (BuffStat stat in Enum.GetValues<BuffStat>())
            {
                var cost = ColosseumPriceRegistry.GetBuffCost(stat);
                for (var i = 0; i < ColosseumBuffShop.MaxPurchasesPerStat; i++)
                {
                    pool.Add(new Candidate { Kind = CandidateKind.Buff, BuffStat = stat, Cost = cost });
                }
            }

            return pool;
        }

        private void ApplyGear(ColosseumBuild build, Candidate option)
        {
            switch (option.Slot)
            {
                case EquipmentSlot.MainHand: build.GearMainHandId = option.GearItemId; break;
                case EquipmentSlot.OffHand: build.GearOffHandId = option.GearItemId; break;
                case EquipmentSlot.Helmet: build.GearHelmetId = option.GearItemId; break;
                case EquipmentSlot.Body: build.GearBodyId = option.GearItemId; break;
                case EquipmentSlot.Legs: build.GearLegsId = option.GearItemId; break;
                case EquipmentSlot.Gloves: build.GearGlovesId = option.GearItemId; break;
                case EquipmentSlot.Boots: build.GearBootsId = option.GearItemId; break;
                case EquipmentSlot.Ring: build.GearRingId = option.GearItemId; break;
                case EquipmentSlot.Amulet: build.GearAmuletId = option.GearItemId; break;
            }
        }

        private void ApplyPet(ColosseumBuild build, Candidate option)
        {
            build.PetId = option.PetId;
            build.PetTier = PetRegistry.Get(option.PetId!).Tier;
        }

        private void ApplyBuff(ColosseumBuild build, Candidate option)
        {
            switch (option.BuffStat)
            {
                case BuffStat.Attack: build.BuffAttackPurchases++; break;
                case BuffStat.Defense: build.BuffDefensePurchases++; break;
                case BuffStat.Health: build.BuffHealthPurchases++; break;
            }
        }

        private void Shuffle(List<Candidate> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // =========================
        // CANDIDATE MODEL
        // Internal only - never persisted, just used to drive the shuffle.
        // =========================
        private enum CandidateKind { Gear, Pet, Passive, Buff }

        private class Candidate
        {
            public CandidateKind Kind { get; set; }
            public int Cost { get; set; }

            public EquipmentSlot? Slot { get; set; }
            public string? GearItemId { get; set; }

            public string? PetId { get; set; }

            public PetPassive? Passive { get; set; }

            public BuffStat? BuffStat { get; set; }
        }
    }
}
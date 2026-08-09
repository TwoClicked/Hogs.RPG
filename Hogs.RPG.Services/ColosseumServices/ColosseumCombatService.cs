using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Entities.PetObjects;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.GameData.Colosseum;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.Registries;

namespace Hogs.RPG.Services.ColosseumServices
{
    /// <summary>
    /// Fully auto-resolves one Colosseum match between two builds - no live
    /// Discord input required, same philosophy as Tower's floor clears.
    /// Reuses the same damage formula Tower/Raid use
    /// (Attack * 100/(100+Defense)) and the existing PetPassiveService so
    /// pet passive behavior stays identical to the live game, just applied
    /// symmetrically to both sides instead of player-vs-fixed-enemy.
    ///
    /// This service is Discord-agnostic on purpose - it takes two
    /// participants (with resolved display names) and returns a winner plus
    /// a combat log string. Posting that log to a match thread is
    /// ColosseumBracketService's job, not this one.
    /// </summary>
    public class ColosseumCombatService
    {
        private readonly PetPassiveService _petPassiveService;
        private readonly Random _random = new();

        // Every fresh Player starts at Attack 5 / Defense 5 / MaxHealth 100
        // (see PlayerCommands new-player creation) - using the same baseline
        // here keeps Colosseum stats in a familiar range rather than
        // inventing new numbers, and keeps everyone equal regardless of
        // their real account's level.
        private const int BaseAttack = 5;
        private const int BaseDefense = 5;
        private const int BaseHealth = 100;


        private const int ColosseumPetLevel = 15;

        private const int MaxRounds = 100; // safety cap, formula guarantees >=1 dmg/hit so this shouldn't ever bind

        public ColosseumCombatService(PetPassiveService petPassiveService)
        {
            _petPassiveService = petPassiveService;
        }

        public ColosseumCombatResult ResolveMatch(
            ColosseumParticipant participantA, string displayNameA,
            ColosseumParticipant participantB, string displayNameB)
        {
            var a = BuildCombatant(participantA, displayNameA);
            var b = BuildCombatant(participantB, displayNameB);

            var log = new List<string>
            {
                $"⚔️ **{a.DisplayName}** (ATK {a.Attack} / DEF {a.Defense} / HP {a.MaxHealth}) vs **{b.DisplayName}** (ATK {b.Attack} / DEF {b.Defense} / HP {b.MaxHealth})",
                ""
            };

            // Coin flip for who swings first - no structural advantage for
            // whichever participant happens to be "A" in the match record.
            var attacker = _random.NextDouble() < 0.5 ? a : b;
            var defender = attacker == a ? b : a;

            var round = 1;
            while (a.CurrentHealth > 0 && b.CurrentHealth > 0 && round <= MaxRounds)
            {
                ResolveStrike(attacker, defender, log);

                if (defender.CurrentHealth <= 0)
                    break;

                (attacker, defender) = (defender, attacker);
                round++;
            }

            var winner = a.CurrentHealth > 0 ? a : b;
            var loser = winner == a ? b : a;

            log.Add("");
            log.Add($"🏆 **{winner.DisplayName}** wins! ({winner.CurrentHealth}/{winner.MaxHealth} HP remaining)");

            return new ColosseumCombatResult
            {
                WinnerParticipantId = winner.ParticipantId,
                LoserParticipantId = loser.ParticipantId,
                CombatLog = string.Join("\n", log)
            };
        }

        // =========================
        // ONE STRIKE
        // =========================
        private void ResolveStrike(ColosseumCombatant attacker, ColosseumCombatant defender, List<string> log)
        {
            int dmg = (int)(attacker.Attack * (100.0 / (100.0 + defender.Defense)));
            dmg = Math.Max(1, dmg);

            // Attacker's pet: DoubleStrike / Executioner (Executioner needs
            // the defender's current HP% to check its low-HP threshold).
            var (outgoingDmg, outgoingTrigger) = _petPassiveService.ModifyOutgoingDamage(
                dmg, attacker.PetForPassives, attacker.PetDefinition, defender.CurrentHealth, defender.MaxHealth);
            dmg = outgoingDmg;

            // Defender's pet: GuardianShield chance to reduce incoming damage.
            var (mitigatedDmg, incomingTrigger) = _petPassiveService.ModifyIncomingDamage(dmg, defender.PetForPassives);
            dmg = mitigatedDmg;

            defender.CurrentHealth = Math.Max(0, defender.CurrentHealth - dmg);

            log.Add($"🗡️ **{attacker.DisplayName}** hits **{defender.DisplayName}** for **{dmg}** damage! ({defender.CurrentHealth}/{defender.MaxHealth} HP)");
            if (outgoingTrigger != null) log.Add(outgoingTrigger);
            if (incomingTrigger != null) log.Add(incomingTrigger);

            // Attacker's pet: Lifesteal heals off the damage actually dealt.
            var healing = _petPassiveService.ApplyOnHitEffects(dmg, null, attacker.PetForPassives);
            if (healing > 0)
            {
                attacker.CurrentHealth = Math.Min(attacker.MaxHealth, attacker.CurrentHealth + healing);
                log.Add($"❤️ **{attacker.DisplayName}** lifesteals **{healing}** HP! ({attacker.CurrentHealth}/{attacker.MaxHealth} HP)");
            }

            // Defender's pet: Thorns reflects some damage back at the attacker.
            var reflect = _petPassiveService.ApplyOnHitTaken(dmg, defender.PetForPassives);
            if (reflect > 0 && defender.CurrentHealth > 0)
            {
                attacker.CurrentHealth = Math.Max(0, attacker.CurrentHealth - reflect);
                log.Add($"🌵 **{defender.DisplayName}**'s Thorns reflects **{reflect}** damage back! (**{attacker.DisplayName}**: {attacker.CurrentHealth}/{attacker.MaxHealth} HP)");
            }
        }

        // =========================
        // BUILD COMBATANT FROM COLOSSEUM BUILD
        // =========================
        private ColosseumCombatant BuildCombatant(ColosseumParticipant participant, string displayName)
        {
            var build = participant.Build ?? throw new Exception($"Colosseum: participant {participant.Id} has no build to resolve combat with.");

            int attack = BaseAttack;
            int defense = BaseDefense;
            int health = BaseHealth;

            // ===== Gear =====
            var slots = new[]
            {
                (EquipmentSlot.MainHand, build.GearMainHandId),
                (EquipmentSlot.OffHand, build.GearOffHandId),
                (EquipmentSlot.Helmet, build.GearHelmetId),
                (EquipmentSlot.Body, build.GearBodyId),
                (EquipmentSlot.Legs, build.GearLegsId),
                (EquipmentSlot.Gloves, build.GearGlovesId),
                (EquipmentSlot.Boots, build.GearBootsId),
                (EquipmentSlot.Ring, build.GearRingId),
                (EquipmentSlot.Amulet, build.GearAmuletId),
            };

            foreach (var (slot, purchasedId) in slots)
            {
                var itemId = ColosseumPriceRegistry.ResolveGearId(slot, purchasedId);
                if (EquipmentRegistry.All.TryGetValue(itemId, out var item))
                {
                    attack += item.Attack;
                    defense += item.Defense;
                    health += item.Health;
                }
            }

            // ===== Pet =====
            var petId = ColosseumPriceRegistry.ResolvePetId(build.PetId);
            PetDefinition? petDef = null;
            if (PetRegistry.All.TryGetValue(petId, out var resolvedPetDef))
            {
                petDef = resolvedPetDef;
                attack += resolvedPetDef.BaseAttack + (int)(ColosseumPetLevel * resolvedPetDef.Scaling);
                defense += resolvedPetDef.BaseDefense + (int)(ColosseumPetLevel * resolvedPetDef.Scaling);
                health += resolvedPetDef.BaseHealth + (int)(ColosseumPetLevel * resolvedPetDef.Scaling * 5);
            }

            // ===== Store buffs =====
            attack += build.BuffAttackPurchases * ColosseumBuffShop.AttackBuffAmount;
            defense += build.BuffDefensePurchases * ColosseumBuffShop.DefenseBuffAmount;
            health += build.BuffHealthPurchases * ColosseumBuffShop.HealthBuffAmount;

            // PetPassiveService reads passives off a PlayerPet entity
            // (Passive1/Passive2). Colosseum builds only ever have one
            // passive slot, so this is a throwaway, never-persisted PlayerPet
            // just to reuse that existing logic without duplicating it.
            var petForPassives = new PlayerPet
            {
                PetId = petId,
                Passive1 = build.PetPassive,
                Passive2 = null
            };

            return new ColosseumCombatant
            {
                ParticipantId = participant.Id,
                DisplayName = displayName,
                Attack = attack,
                Defense = defense,
                MaxHealth = health,
                CurrentHealth = health,
                PetDefinition = petDef,
                PetForPassives = petForPassives
            };
        }

        // =========================
        // INTERNAL COMBATANT MODEL
        // =========================
        private class ColosseumCombatant
        {
            public int ParticipantId { get; set; }
            public string DisplayName { get; set; } = "";
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int MaxHealth { get; set; }
            public int CurrentHealth { get; set; }
            public PetDefinition? PetDefinition { get; set; }
            public PlayerPet PetForPassives { get; set; } = null!;
        }
    }

    /// <summary>
    /// Result of one resolved Colosseum match - winner/loser participant ids
    /// plus the full text log to post in the match thread.
    /// </summary>
    public class ColosseumCombatResult
    {
        public int WinnerParticipantId { get; set; }
        public int LoserParticipantId { get; set; }
        public string CombatLog { get; set; } = "";
    }
}
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
    /// Log formatting is deliberately compact: each strike (including any
    /// passive triggers - double strike, guardian shield, lifesteal,
    /// thorns) collapses onto a single line rather than one line per
    /// effect, and very long fights (tanky Thorns-vs-Thorns grinds
    /// especially) get truncated to the first/last few exchanges rather
    /// than printing every single round - a 40-round slugfest was
    /// producing 100+ lines of near-identical output before this.
    ///
    /// This service is Discord-agnostic on purpose - it takes two
    /// participants (with resolved display names) and returns a winner plus
    /// a combat log string. Posting that log to a match thread is
    /// ColosseumScheduler's job, not this one.
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

        // Colosseum pets are treated as this fixed level for stat purposes -
        // there's no leveling/XP context in a sandboxed build. Bumped up
        // from an initial level 1 so pet tier actually contributes
        // meaningful stats, not just the passive slot.
        private const int ColosseumPetLevel = 15;

        private const int MaxRounds = 100; // safety cap, formula guarantees >=1 dmg/hit so this shouldn't ever bind

        // Long fights get condensed to this many exchanges from the start
        // and this many from the end, with a summary line in between,
        // rather than printing every single round.
        private const int KeepExchangesEachEnd = 5;

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

            var strikeLines = new List<string>();

            // Coin flip for who swings first - no structural advantage for
            // whichever participant happens to be "A" in the match record.
            var attacker = _random.NextDouble() < 0.5 ? a : b;
            var defender = attacker == a ? b : a;

            var round = 1;
            while (a.CurrentHealth > 0 && b.CurrentHealth > 0 && round <= MaxRounds)
            {
                ResolveStrike(attacker, defender, strikeLines);

                if (defender.CurrentHealth <= 0)
                    break;

                (attacker, defender) = (defender, attacker);
                round++;
            }

            var winner = a.CurrentHealth > 0 ? a : b;
            var loser = winner == a ? b : a;

            var log = new List<string>
            {
                $"⚔️ **{a.DisplayName}** (ATK {a.Attack} / DEF {a.Defense} / HP {a.MaxHealth}) vs **{b.DisplayName}** (ATK {b.Attack} / DEF {b.Defense} / HP {b.MaxHealth})",
                ""
            };

            log.AddRange(CondenseStrikeLines(strikeLines));

            log.Add("");
            log.Add($"🏆 **{winner.DisplayName}** wins! ({winner.CurrentHealth}/{winner.MaxHealth} HP remaining)");

            return new ColosseumCombatResult
            {
                WinnerParticipantId = winner.ParticipantId,
                LoserParticipantId = loser.ParticipantId,
                CombatLog = string.Join("\n", log)
            };
        }

        // Keeps the first and last few exchanges of a long fight, replacing
        // the middle with a one-line summary, so a 30+ round grind doesn't
        // dump 30+ near-identical lines into the thread.
        private List<string> CondenseStrikeLines(List<string> lines)
        {
            if (lines.Count <= KeepExchangesEachEnd * 2 + 1)
                return lines;

            var result = new List<string>();
            result.AddRange(lines.Take(KeepExchangesEachEnd));

            var omitted = lines.Count - (KeepExchangesEachEnd * 2);
            result.Add($"*… {omitted} more exchanges …*");

            result.AddRange(lines.Skip(lines.Count - KeepExchangesEachEnd));
            return result;
        }

        // =========================
        // ONE STRIKE
        // Builds a single combined line - base hit plus any passive
        // triggers (outgoing/incoming modifiers, lifesteal, thorns) all
        // appended to the same line rather than logged separately.
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

            var line = new System.Text.StringBuilder();
            line.Append($"🗡️ **{attacker.DisplayName}** hits **{defender.DisplayName}** for **{dmg}** ({defender.CurrentHealth}/{defender.MaxHealth} HP)");

            if (outgoingTrigger != null) line.Append($" · {outgoingTrigger}");
            if (incomingTrigger != null) line.Append($" · {incomingTrigger}");

            // Attacker's pet: Lifesteal heals off the damage actually dealt.
            var healing = _petPassiveService.ApplyOnHitEffects(dmg, null, attacker.PetForPassives);
            if (healing > 0)
            {
                attacker.CurrentHealth = Math.Min(attacker.MaxHealth, attacker.CurrentHealth + healing);
                line.Append($" · ❤️ lifesteals {healing} ({attacker.CurrentHealth}/{attacker.MaxHealth})");
            }

            // Defender's pet: Thorns reflects some damage back at the attacker.
            var reflect = _petPassiveService.ApplyOnHitTaken(dmg, defender.PetForPassives);
            if (reflect > 0 && defender.CurrentHealth > 0)
            {
                attacker.CurrentHealth = Math.Max(0, attacker.CurrentHealth - reflect);
                line.Append($" · 🌵 thorns {reflect} back ({attacker.DisplayName}: {attacker.CurrentHealth}/{attacker.MaxHealth})");
            }

            log.Add(line.ToString());
        }

        // =========================
        // BUILD COMBATANT FROM COLOSSEUM BUILD
        // =========================
        private ColosseumCombatant BuildCombatant(ColosseumParticipant participant, string displayName)
        {
            var build = participant.Build ?? throw new Exception($"Colosseum: participant {participant.Id} has no build to resolve combat with.");

            var (attack, defense, health) = ColosseumStatCalculator.CalculateStats(build);

            var petId = ColosseumPriceRegistry.ResolvePetId(build.PetId);
            PetRegistry.All.TryGetValue(petId, out var petDef);

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
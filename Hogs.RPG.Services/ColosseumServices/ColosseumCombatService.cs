using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Entities.PetObjects;
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
    /// Combat is SIMULTANEOUS: both sides' damage for a round is computed
    /// from stats as they stood at the start of that round, then applied
    /// together - there's no "attacker swings, defender's health updates,
    /// then defender swings" sequencing. HP is allowed to go negative
    /// during a round (not clamped to 0) specifically so a round where
    /// BOTH sides die can be resolved fairly: whoever's HP is LESS negative
    /// (e.g. -34 beats -50) wins that exchange, since they were closer to
    /// surviving it.
    ///
    /// This service is Discord-agnostic on purpose - it takes two
    /// participants (with resolved display names) and returns a winner plus
    /// a combat log string. Posting that log to a match thread is
    /// ColosseumScheduler's job, not this one.
    /// </summary>
    public class ColosseumCombatService
    {
        private readonly PetPassiveService _petPassiveService;

        private const int MaxRounds = 100; // safety cap, formula guarantees >=1 dmg/hit so this shouldn't ever bind

        // Long fights get condensed to this many rounds from the start and
        // this many from the end, with a summary line in between, rather
        // than printing every single round.
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

            var roundLines = new List<string>();
            var round = 1;

            while (a.CurrentHealth > 0 && b.CurrentHealth > 0 && round <= MaxRounds)
            {
                ResolveRound(a, b, roundLines);
                round++;
            }

            var (winner, loser) = DetermineWinner(a, b);

            var log = new List<string>
            {
                $"⚔️ **{a.DisplayName}** (ATK {a.Attack} / DEF {a.Defense} / HP {a.MaxHealth}) vs **{b.DisplayName}** (ATK {b.Attack} / DEF {b.Defense} / HP {b.MaxHealth})",
                ""
            };

            log.AddRange(CondenseStrikeLines(roundLines));

            log.Add("");
            log.Add(winner.CurrentHealth > 0
                ? $"🏆 **{winner.DisplayName}** wins! ({winner.CurrentHealth}/{winner.MaxHealth} HP remaining)"
                : $"🏆 **{winner.DisplayName}** wins the mutual exchange! ({winner.CurrentHealth} HP vs {loser.CurrentHealth} HP - closer to surviving)");

            return new ColosseumCombatResult
            {
                WinnerParticipantId = winner.ParticipantId,
                LoserParticipantId = loser.ParticipantId,
                CombatLog = string.Join("\n", log)
            };
        }

        // =========================
        // WINNER DETERMINATION
        // =========================
        // Normal case: exactly one combatant is at or below 0 HP - they lose.
        // Mutual kill (both at or below 0 in the same round): whoever's HP
        // is LESS negative wins - they were closer to surviving the
        // exchange. Compared as raw HP, not percentage, per design intent:
        // a mutual kill is about who almost made it, not about relative
        // toughness.
        // MaxRounds safety cap (both still alive, extremely rare - only a
        // heavy-Lifesteal stalemate could realistically reach it): compared
        // as a PERCENTAGE of max HP instead, since this is a genuinely
        // different scenario (nobody died) and builds with very different
        // HP pools need a proportional comparison to be fair here.
        private (ColosseumCombatant winner, ColosseumCombatant loser) DetermineWinner(ColosseumCombatant a, ColosseumCombatant b)
        {
            var aDead = a.CurrentHealth <= 0;
            var bDead = b.CurrentHealth <= 0;

            if (aDead && bDead)
            {
                // Mutual kill - less negative HP wins.
                return a.CurrentHealth >= b.CurrentHealth ? (a, b) : (b, a);
            }

            if (aDead) return (b, a);
            if (bDead) return (a, b);

            // Neither died - MaxRounds cap was hit. Compare proportionally.
            double aPercent = (double)a.CurrentHealth / a.MaxHealth;
            double bPercent = (double)b.CurrentHealth / b.MaxHealth;
            return aPercent >= bPercent ? (a, b) : (b, a);
        }

        // Keeps the first and last few rounds of a long fight, replacing
        // the middle with a one-line summary, so a 30+ round grind doesn't
        // dump 30+ near-identical lines into the thread.
        private List<string> CondenseStrikeLines(List<string> lines)
        {
            if (lines.Count <= KeepExchangesEachEnd * 2 + 1)
                return lines;

            var result = new List<string>();
            result.AddRange(lines.Take(KeepExchangesEachEnd));

            var omitted = lines.Count - (KeepExchangesEachEnd * 2);
            result.Add($"*… {omitted} more rounds …*");

            result.AddRange(lines.Skip(lines.Count - KeepExchangesEachEnd));
            return result;
        }

        // =========================
        // ONE ROUND - SIMULTANEOUS
        // Both sides' damage is computed from pre-round stats, then applied
        // together. Thorns reflect from each side factors into the OTHER
        // side's total damage this same round, so two Thorns builds both
        // take extra damage from each other's reflection in one pass.
        // =========================
        private void ResolveRound(ColosseumCombatant a, ColosseumCombatant b, List<string> log)
        {
            int aHpBefore = a.CurrentHealth;
            int bHpBefore = b.CurrentHealth;

            // ===== Base damage each direction =====
            int dmgAtoB = Math.Max(1, (int)(a.Attack * (100.0 / (100.0 + b.Defense))));
            int dmgBtoA = Math.Max(1, (int)(b.Attack * (100.0 / (100.0 + a.Defense))));

            // ===== Outgoing modifiers (DoubleStrike / Executioner) - based on pre-round opponent HP =====
            var (modAtoB, outTriggerA) = _petPassiveService.ModifyOutgoingDamage(dmgAtoB, a.PetForPassives, a.PetDefinition, bHpBefore, b.MaxHealth);
            var (modBtoA, outTriggerB) = _petPassiveService.ModifyOutgoingDamage(dmgBtoA, b.PetForPassives, b.PetDefinition, aHpBefore, a.MaxHealth);
            dmgAtoB = modAtoB;
            dmgBtoA = modBtoA;

            // ===== Incoming modifiers (GuardianShield) =====
            var (mitAtoB, inTriggerB) = _petPassiveService.ModifyIncomingDamage(dmgAtoB, b.PetForPassives);
            var (mitBtoA, inTriggerA) = _petPassiveService.ModifyIncomingDamage(dmgBtoA, a.PetForPassives);
            dmgAtoB = mitAtoB;
            dmgBtoA = mitBtoA;

            // ===== Thorns - defender's reflect adds to the ATTACKER's total damage this round =====
            int reflectOnA = _petPassiveService.ApplyOnHitTaken(dmgBtoA, a.PetForPassives); // A's Thorns, triggered by B's hit
            int reflectOnB = _petPassiveService.ApplyOnHitTaken(dmgAtoB, b.PetForPassives); // B's Thorns, triggered by A's hit

            int totalDmgToA = dmgBtoA + reflectOnB; // B's hit on A, plus B's Thorns retaliating A's hit on B
            int totalDmgToB = dmgAtoB + reflectOnA; // A's hit on B, plus A's Thorns retaliating B's hit on A

            // ===== Apply simultaneously - NOT clamped to 0, so a mutual-kill round can be compared afterward =====
            a.CurrentHealth -= totalDmgToA;
            b.CurrentHealth -= totalDmgToB;

            // ===== Lifesteal - only heals if the healer survived this round (no reviving corpses) =====
            int healA = _petPassiveService.ApplyOnHitEffects(dmgAtoB, null, a.PetForPassives);
            int healB = _petPassiveService.ApplyOnHitEffects(dmgBtoA, null, b.PetForPassives);

            if (a.CurrentHealth > 0 && healA > 0)
                a.CurrentHealth = Math.Min(a.MaxHealth, a.CurrentHealth + healA);

            if (b.CurrentHealth > 0 && healB > 0)
                b.CurrentHealth = Math.Min(b.MaxHealth, b.CurrentHealth + healB);

            // ===== Log line for this round =====
            var line = new System.Text.StringBuilder();
            line.Append($"🗡️ **{a.DisplayName}** hits **{b.DisplayName}** for {dmgAtoB}");
            if (outTriggerA != null) line.Append($" · {outTriggerA}");
            if (inTriggerB != null) line.Append($" · {inTriggerB}");
            line.Append($" | **{b.DisplayName}** hits **{a.DisplayName}** for {dmgBtoA}");
            if (outTriggerB != null) line.Append($" · {outTriggerB}");
            if (inTriggerA != null) line.Append($" · {inTriggerA}");

            if (reflectOnB > 0) line.Append($" · 🌵 {b.DisplayName}'s Thorns reflects {reflectOnB}");
            if (reflectOnA > 0) line.Append($" · 🌵 {a.DisplayName}'s Thorns reflects {reflectOnA}");
            if (a.CurrentHealth > 0 && healA > 0) line.Append($" · ❤️ {a.DisplayName} lifesteals {healA}");
            if (b.CurrentHealth > 0 && healB > 0) line.Append($" · ❤️ {b.DisplayName} lifesteals {healB}");

            line.Append($" ({a.DisplayName}: {a.CurrentHealth}/{a.MaxHealth} | {b.DisplayName}: {b.CurrentHealth}/{b.MaxHealth})");

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
            public int CurrentHealth { get; set; } // allowed to go negative during a round
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
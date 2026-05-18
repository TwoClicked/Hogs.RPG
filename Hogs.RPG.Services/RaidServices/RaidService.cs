using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.RelicServices;

namespace Hogs.RPG.Services.RaidServices
{
    public class RaidService
    {
        private readonly RaidRepository _raidRepo;
        private readonly PlayerRepository _playerRepo;
        private readonly InventoryRepository _inventoryRepo;
        private readonly RelicService _relicService;
        private readonly StatService _statService;
        private readonly PetService _petService;
        private readonly PetPassiveService _petPassiveService;
        private readonly LevelService _levelService;
        private readonly DiscordSocketClient _client;

        private static readonly Random _random = new();

        private const int RaidCooldownHours = 1;
        private const int RaidGoldReward = 1000;
        private const int RaidPlayerXpReward = 1500;
        private const int RaidPetXpReward = 100;
        private const int WipeGoldPenalty = 1000;
        private const float RelicShardDropChance = 0.03f;

        public RaidService(
            RaidRepository raidRepo,
            PlayerRepository playerRepo,
            InventoryRepository inventoryRepo,
            RelicService relicService,
            StatService statService,
            PetService petService,
            PetPassiveService petPassiveService,
            LevelService levelService,
            DiscordSocketClient client)
        {
            _raidRepo = raidRepo;
            _playerRepo = playerRepo;
            _inventoryRepo = inventoryRepo;
            _relicService = relicService;
            _statService = statService;
            _petService = petService;
            _petPassiveService = petPassiveService;
            _levelService = levelService;
            _client = client;
        }

        // =========================
        // ⏳ COOLDOWN CHECK
        // =========================
        public async Task<(bool onCooldown, TimeSpan remaining)> CheckCooldownAsync(ulong discordId)
        {
            var player = await _playerRepo.GetByDiscordIdAsync(discordId);
            if (player == null) return (false, TimeSpan.Zero);

            if (string.IsNullOrEmpty(player.LastRaidAt))
                return (false, TimeSpan.Zero);

            var lastRaid = DateTimeOffset.Parse(player.LastRaidAt);
            var elapsed = DateTimeOffset.UtcNow - lastRaid;

            if (elapsed.TotalHours < RaidCooldownHours)
            {
                var remaining = TimeSpan.FromHours(RaidCooldownHours) - elapsed;
                return (true, remaining);
            }

            return (false, TimeSpan.Zero);
        }

        // =========================
        // 🏰 CREATE LOBBY
        // =========================
        public async Task<(bool success, string message, RaidSession? session)> CreateLobbyAsync(
            ulong discordId, string username, int tier, RaidRole role, ulong channelId)
        {
            var raidDef = RaidRegistry.GetByTier(tier);
            if (raidDef == null)
                return (false, "❌ Invalid raid tier.", null);

            var player = await _playerRepo.GetByDiscordIdAsync(discordId);
            if (player == null)
                return (false, "❌ You need to start your adventure first.", null);

            if (player.Level < raidDef.RequiredLevel)
                return (false, $"❌ You need to be Level {raidDef.RequiredLevel} to enter this raid.", null);

            var (onCooldown, remaining) = await CheckCooldownAsync(discordId);
            if (onCooldown)
                return (false, $"⏳ You can raid again in **{remaining.Hours}h {remaining.Minutes}m**.", null);

            var existing = await _raidRepo.GetPlayerActiveSessionAsync(discordId);
            if (existing != null)
                return (false, "❌ You are already in a raid lobby or active raid.", null);

            var session = new RaidSession
            {
                Tier = tier,
                Status = RaidStatus.Lobby,
                LeaderDiscordId = discordId,
                LobbyChannelId = channelId,
                LobbyMessageId = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _raidRepo.CreateSessionAsync(session);

            var participant = new RaidParticipant
            {
                RaidSessionId = session.Id,
                DiscordId = discordId,
                Role = role,
                CurrentHp = 0,
                MaxHp = 0
            };

            await _raidRepo.AddParticipantAsync(participant);

            return (true, $"✅ Lobby created!", session);
        }

        // =========================
        // 🚪 JOIN LOBBY
        // =========================
        public async Task<(bool success, string message)> JoinLobbyAsync(
            ulong discordId, string username, int sessionId, RaidRole role)
        {
            var session = await _raidRepo.GetSessionAsync(sessionId);
            if (session == null)
                return (false, "❌ Raid lobby not found.");

            if (session.Status != RaidStatus.Lobby)
                return (false, "❌ This raid has already started.");

            var raidDef = RaidRegistry.GetByTier(session.Tier);
            var player = await _playerRepo.GetByDiscordIdAsync(discordId);

            if (player == null)
                return (false, "❌ You need to start your adventure first.");

            if (player.Level < raidDef.RequiredLevel)
                return (false, $"❌ You need to be Level {raidDef.RequiredLevel} to enter this raid.");

            var (onCooldown, remaining) = await CheckCooldownAsync(discordId);
            if (onCooldown)
                return (false, $"⏳ You can raid again in **{remaining.Hours}h {remaining.Minutes}m**.");

            var existing = await _raidRepo.GetPlayerActiveSessionAsync(discordId);
            if (existing != null)
                return (false, "❌ You are already in a raid lobby or active raid.");

            if (session.Participants.Any(p => p.Role == role))
                return (false, $"❌ The {role} role is already taken.");

            if (session.Participants.Count >= 3)
                return (false, "❌ This lobby is already full.");

            var participant = new RaidParticipant
            {
                RaidSessionId = session.Id,
                DiscordId = discordId,
                Role = role,
                CurrentHp = 0,
                MaxHp = 0
            };

            await _raidRepo.AddParticipantAsync(participant);
            return (true, $"✅ You joined as **{role}**!");
        }

        // =========================
        // ❌ REMOVE FROM LOBBY (leader only)
        // =========================
        public async Task<(bool success, string message)> RemoveFromLobbyAsync(
            ulong leaderId, int sessionId, ulong targetDiscordId)
        {
            var session = await _raidRepo.GetSessionAsync(sessionId);
            if (session == null)
                return (false, "❌ Lobby not found.");

            if (session.LeaderDiscordId != leaderId)
                return (false, "❌ Only the raid leader can remove players.");

            if (targetDiscordId == leaderId)
                return (false, "❌ You cannot remove yourself. Use /raid-leave instead.");

            var participant = session.Participants.FirstOrDefault(p => p.DiscordId == targetDiscordId);
            if (participant == null)
                return (false, "❌ That player is not in this lobby.");

            await _raidRepo.RemoveParticipantAsync(participant.Id);
            return (true, $"✅ Player removed from lobby.");
        }

        // =========================
        // 🔑 CHECK KEY
        // =========================
        public async Task<bool> HasKeyAsync(ulong discordId, int tier)
        {
            var keyItemId = $"raid_key_t{tier}";
            var inventory = await _inventoryRepo.GetInventoryAsync(discordId);
            var key = inventory.FirstOrDefault(i => i.ItemId == keyItemId);
            return key != null && key.Quantity > 0;
        }

        // =========================
        // ▶️ START RAID
        // =========================
        public async Task<(bool success, string message)> StartRaidAsync(
            ulong leaderId, int sessionId, ITextChannel raidChannel)
        {
            var session = await _raidRepo.GetSessionAsync(sessionId);
            if (session == null)
                return (false, "❌ Lobby not found.");

            if (session.LeaderDiscordId != leaderId)
                return (false, "❌ Only the raid leader can start the raid.");

            if (session.Participants.Count < 3)
                return (false, "❌ You need 3 players to start a raid.");

            // Check all players have keys
            foreach (var p in session.Participants)
            {
                if (!await HasKeyAsync(p.DiscordId, session.Tier))
                    return (false, $"❌ <@{p.DiscordId}> doesn't have a Tier {session.Tier} Raid Key.");
            }

            var raidDef = RaidRegistry.GetByTier(session.Tier);

            // Consume keys from all players
            foreach (var p in session.Participants)
            {
                var keyItemId = $"raid_key_t{session.Tier}";
                await _inventoryRepo.RemoveItemAsync(p.DiscordId, keyItemId, 1);
            }

            // Scale boss stats off average party stats
            int totalAtk = 0, totalDef = 0, totalHp = 0;

            foreach (var p in session.Participants)
            {
                var player = await _playerRepo.GetByDiscordIdAsync(p.DiscordId);
                var (atk, def, hp) = await _statService.CalculateStatsAsync(player);
                totalAtk += atk;
                totalDef += def;
                totalHp += hp;

                // Set participant HP from their actual max HP
                p.MaxHp = hp;
                p.CurrentHp = hp;
            }

            int avgAtk = totalAtk / 3;
            int avgDef = totalDef / 3;

            session.BossMaxHp = (int)(avgAtk * raidDef.HpMultiplier);
            session.BossCurrentHp = session.BossMaxHp;
            session.BossAttack = (int)(avgDef * raidDef.AttackMultiplier);
            session.BossDefense = (int)(avgAtk * raidDef.DefenseMultiplier);

            // Set initial aggro to tank
            var tank = session.Participants.FirstOrDefault(p => p.Role == RaidRole.Tank);
            if (tank != null)
                session.AggroDiscordId = tank.DiscordId;

            session.Status = RaidStatus.Active;
            session.CurrentRound = 1;
            session.RoundStartedAt = DateTime.UtcNow;

            // Create thread
            var thread = await raidChannel.CreateThreadAsync(
                name: $"⚔️ {raidDef.Name} — Tier {session.Tier} Raid",
                autoArchiveDuration: ThreadArchiveDuration.OneDay,
                type: ThreadType.PublicThread
            );

            session.ThreadId = thread.Id;

            await _raidRepo.SaveSessionAsync(session);

            return (true, thread.Id.ToString());
        }

        // =========================
        // ⚔️ SUBMIT ACTION
        // =========================
        public async Task<(bool success, string message, RaidRoundResult? roundResult)> SubmitActionAsync(
            ulong discordId, int sessionId, string action)
        {
            // Acquire DB-level lock so simultaneous submissions don't race
            await _raidRepo.AcquireSessionLockAsync(sessionId);

            var session = await _raidRepo.GetSessionAsync(sessionId);
            if (session == null)
                return (false, "❌ Raid not found.", null);

            if (session.Status != RaidStatus.Active)
                return (false, "❌ This raid is not active.", null);

            var participant = session.Participants.FirstOrDefault(p => p.DiscordId == discordId);
            if (participant == null)
                return (false, "❌ You are not in this raid.", null);

            // Validate action for role
            if (!IsValidAction(participant.Role, action))
                return (false, "❌ Invalid action for your role.", null);

            participant.HasActedThisRound = true;
            participant.PendingAction = action;

            await _raidRepo.SaveSessionAsync(session);

            // Check if all participants have acted
            if (session.Participants.All(p => p.HasActedThisRound))
            {
                var result = await ResolveRoundAsync(session);
                return (true, "✅ Round resolved!", result);
            }

            return (true, "✅ Action submitted! Waiting for other players.", null);
        }

        // =========================
        // 🎲 RESOLVE ROUND
        // =========================
        private async Task<RaidRoundResult> ResolveRoundAsync(RaidSession session)
        {
            var raidDef = RaidRegistry.GetByTier(session.Tier);
            var result = new RaidRoundResult { Round = session.CurrentRound };

            var tank = session.Participants.First(p => p.Role == RaidRole.Tank);
            var dps = session.Participants.First(p => p.Role == RaidRole.Dps);
            var healer = session.Participants.First(p => p.Role == RaidRole.Healer);

            // =========================
            // PROCESS TANK ACTION
            // =========================
            if (tank.PendingAction == "taunt")
            {
                session.AggroDiscordId = tank.DiscordId;
                result.TankText = "📣 Tank taunted the boss — aggro restored!";
                tank.ShatterCooldownRoundsRemaining = Math.Max(0, tank.ShatterCooldownRoundsRemaining - 1);
            }
            else if (tank.PendingAction == "shatter" && tank.ShatterCooldownRoundsRemaining <= 0)
            {
                var relicBonuses = await _relicService.GetRelicBonusesAsync(tank.DiscordId);
                int shatterRounds = 2 + relicBonuses.ShatterExtraRounds;

                session.ActiveEffects.Add(new ActiveRaidEffect
                {
                    EffectType = ActiveEffectType.Shatter,
                    RoundsRemaining = shatterRounds,
                    Value = 0.20
                });

                tank.ShatterCooldownRoundsRemaining = 3;
                result.TankText = $"💥 Tank used Shatter! Boss defense reduced by 20% for {shatterRounds} rounds.";
            }
            else if (tank.PendingAction == "hold" || tank.PendingAction == "shatter")
            {
                result.TankText = "🛡️ Tank held the line.";
                tank.ShatterCooldownRoundsRemaining = Math.Max(0, tank.ShatterCooldownRoundsRemaining - 1);
            }
            else
            {
                result.TankText = "🛡️ Tank held their ground.";
                tank.ShatterCooldownRoundsRemaining = Math.Max(0, tank.ShatterCooldownRoundsRemaining - 1);
            }

            // =========================
            // PROCESS DPS ACTION
            // =========================
            if (dps.PendingAction == "attack")
            {
                var dpsPlayer = await _playerRepo.GetByDiscordIdAsync(dps.DiscordId);
                var (dpsAtk, _, _) = await _statService.CalculateStatsAsync(dpsPlayer);
                var relicBonuses = await _relicService.GetRelicBonusesAsync(dps.DiscordId);

                // Apply empower buff if active
                var empowerEffect = session.ActiveEffects.FirstOrDefault(e =>
                    e.EffectType == ActiveEffectType.EmpowerAttack &&
                    e.TargetDiscordId == dps.DiscordId);

                if (empowerEffect != null)
                    dpsAtk = (int)(dpsAtk * (1f + (float)empowerEffect.Value));

                // Apply shatter debuff
                float bossDefenseMultiplier = 1f;
                if (session.ActiveEffects.Any(e => e.EffectType == ActiveEffectType.Shatter))
                    bossDefenseMultiplier = 0.80f;

                int effectiveBossDefense = (int)(session.BossDefense * bossDefenseMultiplier);
                int damage = (int)(dpsAtk * (100.0 / (100.0 + effectiveBossDefense)));
                damage = Math.Max(1, damage);

                // =========================
                // 🐾 PET PASSIVES (DPS)
                // =========================
                var dpsPet = await _petService.GetEquippedPetAsync(dps.DiscordId);
                PetDefinition dpsPetDef = null;
                if (dpsPet != null)
                    PetRegistry.All.TryGetValue(dpsPet.PetId, out dpsPetDef);

                damage = _petPassiveService.ModifyOutgoingDamage(
                    damage, dpsPet, dpsPetDef, session.BossCurrentHp, session.BossMaxHp);

                // Executioner bonus
                double bossHpPercent = (double)session.BossCurrentHp / session.BossMaxHp;
                if (bossHpPercent < 0.50 && relicBonuses.ExecutionerBonusPercent > 0)
                    damage = (int)(damage * (1f + relicBonuses.ExecutionerBonusPercent));

                session.BossCurrentHp = Math.Max(0, session.BossCurrentHp - damage);

                // Life steal (relic + pet passive combined)
                int totalHeal = 0;
                if (relicBonuses.LifeStealPercent > 0)
                    totalHeal += (int)(damage * relicBonuses.LifeStealPercent);

                totalHeal += _petPassiveService.ApplyOnHitEffects(damage, null, dpsPet);

                if (totalHeal > 0)
                {
                    dps.CurrentHp = Math.Min(dps.MaxHp, dps.CurrentHp + totalHeal);
                    result.DpsText = $"⚔️ DPS dealt **{damage}** damage! 🩸 Life steal healed for **{totalHeal}**.";
                }
                else
                {
                    result.DpsText = $"⚔️ DPS dealt **{damage}** damage!";
                }
            }

            // =========================
            // PROCESS HEALER ACTION
            // =========================
            if (healer.PendingAction == "heal")
            {
                var healerInventory = await _inventoryRepo.GetInventoryAsync(healer.DiscordId);
                var potion = healerInventory.FirstOrDefault(i => i.ItemId == "health_potion");

                if (potion != null && potion.Quantity > 0)
                {
                    var relicBonuses = await _relicService.GetRelicBonusesAsync(healer.DiscordId);

                    // Find lowest HP participant
                    var target = session.Participants
                        .OrderBy(p => (double)p.CurrentHp / p.MaxHp)
                        .First();

                    float healPercent = 0.25f + relicBonuses.IncreasedHealPercent;
                    int healAmount = (int)(target.MaxHp * healPercent);
                    target.CurrentHp = Math.Min(target.MaxHp, target.CurrentHp + healAmount);

                    // Chance to save potion
                    bool savedPotion = _random.NextDouble() < relicBonuses.ChanceToSavePotion;
                    if (!savedPotion)
                        await _inventoryRepo.RemoveItemAsync(healer.DiscordId, "health_potion", 1);

                    var targetRole = target.Role.ToString();
                    result.HealerText = savedPotion
                        ? $"💚 Healer restored **{healAmount} HP** to the {targetRole}! ✨ Potion saved!"
                        : $"💚 Healer restored **{healAmount} HP** to the {targetRole}.";
                }
                else
                {
                    result.HealerText = "❌ Healer has no potions left!";
                }
            }
            else if (healer.PendingAction == "empower_attack")
            {
                var relicBonuses = await _relicService.GetRelicBonusesAsync(healer.DiscordId);
                int empowerRounds = 1 + relicBonuses.EmpowerExtraRounds;

                session.ActiveEffects.RemoveAll(e =>
                    e.EffectType == ActiveEffectType.EmpowerAttack &&
                    e.TargetDiscordId == dps.DiscordId);

                session.ActiveEffects.Add(new ActiveRaidEffect
                {
                    EffectType = ActiveEffectType.EmpowerAttack,
                    TargetDiscordId = dps.DiscordId,
                    RoundsRemaining = empowerRounds + 1,
                    Value = 0.15
                });

                result.HealerText = $"✨ Healer empowered the DPS! +15% attack for {empowerRounds} round(s).";
            }
            else if (healer.PendingAction == "empower_defense")
            {
                var relicBonuses = await _relicService.GetRelicBonusesAsync(healer.DiscordId);
                int empowerRounds = 1 + relicBonuses.EmpowerExtraRounds;

                session.ActiveEffects.RemoveAll(e =>
                    e.EffectType == ActiveEffectType.EmpowerDefense &&
                    e.TargetDiscordId == tank.DiscordId);

                session.ActiveEffects.Add(new ActiveRaidEffect
                {
                    EffectType = ActiveEffectType.EmpowerDefense,
                    TargetDiscordId = tank.DiscordId,
                    RoundsRemaining = empowerRounds + 1,
                    Value = 0.15
                });

                result.HealerText = $"✨ Healer empowered the Tank! +15% defense for {empowerRounds} round(s).";
            }
            else
            {
                result.HealerText = "💤 Healer did not act this round.";
            }

            // =========================
            // CHECK VICTORY
            // =========================
            if (session.BossCurrentHp <= 0)
            {
                result.IsVictory = true;
                await HandleVictoryAsync(session, result);
                return result;
            }

            // =========================
            // TICK DOWN ACTIVE EFFECTS
            // =========================
            foreach (var effect in session.ActiveEffects)
                effect.RoundsRemaining--;

            session.ActiveEffects.RemoveAll(e => e.RoundsRemaining <= 0);

            // =========================
            // BOSS ACTIONS
            // =========================

            // Roll for aggro swap (minimum 3 round cooldown between swaps)
            if (_random.NextDouble() < raidDef.AggroSwapChance &&
                session.CurrentRound - session.LastAggroSwapRound >= 2)
            {
                var nonTank = session.Participants
                    .Where(p => p.Role != RaidRole.Tank)
                    .OrderBy(_ => _random.Next())
                    .First();

                session.AggroDiscordId = nonTank.DiscordId;
                session.LastAggroSwapRound = session.CurrentRound;
                result.BossText += $"🔄 **Boss swapped target to {nonTank.Role}!** Tank must Taunt!\n";
            }

            // Roll boss ability
            if (raidDef.AbilityPool.Count > 0 && _random.Next(0, 100) < 30)
            {
                var ability = raidDef.AbilityPool[_random.Next(raidDef.AbilityPool.Count)];
                result.BossText += HandleBossAbility(session, ability, raidDef);
            }

            // Boss attack
            var aggroTarget = session.Participants.First(p => p.DiscordId == session.AggroDiscordId);
            bool aggroOnTank = aggroTarget.Role == RaidRole.Tank;

            int bossAttack = session.BossAttack;

            // Apply empower defense if tank has it
            var tankEmpower = session.ActiveEffects.FirstOrDefault(e =>
                e.EffectType == ActiveEffectType.EmpowerDefense &&
                e.TargetDiscordId == tank.DiscordId);

            // Apply hold the line reduction
            bool tankHolding = tank.PendingAction == "hold";
            float damageReduction = tankHolding ? 0.35f : 0f;
            if (tankEmpower != null) damageReduction += (float)tankEmpower.Value;

            if (aggroOnTank)
            {
                var (_, tankDef, _) = await _statService.CalculateStatsAsync(
                    await _playerRepo.GetByDiscordIdAsync(tank.DiscordId));

                int rawDamage = (int)(bossAttack * (100.0 / (100.0 + tankDef)));
                rawDamage = Math.Max(1, rawDamage);
                int finalDamage = (int)(rawDamage * (1f - damageReduction));
                finalDamage = Math.Max(1, finalDamage);

                // =========================
                // 🐾 PET PASSIVES (TANK)
                // =========================
                var tankPet = await _petService.GetEquippedPetAsync(tank.DiscordId);
                finalDamage = _petPassiveService.ModifyIncomingDamage(finalDamage, tankPet);
                finalDamage = Math.Max(1, finalDamage);

                tank.CurrentHp = Math.Max(0, tank.CurrentHp - finalDamage);
                result.BossText += $"🗡️ Boss attacks **Tank** for **{finalDamage}** damage.";

                int reflect = _petPassiveService.ApplyOnHitTaken(finalDamage, tankPet);
                if (reflect > 0)
                {
                    session.BossCurrentHp = Math.Max(0, session.BossCurrentHp - reflect);
                    result.BossText += $"\n🌵 **Thorns reflects {reflect} damage** back at the boss!";
                }
            }
            else
            {
                // Boss hits non-tank at 4x damage
                var (_, targetDef, _) = await _statService.CalculateStatsAsync(
                    await _playerRepo.GetByDiscordIdAsync(aggroTarget.DiscordId));

                int rawDamage = (int)(bossAttack * (100.0 / (100.0 + targetDef)));
                rawDamage = Math.Max(1, rawDamage);
                int finalDamage = rawDamage * 4;

                aggroTarget.CurrentHp = Math.Max(0, aggroTarget.CurrentHp - finalDamage);
                result.BossText += $"⚠️ Boss hits **{aggroTarget.Role}** for **{finalDamage}** damage! *(4x — no aggro!)*";
            }

            // Tick Venom DoT
            var venom = session.ActiveEffects.FirstOrDefault(e => e.EffectType == ActiveEffectType.Venom);
            if (venom != null)
            {
                foreach (var p in session.Participants)
                {
                    int venomDamage = (int)venom.Value;
                    p.CurrentHp = Math.Max(0, p.CurrentHp - venomDamage);
                }
                result.BossText += $"\n☠️ Venom deals **{(int)venom.Value}** damage to all party members!";
            }

            // =========================
            // RESET ROUND ACTIONS
            // =========================
            foreach (var p in session.Participants)
            {
                p.HasActedThisRound = false;
                p.PendingAction = null;
            }

            // =========================
            // CHECK WIPE
            // =========================
            var dead = session.Participants.FirstOrDefault(p => p.CurrentHp <= 0);
            if (dead != null)
            {
                result.IsWipe = true;
                result.WipeReason = $"💀 **{dead.Role}** has fallen!";
                await HandleWipeAsync(session, result);
                return result;
            }

            session.RoundStatusMessageId = 0;
            session.RoundStartedAt = DateTime.UtcNow;
            session.CurrentRound++;
            result.Session = session;

            await _raidRepo.SaveSessionAsync(session);
            return result;
        }

        // =========================
        // 🔥 BOSS ABILITY HANDLER
        // =========================
        private string HandleBossAbility(RaidSession session, BossAbilityType ability, RaidDefinition raidDef)
        {
            switch (ability)
            {
                case BossAbilityType.SavageCleave:
                    int cleaveDamage = (int)(session.BossAttack * 0.5);
                    foreach (var p in session.Participants)
                        p.CurrentHp = Math.Max(0, p.CurrentHp - cleaveDamage);
                    return $"\n🪓 **Savage Cleave!** All party members take **{cleaveDamage}** damage!";

                case BossAbilityType.CrushingBlow:
                    var tank = session.Participants.First(p => p.Role == RaidRole.Tank);
                    int crushDamage = session.BossAttack * 2;
                    tank.CurrentHp = Math.Max(0, tank.CurrentHp - crushDamage);
                    return $"\n💢 **Crushing Blow!** Tank takes **{crushDamage}** damage!";

                case BossAbilityType.Enrage:
                    if (!session.ActiveEffects.Any(e => e.EffectType == ActiveEffectType.Enrage))
                    {
                        session.BossAttack = (int)(session.BossAttack * 1.3);
                        session.ActiveEffects.Add(new ActiveRaidEffect
                        {
                            EffectType = ActiveEffectType.Enrage,
                            RoundsRemaining = 99,
                            Value = 0
                        });
                        return "\n😡 **Boss Enraged!** Attack permanently increased!";
                    }
                    return "";

                case BossAbilityType.Frenzy:
                    int frenzyDamage = (int)(session.BossAttack * 0.75);
                    var target = session.Participants.First(p => p.DiscordId == session.AggroDiscordId);
                    target.CurrentHp = Math.Max(0, target.CurrentHp - frenzyDamage);
                    return $"\n💨 **Frenzy!** Boss strikes again for **{frenzyDamage}** damage!";

                case BossAbilityType.Venom:
                    if (!session.ActiveEffects.Any(e => e.EffectType == ActiveEffectType.Venom))
                    {
                        int venomDamage = Math.Max(1, session.BossAttack / 10);
                        session.ActiveEffects.Add(new ActiveRaidEffect
                        {
                            EffectType = ActiveEffectType.Venom,
                            RoundsRemaining = 3,
                            Value = venomDamage
                        });
                        return $"\n🐍 **Venom!** Party poisoned for **{venomDamage}** damage per round for 3 rounds!";
                    }
                    return "";

                case BossAbilityType.Execute:
                    var weakest = session.Participants
                        .Where(p => (double)p.CurrentHp / p.MaxHp < 0.20)
                        .OrderBy(p => p.CurrentHp)
                        .FirstOrDefault();
                    if (weakest != null)
                    {
                        int executeDamage = session.BossAttack * 3;
                        weakest.CurrentHp = Math.Max(0, weakest.CurrentHp - executeDamage);
                        return $"\n💀 **Execute!** Boss targets the weakened **{weakest.Role}** for **{executeDamage}** damage!";
                    }
                    return "";

                default:
                    return "";
            }
        }

        // =========================
        // 🏆 VICTORY
        // =========================
        private async Task HandleVictoryAsync(RaidSession session, RaidRoundResult result)
        {
            session.Status = RaidStatus.Victory;

            var raidDef = RaidRegistry.GetByTier(session.Tier);

            foreach (var p in session.Participants)
            {
                var player = await _playerRepo.GetByDiscordIdAsync(p.DiscordId);
                if (player == null) continue;

                var relicBonuses = await _relicService.GetRelicBonusesAsync(p.DiscordId);

                int gold = (int)(RaidGoldReward * (1f + relicBonuses.BonusGoldPercent));
                int xp = (int)(RaidPlayerXpReward * (1f + relicBonuses.BonusPlayerXpPercent));
                int petXp = (int)(RaidPetXpReward * (1f + relicBonuses.BonusPetXpPercent));

                player.Gold += gold;
                player.XP += xp;
                player.LastRaidAt = DateTimeOffset.UtcNow.ToString("o");

                var (levelMessage, _) = _levelService.CheckLevelUp(player);
                await _playerRepo.UpdatePlayerAsync(player);

                await _petService.AddXPAsync(p.DiscordId, petXp);

                // Relic shard drop (scaled by loot roll relic bonus)
                float shardDropChance = RelicShardDropChance + relicBonuses.BonusLootRollPercent;
                bool shardDropped = _random.NextDouble() < shardDropChance;
                if (shardDropped)
                    await _relicService.GiveShardAsync(p.DiscordId, session.Tier);

                result.Rewards.Add(new RaidReward
                {
                    DiscordId = p.DiscordId,
                    Role = p.Role,
                    Gold = gold,
                    PlayerXp = xp,
                    PetXp = petXp,
                    ShardDropped = shardDropped,
                    ShardTier = session.Tier,
                    LevelUpMessage = levelMessage
                });
            }

            await _raidRepo.SaveSessionAsync(session);
        }

        // =========================
        // 💀 WIPE
        // =========================
        private async Task HandleWipeAsync(RaidSession session, RaidRoundResult result)
        {
            session.Status = RaidStatus.Wiped;

            foreach (var p in session.Participants)
            {
                var player = await _playerRepo.GetByDiscordIdAsync(p.DiscordId);
                if (player == null) continue;

                player.Gold = Math.Max(0, player.Gold - WipeGoldPenalty);
                player.LastRaidAt = DateTimeOffset.UtcNow.ToString("o");

                var (_, _, maxHp) = await _statService.CalculateStatsAsync(player);
                player.Health = maxHp;

                await _playerRepo.UpdatePlayerAsync(player);
            }

            await _raidRepo.SaveSessionAsync(session);
        }

        public async Task UpdateRoundStatusMessageIdAsync(int sessionId, ulong messageId)
        {
            var session = await _raidRepo.GetSessionAsync(sessionId);
            if (session == null) return;
            session.RoundStatusMessageId = messageId;
            await _raidRepo.SaveSessionAsync(session);
        }

        // =========================
        // ✅ VALIDATE ACTION
        // =========================
        private bool IsValidAction(RaidRole role, string action)
        {
            return role switch
            {
                RaidRole.Dps => action == "attack",
                RaidRole.Tank => action is "hold" or "taunt" or "shatter",
                RaidRole.Healer => action is "heal" or "empower_attack" or "empower_defense",
                _ => false
            };
        }

        // =========================
        // 🔍 GET SESSION
        // =========================
        public async Task<RaidSession?> GetSessionAsync(int sessionId)
        {
            return await _raidRepo.GetSessionAsync(sessionId);
        }

        public async Task<RaidSession?> GetPlayerActiveSessionAsync(ulong discordId)
        {
            return await _raidRepo.GetPlayerActiveSessionAsync(discordId);
        }

        public async Task<RaidSession?> GetActiveByThreadAsync(ulong threadId)
        {
            return await _raidRepo.GetActiveByThreadAsync(threadId);
        }

        public async Task UpdateLobbyMessageIdAsync(int sessionId, ulong messageId)
        {
            var session = await _raidRepo.GetSessionAsync(sessionId);
            if (session == null) return;
            session.LobbyMessageId = messageId;
            await _raidRepo.SaveSessionAsync(session);
        }

        public async Task RemoveParticipantDirectAsync(int participantId)
        {
            await _raidRepo.RemoveParticipantAsync(participantId);
        }

        public async Task DeleteSessionDirectAsync(int sessionId)
        {
            await _raidRepo.DeleteSessionAsync(sessionId);
        }
    }
}
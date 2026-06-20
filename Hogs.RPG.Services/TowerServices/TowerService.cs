using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities.TowerObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Tower;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AchievementServices;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.PlayerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hogs.RPG.Services.TowerServices
{
    public class TowerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;
        private readonly Random _random = new();

        private readonly Dictionary<string, TowerSession> _sessions = new();
        private readonly Dictionary<ulong, string> _playerSession = new();
        private readonly List<ulong> _completedThreadIds = new();
        private DateTime _lastCleanupDate = DateTime.MinValue;

        private const int FloorIntervalSeconds = 10;
        private const int GoldPerFloor = 15;
        private const int FlatXp = 2500;
        private const int FlatPetXp = 250;

        public TowerService(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;
        }

        // =========================
        // BACKGROUND LOOP
        // =========================
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🗼 TowerService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(2000, stoppingToken);

                var due = _sessions.Values
                    .Where(s => s.Status == TowerStatus.Running && s.NextFloorAt.HasValue && s.NextFloorAt <= DateTime.UtcNow)
                    .ToList();

                foreach (var session in due)
                {
                    try { await ResolveNextFloorAsync(session); }
                    catch (Exception ex) { Console.WriteLine($"❌ Tower floor error [{session.SessionId}]: {ex.Message}"); }
                }

                // Expire stale lobbies after 10 minutes
                var stale = _sessions.Values
                    .Where(s => s.Status == TowerStatus.Lobby && (DateTime.UtcNow - s.CreatedAt).TotalMinutes > 10)
                    .ToList();

                foreach (var s in stale)
                    RemoveSession(s.SessionId);

                // Daily thread cleanup at 03:00 UTC
                try
                {
                    var now = DateTime.UtcNow;
                    if (now.Hour == 3 && _lastCleanupDate.Date != now.Date)
                    {
                        _lastCleanupDate = now;
                        await CleanupCompletedThreadsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Tower thread cleanup error: {ex.Message}");
                }
            }
        }

        // =========================
        // 🧹 DAILY THREAD CLEANUP
        // =========================
        private async Task CleanupCompletedThreadsAsync()
        {
            Console.WriteLine("🧹 Running daily Tower thread cleanup...");

            List<ulong> toClean;
            lock (_completedThreadIds)
            {
                toClean = _completedThreadIds.ToList();
                _completedThreadIds.Clear();
            }

            int deleted = 0;
            foreach (var threadId in toClean)
            {
                try
                {
                    var thread = _client.GetChannel(threadId) as IThreadChannel;
                    if (thread != null)
                    {
                        await thread.DeleteAsync();
                        deleted++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not delete tower thread {threadId}: {ex.Message}");
                }
            }

            Console.WriteLine($"🧹 Tower cleanup done: {deleted} thread(s) deleted.");
        }

        // =========================
        // LOBBY MANAGEMENT
        // =========================
        public TowerSession? CreateLobby(ulong userId, string username, TowerMode mode, ulong channelId)
        {
            if (_playerSession.ContainsKey(userId)) return null;

            var session = new TowerSession
            {
                Mode = mode,
                ChannelId = channelId
            };

            session.Participants.Add(new TowerParticipant
            {
                DiscordId = userId,
                Username = username
            });

            _sessions[session.SessionId] = session;
            _playerSession[userId] = session.SessionId;
            return session;
        }

        public (bool success, string error) TryJoin(string sessionId, ulong userId, string username)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Lobby not found.");
            if (session.Status != TowerStatus.Lobby)
                return (false, "Run has already started.");
            if (session.Mode == TowerMode.Solo)
                return (false, "This is a solo run.");
            if (session.Participants.Count >= 2)
                return (false, "Lobby is full.");
            if (_playerSession.ContainsKey(userId))
                return (false, "You are already in a Tower run.");

            session.Participants.Add(new TowerParticipant
            {
                DiscordId = userId,
                Username = username
            });

            _playerSession[userId] = sessionId;
            return (true, "");
        }

        public bool ToggleReady(string sessionId, ulong userId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session)) return false;
            var p = session.Participants.FirstOrDefault(x => x.DiscordId == userId);
            if (p == null) return false;
            p.ReadyToStart = !p.ReadyToStart;
            return true;
        }

        public bool AllReady(string sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session)) return false;
            if (session.Mode == TowerMode.Duo && session.Participants.Count < 2) return false;
            return session.Participants.All(p => p.ReadyToStart);
        }

        // =========================
        // START RUN
        // =========================
        public async Task<(bool success, string error)> StartRunAsync(string sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found.");

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();

            // Check daily cooldowns
            foreach (var p in session.Participants)
            {
                var player = await playerRepo.GetByDiscordIdAsync(p.DiscordId);
                if (player == null) return (false, $"Player <@{p.DiscordId}> not found.");

                var lastRun = session.Mode == TowerMode.Solo ? player.LastSoloTowerRun : player.LastDuoTowerRun;
                if (lastRun.HasValue && lastRun.Value.Date == DateTime.UtcNow.Date)
                    return (false, $"<@{p.DiscordId}> has already done their {session.Mode} Tower run today.");

                var (atk, def, maxHp) = await statService.CalculateStatsAsync(player);
                p.BaseAttack = atk;
                p.BaseDefense = def;
                p.MaxHp = maxHp;
                p.CurrentHp = maxHp;
            }

            // Create thread
            var channel = _client.GetChannel(session.ChannelId) as ITextChannel;
            if (channel == null) return (false, "Tower channel not found.");

            string threadName = session.Mode == TowerMode.Duo
                ? $"🗼 Duo Tower — {string.Join(" & ", session.Participants.Select(p => p.Username))}"
                : $"🗼 Solo Tower — {session.Participants[0].Username}";

            var thread = await channel.CreateThreadAsync(
                name: threadName,
                autoArchiveDuration: ThreadArchiveDuration.OneDay,
                type: ThreadType.PublicThread
            );

            session.ThreadId = thread.Id;
            session.Status = TowerStatus.Running;
            session.NextFloorAt = DateTime.UtcNow.AddSeconds(3);

            // Mark daily cooldowns
            foreach (var p in session.Participants)
            {
                var player = await playerRepo.GetByDiscordIdAsync(p.DiscordId);
                if (player == null) continue;

                if (session.Mode == TowerMode.Solo) player.LastSoloTowerRun = DateTime.UtcNow;
                else player.LastDuoTowerRun = DateTime.UtcNow;

                await playerRepo.UpdatePlayerAsync(player);
            }

            await thread.SendMessageAsync(embed: BuildStartEmbed(session));

            return (true, "");
        }

        // =========================
        // FLOOR RESOLUTION
        // =========================
        private async Task ResolveNextFloorAsync(TowerSession session)
        {
            session.NextFloorAt = null;
            session.Floor++;

            // Tick debuff durations and clear expired
            foreach (var p in session.Participants)
            {
                p.TookDamageThisFloor = false;

                foreach (var debuff in p.Debuffs.Where(d => d.FloorsRemaining > 0))
                    debuff.FloorsRemaining--;

                p.Debuffs.RemoveAll(d => d.FloorsRemaining == 0);

                foreach (var buff in p.Buffs)
                    if (buff.DisabledForFloors > 0) buff.DisabledForFloors--;
            }

            bool isBoss = session.Floor % 25 == 0;
            bool isElite = !isBoss && session.Floor % 5 == 0;
            TowerFloorEventType eventType;

            if (isBoss)
                eventType = TowerFloorEventType.Boss;
            else if (isElite)
                eventType = TowerFloorEventType.Elite;
            else
                eventType = RollEventType();

            TowerBossDefinition? bossDef = null;
            string combatLog;

            if (eventType == TowerFloorEventType.Boss)
            {
                bossDef = TowerBossRegistry.GetForFloor(session.Floor);
                combatLog = await ResolveBossAsync(session, bossDef);
            }
            else
            {
                combatLog = eventType switch
                {
                    TowerFloorEventType.Combat       => ResolveCombat(session, false, false),
                    TowerFloorEventType.Elite        => ResolveCombat(session, true, false),
                    TowerFloorEventType.TreasureRoom => ResolveTreasureRoom(session),
                    TowerFloorEventType.CursedFloor  => ResolveCursedFloor(session),
                    TowerFloorEventType.RestSite     => ResolveRestSite(session),
                    _                                => ResolveCombat(session, false, false)
                };
            }

            // Accumulate gold
            foreach (var p in session.Participants)
                p.AccumulatedGold += GoldPerFloor;

            var thread = _client.GetChannel(session.ThreadId) as IThreadChannel;
            if (thread == null) return;

            bool anyDead = session.Participants.Any(p => p.CurrentHp <= 0);

            // Duo: if one partner dies, pass their buffs/debuffs to the survivor and let them continue
            if (anyDead && session.Mode == TowerMode.Duo)
            {
                var dead = session.Participants.Where(p => p.CurrentHp <= 0).ToList();
                var alive = session.Participants.FirstOrDefault(p => p.CurrentHp > 0);

                if (alive != null && dead.Count < session.Participants.Count)
                {
                    await thread.SendMessageAsync(embed: BuildFloorEmbed(session, combatLog, eventType, bossDef));

                    foreach (var fallen in dead)
                    {
                        // Merge buffs
                        foreach (var buff in fallen.Buffs)
                        {
                            var existing = alive.Buffs.FirstOrDefault(b => b.Type == buff.Type);
                            if (existing != null) existing.Stacks = Math.Min(existing.Stacks + buff.Stacks, 5);
                            else alive.Buffs.Add(new TowerBuff { Type = buff.Type, Stacks = buff.Stacks });
                        }
                        // Merge debuffs (avoid duplicates of same type)
                        foreach (var debuff in fallen.Debuffs)
                        {
                            if (!alive.Debuffs.Any(d => d.Type == debuff.Type))
                                alive.Debuffs.Add(new TowerDebuff { Type = debuff.Type, FloorsRemaining = debuff.FloorsRemaining });
                        }
                    }

                    string fallenNames = string.Join(" & ", dead.Select(p => $"**{p.Username}**"));
                    await thread.SendMessageAsync(embed: new EmbedBuilder()
                        .WithTitle("💀 A partner has fallen!")
                        .WithDescription($"{fallenNames} has died — their buffs and debuffs have been passed to **{alive.Username}**.\n\nThe climb continues alone.")
                        .WithColor(Color.DarkRed)
                        .Build());

                    // Remove dead participants so the run continues solo
                    session.Participants.RemoveAll(p => p.CurrentHp <= 0);

                    if (session.Floor % 10 == 0)
                    {
                        session.Status = TowerStatus.Checkpoint;
                        await PostCheckpointAsync(session, thread);
                    }
                    else
                    {
                        session.NextFloorAt = DateTime.UtcNow.AddSeconds(FloorIntervalSeconds);
                    }
                    return;
                }
            }

            if (anyDead)
            {
                await thread.SendMessageAsync(embed: BuildFloorEmbed(session, combatLog, eventType, bossDef));
                await EndRunAsync(session, thread);
                return;
            }

            await thread.SendMessageAsync(embed: BuildFloorEmbed(session, combatLog, eventType, bossDef));

            if (session.Floor % 10 == 0)
            {
                session.Status = TowerStatus.Checkpoint;
                await PostCheckpointAsync(session, thread);
            }
            else
            {
                session.NextFloorAt = DateTime.UtcNow.AddSeconds(FloorIntervalSeconds);
            }
        }

        private TowerFloorEventType RollEventType()
        {
            int roll = _random.Next(100);
            return roll switch
            {
                < 45 => TowerFloorEventType.Combat,
                < 60 => TowerFloorEventType.TreasureRoom,
                < 80 => TowerFloorEventType.CursedFloor,
                _    => TowerFloorEventType.RestSite
            };
        }

        private TowerDebuffType RollCombatDebuff() =>
            _random.Next(2) == 0 ? TowerDebuffType.Bleeding : TowerDebuffType.Weakened;

        // =========================
        // COMBAT
        // =========================
        private string ResolveCombat(TowerSession session, bool isElite, bool isBoss, TowerBossDefinition? bossDef = null)
        {
            int floor = session.Floor;
            bool isDuo = session.Mode == TowerMode.Duo;

            int baseEnemyHp  = isDuo ? 200 + (floor * 40) : 150 + (floor * 30);
            int baseEnemyAtk = 17 + (floor * 9);
            int baseEnemyDef = 5 + (floor * 3);

            float hpMult  = isBoss ? bossDef!.HpMultiplier  : isElite ? 1.6f : 1f;
            float atkMult = isBoss ? bossDef!.AtkMultiplier : isElite ? 1.6f : 1f;

            int enemyHp  = (int)(baseEnemyHp  * hpMult);
            int enemyAtk = (int)(baseEnemyAtk * atkMult);
            int enemyDef = baseEnemyDef;

            var log = new System.Text.StringBuilder();

            foreach (var p in session.Participants)
            {
                // Bleeding damage at start of floor
                var bleeding = p.Debuffs.FirstOrDefault(d => d.Type == TowerDebuffType.Bleeding);
                if (bleeding != null)
                {
                    int bleedDmg = Math.Max(1, (int)(p.MaxHp * Math.Min(0.25f, 0.05f * bleeding.Stacks)));
                    p.CurrentHp = Math.Max(0, p.CurrentHp - bleedDmg);
                    string stackNote = bleeding.Stacks > 1 ? $" (x{bleeding.Stacks})" : "";
                    log.AppendLine($"🩸 **{p.Username}** bleeds for **{bleedDmg}** HP!{stackNote}");
                    if (p.CurrentHp <= 0)
                    {
                        log.AppendLine($"💀 **{p.Username}** bled out!");
                        continue; // dead — skip attack and lifesteal
                    }
                }

                // Player damage
                int playerDmg = CalcPlayerDamage(p, enemyDef, isElite || isBoss);
                log.AppendLine($"⚔️ **{p.Username}** deals **{playerDmg}** damage!");
                enemyHp -= playerDmg;

                // Lifesteal
                int lifesteal = CalcLifesteal(p, playerDmg);
                if (lifesteal > 0)
                {
                    p.CurrentHp = Math.Min(p.MaxHp, p.CurrentHp + lifesteal);
                    log.AppendLine($"  🩸 Lifesteal heals **{lifesteal}** HP.");
                }
            }

            log.AppendLine();

            // Enemy attacks each player
            foreach (var p in session.Participants)
            {
                // Evasion dodge
                float dodgeChance = Math.Min(0.45f, GetBuffStacks(p, TowerBuffType.Evasion) * 0.15f);
                if (dodgeChance > 0 && _random.NextDouble() < dodgeChance)
                {
                    log.AppendLine($"💨 **{p.Username}** dodges the attack!");
                    continue;
                }

                // Armor penetration scales with floor — high defense stops being a wall at deeper floors
                float penetration = Math.Min(0.70f, floor * 0.012f);
                int effectiveDef = (int)(p.BaseDefense * (1f - penetration));
                int rawEnemyDmg = (int)(enemyAtk * (100.0 / (100.0 + effectiveDef)));
                rawEnemyDmg = Math.Max(1, rawEnemyDmg);

                // IronSkin reduction
                float ironSkinReduction = Math.Min(0.60f, GetBuffStacks(p, TowerBuffType.IronSkin) * 0.15f);
                rawEnemyDmg = (int)(rawEnemyDmg * (1f - ironSkinReduction));
                rawEnemyDmg = Math.Max(1, rawEnemyDmg);

                // Duo split — enemy spreads damage across two players
                if (isDuo) rawEnemyDmg = (int)(rawEnemyDmg * 0.65f);

                p.CurrentHp = Math.Max(0, p.CurrentHp - rawEnemyDmg);
                p.TookDamageThisFloor = true;
                log.AppendLine($"👹 Enemy hits **{p.Username}** for **{rawEnemyDmg}** damage!");

                // Thorns reflect
                float thornsPercent = GetBuffStacks(p, TowerBuffType.Thorns) * 0.15f;
                if (thornsPercent > 0)
                {
                    int reflect = (int)(rawEnemyDmg * thornsPercent);
                    log.AppendLine($"  🌵 Thorns reflects **{reflect}** damage!");
                }
            }

            // Frenzy update
            foreach (var p in session.Participants)
            {
                if (p.TookDamageThisFloor) p.FrenzyStacks = 0;
                else if (HasActiveBuff(p, TowerBuffType.Frenzy)) p.FrenzyStacks++;
            }

            return log.ToString().TrimEnd();
        }

        private async Task<string> ResolveBossAsync(TowerSession session, TowerBossDefinition bossDef)
        {
            var log = new System.Text.StringBuilder();

            log.AppendLine($"💀 **{bossDef.Name}** appears!");
            log.AppendLine($"*{bossDef.Description}*");
            log.AppendLine();
            log.AppendLine(bossDef.SpecialMechanicText);
            log.AppendLine();
            log.Append(ResolveCombat(session, false, true, bossDef));

            // Apply boss special effects by registry index (immune to renames)
            int bossIndex = ((session.Floor / 25) - 1) % TowerBossRegistry.All.Count;
            if (bossIndex == 1) // Boss 2 — Weakened
            {
                foreach (var p in session.Participants)
                    AddDebuffSafe(p, TowerDebuffType.Weakened, 3);
                log.AppendLine("\n😵 All players are **Weakened** for 3 floors!");
            }
            else if (bossIndex == 3) // Boss 4 — Bleeding
            {
                foreach (var p in session.Participants)
                    AddDebuffSafe(p, TowerDebuffType.Bleeding, -1);
                log.AppendLine("\n🩸 All players are **Bleeding** for the rest of the run!");
            }
            else if (bossIndex == 4) // Boss 5 — Strip a buff
            {
                foreach (var p in session.Participants)
                {
                    if (p.Buffs.Count > 0)
                    {
                        int idx = _random.Next(p.Buffs.Count);
                        string removed = TowerBuffPool.Get(p.Buffs[idx].Type).Name;
                        p.Buffs.RemoveAt(idx);
                        log.AppendLine($"\n💀 **{p.Username}** loses **{removed}**!");
                    }
                }
            }

            // Sigil drop check (5% chance per participant)
            if (session.Floor % 25 == 0)
            {
                using var scope = _scopeFactory.CreateScope();
                var sigilRepo = scope.ServiceProvider.GetRequiredService<SigilRepository>();
                var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();

                foreach (var p in session.Participants)
                {
                    if (_random.NextDouble() < 0.05)
                    {
                        var sigilDef = SigilRegistry.Random(_random);
                        int current = await sigilRepo.GetCountAsync(p.DiscordId, sigilDef.Id);

                        if (current >= SigilRegistry.MaxStacks)
                        {
                            var player = await playerRepo.GetByDiscordIdAsync(p.DiscordId);
                            if (player != null)
                            {
                                player.Gold += SigilRegistry.CompensationGold;
                                await playerRepo.UpdatePlayerAsync(player);
                            }
                            log.AppendLine($"\n{sigilDef.Emoji} **{p.Username}** found a **{sigilDef.Name}** but already has 3 stacks! Compensated with **{SigilRegistry.CompensationGold} gold**.");
                        }
                        else
                        {
                            await sigilRepo.IncrementAsync(p.DiscordId, sigilDef.Id);
                            log.AppendLine($"\n✨ **{p.Username}** found a **{sigilDef.Name}**! ({current + 1}/3 stacks) — {sigilDef.BonusPerStack}");
                        }
                    }
                }
            }

            return log.ToString().TrimEnd();
        }

        private string ResolveTreasureRoom(TowerSession session)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine("🎁 **Treasure Room!** The floor is empty — but something glimmers in the dark.");

            foreach (var p in session.Participants)
            {
                var buff = RollRandomBuff();
                AddBuff(p, buff);
                var def = TowerBuffPool.Get(buff);
                log.AppendLine($"✨ **{p.Username}** finds **{def.Emoji} {def.Name}**!");
            }

            return log.ToString().TrimEnd();
        }

        private string ResolveCursedFloor(TowerSession session)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine("☠️ **Cursed Floor!** A dark rune flares beneath your feet.");

            foreach (var p in session.Participants)
            {
                var debuff = RollCombatDebuff();
                var def = TowerDebuffPool.Get(debuff);
                AddDebuffSafe(p, debuff, def.DefaultDuration);
                log.AppendLine($"👁️ **{p.Username}** is afflicted with **{def.Emoji} {def.Name}**! — {def.Description}");
            }

            return log.ToString().TrimEnd();
        }

        private string ResolveRestSite(TowerSession session)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine("💚 **Rest Site!** A dim flame flickers here. You take a moment to breathe.");

            foreach (var p in session.Participants)
            {
                int healed = (int)(p.MaxHp * 0.20f);
                p.CurrentHp = Math.Min(p.MaxHp, p.CurrentHp + healed);
                log.AppendLine($"💚 **{p.Username}** recovers **{healed}** HP. ({p.CurrentHp}/{p.MaxHp})");
            }

            return log.ToString().TrimEnd();
        }

        // =========================
        // CHECKPOINT
        // =========================
        private async Task PostCheckpointAsync(TowerSession session, IThreadChannel thread)
        {
            var shackleNotices = new List<string>();

            var shackledIds = new HashSet<ulong>();
            foreach (var p in session.Participants)
            {
                p.CheckpointDone = false;
                p.PendingBuffChoices = RollThreeBuffs();

                // Auto-consume Shackled — player loses their rest option this checkpoint
                if (p.Debuffs.Any(d => d.Type == TowerDebuffType.Shackled))
                {
                    p.Debuffs.RemoveAll(d => d.Type == TowerDebuffType.Shackled);
                    shackledIds.Add(p.DiscordId);
                    shackleNotices.Add($"🔗 **{p.Username}** is Shackled — Rest is unavailable this checkpoint.");
                }
            }

            string shackleText = shackleNotices.Count > 0 ? "\n" + string.Join("\n", shackleNotices) : "";

            var embed = new EmbedBuilder()
                .WithTitle($"🏁 Checkpoint — Floor {session.Floor}")
                .WithDescription("The tower falls silent. Choose your reward before pushing on." + shackleText)
                .WithColor(Color.Gold);

            foreach (var p in session.Participants)
                embed.AddField($"{p.Username} — ❤️ {p.CurrentHp}/{p.MaxHp}", FormatBuffDebuffLine(p), false);

            var components = BuildCheckpointComponents(session, shackledIds);

            // In duo, mention each player above their row of buttons so it's obvious whose is whose
            string mention = session.Mode == TowerMode.Duo
                ? string.Join("  |  ", session.Participants.Select(p => $"<@{p.DiscordId}>"))
                : null;

            await thread.SendMessageAsync(text: mention, embed: embed.Build(), components: components);
        }

        public async Task<(bool success, string message)> HandleCheckpointChoiceAsync(
            string sessionId, ulong userId, string choice)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found.");

            var p = session.Participants.FirstOrDefault(x => x.DiscordId == userId);
            if (p == null) return (false, "You are not in this run.");
            if (p.CheckpointDone) return (false, "You have already made your checkpoint choice.");

            string resultMsg;

            switch (choice)
            {
                case "scavenge":
                    int gold = session.Floor * 10;
                    p.AccumulatedGold += gold;
                    p.CheckpointDone = true;
                    resultMsg = $"💰 You scavenge **{gold} gold** from the floor.";
                    break;

                case "rest":
                    bool shackled = p.Debuffs.Any(d => d.Type == TowerDebuffType.Shackled);
                    if (shackled)
                    {
                        p.Debuffs.RemoveAll(d => d.Type == TowerDebuffType.Shackled);
                        p.CheckpointDone = true;
                        resultMsg = "🔗 You are **Shackled** — your rest is skipped. The shackles break.";
                        break;
                    }
                    int healed = (int)(p.MaxHp * 0.30f);
                    p.CurrentHp = Math.Min(p.MaxHp, p.CurrentHp + healed);
                    p.CheckpointDone = true;
                    resultMsg = $"💚 You rest and recover **{healed}** HP. ({p.CurrentHp}/{p.MaxHp})";
                    break;

                case "buff":
                    // Don't mark done yet — player still needs to pick a buff
                    resultMsg = "✨ Choose a buff:";
                    return (true, resultMsg);

                case "gamble":
                    resultMsg = ApplyGamble(session, p);
                    p.CheckpointDone = true;
                    break;

                case "removedebuff":
                    if (p.Debuffs.Count == 0)
                        return (false, "❌ You have no debuffs to remove.");
                    if (p.DebuffRemovesRemaining <= 0)
                        return (false, "❌ You have used all 5 Remove Debuff charges for this run.");
                    if (p.Debuffs.Count == 1)
                    {
                        var d0 = p.Debuffs[0];
                        var removed = TowerDebuffPool.Get(d0.Type);
                        d0.Stacks--;
                        if (d0.Stacks <= 0) p.Debuffs.RemoveAt(0);
                        p.DebuffRemovesRemaining--;
                        string stackMsg0 = d0.Stacks > 0 ? $" (reduced to x{d0.Stacks})" : " (fully cleansed)";
                        p.CheckpointDone = true;
                        resultMsg = $"🗑️ **{removed.Emoji} {removed.Name}**{stackMsg0}. Removes left: **{p.DebuffRemovesRemaining}**";
                    }
                    else
                    {
                        // Multiple debuffs — let player pick which to remove
                        resultMsg = "removedebuff_pick";
                        return (true, resultMsg);
                    }
                    break;

                default:
                    return (false, "Unknown choice.");
            }

            await TryResumeIfAllDoneAsync(session);
            return (true, resultMsg);
        }

        public async Task<(bool success, string message)> HandleBuffPickAsync(
            string sessionId, ulong userId, int buffIndex)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found.");

            var p = session.Participants.FirstOrDefault(x => x.DiscordId == userId);
            if (p == null) return (false, "You are not in this run.");
            if (p.CheckpointDone) return (false, "You have already made your checkpoint choice.");
            if (p.PendingBuffChoices == null || buffIndex >= p.PendingBuffChoices.Count)
                return (false, "Invalid buff choice.");

            var chosen = p.PendingBuffChoices[buffIndex];
            AddBuff(p, chosen);
            p.PendingBuffChoices = null;
            p.CheckpointDone = true;

            var def = TowerBuffPool.Get(chosen);
            await TryResumeIfAllDoneAsync(session);

            return (true, $"✨ You gain **{def.Emoji} {def.Name}**! — {def.Description}");
        }

        private async Task TryResumeIfAllDoneAsync(TowerSession session)
        {
            if (!session.Participants.All(p => p.CheckpointDone)) return;

            session.Status = TowerStatus.Running;
            session.NextFloorAt = DateTime.UtcNow.AddSeconds(FloorIntervalSeconds);

            var thread = _client.GetChannel(session.ThreadId) as IThreadChannel;
            if (thread != null)
            {
                await thread.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("▶️ Continuing the climb...")
                    .WithDescription($"All players have made their choice. Floor **{session.Floor + 1}** in {FloorIntervalSeconds} seconds...")
                    .WithColor(Color.DarkGrey)
                    .Build());
            }
        }

        private string ApplyGamble(TowerSession session, TowerParticipant p)
        {
            var debuff = RollRandomDebuff();
            var debuffDef = TowerDebuffPool.Get(debuff);
            AddDebuffSafe(p, debuff, debuffDef.DefaultDuration, session.Floor);

            int roll = _random.Next(3);
            string reward;

            if (roll == 0)
            {
                var buff = RollRandomBuff();
                AddBuff(p, buff);
                AddBuff(p, buff);
                var buffDef = TowerBuffPool.Get(buff);
                reward = $"gained **2 stacks of {buffDef.Emoji} {buffDef.Name}**";
            }
            else if (roll == 1)
            {
                int gold = session.Floor * 25;
                p.AccumulatedGold += gold;
                reward = $"earned **{gold} bonus gold**";
            }
            else
            {
                int healed = p.MaxHp;
                p.CurrentHp = Math.Min(p.MaxHp, p.CurrentHp + healed);
                reward = "was **fully healed**";
            }

            return $"🎲 **{p.Username}** gambles — afflicted with **{debuffDef.Emoji} {debuffDef.Name}** but {reward}!";
        }

        // =========================
        // END RUN
        // =========================
        private async Task EndRunAsync(TowerSession session, IThreadChannel thread)
        {
            session.Status = TowerStatus.Dead;

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var petService = scope.ServiceProvider.GetRequiredService<PetServices.PetService>();
            var achievementService = scope.ServiceProvider.GetRequiredService<AchievementServices.AchievementService>();

            var rewardLines = new System.Text.StringBuilder();

            foreach (var p in session.Participants)
            {
                int gold = p.AccumulatedGold;
                var player = await playerRepo.GetByDiscordIdAsync(p.DiscordId);
                if (player == null) continue;

                player.Gold += gold;
                player.XP += FlatXp;

                if (session.Mode == TowerMode.Solo && session.Floor > player.BestSoloTowerFloor)
                    player.BestSoloTowerFloor = session.Floor;
                else if (session.Mode == TowerMode.Duo && session.Floor > player.BestDuoTowerFloor)
                    player.BestDuoTowerFloor = session.Floor;

                await playerRepo.UpdatePlayerAsync(player);
                await petService.AddXPAsync(p.DiscordId, FlatPetXp);
                await achievementService.CheckAndAwardAsync(p.DiscordId);

                string status = p.CurrentHp <= 0 ? "💀 Fell" : "🏃 Survived";
                rewardLines.AppendLine($"{status} **{p.Username}** — 💰 +{gold} gold | ⭐ +{FlatXp} XP | 🐾 +{FlatPetXp} pet XP");
            }

            int floorReached = session.Floor;

            await thread.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle($"🗼 Run Over — Floor {floorReached} reached")
                .WithDescription(rewardLines.ToString().TrimEnd())
                .WithColor(Color.DarkRed)
                .WithFooter("The tower remains. Come back tomorrow.")
                .Build());

            await thread.ModifyAsync(t => t.Archived = true);

            lock (_completedThreadIds)
                _completedThreadIds.Add(thread.Id);

            RemoveSession(session.SessionId);
        }

        // =========================
        // HELPERS
        // =========================
        private int CalcPlayerDamage(TowerParticipant p, int enemyDef, bool isSpecialFloor)
        {
            int def = HasActiveBuff(p, TowerBuffType.Precision)
                ? (int)(enemyDef * (1f - GetBuffStacks(p, TowerBuffType.Precision) * 0.33f))
                : enemyDef;

            def = Math.Max(0, def);

            int dmg = (int)(p.BaseAttack * (100.0 / (100.0 + def)));
            dmg = Math.Max(1, dmg);

            // Weakened — each stack reduces damage by 20%, capped at 80% total reduction
            var weakened = p.Debuffs.FirstOrDefault(d => d.Type == TowerDebuffType.Weakened);
            if (weakened != null)
                dmg = (int)(dmg * Math.Max(0.20f, 1f - weakened.Stacks * 0.20f));

            // Executioner on elite/boss floors
            if (isSpecialFloor && HasActiveBuff(p, TowerBuffType.Executioner))
                dmg = (int)(dmg * (1f + GetBuffStacks(p, TowerBuffType.Executioner) * 0.25f));

            // Frenzy
            if (HasActiveBuff(p, TowerBuffType.Frenzy) && p.FrenzyStacks > 0)
                dmg = (int)(dmg * (1f + p.FrenzyStacks * 0.05f));

            // Double Strike
            float dsChance = Math.Min(0.60f, GetBuffStacks(p, TowerBuffType.DoubleStrike) * 0.20f);
            if (dsChance > 0 && _random.NextDouble() < dsChance)
                dmg *= 2;

            return dmg;
        }

        private int CalcLifesteal(TowerParticipant p, int dmgDealt)
        {
            float percent = GetBuffStacks(p, TowerBuffType.Bloodthirst) * 0.10f;
            return percent > 0 ? (int)(dmgDealt * percent) : 0;
        }

        private void AddBuff(TowerParticipant p, TowerBuffType type)
        {
            var existing = p.Buffs.FirstOrDefault(b => b.Type == type);
            if (existing != null) existing.Stacks++;
            else p.Buffs.Add(new TowerBuff { Type = type, Stacks = 1 });
        }

        private bool HasActiveBuff(TowerParticipant p, TowerBuffType type) =>
            p.Buffs.Any(b => b.Type == type && b.DisabledForFloors <= 0);

        private int GetBuffStacks(TowerParticipant p, TowerBuffType type)
        {
            var buff = p.Buffs.FirstOrDefault(b => b.Type == type && b.DisabledForFloors <= 0);
            return buff?.Stacks ?? 0;
        }

        private TowerBuffType RollRandomBuff() =>
            TowerBuffPool.All[_random.Next(TowerBuffPool.All.Count)].Type;

        private TowerDebuffType RollRandomDebuff() =>
            TowerDebuffPool.All[_random.Next(TowerDebuffPool.All.Count)].Type;

        // Adds a debuff stack. Same type merges into one entry and increments Stacks.
        private void AddDebuffSafe(TowerParticipant p, TowerDebuffType type, int floorsRemaining, int currentFloor = 0)
        {
            // Shackled: once per run, and only after floor 30
            if (type == TowerDebuffType.Shackled && (p.HasBeenShackled || currentFloor < 30)) return;

            var existing = p.Debuffs.FirstOrDefault(d => d.Type == type);
            if (existing != null)
            {
                existing.Stacks++;
                // For temporary debuffs, also refresh the duration
                if (existing.FloorsRemaining >= 0 && floorsRemaining > 0)
                    existing.FloorsRemaining = Math.Max(existing.FloorsRemaining, floorsRemaining);
            }
            else
            {
                p.Debuffs.Add(new TowerDebuff { Type = type, FloorsRemaining = floorsRemaining, Stacks = 1 });
                if (type == TowerDebuffType.Shackled) p.HasBeenShackled = true;
            }
        }

        private List<TowerBuffType> RollThreeBuffs()
        {
            var pool = TowerBuffPool.All.Select(b => b.Type).ToList();
            var result = new List<TowerBuffType>();
            while (result.Count < 3 && pool.Count > 0)
            {
                int idx = _random.Next(pool.Count);
                result.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
            return result;
        }

        private string FormatBuffDebuffLine(TowerParticipant p)
        {
            var buffs = p.Buffs.Select(b =>
            {
                var def = TowerBuffPool.Get(b.Type);
                string disabled = b.DisabledForFloors > 0 ? " *(disabled)*" : "";
                return $"{def.Emoji} {def.Name} x{b.Stacks}{disabled}";
            });

            var debuffs = p.Debuffs.Select(d =>
            {
                var def = TowerDebuffPool.Get(d.Type);
                string stacks = d.Stacks > 1 ? $" x{d.Stacks}" : "";
                return $"{def.Emoji} {def.Name}{stacks}";
            });

            var parts = new List<string>();
            if (buffs.Any()) parts.Add(string.Join(" · ", buffs));
            if (debuffs.Any()) parts.Add(string.Join(" · ", debuffs));
            return parts.Count > 0 ? string.Join("\n", parts) : "*No active effects*";
        }

        private string FormatHpBar(int current, int max)
        {
            int filled = max > 0 ? (int)((double)current / max * 10) : 0;
            return new string('█', filled) + new string('░', 10 - filled) + $" {current}/{max}";
        }

        // =========================
        // EMBEDS & COMPONENTS
        // =========================
        private Embed BuildStartEmbed(TowerSession session)
        {
            string names = string.Join(" & ", session.Participants.Select(p => p.Username));
            return new EmbedBuilder()
                .WithTitle($"🗼 Tower of Doom — {session.Mode} Run")
                .WithDescription($"**{names}** begin their ascent.\n\nThe first floor awaits in a few seconds...")
                .WithColor(Color.DarkRed)
                .Build();
        }

        private Embed BuildFloorEmbed(TowerSession session, string combatLog, TowerFloorEventType eventType, TowerBossDefinition? bossDef = null)
        {
            string emoji = eventType switch
            {
                TowerFloorEventType.Boss        => "💀",
                TowerFloorEventType.Elite       => "👑",
                TowerFloorEventType.TreasureRoom => "🎁",
                TowerFloorEventType.CursedFloor  => "☠️",
                TowerFloorEventType.RestSite     => "💚",
                _                               => "⚔️"
            };

            var builder = new EmbedBuilder()
                .WithTitle($"{emoji} Floor {session.Floor}")
                .WithDescription(combatLog)
                .WithColor(eventType == TowerFloorEventType.Boss ? Color.DarkRed : Color.DarkGrey);

            if (bossDef != null && !string.IsNullOrWhiteSpace(bossDef.ImageUrl))
                builder.WithImageUrl(bossDef.ImageUrl);

            foreach (var p in session.Participants)
            {
                string status = p.CurrentHp <= 0 ? "💀 DEAD" : $"❤️ {FormatHpBar(p.CurrentHp, p.MaxHp)}";
                builder.AddField(p.Username, $"{status}\n{FormatBuffDebuffLine(p)}", true);
            }

            if (session.Participants.All(p => p.CurrentHp > 0))
            {
                bool nextIsCheckpoint = (session.Floor + 1) % 10 == 0;
                bool nextIsBoss = (session.Floor + 1) % 25 == 0;
                string next = nextIsBoss ? "⚠️ Boss next!" : nextIsCheckpoint ? "🏁 Checkpoint next!" : $"Next floor in {FloorIntervalSeconds}s";
                builder.WithFooter(next);
            }

            return builder.Build();
        }

        private MessageComponent BuildCheckpointComponents(TowerSession session, HashSet<ulong>? shackledIds = null)
        {
            var builder = new ComponentBuilder();

            // Each player gets their own row. Discord allows max 5 rows of 5 buttons.
            // Solo: row 0. Duo: player 1 = row 0, player 2 = row 1.
            foreach (var (p, row) in session.Participants.Select((p, i) => (p, i)))
            {
                bool shackled = shackledIds?.Contains(p.DiscordId) ?? false;
                bool hasDebuffs = p.Debuffs.Count > 0;
                bool canRemoveDebuff = hasDebuffs && p.DebuffRemovesRemaining > 0;
                string removeLabel = $"🗑️ Remove Debuff ({p.DebuffRemovesRemaining} left)";

                builder.WithButton($"💰 Scavenge ({session.Floor * 10}g)", $"tower_cp:{session.SessionId}:{p.DiscordId}:scavenge", ButtonStyle.Secondary, row: row);
                builder.WithButton("💚 Rest",         $"tower_cp:{session.SessionId}:{p.DiscordId}:rest",          ButtonStyle.Success,   row: row, disabled: shackled);
                builder.WithButton("✨ Pick Buff",    $"tower_cp:{session.SessionId}:{p.DiscordId}:buff",          ButtonStyle.Primary,   row: row);
                builder.WithButton("🎲 Gamble",       $"tower_cp:{session.SessionId}:{p.DiscordId}:gamble",        ButtonStyle.Danger,    row: row);
                builder.WithButton(removeLabel,        $"tower_cp:{session.SessionId}:{p.DiscordId}:removedebuff", ButtonStyle.Secondary, row: row, disabled: !canRemoveDebuff);
            }

            return builder.Build();
        }

        public MessageComponent BuildDebuffPickComponents(string sessionId, ulong playerId, List<TowerDebuff> debuffs)
        {
            var builder = new ComponentBuilder();
            for (int i = 0; i < debuffs.Count; i++)
            {
                var def = TowerDebuffPool.Get(debuffs[i].Type);
                builder.WithButton($"{def.Emoji} Remove {def.Name}", $"tower_rmdebuff:{sessionId}:{playerId}:{i}", ButtonStyle.Danger, row: 0);
            }
            return builder.Build();
        }

        public async Task<(bool success, string message)> HandleDebuffRemoveAsync(string sessionId, ulong userId, int index)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found.");

            var p = session.Participants.FirstOrDefault(x => x.DiscordId == userId);
            if (p == null) return (false, "You are not in this run.");
            if (p.CheckpointDone) return (false, "You have already made your checkpoint choice.");
            if (index < 0 || index >= p.Debuffs.Count) return (false, "Invalid choice.");

            var debuff = p.Debuffs[index];
            var def = TowerDebuffPool.Get(debuff.Type);

            debuff.Stacks--;
            if (debuff.Stacks <= 0)
                p.Debuffs.RemoveAt(index);

            p.DebuffRemovesRemaining--;
            p.CheckpointDone = true;

            string stackMsg = debuff.Stacks > 0 ? $" (reduced to x{debuff.Stacks})" : " (fully cleansed)";
            await TryResumeIfAllDoneAsync(session);
            return (true, $"🗑️ **{def.Emoji} {def.Name}**{stackMsg}. Removes left: **{p.DebuffRemovesRemaining}**");
        }

        public MessageComponent BuildBuffPickComponents(string sessionId, ulong playerId, List<TowerBuffType> choices)
        {
            var builder = new ComponentBuilder();

            for (int i = 0; i < choices.Count; i++)
            {
                var def = TowerBuffPool.Get(choices[i]);
                builder.WithButton($"{def.Emoji} {def.Name}", $"tower_buff:{sessionId}:{playerId}:{i}", ButtonStyle.Primary, row: 0);
            }

            return builder.Build();
        }

        public TowerSession? GetSession(string sessionId) =>
            _sessions.GetValueOrDefault(sessionId);

        public TowerSession? GetSessionForPlayer(ulong userId)
        {
            if (!_playerSession.TryGetValue(userId, out var sid)) return null;
            return _sessions.GetValueOrDefault(sid);
        }

        public bool IsInSession(ulong userId) => _playerSession.ContainsKey(userId);

        private void RemoveSession(string sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session)) return;
            foreach (var p in session.Participants)
                _playerSession.Remove(p.DiscordId);
            _sessions.Remove(sessionId);
        }
    }
}

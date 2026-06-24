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
using System.Linq;

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

                var bossDue = _sessions.Values
                    .Where(s => s.Status == TowerStatus.Running && s.NextBossRoundAt.HasValue && s.NextBossRoundAt <= DateTime.UtcNow)
                    .ToList();

                foreach (var session in bossDue)
                {
                    try { await ResolveBossRoundAsync(session); }
                    catch (Exception ex) { Console.WriteLine($"❌ Tower boss round error [{session.SessionId}]: {ex.Message}"); }
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
            session.Status = TowerStatus.StartPick;

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
            await PostStartBuffChoiceAsync(session, thread);

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
            var shakeOffNotices = new List<string>();
            foreach (var p in session.Participants)
            {
                p.TookDamageThisFloor = false;

                foreach (var debuff in p.Debuffs.Where(d => d.FloorsRemaining > 0))
                    debuff.FloorsRemaining--;

                p.Debuffs.RemoveAll(d => d.FloorsRemaining == 0);

                // 5% chance each floor to shake off a random debuff.
                // Solo loses the whole debuff; duo only loses one stack.
                if (p.Debuffs.Count > 0 && _random.NextDouble() < 0.05)
                {
                    int luckyIndex = _random.Next(p.Debuffs.Count);
                    var luckyDebuff = p.Debuffs[luckyIndex];
                    var luckyDef = TowerDebuffPool.Get(luckyDebuff.Type);

                    if (session.Mode == TowerMode.Solo)
                    {
                        p.Debuffs.RemoveAt(luckyIndex);
                        shakeOffNotices.Add($"🍀 **{p.Username}** shakes off **{luckyDef.Emoji} {luckyDef.Name}** completely!");
                    }
                    else
                    {
                        luckyDebuff.Stacks--;

                        if (luckyDebuff.Stacks <= 0)
                        {
                            p.Debuffs.RemoveAt(luckyIndex);
                            shakeOffNotices.Add($"🍀 **{p.Username}** shakes off **{luckyDef.Emoji} {luckyDef.Name}** completely!");
                        }
                        else
                        {
                            shakeOffNotices.Add($"🍀 **{p.Username}** shakes off a stack of **{luckyDef.Emoji} {luckyDef.Name}** (down to x{luckyDebuff.Stacks}).");
                        }
                    }
                }

                foreach (var buff in p.Buffs)
                    if (buff.DisabledForFloors > 0) buff.DisabledForFloors--;
            }

            if (shakeOffNotices.Count > 0)
            {
                var shakeThread = _client.GetChannel(session.ThreadId) as IThreadChannel;
                if (shakeThread != null)
                    await shakeThread.SendMessageAsync(string.Join("\n", shakeOffNotices));
            }

            bool isBoss = session.Floor % 25 == 0;
            bool isElite = !isBoss && session.Floor % 5 == 0;
            TowerFloorEventType eventType;

            if (isBoss)
                eventType = TowerFloorEventType.Boss;
            else if (isElite)
                eventType = TowerFloorEventType.Elite;
            else if (session.Floor > 30 && session.MerchantFloor == 0 && _random.NextDouble() < 0.10)
                eventType = TowerFloorEventType.Merchant;
            else
                eventType = RollEventType();

            // Boss floors — show pre-boss choice (full heal or remove debuff stack) then start the fight
            if (eventType == TowerFloorEventType.Boss)
            {
                var bossThread = _client.GetChannel(session.ThreadId) as IThreadChannel;
                if (bossThread == null) return;
                await PostPreBossChoiceAsync(session, TowerBossRegistry.GetForFloor(session.Floor), bossThread);
                return;
            }

            string combatLog = eventType switch
            {
                TowerFloorEventType.Combat       => ResolveCombat(session, false, false),
                TowerFloorEventType.Elite        => ResolveCombat(session, true, false),
                TowerFloorEventType.TreasureRoom => ResolveTreasureRoom(session),
                TowerFloorEventType.CursedFloor  => ResolveCursedFloor(session),
                TowerFloorEventType.RestSite     => ResolveRestSite(session),
                TowerFloorEventType.Merchant     => ResolveMerchantFloor(session),
                _                                => ResolveCombat(session, false, false)
            };

            // Accumulate gold (boss floors accumulate on kill)
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
                    await thread.SendMessageAsync(embed: BuildFloorEmbed(session, combatLog, eventType));

                    foreach (var fallen in dead)
                    {
                        // Merge buffs — capped overflow becomes random buffs instead of being wasted
                        foreach (var buff in fallen.Buffs)
                            MergeBuffOnDeath(alive, buff);
                        // Merge debuffs, preserving full stack counts
                        foreach (var debuff in fallen.Debuffs)
                            MergeDebuffOnDeath(alive, debuff);
                    }

                    alive.PartnerDied = true;
                    string fallenNames = string.Join(" & ", dead.Select(p => $"**{p.Username}**"));
                    await thread.SendMessageAsync(embed: new EmbedBuilder()
                        .WithTitle("💀 A partner has fallen!")
                        .WithDescription($"{fallenNames} has died — their buffs and debuffs have been passed to **{alive.Username}**.\n\nThe climb continues alone. ⚠️ **Incoming damage doubled!**")
                        .WithColor(Color.DarkRed)
                        .Build());

                    // Move dead participants to FallenParticipants (not discarded) so they still get end-of-run rewards
                    // and have their player-session lock cleared when the run ends
                    session.FallenParticipants.AddRange(dead);
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
                await thread.SendMessageAsync(embed: BuildFloorEmbed(session, combatLog, eventType));
                await EndRunAsync(session, thread);
                return;
            }

            await thread.SendMessageAsync(embed: BuildFloorEmbed(session, combatLog, eventType));

            if (eventType == TowerFloorEventType.Merchant)
            {
                // Merchant floors wait on an explicit "Move On" click, not a timer
                session.Status = TowerStatus.Shopping;
                foreach (var sp in session.Participants)
                    sp.CheckpointDone = false;
                await PostMerchantShopAsync(session, thread);
            }
            else if (session.Floor % 10 == 0)
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
                var (playerDmg, doubleStruck) = CalcPlayerDamage(p, enemyDef, isElite || isBoss);
                log.AppendLine($"⚔️ **{p.Username}** deals **{playerDmg}** damage!");
                if (doubleStruck)
                    log.AppendLine($"  ⚡ **Double Strike!** {p.Username} hits twice!");
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

                // Duo split — enemy spreads damage across two players.
                // If the partner already died, the survivor has no backup and takes double instead.
                if (p.PartnerDied)
                    rawEnemyDmg *= 2;
                else if (isDuo)
                    rawEnemyDmg = (int)(rawEnemyDmg * 0.65f);

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

        // =========================
        // MULTI-ROUND BOSS FIGHT
        // =========================
        private async Task StartBossFightAsync(TowerSession session, TowerBossDefinition bossDef, IThreadChannel thread)
        {
            int floor = session.Floor;
            bool isDuo = session.Mode == TowerMode.Duo;

            int bossHp = isDuo ? (80 + floor * 16) * 2 : 50 + floor * 12;
            bossHp = (int)(bossHp * bossDef.HpMultiplier);

            session.BossCurrentHp = bossHp;
            session.BossMaxHp = bossHp;
            session.ActiveBoss = bossDef;
            session.BossRound = 0;

            var intro = new EmbedBuilder()
                .WithTitle($"💀 Floor {floor} — Boss Encounter!")
                .WithColor(Color.DarkRed)
                .WithDescription($"**{bossDef.Name}** emerges from the darkness!\n*{bossDef.Description}*\n\n{bossDef.SpecialMechanicText}\n\n⚔️ **Round 1 begins in 5 seconds...**");

            if (!string.IsNullOrWhiteSpace(bossDef.ImageUrl))
                intro.WithImageUrl(bossDef.ImageUrl);

            foreach (var p in session.Participants)
                intro.AddField(p.Username, $"❤️ {FormatHpBar(p.CurrentHp, p.MaxHp)}", true);

            intro.AddField($"👹 {bossDef.Name}", FormatHpBar(session.BossCurrentHp, session.BossMaxHp), false);

            await thread.SendMessageAsync(embed: intro.Build());

            session.NextBossRoundAt = DateTime.UtcNow.AddSeconds(5);
        }

        private async Task ResolveBossRoundAsync(TowerSession session)
        {
            session.NextBossRoundAt = null;
            session.BossRound++;

            var bossDef = session.ActiveBoss!;
            var thread = _client.GetChannel(session.ThreadId) as IThreadChannel;
            if (thread == null) return;

            string combatLog = ResolveBossRoundCombat(session);

            bool bossDefeated = session.BossCurrentHp <= 0;
            bool anyDead = session.Participants.Any(p => p.CurrentHp <= 0);

            // Duo: if one partner dies during boss fight, pass buffs/debuffs to survivor
            if (anyDead && !bossDefeated && session.Mode == TowerMode.Duo)
            {
                var dead = session.Participants.Where(p => p.CurrentHp <= 0).ToList();
                var alive = session.Participants.FirstOrDefault(p => p.CurrentHp > 0);
                if (alive != null && dead.Count < session.Participants.Count)
                {
                    await thread.SendMessageAsync(embed: BuildBossRoundEmbed(session, bossDef, combatLog, false, false));
                    foreach (var fallen in dead)
                    {
                        foreach (var buff in fallen.Buffs)
                            MergeBuffOnDeath(alive, buff);
                        foreach (var debuff in fallen.Debuffs)
                            MergeDebuffOnDeath(alive, debuff);
                    }
                    alive.PartnerDied = true;
                    string fallenNames = string.Join(" & ", dead.Select(p => $"**{p.Username}**"));
                    await thread.SendMessageAsync(embed: new EmbedBuilder()
                        .WithTitle("💀 A partner has fallen!")
                        .WithDescription($"{fallenNames} fell to the boss — their buffs and debuffs pass to **{alive.Username}**.\n\nThe fight continues alone... ⚠️ **Incoming damage doubled!**")
                        .WithColor(Color.DarkRed).Build());
                    session.FallenParticipants.AddRange(dead);
                    session.Participants.RemoveAll(p => p.CurrentHp <= 0);
                    anyDead = false;
                }
            }

            if (anyDead)
            {
                await thread.SendMessageAsync(embed: BuildBossRoundEmbed(session, bossDef, combatLog, false, true));
                await EndRunAsync(session, thread);
                return;
            }

            if (bossDefeated)
            {
                string specialText = ApplyBossSpecialEffect(session, bossDef);
                if (!string.IsNullOrEmpty(specialText)) combatLog += "\n\n" + specialText;

                string sigilText = await HandleSigilDropAsync(session);
                if (!string.IsNullOrEmpty(sigilText)) combatLog += "\n" + sigilText;

                foreach (var p in session.Participants)
                    p.AccumulatedGold += GoldPerFloor;

                session.ActiveBoss = null;
                session.BossRound = 0;

                await thread.SendMessageAsync(embed: BuildBossRoundEmbed(session, bossDef, combatLog, true, false));

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

            await thread.SendMessageAsync(embed: BuildBossRoundEmbed(session, bossDef, combatLog, false, false));
            session.NextBossRoundAt = DateTime.UtcNow.AddSeconds(5);
        }

        private string ResolveBossRoundCombat(TowerSession session)
        {
            var bossDef = session.ActiveBoss!;
            int floor = session.Floor;
            bool isDuo = session.Mode == TowerMode.Duo;

            int enemyAtk = (int)((17 + floor * 9) * bossDef.AtkMultiplier);
            int enemyDef = 5 + floor * 3;

            var log = new System.Text.StringBuilder();

            // Player attacks boss
            foreach (var p in session.Participants)
            {
                var bleeding = p.Debuffs.FirstOrDefault(d => d.Type == TowerDebuffType.Bleeding);
                if (bleeding != null)
                {
                    int bleedDmg = Math.Max(1, (int)(p.MaxHp * Math.Min(0.25f, 0.05f * bleeding.Stacks)));
                    p.CurrentHp = Math.Max(0, p.CurrentHp - bleedDmg);
                    string stackNote = bleeding.Stacks > 1 ? $" (x{bleeding.Stacks})" : "";
                    log.AppendLine($"🩸 **{p.Username}** bleeds for **{bleedDmg}** HP!{stackNote}");
                    if (p.CurrentHp <= 0) { log.AppendLine($"💀 **{p.Username}** bled out!"); continue; }
                }

                var (playerDmg, doubleStruck) = CalcPlayerDamage(p, enemyDef, true);
                log.AppendLine($"⚔️ **{p.Username}** deals **{playerDmg}** damage!");
                if (doubleStruck)
                    log.AppendLine($"  ⚡ **Double Strike!** {p.Username} hits twice!");
                session.BossCurrentHp = Math.Max(0, session.BossCurrentHp - playerDmg);

                int lifesteal = CalcLifesteal(p, playerDmg);
                if (lifesteal > 0)
                {
                    p.CurrentHp = Math.Min(p.MaxHp, p.CurrentHp + lifesteal);
                    log.AppendLine($"  🩸 Lifesteal heals **{lifesteal}** HP.");
                }
            }

            log.AppendLine();

            if (session.BossCurrentHp <= 0)
            {
                log.AppendLine($"💥 **{bossDef.Name}** has been slain!");
                return log.ToString().TrimEnd();
            }

            // Boss attacks each player
            foreach (var p in session.Participants)
            {
                if (p.CurrentHp <= 0) continue;

                float dodgeChance = Math.Min(0.45f, GetBuffStacks(p, TowerBuffType.Evasion) * 0.15f);
                if (dodgeChance > 0 && _random.NextDouble() < dodgeChance)
                {
                    log.AppendLine($"💨 **{p.Username}** dodges the attack!");
                    continue;
                }

                float penetration = Math.Min(0.70f, floor * 0.012f);
                int effectiveDef = (int)(p.BaseDefense * (1f - penetration));
                int rawEnemyDmg = (int)(enemyAtk * (100.0 / (100.0 + effectiveDef)));
                rawEnemyDmg = Math.Max(1, rawEnemyDmg);

                float ironSkinReduction = Math.Min(0.60f, GetBuffStacks(p, TowerBuffType.IronSkin) * 0.15f);
                rawEnemyDmg = (int)(rawEnemyDmg * (1f - ironSkinReduction));
                rawEnemyDmg = Math.Max(1, rawEnemyDmg);

                if (p.PartnerDied)
                    rawEnemyDmg *= 2;
                else if (isDuo)
                    rawEnemyDmg = (int)(rawEnemyDmg * 0.65f);

                p.CurrentHp = Math.Max(0, p.CurrentHp - rawEnemyDmg);
                p.TookDamageThisFloor = true;
                log.AppendLine($"👹 **{bossDef.Name}** hits **{p.Username}** for **{rawEnemyDmg}** damage!");

                // Thorns now matters — reflects back to the boss across multiple rounds
                float thornsPercent = GetBuffStacks(p, TowerBuffType.Thorns) * 0.15f;
                if (thornsPercent > 0)
                {
                    int reflect = (int)(rawEnemyDmg * thornsPercent);
                    session.BossCurrentHp = Math.Max(0, session.BossCurrentHp - reflect);
                    log.AppendLine($"  🌵 Thorns reflects **{reflect}** damage back at {bossDef.Name}!");
                }
            }

            foreach (var p in session.Participants)
            {
                if (p.TookDamageThisFloor) p.FrenzyStacks = 0;
                else if (HasActiveBuff(p, TowerBuffType.Frenzy)) p.FrenzyStacks++;
            }

            return log.ToString().TrimEnd();
        }

        private Embed BuildBossRoundEmbed(TowerSession session, TowerBossDefinition bossDef, string combatLog, bool bossDefeated, bool allDead)
        {
            string title = bossDefeated
                ? $"🏆 Floor {session.Floor} — {bossDef.Name} Defeated!"
                : allDead ? $"💀 Floor {session.Floor} — Slain by {bossDef.Name}"
                : $"💀 Floor {session.Floor} — {bossDef.Name} · Round {session.BossRound}";

            var builder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(combatLog)
                .WithColor(bossDefeated ? Color.Gold : Color.DarkRed);

            if (!string.IsNullOrWhiteSpace(bossDef.ImageUrl))
                builder.WithImageUrl(bossDef.ImageUrl);

            foreach (var p in session.Participants)
            {
                string status = p.CurrentHp <= 0 ? "💀 DEAD" : $"❤️ {FormatHpBar(p.CurrentHp, p.MaxHp)}";
                builder.AddField(p.Username, $"{status}\n{FormatBuffDebuffLine(p)}", true);
            }

            if (!bossDefeated && !allDead)
                builder.AddField($"👹 {bossDef.Name}", FormatHpBar(session.BossCurrentHp, session.BossMaxHp), false)
                       .WithFooter("Next round in 5 seconds...");
            else if (bossDefeated)
                builder.AddField($"👹 {bossDef.Name}", "💀 Defeated!", false)
                       .WithFooter($"Continuing the climb in {FloorIntervalSeconds} seconds...");

            return builder.Build();
        }

        private string ApplyBossSpecialEffect(TowerSession session, TowerBossDefinition bossDef)
        {
            int bossIndex = ((session.Floor / 25) - 1) % TowerBossRegistry.All.Count;
            if (bossIndex == 1)
            {
                foreach (var p in session.Participants)
                    AddDebuffSafe(p, TowerDebuffType.Weakened, -1);
                return "😵 With its dying breath, **Weakened** seeps into all players until cleansed!";
            }
            if (bossIndex == 3)
            {
                foreach (var p in session.Participants)
                    AddDebuffSafe(p, TowerDebuffType.Bleeding, -1);
                return "🩸 The boss's venom seeps in — all players are **Bleeding** for the rest of the run!";
            }

            // Removes up to totalStacks buff stacks, spread randomly across whatever buffs
            // the player has (e.g. 2x Precision, 2x Bloodthirst, 1x IronSkin) instead of
            // wiping out one buff entirely. A Buff Protector charge blocks the theft outright.
            string RemoveBuffStacks(int totalStacks)
            {
                var lines = new List<string>();
                foreach (var p in session.Participants)
                {
                    if (p.BuffProtectorCharges > 0)
                    {
                        p.BuffProtectorCharges--;
                        lines.Add($"🛡️ **{p.Username}**'s Buff Protector shatters, blocking the theft!");
                        continue;
                    }

                    var removedCounts = new Dictionary<TowerBuffType, int>();
                    int remaining = totalStacks;

                    while (remaining > 0 && p.Buffs.Count > 0)
                    {
                        int idx = _random.Next(p.Buffs.Count);
                        var buff = p.Buffs[idx];
                        buff.Stacks--;
                        removedCounts[buff.Type] = removedCounts.GetValueOrDefault(buff.Type) + 1;
                        if (buff.Stacks <= 0) p.Buffs.RemoveAt(idx);
                        remaining--;
                    }

                    if (removedCounts.Count > 0)
                    {
                        string detail = string.Join(", ", removedCounts.Select(kv => $"{kv.Value}x {TowerBuffPool.Get(kv.Key).Name}"));
                        lines.Add($"💀 **{p.Username}** loses **{detail}**!");
                    }
                }
                return string.Join("\n", lines);
            }

            if (bossIndex == 4)
                return RemoveBuffStacks(2);

            void InflictWeakened(int stacks)
            {
                foreach (var p in session.Participants)
                    for (int i = 0; i < stacks; i++)
                        AddDebuffSafe(p, TowerDebuffType.Weakened, -1);
            }

            void InflictBleeding()
            {
                foreach (var p in session.Participants)
                    AddDebuffSafe(p, TowerDebuffType.Bleeding, -1);
            }

            if (bossIndex == 5)
                return RemoveBuffStacks(4);

            if (bossIndex == 6)
            {
                string stripped6 = RemoveBuffStacks(6);
                InflictWeakened(3);
                return $"{stripped6}\n😵 All players are also **Weakened x3**!".TrimStart('\n');
            }

            if (bossIndex == 7)
            {
                string stripped7 = RemoveBuffStacks(8);
                InflictBleeding();
                InflictWeakened(2);
                return $"{stripped7}\n🩸 All players are also **Bleeding** and **Weakened x2**!".TrimStart('\n');
            }

            if (bossIndex == 8)
                return RemoveBuffStacks(10);

            if (bossIndex == 9)
            {
                InflictBleeding();
                return "🩸 A final plague spreads — all players are **Bleeding** for the rest of the run!";
            }

            if (bossIndex == 10)
            {
                string stripped = RemoveBuffStacks(4);
                InflictWeakened(2);
                return $"{stripped}\n😵 All players are also **Weakened x2**!".TrimStart('\n');
            }

            if (bossIndex == 11)
            {
                string stripped = RemoveBuffStacks(8);
                InflictBleeding();
                return $"{stripped}\n🩸 All players are also **Bleeding** for the rest of the run!".TrimStart('\n');
            }

            if (bossIndex == 12)
            {
                InflictBleeding();
                InflictWeakened(3);
                return "🩸 The boss's last curse takes hold — all players are **Bleeding** and **Weakened x3**!";
            }

            if (bossIndex == 13)
            {
                string stripped = RemoveBuffStacks(8);
                InflictWeakened(3);
                return $"{stripped}\n😵 All players are also **Weakened x3**!".TrimStart('\n');
            }

            if (bossIndex == 14)
            {
                string stripped = RemoveBuffStacks(10);
                InflictBleeding();
                InflictWeakened(3);
                return $"{stripped}\n☠️ All players are also **Bleeding** and **Weakened x3**!".TrimStart('\n');
            }

            return "";
        }

        private async Task<string> HandleSigilDropAsync(TowerSession session)
        {
            if (session.Floor % 25 != 0) return "";

            var log = new System.Text.StringBuilder();
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
                        if (player != null) { player.Gold += SigilRegistry.CompensationGold; await playerRepo.UpdatePlayerAsync(player); }
                        log.AppendLine($"{sigilDef.Emoji} **{p.Username}** found a **{sigilDef.Name}** but already has 3 stacks! Compensated with **{SigilRegistry.CompensationGold} gold**.");
                    }
                    else
                    {
                        await sigilRepo.IncrementAsync(p.DiscordId, sigilDef.Id);
                        log.AppendLine($"✨ **{p.Username}** found a **{sigilDef.Name}**! ({current + 1}/3 stacks) — {sigilDef.BonusPerStack}");
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
                var granted = AddBuff(p, buff);
                var def = TowerBuffPool.Get(granted);
                string note = granted == buff ? "" : $" (**{TowerBuffPool.Get(buff).Name}** was already maxed)";
                log.AppendLine($"✨ **{p.Username}** finds **{def.Emoji} {def.Name}**!{note}");
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
                int healed = (int)(p.MaxHp * 0.25f);
                p.CurrentHp = Math.Min(p.MaxHp, p.CurrentHp + healed);
                log.AppendLine($"💚 **{p.Username}** recovers **{healed}** HP. ({p.CurrentHp}/{p.MaxHp})");
            }

            return log.ToString().TrimEnd();
        }

        private string ResolveMerchantFloor(TowerSession session)
        {
            session.MerchantFloor = session.Floor;

            var log = new System.Text.StringBuilder();
            log.AppendLine("🛒 **A traveling merchant emerges from the shadows!**");
            log.AppendLine("\"Gold for gear, climber? I don't ask where it came from. But I'm only passing through — buy now or don't.\"");
            return log.ToString().TrimEnd();
        }

        // =========================
        // MERCHANT SHOP
        // =========================
        private static readonly Dictionary<string, int> ShopPrices = BuildShopPrices();

        private static Dictionary<string, int> BuildShopPrices()
        {
            var prices = new Dictionary<string, int>
            {
                ["health"] = 100,
                ["attack"] = 100,
                ["defense"] = 100,
                ["bandage"] = 300,
                ["zoomerjuice"] = 250,
                ["buffprotector"] = 300,
            };
            foreach (var buffDef in TowerBuffPool.All)
                prices[$"buff_{buffDef.Type}"] = 250;
            return prices;
        }

        private async Task PostMerchantShopAsync(TowerSession session, IThreadChannel thread)
        {
            foreach (var p in session.Participants)
            {
                var components = BuildMerchantComponents(session.SessionId, p);
                string mention = session.Mode == TowerMode.Duo ? $"<@{p.DiscordId}> — " : "";
                await thread.SendMessageAsync(
                    $"{mention}🛒 The merchant opens their cloak. You have **{p.AccumulatedGold}** gold to spend. Each item can only be bought once. Take your time — click **▶️ Move On** when you're done to continue the climb. They won't be back this run.",
                    components: components);
            }
        }

        public MessageComponent BuildMerchantComponents(string sessionId, TowerParticipant p)
        {
            var builder = new ComponentBuilder();
            int row = 0, col = 0;
            bool done = p.CheckpointDone;

            void AddItem(string key, string label, bool unlocked)
            {
                int cost = ShopPrices[key];
                bool purchased = p.PurchasedShopItems.Contains(key);
                bool canAfford = p.AccumulatedGold >= cost;

                builder.WithButton(
                    purchased ? $"✅ {label}" : $"{label} ({cost}g)",
                    $"tower_shop:{sessionId}:{p.DiscordId}:{key}",
                    purchased ? ButtonStyle.Success : ButtonStyle.Secondary,
                    row: row,
                    disabled: purchased || !canAfford || !unlocked || done);

                col++;
                if (col >= 5) { col = 0; row++; }
            }

            bool hasBleeding = p.Debuffs.Any(d => d.Type == TowerDebuffType.Bleeding);
            bool hasWeakened = p.Debuffs.Any(d => d.Type == TowerDebuffType.Weakened);

            AddItem("health", "❤️ Increase Health (+100)", true);
            AddItem("attack", "⚔️ Increase Attack (+20)", true);
            AddItem("defense", "🛡️ Increase Defense (+20)", true);
            AddItem("bandage", "🩹 Bandage", hasBleeding);
            AddItem("zoomerjuice", "🧃 Zoomer Juice", hasWeakened);
            AddItem("buffprotector", "🛡️ Buff Protector", true);

            foreach (var buffDef in TowerBuffPool.All)
                AddItem($"buff_{buffDef.Type}", $"{buffDef.Emoji} {buffDef.Name}", true);

            // Move On always gets its own row, after every item row
            builder.WithButton(
                done ? "✅ Moved On" : "▶️ Move On",
                $"tower_shop_done:{sessionId}:{p.DiscordId}",
                done ? ButtonStyle.Success : ButtonStyle.Primary,
                row: row + 1,
                disabled: done);

            return builder.Build();
        }

        public async Task<(bool success, string message)> HandleMerchantPurchaseAsync(string sessionId, ulong userId, string itemKey)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found.");

            if (session.Status != TowerStatus.Shopping || session.Floor != session.MerchantFloor)
                return (false, "❌ The merchant has already left. They won't be back this run.");

            var p = session.Participants.FirstOrDefault(x => x.DiscordId == userId);
            if (p == null) return (false, "You are not in this run.");

            if (p.CheckpointDone)
                return (false, "❌ You've already moved on from the merchant.");

            if (!ShopPrices.TryGetValue(itemKey, out int cost))
                return (false, "Unknown item.");

            if (p.PurchasedShopItems.Contains(itemKey))
                return (false, "❌ You've already bought that.");

            if (p.AccumulatedGold < cost)
                return (false, $"❌ You need **{cost}** gold for that.");

            string resultMsg;

            if (itemKey == "bandage")
            {
                if (!p.Debuffs.Any(d => d.Type == TowerDebuffType.Bleeding))
                    return (false, "❌ You aren't bleeding.");
                p.Debuffs.RemoveAll(d => d.Type == TowerDebuffType.Bleeding);
                resultMsg = "🩹 The bleeding stops completely.";
            }
            else if (itemKey == "zoomerjuice")
            {
                if (!p.Debuffs.Any(d => d.Type == TowerDebuffType.Weakened))
                    return (false, "❌ You aren't weakened.");
                p.Debuffs.RemoveAll(d => d.Type == TowerDebuffType.Weakened);
                resultMsg = "🧃 You feel revitalized.";
            }
            else if (itemKey == "buffprotector")
            {
                p.BuffProtectorCharges++;
                resultMsg = "🛡️ You tuck the Buff Protector away — it'll block the next buff theft completely.";
            }
            else if (itemKey == "health")
            {
                p.MaxHp += 100;
                p.CurrentHp += 100;
                resultMsg = "❤️ You feel sturdier. **+100 Max HP**";
            }
            else if (itemKey == "attack")
            {
                p.BaseAttack += 20;
                resultMsg = "⚔️ Your weapon feels sharper. **+20 Attack**";
            }
            else if (itemKey == "defense")
            {
                p.BaseDefense += 20;
                resultMsg = "🛡️ Your armor feels tougher. **+20 Defense**";
            }
            else if (itemKey.StartsWith("buff_") && Enum.TryParse<TowerBuffType>(itemKey.Substring(5), out var buffType))
            {
                var granted = AddBuff(p, buffType);
                var def = TowerBuffPool.Get(granted);
                resultMsg = granted == buffType
                    ? $"✨ You purchase **{def.Emoji} {def.Name}**! — {def.Description}"
                    : $"✨ **{TowerBuffPool.Get(buffType).Name}** is already maxed — instead you gain **{def.Emoji} {def.Name}**! — {def.Description}";
            }
            else
            {
                return (false, "Unknown item.");
            }

            p.AccumulatedGold -= cost;
            p.PurchasedShopItems.Add(itemKey);

            return (true, $"{resultMsg}\n💰 Gold remaining: **{p.AccumulatedGold}**");
        }

        public async Task<(bool success, string message)> HandleMerchantMoveOnAsync(string sessionId, ulong userId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found.");

            if (session.Status != TowerStatus.Shopping)
                return (false, "❌ The merchant has already left.");

            var p = session.Participants.FirstOrDefault(x => x.DiscordId == userId);
            if (p == null) return (false, "You are not in this run.");
            if (p.CheckpointDone) return (false, "You've already moved on.");

            p.CheckpointDone = true;
            await TryResumeIfAllDoneAsync(session);

            return (true, "▶️ You finish up at the merchant's stall.");
        }

        // =========================
        // PRE-BOSS CHOICE
        // =========================
        private async Task PostPreBossChoiceAsync(TowerSession session, TowerBossDefinition bossDef, IThreadChannel thread)
        {
            session.ActiveBoss = bossDef;
            session.Status = TowerStatus.PreBoss;

            foreach (var p in session.Participants)
                p.CheckpointDone = false;

            var embed = new EmbedBuilder()
                .WithTitle($"⚔️ Boss Incoming — Floor {session.Floor}")
                .WithDescription($"⚔️ **{bossDef.Name}** guards this floor. Prepare yourself before engaging.")
                .WithColor(Color.DarkRed);

            foreach (var p in session.Participants)
                embed.AddField($"{p.Username} — ❤️ {p.CurrentHp}/{p.MaxHp}", FormatBuffDebuffLine(p), false);

            if (session.Mode == TowerMode.Duo && session.Participants.Count > 1)
            {
                await thread.SendMessageAsync(embed: embed.Build());
                foreach (var p in session.Participants)
                {
                    var pc = BuildPreBossComponents(session.SessionId, p);
                    await thread.SendMessageAsync($"<@{p.DiscordId}> — choose before the battle:", components: pc);
                }
            }
            else
            {
                var p = session.Participants[0];
                await thread.SendMessageAsync(embed: embed.Build(), components: BuildPreBossComponents(session.SessionId, p));
            }
        }

        private MessageComponent BuildPreBossComponents(string sessionId, TowerParticipant p)
        {
            bool hasDebuffs = p.Debuffs.Count > 0;
            return new ComponentBuilder()
                .WithButton("💚 Full Heal",           $"tower_preboss:{sessionId}:{p.DiscordId}:heal",        ButtonStyle.Success,   row: 0)
                .WithButton("🗑️ Remove Debuff Stack", $"tower_preboss:{sessionId}:{p.DiscordId}:removedebuff", ButtonStyle.Secondary, row: 0, disabled: !hasDebuffs)
                .Build();
        }

        public async Task<(bool success, string message)> HandlePreBossChoiceAsync(string sessionId, ulong userId, string choice)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found.");

            var p = session.Participants.FirstOrDefault(x => x.DiscordId == userId);
            if (p == null) return (false, "You are not in this run.");
            if (p.CheckpointDone) return (false, "You have already made your choice.");

            string resultMsg;

            switch (choice)
            {
                case "heal":
                    p.CurrentHp = p.MaxHp;
                    p.CheckpointDone = true;
                    resultMsg = $"💚 You recover fully. (**{p.MaxHp}/{p.MaxHp}** HP)";
                    break;

                case "removedebuff":
                    if (p.Debuffs.Count == 0)
                        return (false, "You have no debuffs.");

                    if (p.Debuffs.Count == 1)
                    {
                        var debuff = p.Debuffs[0];
                        var def = TowerDebuffPool.Get(debuff.Type);
                        debuff.Stacks--;
                        if (debuff.Stacks <= 0) p.Debuffs.RemoveAt(0);
                        string stackMsg0 = debuff.Stacks > 0 ? $" (reduced to x{debuff.Stacks})" : " (fully cleansed)";
                        p.CheckpointDone = true;
                        resultMsg = $"🗑️ **{def.Emoji} {def.Name}**{stackMsg0}.";
                    }
                    else
                    {
                        return (true, "preboss_removedebuff_pick");
                    }
                    break;

                default:
                    return (false, "Unknown choice.");
            }

            await TryResumeIfAllDoneAsync(session);
            return (true, resultMsg);
        }

        public async Task<(bool success, string message)> HandlePreBossDebuffPickAsync(string sessionId, ulong userId, int index)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found.");

            var p = session.Participants.FirstOrDefault(x => x.DiscordId == userId);
            if (p == null) return (false, "You are not in this run.");
            if (p.CheckpointDone) return (false, "You have already made your choice.");
            if (index < 0 || index >= p.Debuffs.Count) return (false, "Invalid choice.");

            var debuff = p.Debuffs[index];
            var def = TowerDebuffPool.Get(debuff.Type);
            debuff.Stacks--;
            if (debuff.Stacks <= 0) p.Debuffs.RemoveAt(index);

            p.CheckpointDone = true;
            string stackMsg = debuff.Stacks > 0 ? $" (reduced to x{debuff.Stacks})" : " (fully cleansed)";
            await TryResumeIfAllDoneAsync(session);
            return (true, $"🗑️ **{def.Emoji} {def.Name}**{stackMsg}.");
        }

        public MessageComponent BuildPreBossDebuffPickComponents(string sessionId, ulong playerId, List<TowerDebuff> debuffs)
        {
            var builder = new ComponentBuilder();
            for (int i = 0; i < debuffs.Count; i++)
            {
                var def = TowerDebuffPool.Get(debuffs[i].Type);
                builder.WithButton($"{def.Emoji} {def.Name} (x{debuffs[i].Stacks})", $"tower_preboss_rm:{sessionId}:{playerId}:{i}", ButtonStyle.Danger, row: 0);
            }
            return builder.Build();
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

            if (session.Mode == TowerMode.Duo && session.Participants.Count > 1)
            {
                // In duo: send one shared status embed, then a separate button message per player
                await thread.SendMessageAsync(embed: embed.Build());

                foreach (var p in session.Participants)
                {
                    bool shackled = shackledIds.Contains(p.DiscordId);
                    bool canRemoveDebuff = p.Debuffs.Count > 0 && p.DebuffRemovesRemaining > 0;
                    string removeLabel = $"🗑️ Remove Debuff ({p.DebuffRemovesRemaining} left)";

                    string scavengeLabel = p.HasScavenged ? "💰 Scavenge (used)" : $"💰 Scavenge ({session.Floor * 10}g)";
                    var playerComponents = new ComponentBuilder()
                        .WithButton(scavengeLabel, $"tower_cp:{session.SessionId}:{p.DiscordId}:scavenge", ButtonStyle.Secondary, row: 0, disabled: p.HasScavenged)
                        .WithButton("💚 Rest",      $"tower_cp:{session.SessionId}:{p.DiscordId}:rest",          ButtonStyle.Success,   row: 0, disabled: shackled)
                        .WithButton("✨ Pick Buff", $"tower_cp:{session.SessionId}:{p.DiscordId}:buff",          ButtonStyle.Primary,   row: 0)
                        .WithButton("🎲 Gamble",    $"tower_cp:{session.SessionId}:{p.DiscordId}:gamble",        ButtonStyle.Danger,    row: 0)
                        .WithButton(removeLabel,     $"tower_cp:{session.SessionId}:{p.DiscordId}:removedebuff", ButtonStyle.Secondary, row: 0, disabled: !canRemoveDebuff)
                        .Build();

                    await thread.SendMessageAsync($"<@{p.DiscordId}> — choose your reward:", components: playerComponents);
                }
            }
            else
            {
                var components = BuildCheckpointComponents(session, shackledIds);
                await thread.SendMessageAsync(embed: embed.Build(), components: components);
            }
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
                {
                    if (p.HasScavenged)
                        return (false, "❌ You've already scavenged this run.");

                    p.HasScavenged = true;
                    p.CheckpointDone = true;

                    var ambushBoss = TowerBossRegistry.All[_random.Next(TowerBossRegistry.All.Count)];
                    bool isDuoMode = session.Mode == TowerMode.Duo;
                    int ambushHp = isDuoMode ? 200 + session.Floor * 40 : 150 + session.Floor * 30;
                    int ambushDef = 5 + session.Floor * 3;

                    var (ambushDmg, _) = CalcPlayerDamage(p, ambushDef, true);

                    if (ambushDmg >= ambushHp)
                    {
                        int gold = session.Floor * 10;
                        p.AccumulatedGold += gold;
                        var buff = RollRandomBuff();
                        var granted = AddBuff(p, buff);
                        var buffDef = TowerBuffPool.Get(granted);
                        resultMsg = $"💰 While scavenging, **{ambushBoss.Name}** ambushes you! You strike them down and loot **{gold} gold** and gain **{buffDef.Emoji} {buffDef.Name}**!";
                    }
                    else
                    {
                        resultMsg = $"💀 While scavenging, **{ambushBoss.Name}** ambushes you! You're overpowered and forced to flee empty-handed.";
                    }
                    break;
                }

                case "rest":
                    bool shackled = p.Debuffs.Any(d => d.Type == TowerDebuffType.Shackled);
                    if (shackled)
                    {
                        p.Debuffs.RemoveAll(d => d.Type == TowerDebuffType.Shackled);
                        p.CheckpointDone = true;
                        resultMsg = "🔗 You are **Shackled** — your rest is skipped. The shackles break.";
                        break;
                    }
                    int healed = (int)(p.MaxHp * 0.50f);
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
                        return (false, "❌ You have used all 3 Remove Debuff charges for this run.");
                    if (p.Debuffs.Count == 1)
                    {
                        var d0 = p.Debuffs[0];
                        var removed = TowerDebuffPool.Get(d0.Type);
                        p.Debuffs.RemoveAt(0);
                        p.DebuffRemovesRemaining--;
                        p.CheckpointDone = true;
                        resultMsg = $"🗑️ **{removed.Emoji} {removed.Name}** fully cleansed. Removes left: **{p.DebuffRemovesRemaining}**";
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
            if (p.CheckpointDone) return (false, "You have already made your choice.");
            if (p.PendingBuffChoices == null || buffIndex >= p.PendingBuffChoices.Count)
                return (false, "Invalid buff choice.");

            var chosen = p.PendingBuffChoices[buffIndex];
            var granted = AddBuff(p, chosen);
            p.PendingBuffChoices = null;
            p.CheckpointDone = true;

            var def = TowerBuffPool.Get(granted);
            await TryResumeIfAllDoneAsync(session);

            string msg = granted == chosen
                ? $"✨ You gain **{def.Emoji} {def.Name}**! — {def.Description}"
                : $"✨ **{TowerBuffPool.Get(chosen).Name}** is already maxed — instead you gain **{def.Emoji} {def.Name}**! — {def.Description}";

            return (true, msg);
        }

        private async Task TryResumeIfAllDoneAsync(TowerSession session)
        {
            if (!session.Participants.All(p => p.CheckpointDone)) return;

            var thread = _client.GetChannel(session.ThreadId) as IThreadChannel;

            if (session.Status == TowerStatus.PreBoss)
            {
                session.Status = TowerStatus.Running;
                if (thread != null && session.ActiveBoss != null)
                    await StartBossFightAsync(session, session.ActiveBoss, thread);
                return;
            }

            if (session.Status == TowerStatus.StartPick)
            {
                session.Status = TowerStatus.Running;
                session.NextFloorAt = DateTime.UtcNow.AddSeconds(3);

                if (thread != null)
                {
                    await thread.SendMessageAsync(embed: new EmbedBuilder()
                        .WithTitle("▶️ The climb begins!")
                        .WithDescription("Floor 1 in a few seconds...")
                        .WithColor(Color.DarkGrey)
                        .Build());
                }
                return;
            }

            if (session.Status == TowerStatus.Shopping)
            {
                session.Status = TowerStatus.Running;
                session.NextFloorAt = DateTime.UtcNow.AddSeconds(FloorIntervalSeconds);

                if (thread != null)
                {
                    await thread.SendMessageAsync(embed: new EmbedBuilder()
                        .WithTitle("▶️ Continuing the climb...")
                        .WithDescription($"The merchant packs up. Floor **{session.Floor + 1}** in {FloorIntervalSeconds} seconds...")
                        .WithColor(Color.DarkGrey)
                        .Build());
                }
                return;
            }

            session.Status = TowerStatus.Running;
            session.NextFloorAt = DateTime.UtcNow.AddSeconds(FloorIntervalSeconds);

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
                var granted = AddBuff(p, buff);
                AddBuff(p, granted);
                var buffDef = TowerBuffPool.Get(granted);
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

            foreach (var p in session.Participants.Concat(session.FallenParticipants))
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
        private (int damage, bool doubleStruck) CalcPlayerDamage(TowerParticipant p, int enemyDef, bool isSpecialFloor)
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
            bool doubleStruck = dsChance > 0 && _random.NextDouble() < dsChance;
            if (doubleStruck)
                dmg *= 2;

            return (dmg, doubleStruck);
        }

        private int CalcLifesteal(TowerParticipant p, int dmgDealt)
        {
            float percent = GetBuffStacks(p, TowerBuffType.Bloodthirst) * 0.10f;
            return percent > 0 ? (int)(dmgDealt * percent) : 0;
        }

        // Grants a stack of the given buff. If the player is already capped on it,
        // transforms the grant into a different, non-capped buff instead of wasting it.
        // Returns the buff type that was actually granted (may differ from the requested one).
        private TowerBuffType AddBuff(TowerParticipant p, TowerBuffType type)
        {
            var def = TowerBuffPool.Get(type);
            var existing = p.Buffs.FirstOrDefault(b => b.Type == type);

            if (existing != null && existing.Stacks >= def.MaxStacks)
            {
                var candidates = TowerBuffPool.All
                    .Where(b => b.Type != type)
                    .Where(b =>
                    {
                        var stack = p.Buffs.FirstOrDefault(x => x.Type == b.Type);
                        return stack == null || stack.Stacks < b.MaxStacks;
                    })
                    .ToList();

                if (candidates.Count == 0) return type; // everything is capped — nothing left to give

                type = candidates[_random.Next(candidates.Count)].Type;
                existing = p.Buffs.FirstOrDefault(b => b.Type == type);
            }

            if (existing != null) existing.Stacks++;
            else p.Buffs.Add(new TowerBuff { Type = type, Stacks = 1 });

            return type;
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

        // Transfers a fallen partner's debuff to the survivor preserving its full stack count
        // (unlike AddDebuffSafe, which only ever adds one stack at a time).
        private void MergeDebuffOnDeath(TowerParticipant alive, TowerDebuff debuff)
        {
            if (debuff.Type == TowerDebuffType.Shackled && alive.HasBeenShackled) return;

            var existing = alive.Debuffs.FirstOrDefault(d => d.Type == debuff.Type);
            if (existing != null)
            {
                existing.Stacks += debuff.Stacks;
                if (existing.FloorsRemaining >= 0 && debuff.FloorsRemaining >= 0)
                    existing.FloorsRemaining = Math.Max(existing.FloorsRemaining, debuff.FloorsRemaining);
            }
            else
            {
                alive.Debuffs.Add(new TowerDebuff { Type = debuff.Type, FloorsRemaining = debuff.FloorsRemaining, Stacks = debuff.Stacks });
                if (debuff.Type == TowerDebuffType.Shackled) alive.HasBeenShackled = true;
            }
        }

        // Transfers a fallen partner's buff to the survivor, respecting the buff's own cap.
        // Any stacks that would overflow the cap become random buffs instead of being wasted.
        private void MergeBuffOnDeath(TowerParticipant alive, TowerBuff buff)
        {
            var def = TowerBuffPool.Get(buff.Type);
            var existing = alive.Buffs.FirstOrDefault(b => b.Type == buff.Type);
            int currentStacks = existing?.Stacks ?? 0;
            int roomLeft = Math.Max(0, def.MaxStacks - currentStacks);
            int applied = Math.Min(buff.Stacks, roomLeft);
            int overflow = buff.Stacks - applied;

            if (applied > 0)
            {
                if (existing != null) existing.Stacks += applied;
                else alive.Buffs.Add(new TowerBuff { Type = buff.Type, Stacks = applied });
            }

            for (int i = 0; i < overflow; i++)
                AddBuff(alive, RollRandomBuff());
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
                .WithDescription($"**{names}** begin their ascent.\n\nChoose your starting buff before the climb begins...")
                .WithColor(Color.DarkRed)
                .Build();
        }

        private async Task PostStartBuffChoiceAsync(TowerSession session, IThreadChannel thread)
        {
            foreach (var p in session.Participants)
            {
                p.CheckpointDone = false;
                p.PendingBuffChoices = RollThreeBuffs();
            }

            foreach (var p in session.Participants)
            {
                var components = BuildBuffPickComponents(session.SessionId, p.DiscordId, p.PendingBuffChoices!);
                var desc = string.Join("\n", p.PendingBuffChoices!.Select(b =>
                {
                    var def = TowerBuffPool.Get(b);
                    return $"{def.Emoji} **{def.Name}** — {def.Description}";
                }));

                string mention = session.Mode == TowerMode.Duo ? $"<@{p.DiscordId}> — " : "";
                await thread.SendMessageAsync(
                    text: $"{mention}✨ Choose your starting buff:",
                    embed: new EmbedBuilder()
                        .WithTitle("✨ Choose a Starting Buff")
                        .WithDescription(desc)
                        .WithColor(Color.Blue)
                        .Build(),
                    components: components);
            }
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
                TowerFloorEventType.Merchant     => "🛒",
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
                string next = eventType == TowerFloorEventType.Merchant
                    ? "🛒 Shop now — the climb resumes when everyone clicks Move On"
                    : nextIsBoss ? "⚠️ Boss next!" : nextIsCheckpoint ? "🏁 Checkpoint next!" : $"Next floor in {FloorIntervalSeconds}s";
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
                string scavengeLabel = p.HasScavenged ? "💰 Scavenge (used)" : $"💰 Scavenge ({session.Floor * 10}g)";

                builder.WithButton(scavengeLabel, $"tower_cp:{session.SessionId}:{p.DiscordId}:scavenge", ButtonStyle.Secondary, row: row, disabled: p.HasScavenged);
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

            p.Debuffs.RemoveAt(index);
            p.DebuffRemovesRemaining--;
            p.CheckpointDone = true;

            await TryResumeIfAllDoneAsync(session);
            return (true, $"🗑️ **{def.Emoji} {def.Name}** fully cleansed. Removes left: **{p.DebuffRemovesRemaining}**");
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
            foreach (var p in session.Participants.Concat(session.FallenParticipants))
                _playerSession.Remove(p.DiscordId);
            _sessions.Remove(sessionId);
        }
    }
}

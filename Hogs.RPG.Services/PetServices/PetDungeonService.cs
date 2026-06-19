using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities.DungeonObjects;
using Hogs.RPG.Core.Entities.PetObjects;
using Hogs.RPG.Core.GameData.Achievements;
using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AchievementServices;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.RelicServices;
using Microsoft.Extensions.DependencyInjection;

namespace Hogs.RPG.Services.DungeonServices
{
    public class PetDungeonService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;
        private readonly IMessageChannel _announcementChannel;
        private readonly Random _random = new();

        private readonly Dictionary<ulong, ActiveDungeon> _active = new();
        private readonly Dictionary<ulong, DateTime> _lastAction = new();
        private readonly Dictionary<ulong, (ulong messageId, ulong channelId)> _messages = new();

        public PetDungeonService(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;
            _announcementChannel = client.GetChannel(1485357755433750549) as IMessageChannel;
        }

        // =========================
        // START
        // =========================
        public async Task<DungeonResult> StartPetDungeonAsync(ulong userId, string dungeonId)
        {
            if (_active.ContainsKey(userId))
                return Simple("❌ You are already in a dungeon. Finish it first.");

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();

            var player = await playerRepo.GetByDiscordIdAsync(userId);
            if (player == null)
                return Simple("Use /startadventure first.");

            // =========================
            // 🏆 ACHIEVEMENT-BASED COOLDOWN — persisted to DB
            // =========================
            if (player.LastPetDungeonAt.HasValue)
            {
                var bonus = AchievementMilestones.GetBonus(player.AchievementCount);
                int cooldownMins = bonus.DungeonCooldownMinutes;
                var elapsed = DateTime.UtcNow - player.LastPetDungeonAt.Value;

                if (elapsed.TotalMinutes < cooldownMins)
                {
                    var remaining = TimeSpan.FromMinutes(cooldownMins) - elapsed;
                    int h = (int)remaining.TotalHours;
                    int m = remaining.Minutes;
                    string timeText = h > 0 ? $"{h}h {m}m" : $"{m}m";
                    return Simple($"⏳ You must wait **{timeText}** before entering another dungeon.");
                }
            }

            if (!PetDungeonRegistry.All.TryGetValue(dungeonId, out var dungeon))
                return Simple("❌ Pet dungeon not found.");

            if (player.Level < dungeon.RequiredLevel)
                return Simple($"❌ You must be level **{dungeon.RequiredLevel}** to enter {dungeon.Name}.");

            var (attack, defense, health) = statService.CalculateStats(player);

            var session = new ActiveDungeon
            {
                PlayerId = userId,
                DungeonId = dungeonId,
                Floor = 1,
                Attack = attack,
                Defense = defense,
                MaxHealth = health,
                PlayerHealth = health,
                EnemyMaxHealth = dungeon.BaseEnemyHealth,
                EnemyHealth = dungeon.BaseEnemyHealth,
                IsBoss = false,
                CurrentImageUrl = null
            };

            _active[userId] = session;

            // Stamp cooldown immediately on entry
            player.LastPetDungeonAt = DateTime.UtcNow;
            await playerRepo.UpdatePlayerAsync(player);

            if (_announcementChannel != null)
                await _announcementChannel.SendMessageAsync($"🐾 <@{userId}> entered **{dungeon.Name}**");

            return Wrap(BuildEmbed(session, dungeon, "🐾 You enter the pet dungeon..."));
        }

        // =========================
        // ATTACK
        // =========================
        public async Task<DungeonResult> AttackAsync(ulong userId)
        {
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a pet dungeon.");

            if (IsOnCooldown(userId, 1, out int remaining))
                return Wrap(BuildEmbed(session, GetDungeon(session), $"⏳ Wait {remaining}s before acting again."));

            if (!PetDungeonRegistry.All.TryGetValue(session.DungeonId, out var dungeon))
                return Simple("❌ Dungeon data missing.");

            if (session.StunnedThisTurn)
            {
                session.StunnedThisTurn = false;
                _lastAction[userId] = DateTime.UtcNow;
                return Wrap(BuildEmbed(session, GetDungeon(session), "😵 **You are stunned and cannot act this turn!**"));
            }

            using var scope = _scopeFactory.CreateScope();
            var petService = scope.ServiceProvider.GetRequiredService<PetService>();
            var petPassiveService = scope.ServiceProvider.GetRequiredService<PetPassiveService>();
            var relicService = scope.ServiceProvider.GetRequiredService<RelicService>();

            var pet = await petService.GetEquippedPetAsync(userId);
            PetDefinition petDef = null;
            if (pet != null)
                PetRegistry.All.TryGetValue(pet.PetId, out petDef);

            int enemyDefense = session.IsBoss ? dungeon.Boss.Defense : 10;

            int playerDamage = (int)(session.Attack * (100.0 / (100 + enemyDefense)));
            playerDamage = Math.Max(1, playerDamage);

            if (session.ToxicAttackReductionTurnsRemaining > 0)
                playerDamage = (int)(playerDamage * 0.80);

            var (modifiedPlayerDamage, outgoingTriggerText) = petPassiveService.ModifyOutgoingDamage(playerDamage, pet, petDef, session.EnemyHealth, session.EnemyMaxHealth);
            playerDamage = modifiedPlayerDamage;

            var relicBonuses = await relicService.GetRelicBonusesAsync(userId);

            // =========================
            // 💎 RELIC: CONSECUTIVE HIT BONUS
            // =========================
            if (relicBonuses.ConsecutiveHitBonusPercent > 0)
            {
                session.ConsecutiveHits++;
                playerDamage = (int)(playerDamage * (1f + relicBonuses.ConsecutiveHitBonusPercent * session.ConsecutiveHits));
            }
            else
            {
                session.ConsecutiveHits = 0;
            }

            // =========================
            // 💎 RELIC: EXECUTIONER
            // =========================
            double enemyHpPercent = (double)session.EnemyHealth / session.EnemyMaxHealth;
            if (enemyHpPercent < 0.50 && relicBonuses.ExecutionerBonusPercent > 0)
                playerDamage = (int)(playerDamage * (1f + relicBonuses.ExecutionerBonusPercent));

            session.EnemyHealth = Math.Max(0, session.EnemyHealth - playerDamage);
            var text = $"⚔ You deal {playerDamage} damage!\n";

            if (!string.IsNullOrEmpty(outgoingTriggerText))
                text += outgoingTriggerText + "\n";

            // =========================
            // 💎 RELIC: LIFESTEAL
            // =========================
            int heal = petPassiveService.ApplyOnHitEffects(playerDamage, null, pet);
            if (relicBonuses.LifeStealPercent > 0)
                heal += (int)(playerDamage * relicBonuses.LifeStealPercent);

            if (heal > 0)
            {
                session.PlayerHealth = Math.Min(session.MaxHealth, session.PlayerHealth + heal);
                text += $"🩸 Lifesteal heals you for {heal}!\n";
            }

            if (session.EnemyHealth <= 0)
            {
                _lastAction[userId] = DateTime.UtcNow;
                return await NextFloor(userId, session, text);
            }

            int enemyAttack = session.IsBoss
                ? dungeon.Boss.Attack
                : dungeon.BaseEnemyAttack + (session.Floor * dungeon.EnemyAttackScaling);

            int enemyDamage = (int)(enemyAttack * (100.0 / (100 + session.Defense)));
            enemyDamage = Math.Max(5, enemyDamage);

            var (modifiedEnemyDamage, shieldTriggerText) = petPassiveService.ModifyIncomingDamage(enemyDamage, pet);
            enemyDamage = modifiedEnemyDamage;

            string behaviorText = null;
            if (session.IsBoss)
                behaviorText = HandleBossBehavior(session, dungeon.Boss, ref enemyDamage);

            if (_random.Next(0, 100) < 10)
            {
                enemyDamage = (int)(enemyDamage * 2);
                behaviorText = string.IsNullOrEmpty(behaviorText) ? "💢 Critical hit!" : behaviorText + "\n💢 Critical hit!";
            }

            session.PlayerHealth = Math.Max(0, session.PlayerHealth - enemyDamage);
            text += $"👹 Enemy hits you for {enemyDamage}!";

            if (!string.IsNullOrEmpty(shieldTriggerText))
                text += "\n" + shieldTriggerText;

            int reflect = petPassiveService.ApplyOnHitTaken(enemyDamage, pet);
            if (reflect > 0)
            {
                session.EnemyHealth = Math.Max(0, session.EnemyHealth - reflect);
                text += $"\n🌵 Thorns reflects {reflect} damage!";
            }

            if (!string.IsNullOrEmpty(behaviorText))
                text += "\n" + behaviorText;

            if (session.PlayerHealth <= 0)
            {
                _lastAction[userId] = DateTime.UtcNow;
                return await HandleDeath(userId);
            }

            _lastAction[userId] = DateTime.UtcNow;
            return Wrap(BuildEmbed(session, dungeon, text));
        }

        // =========================
        // HEAL
        // =========================
        public async Task<DungeonResult> HealAsync(ulong userId)
        {
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a pet dungeon.");

            if (IsOnCooldown(userId, 2, out int remaining))
                return Wrap(BuildEmbed(session, GetDungeon(session), $"⏳ Wait {remaining}s before acting again."));

            if (session.PlayerHealth == session.MaxHealth)
                return Wrap(BuildEmbed(session, GetDungeon(session), "❤️ Already at full health."));

            using var scope = _scopeFactory.CreateScope();
            var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();
            var relicService = scope.ServiceProvider.GetRequiredService<RelicService>();

            var inventory = await inventoryService.GetInventoryAsync(userId);
            var potion = inventory.Find(i => i.ItemId == "health_potion");

            if (potion == null || potion.Quantity <= 0)
                return Wrap(BuildEmbed(session, GetDungeon(session), "❌ You have no health potions."));

            var relicBonuses = await relicService.GetRelicBonusesAsync(userId);

            // =========================
            // 💎 RELIC: CHANCE TO SAVE POTION
            // =========================
            bool potionConsumed = true;
            if (relicBonuses.ChanceToSavePotion > 0 && new Random().NextDouble() < relicBonuses.ChanceToSavePotion)
                potionConsumed = false;

            if (potionConsumed)
                await inventoryService.TakeItemAsync(userId, "health_potion", 1);

            // =========================
            // 💎 RELIC: INCREASED HEAL PERCENT
            // =========================
            int healAmount = session.MaxHealth;
            if (relicBonuses.IncreasedHealPercent > 0)
                healAmount = (int)(healAmount * (1f + relicBonuses.IncreasedHealPercent));

            session.PlayerHealth = Math.Min(session.MaxHealth, session.PlayerHealth + healAmount);

            // Reset consecutive hits on heal
            session.ConsecutiveHits = 0;

            string savedText = potionConsumed ? "" : " *(potion saved!)*";
            _lastAction[userId] = DateTime.UtcNow;
            return Wrap(BuildEmbed(session, GetDungeon(session), $"🧪 You healed to full!{savedText}"));
        }

        // =========================
        // FLEE
        // =========================
        public async Task<DungeonResult> FleeAsync(ulong userId)
        {
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a pet dungeon.");

            _active.Remove(userId);

            var dungeon = GetDungeonById(session.DungeonId);

            if (_announcementChannel != null)
                await _announcementChannel.SendMessageAsync($"🏃 <@{userId}> fled **{dungeon?.Name ?? "a pet dungeon"}**");

            return new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithDescription("🏃 You fled the pet dungeon.")
                    .WithColor(Color.DarkGrey)
                    .Build(),
                IsFinished = true
            };
        }

        // =========================
        // NEXT FLOOR
        // =========================
        private async Task<DungeonResult> NextFloor(ulong userId, ActiveDungeon session, string text)
        {
            session.Floor++;
            var dungeon = GetDungeon(session);

            if (session.Floor > dungeon.Floors)
                return await Complete(userId, session);

            if (session.Floor == dungeon.Floors)
            {
                var boss = dungeon.Boss;
                session.EnemyMaxHealth = boss.MaxHealth;
                session.EnemyHealth = boss.MaxHealth;
                session.IsBoss = true;
                session.CurrentImageUrl = boss.ImageUrl;
                return Wrap(BuildEmbed(session, dungeon, text + "\n🔥 **The boss appears!**"));
            }

            session.EnemyMaxHealth = dungeon.BaseEnemyHealth + (session.Floor * dungeon.EnemyHealthScaling);
            session.EnemyHealth = session.EnemyMaxHealth;

            return Wrap(BuildEmbed(session, dungeon, text + "\n➡ You move deeper..."));
        }

        // =========================
        // COMPLETE
        // =========================
        private async Task<DungeonResult> Complete(ulong userId, ActiveDungeon session)
        {
            _active.Remove(userId);

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var petService = scope.ServiceProvider.GetRequiredService<PetService>();
            var levelService = scope.ServiceProvider.GetRequiredService<LevelService>();
            var relicService = scope.ServiceProvider.GetRequiredService<RelicService>();

            var player = await playerRepo.GetByDiscordIdAsync(userId);
            var dungeon = GetDungeonById(session.DungeonId);
            var relicBonuses = await relicService.GetRelicBonusesAsync(userId);

            int gold = (int)(250 * (1f + relicBonuses.BonusGoldPercent));
            int xp = (int)(500 * (1f + relicBonuses.BonusPlayerXpPercent));
            int petXp = (int)(250 * (1f + relicBonuses.BonusPetXpPercent));

            player.Gold += gold;
            player.XP += xp;
            player.DungeonRunsCompleted++;

            var (petLeveledUp, petNewLevel) = await petService.AddXPAsync(userId, petXp);
            string petLevelMessage = "";
            if (petLeveledUp)
            {
                var equippedPet = await petService.GetEquippedPetAsync(userId);
                if (equippedPet != null && PetRegistry.All.TryGetValue(equippedPet.PetId, out var petDef))
                    petLevelMessage = $"\n\n🐾 **{petDef.Icon} {petDef.Name}** leveled up! Now **Level {petNewLevel}** 🎉";
            }

            var (levelMessage, _) = levelService.CheckLevelUp(player);

            string petDropText = "";
            var petDrops = dungeon?.Boss?.PetDrops;

            if (petDrops != null)
            {
                foreach (var drop in petDrops)
                {
                    int roll = _random.Next(1, 101);
                    if (roll <= drop.ChancePercent)
                    {
                        bool isCompanion = false;

                        if (drop.PetId == "bandit" && !player.HasAlchemistPet)
                        {
                            player.HasAlchemistPet = true;
                            player.AlchemistCompanionUnlocked = true;
                            petDropText += "\n🎉 **Bandit, the Workshop Assistant** is now your Alchemist Companion! (+10% Alchemy XP)";
                            isCompanion = true;
                        }
                        else if (drop.PetId == "ravens_of_odin" && !player.HasGatherPet)
                        {
                            player.HasGatherPet = true;
                            player.GatherCompanionUnlocked = true;
                            petDropText += "\n🎉 **The Ravens of Odin** are now your Gather Companions! (+15% yield, +50 max energy)";
                            isCompanion = true;
                        }
                        else if (drop.PetId == "furny_da_clanka" && !player.HasBlacksmithPet)
                        {
                            player.HasBlacksmithPet = true;
                            player.BlacksmithCompanionUnlocked = true;
                            petDropText += "\n🎉 **Furny da Clanka** is now your Blacksmith Companion! (+10% Smithing XP)";
                            isCompanion = true;
                        }

                        if (!isCompanion)
                        {
                            await petService.GivePetAsync(userId, drop.PetId);
                            petDropText += PetRegistry.All.TryGetValue(drop.PetId, out var droppedPet)
                                ? $"\n🎉 **{droppedPet.Icon} {droppedPet.Name}** joined your party!"
                                : $"\n🎉 **New pet acquired!**";
                        }
                    }
                }
            }

            await playerRepo.UpdatePlayerAsync(player);

            var achievementService = scope.ServiceProvider.GetRequiredService<AchievementService>();
            await achievementService.CheckAndAwardAsync(userId);

            if (_announcementChannel != null)
                await _announcementChannel.SendMessageAsync(
                    $"🐾 <@{userId}> cleared **{dungeon?.Name ?? "a pet dungeon"}**!\n" +
                    $"+{gold} Gold\n+{xp} XP\n+{petXp} Pet XP{petDropText}{petLevelMessage}{levelMessage}");

            return new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithTitle("🐾 Pet Dungeon Complete!")
                    .WithDescription($"+{gold} Gold\n+{xp} XP\n+{petXp} Pet XP{petDropText}{petLevelMessage}{levelMessage}")
                    .WithColor(Color.Green)
                    .Build(),
                IsFinished = true
            };
        }

        // =========================
        // DEATH
        // =========================
        private async Task<DungeonResult> HandleDeath(ulong userId)
        {
            _active.Remove(userId);

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();

            var player = await playerRepo.GetByDiscordIdAsync(userId);

            player.Gold = Math.Max(0, player.Gold - 250);
            player.XP = Math.Max(0, (int)(player.XP * 0.8f));
            player.Deaths++;

            var (_, _, maxHealth) = statService.CalculateStats(player);
            player.Health = maxHealth;

            await playerRepo.UpdatePlayerAsync(player);

            if (_announcementChannel != null)
                await _announcementChannel.SendMessageAsync($"💀 <@{userId}> died in a **pet dungeon**");

            var achievementService = scope.ServiceProvider.GetRequiredService<AchievementService>();
            await achievementService.CheckAndAwardAsync(userId);

            return new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithTitle("💀 You Died")
                    .WithDescription("Lost 250 gold and 20% of your current XP.\n❤️ Restored to full health.")
                    .WithColor(Color.DarkRed)
                    .Build(),
                IsFinished = true
            };
        }

        // =========================
        // RESET COOLDOWN (shop purchase)
        // =========================
        public async Task ResetPetDungeonCooldownAsync(ulong userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var player = await playerRepo.GetByDiscordIdAsync(userId);
            if (player == null) return;
            player.LastPetDungeonAt = null;
            await playerRepo.UpdatePlayerAsync(player);
        }

        // =========================
        // MESSAGE TRACKING
        // =========================
        public void SetDungeonMessage(ulong userId, ulong messageId, ulong channelId)
            => _messages[userId] = (messageId, channelId);

        public async Task UpdateDungeonMessageAsync(ulong userId, DungeonResult result)
        {
            if (!_messages.TryGetValue(userId, out var info)) return;
            var (messageId, channelId) = info;

            var channel = _client.GetChannel(channelId) as IMessageChannel;

            if (channel == null)
            {
                var user = _client.GetUser(userId);
                if (user != null)
                    channel = await user.CreateDMChannelAsync();
            }

            if (channel == null) return;

            var msg = await channel.GetMessageAsync(messageId) as IUserMessage;
            if (msg == null) return;

            await msg.ModifyAsync(m =>
            {
                m.Embed = result.Embed;
                m.Components = result.IsFinished
                    ? new ComponentBuilder().Build()
                    : new ComponentBuilder()
                        .WithButton("⚔ Attack", "petdungeon_attack", ButtonStyle.Danger)
                        .WithButton("🧪 Heal", "petdungeon_heal", ButtonStyle.Success)
                        .WithButton("🏃 Flee", "petdungeon_flee", ButtonStyle.Secondary)
                        .Build();
            });
        }

        // =========================
        // BOSS BEHAVIORS
        // =========================
        private string HandleBossBehavior(ActiveDungeon session, DungeonBossDefinition boss, ref int enemyDamage)
        {
            if (boss == null || string.IsNullOrEmpty(boss.BehaviorId)) return null;

            switch (boss.BehaviorId)
            {
                case "rage":
                    if (!session.RageTriggered && session.EnemyHealth <= boss.MaxHealth * 0.3)
                    {
                        session.RageTriggered = true;
                        int heal = (int)(boss.MaxHealth * 0.50);
                        session.EnemyHealth += heal;
                        return $"🔥 **{boss.Name} enters RAGE! (+{heal} HP)**";
                    }
                    if (session.RageTriggered) enemyDamage *= 2;
                    return null;

                case "lifesteal_smash":
                    if (_random.Next(0, 100) < 25)
                    {
                        enemyDamage = (int)(enemyDamage * 1.8);
                        int heal = enemyDamage;
                        session.EnemyHealth = Math.Min(session.EnemyMaxHealth, session.EnemyHealth + heal);
                        return $"🩸 **{boss.Name} absorbs {heal} HP!**";
                    }
                    return null;

                case "crushing_blow":
                    if (_random.Next(0, 100) < 20)
                    {
                        int reducedDef = session.Defense / 2;
                        enemyDamage = (int)(boss.Attack * (100.0 / (100 + reducedDef)));
                        enemyDamage = (int)(enemyDamage * 1.5);
                        return $"💥 **{boss.Name} shatters your defenses!**";
                    }
                    return null;

                case "toxic_concoction":
                    if (!session.ToxicConcoctionTriggered && session.EnemyHealth <= boss.MaxHealth * 0.5)
                    {
                        session.ToxicConcoctionTriggered = true;
                        session.ToxicPoisonTurnsRemaining = 3;
                        session.ToxicAttackReductionTurnsRemaining = 2;
                        return $"🧪 **{boss.Name} hurls a volatile brew! You are poisoned for 3 turns and weakened for 2!**";
                    }
                    if (session.ToxicPoisonTurnsRemaining > 0 || session.ToxicAttackReductionTurnsRemaining > 0)
                    {
                        string behaviorMsg = null;
                        if (session.ToxicPoisonTurnsRemaining > 0)
                        {
                            int poisonDmg = 40;
                            enemyDamage += poisonDmg;
                            session.ToxicPoisonTurnsRemaining--;
                            string suffix = session.ToxicPoisonTurnsRemaining > 0
                                ? $" ({session.ToxicPoisonTurnsRemaining} turns remaining)"
                                : " (poison fades...)";
                            behaviorMsg = $"☠️ **The toxic brew burns for {poisonDmg} extra damage!{suffix}**";
                        }
                        if (session.ToxicAttackReductionTurnsRemaining > 0)
                            session.ToxicAttackReductionTurnsRemaining--;
                        return behaviorMsg;
                    }
                    return null;

                case "raven_swarm":
                    if (_random.Next(0, 100) < 25)
                    {
                        int singleHit = (int)(enemyDamage * 0.45);
                        singleHit = Math.Max(5, singleHit);
                        enemyDamage = singleHit * 3;
                        return $"🪶 **The Ravens of Odin split into a swarm — 3 strikes of {singleHit} each!**";
                    }
                    return null;

                case "forge_slam":
                    if (!session.ForgeFuryTriggered && session.EnemyHealth <= boss.MaxHealth * 0.4)
                    {
                        session.ForgeFuryTriggered = true;
                        return $"🔥 **{boss.Name} enters FORGE FURY! Every strike burns hotter!**";
                    }
                    if (session.ForgeFuryTriggered)
                    {
                        enemyDamage = (int)(enemyDamage * 1.3);
                        if (_random.Next(0, 100) < 15)
                        {
                            session.StunnedThisTurn = true;
                            return $"🔨 **{boss.Name} lands a stunning blow — you are STUNNED!**";
                        }
                    }
                    return null;
            }

            return null;
        }

        // =========================
        // HELPERS
        // =========================
        private DungeonDefinition GetDungeon(ActiveDungeon session) => GetDungeonById(session.DungeonId);
        private DungeonDefinition GetDungeonById(string id)
        {
            PetDungeonRegistry.All.TryGetValue(id, out var d);
            return d;
        }

        private bool IsOnCooldown(ulong userId, int seconds, out int remaining)
        {
            remaining = 0;
            if (!_lastAction.TryGetValue(userId, out var last)) return false;
            var diff = DateTime.UtcNow - last;
            if (diff.TotalSeconds >= seconds) return false;
            remaining = seconds - (int)diff.TotalSeconds;
            return true;
        }

        private DungeonResult Wrap(Embed embed) => new() { Embed = embed };
        private DungeonResult Simple(string text) => new() { Embed = new EmbedBuilder().WithDescription(text).Build() };

        private Embed BuildEmbed(ActiveDungeon s, DungeonDefinition dungeon, string text)
        {
            int filled = Math.Max(0, Math.Min(10, (int)((double)s.PlayerHealth / s.MaxHealth * 10)));
            string playerBar = new string('█', filled) + new string('░', 10 - filled);
            int eFilled = Math.Max(0, Math.Min(10, (int)((double)s.EnemyHealth / s.EnemyMaxHealth * 10)));
            string enemyBar = new string('█', eFilled) + new string('░', 10 - eFilled);

            var embed = new EmbedBuilder()
                .WithTitle($"🐾 {dungeon?.Name ?? "Pet Dungeon"} — Floor {s.Floor}")
                .WithDescription(text)
                .AddField("❤️ Player HP", $"{playerBar}\n{s.PlayerHealth}/{s.MaxHealth}", true)
                .AddField("👹 Enemy HP", $"{enemyBar}\n{s.EnemyHealth}/{s.EnemyMaxHealth}", true)
                .WithColor(s.IsBoss ? Color.Purple : Color.Green);

            if (!string.IsNullOrEmpty(s.CurrentImageUrl))
                embed.WithImageUrl(s.CurrentImageUrl);

            return embed.Build();
        }
    }
}
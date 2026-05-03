using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
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
        private readonly Dictionary<ulong, DateTime> _lastDungeonRun = new();
        private readonly Dictionary<ulong, DateTime> _lastAction = new();
        private readonly Dictionary<ulong, (ulong messageId, ulong channelId)> _messages = new();

        private const int CooldownMinutes = 60;

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

            if (_lastDungeonRun.TryGetValue(userId, out var lastRun))
            {
                var elapsed = DateTime.UtcNow - lastRun;
                if (elapsed.TotalMinutes < CooldownMinutes)
                {
                    int remaining = CooldownMinutes - (int)elapsed.TotalMinutes;
                    return Simple($"⏳ You must wait **{remaining}m** before entering another dungeon.");
                }
            }

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();

            var player = await playerRepo.GetByDiscordIdAsync(userId);
            if (player == null)
                return Simple("Use /startadventure first.");

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
            _lastDungeonRun[userId] = DateTime.UtcNow;

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

            // Player attacks
            int playerDamage = (int)(session.Attack * (100.0 / (100 + 10)));
            playerDamage = Math.Max(1, playerDamage);
            session.EnemyHealth = Math.Max(0, session.EnemyHealth - playerDamage);

            var text = $"⚔ You deal {playerDamage} damage!\n";

            if (session.EnemyHealth <= 0)
            {
                _lastAction[userId] = DateTime.UtcNow;
                return await NextFloor(userId, session, text);
            }

            // Enemy attacks back
            int enemyAttack = session.IsBoss
                ? dungeon.Boss.Attack
                : dungeon.BaseEnemyAttack + (session.Floor * dungeon.EnemyAttackScaling);

            int enemyDamage = (int)(enemyAttack * (100.0 / (100 + session.Defense)));
            enemyDamage = Math.Max(5, enemyDamage);

            // Boss behaviors
            string behaviorText = null;
            if (session.IsBoss)
                behaviorText = HandleBossBehavior(session, dungeon.Boss, ref enemyDamage);

            // Crit
            if (_random.Next(0, 100) < 10)
            {
                enemyDamage = (int)(enemyDamage * 2);
                behaviorText = string.IsNullOrEmpty(behaviorText) ? "💢 Critical hit!" : behaviorText + "\n💢 Critical hit!";
            }

            session.PlayerHealth = Math.Max(0, session.PlayerHealth - enemyDamage);
            text += $"👹 Enemy hits you for {enemyDamage}!";

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

            var inventory = await inventoryService.GetInventoryAsync(userId);
            var potion = inventory.Find(i => i.ItemId == "health_potion");

            if (potion == null || potion.Quantity <= 0)
                return Wrap(BuildEmbed(session, GetDungeon(session), "❌ You have no health potions."));

            await inventoryService.TakeItemAsync(userId, "health_potion", 1);
            session.PlayerHealth = session.MaxHealth;

            _lastAction[userId] = DateTime.UtcNow;
            return Wrap(BuildEmbed(session, GetDungeon(session), "🧪 You healed to full!"));
        }

        // =========================
        // FLEE
        // =========================
        public async Task<DungeonResult> FleeAsync(ulong userId)
        {
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a pet dungeon.");

            _active.Remove(userId);
            _lastDungeonRun[userId] = DateTime.UtcNow;

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

            var player = await playerRepo.GetByDiscordIdAsync(userId);
            var dungeon = GetDungeonById(session.DungeonId);

            player.Gold += 250;
            player.XP += 1000;
            player.DungeonRunsCompleted++;

            // Pet XP for equipped pet
            var (petLeveledUp, petNewLevel) = await petService.AddXPAsync(userId, 50);
            string petLevelMessage = "";
            if (petLeveledUp)
            {
                var equippedPet = await petService.GetEquippedPetAsync(userId);
                if (equippedPet != null && PetRegistry.All.TryGetValue(equippedPet.PetId, out var petDef))
                    petLevelMessage = $"\n\n🐾 **{petDef.Icon} {petDef.Name}** leveled up! Now **Level {petNewLevel}** 🎉";
            }

            var (levelMessage, _) = levelService.CheckLevelUp(player);

            // Pet drop check
            string petDropText = "";
            var petDrops = dungeon?.Boss?.PetDrops;

            if (petDrops != null)
            {
                foreach (var drop in petDrops)
                {
                    int roll = _random.Next(1, 101);
                    if (roll <= drop.ChancePercent)
                    {
                        await petService.GivePetAsync(userId, drop.PetId);

                        if (PetRegistry.All.TryGetValue(drop.PetId, out var droppedPet))
                            petDropText += $"\n🎉 **{droppedPet.Icon} {droppedPet.Name}** joined your party!";
                        else
                            petDropText += $"\n🎉 **New pet acquired!**";
                    }
                }
            }

            await playerRepo.UpdatePlayerAsync(player);

            if (_announcementChannel != null)
                await _announcementChannel.SendMessageAsync(
                    $"🐾 <@{userId}> cleared **{dungeon?.Name ?? "a pet dungeon"}**!\n" +
                    $"+250 Gold\n+1000 XP{petDropText}{petLevelMessage}{levelMessage}"
                );

            return new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithTitle("🐾 Pet Dungeon Complete!")
                    .WithDescription($"+250 Gold\n+1000 XP\n+50 Pet XP{petDropText}{petLevelMessage}{levelMessage}")
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
            _lastDungeonRun[userId] = DateTime.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();

            var player = await playerRepo.GetByDiscordIdAsync(userId);

            var dungeon = GetDungeonById(player != null ? _active.ContainsKey(0) ? "" : "" : "");

            player.Gold = Math.Max(0, player.Gold - 250);
            player.XP = Math.Max(0, (int)(player.XP * 0.8f));

            var (_, _, maxHealth) = statService.CalculateStats(player);
            player.Health = maxHealth;

            await playerRepo.UpdatePlayerAsync(player);

            if (_announcementChannel != null)
                await _announcementChannel.SendMessageAsync($"💀 <@{userId}> died in a **pet dungeon**");

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
        // MESSAGE TRACKING
        // =========================
        public void SetDungeonMessage(ulong userId, ulong messageId, ulong channelId)
            => _messages[userId] = (messageId, channelId);

        public async Task UpdateDungeonMessageAsync(ulong userId, DungeonResult result)
        {
            if (!_messages.TryGetValue(userId, out var info)) return;
            var (messageId, channelId) = info;

            // DM channels aren't cached — fall back to creating the DM channel
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

                case "defensive_cloud":
                    if (_random.Next(0, 100) < 30)
                    {
                        session.CloudActive = true;
                        return $"🌫️ **{boss.Name} vanishes into a toxic mist!**";
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
            int filled = (int)((double)s.PlayerHealth / s.MaxHealth * 10);
            string playerBar = new string('█', filled) + new string('░', 10 - filled);
            int eFilled = (int)((double)s.EnemyHealth / s.EnemyMaxHealth * 10);
            string enemyBar = new string('█', eFilled) + new string('░', 10 - eFilled);

            return new EmbedBuilder()
                .WithTitle($"🐾 {dungeon?.Name ?? "Pet Dungeon"} — Floor {s.Floor}")
                .WithDescription(text)
                .AddField("❤️ Player HP", $"{playerBar}\n{s.PlayerHealth}/{s.MaxHealth}", true)
                .AddField("👹 Enemy HP", $"{enemyBar}\n{s.EnemyHealth}/{s.EnemyMaxHealth}", true)
                .WithColor(s.IsBoss ? Color.Purple : Color.Green)
                .Build();
        }
    }
}

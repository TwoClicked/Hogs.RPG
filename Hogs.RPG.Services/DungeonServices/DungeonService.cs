using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.Game
{
    public class DungeonService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;
        private readonly IMessageChannel _announcementChannel;

        private readonly Dictionary<ulong, (ulong messageId, ulong channelId)> _lastMessages = new();
        private readonly Dictionary<ulong, DateTime> _lastAction = new();
        private readonly Dictionary<ulong, DateTime> _lastDungeonRun = new();
        private readonly Dictionary<ulong, ActiveDungeon> _active = new();
        private readonly Random _random = new();

        public DungeonService(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;
            _announcementChannel = client.GetChannel(1485357755433750549) as IMessageChannel;
        }

        // =========================
        // START
        // =========================
        public async Task<DungeonResult> StartDungeonAsync(ulong userId, string dungeonId)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepository = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();

            // Prevent multiple active sessions
            if (_active.ContainsKey(userId))
                return Simple("You are already in a dungeon.");

            // Dungeon cooldown check
            const int COOLDOWN_HOURS = 1;

            if (_lastDungeonRun.TryGetValue(userId, out var lastRun))
            {
                var diff = DateTime.UtcNow - lastRun;

                if (diff.TotalHours < COOLDOWN_HOURS)
                {
                    var remaining = TimeSpan.FromHours(COOLDOWN_HOURS) - diff;

                    var hours = remaining.Hours;
                    var minutes = remaining.Minutes;

                    var timeText = hours > 0
                        ? $"{hours}h {minutes}m"
                        : $"{minutes}m";

                    return Simple($"⏳ You must wait {timeText} before entering another dungeon.");
                }
            }

            var player = await playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return Simple("Use /startadventure first.");

            var dungeon = DungeonRegistry.All[dungeonId];

            if (player.Level < dungeon.RequiredLevel)
                return Simple($"You must be level {dungeon.RequiredLevel}.");

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
            {
                await _announcementChannel.SendMessageAsync(
                    $"🏰 <@{userId}> entered **{dungeon.Name}**"
                );
            }

            return Wrap(BuildEmbed(session, "🏰 You enter the dungeon..."));
        }

        // =========================
        // ATTACK
        // =========================
        public async Task<DungeonResult> AttackAsync(ulong userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var petService = scope.ServiceProvider.GetRequiredService<PetService>();
            var petPassiveService = scope.ServiceProvider.GetRequiredService<PetPassiveService>();

            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a dungeon.");

            var pet = await petService.GetEquippedPetAsync(userId);
            PetDefinition petDef = null;

            if (pet != null)
            {
                PetRegistry.All.TryGetValue(pet.PetId, out petDef);
            }

            if (IsOnCooldown(userId, 1, out int remaining))
            {
                return Wrap(BuildEmbed(session,
                    $"⏳ You must wait {remaining}s before acting again.",
                    session.CurrentImageUrl));
            }

            var dungeon = DungeonRegistry.All[session.DungeonId];

            // =========================
            // ⚔ PLAYER ATTACK
            // =========================
            int enemyDefense = GetEnemyDefense(session);

            if (session.CloudActive)
            {
                enemyDefense *= 9999;
                session.CloudActive = false;
            }

            int playerDamage = (int)(session.Attack * (100.0 / (100 + enemyDefense)));
            playerDamage = Math.Max(1, playerDamage);

            playerDamage = petPassiveService.ModifyOutgoingDamage(
                playerDamage,
                pet,
                petDef,
                session.EnemyHealth,
                session.EnemyMaxHealth
            );

            session.EnemyHealth = Math.Max(0, session.EnemyHealth - playerDamage);

            var text = $"⚔ You deal {playerDamage} damage!\n";

            int heal = petPassiveService.ApplyOnHitEffects(playerDamage, null, pet);
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

            // =========================
            // 👹 ENEMY ATTACK
            // =========================
            int enemyAttack = GetEnemyAttack(session, dungeon);
            int enemyDamage = (int)(enemyAttack * (100.0 / (100 + session.Defense)));
            enemyDamage = Math.Max(5, enemyDamage);

            enemyDamage = petPassiveService.ModifyIncomingDamage(enemyDamage, pet);

            string behaviorText = null;

            if (session.IsBoss)
            {
                behaviorText = HandleBossBehavior(session, dungeon.Boss, ref enemyDamage);
            }

            if (_random.Next(0, 100) < 10)
            {
                enemyDamage = (int)(enemyDamage * 2);

                if (string.IsNullOrEmpty(behaviorText))
                    behaviorText = "💢 Critical hit!";
                else
                    behaviorText += "\n💢 Critical hit!";
            }

            session.PlayerHealth = Math.Max(0, session.PlayerHealth - enemyDamage);

            text += $"👹 Enemy hits you for {enemyDamage}!";

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

            return Wrap(BuildEmbed(session, text, session.CurrentImageUrl));
        }

        // =========================
        // HEAL
        // =========================
        public async Task<DungeonResult> HealAsync(ulong userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();

            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a dungeon.");

            if (IsOnCooldown(userId, 2, out int remaining))
            {
                return Wrap(BuildEmbed(session,
                    $"⏳ You must wait {remaining}s before acting again.",
                    session.CurrentImageUrl));
            }

            if (session.PlayerHealth == session.MaxHealth)
                return Wrap(BuildEmbed(session, "❤️ You are already at full health.", session.CurrentImageUrl));

            var inventory = await inventoryService.GetInventoryAsync(userId);
            var potion = inventory.Find(i => i.ItemId == "health_potion");

            if (potion == null || potion.Quantity <= 0)
                return Wrap(BuildEmbed(session, "❌ You have no health potions.", session.CurrentImageUrl));

            await inventoryService.TakeItemAsync(userId, "health_potion", 1);

            session.PlayerHealth = session.MaxHealth;

            _lastAction[userId] = DateTime.UtcNow;

            return Wrap(BuildEmbed(session, "🧪 You healed to full!", session.CurrentImageUrl));
        }

        // =========================
        // FLEE
        // =========================
        public async Task<DungeonResult> FleeAsync(ulong userId)
        {
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a dungeon.");

            _active.Remove(userId);

            var dungeon = DungeonRegistry.All[session.DungeonId];

            if (_announcementChannel != null)
            {
                await _announcementChannel.SendMessageAsync(
                    $"🏃 <@{userId}> fled **{dungeon.Name}**"
                );
            }

            _lastDungeonRun[userId] = DateTime.UtcNow;

            return new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithDescription("🏃 You fled the dungeon.")
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

            var dungeon = DungeonRegistry.All[session.DungeonId];

            if (session.Floor > dungeon.Floors)
                return await CompleteDungeon(userId, session);

            // BOSS FLOOR
            if (session.Floor == dungeon.Floors)
            {
                var boss = dungeon.Boss;

                session.EnemyMaxHealth = boss.MaxHealth;
                session.EnemyHealth = boss.MaxHealth;
                session.IsBoss = true;
                session.CurrentImageUrl = boss.ImageUrl;

                return Wrap(BuildEmbed(session,
                    text + "\n🔥 **The boss appears!**",
                    session.CurrentImageUrl));
            }

            // NORMAL FLOOR
            session.EnemyMaxHealth =
                dungeon.BaseEnemyHealth +
                (session.Floor * dungeon.EnemyHealthScaling);

            session.EnemyHealth = session.EnemyMaxHealth;

            return Wrap(BuildEmbed(session,
                text + "\n➡ You move deeper...",
                session.CurrentImageUrl));
        }

        // =========================
        // DEATH
        // =========================
        private async Task<DungeonResult> HandleDeath(ulong userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepository = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();

            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a dungeon.");

            _active.Remove(userId);

            var player = await playerRepository.GetByDiscordIdAsync(userId);
            var dungeon = DungeonRegistry.All[session.DungeonId];

            player.Gold = Math.Max(0, player.Gold - 250);
            player.XP = Math.Max(0, (int)(player.XP * 0.8f));

            var (attack, defense, maxHealth) = statService.CalculateStats(player);
            player.Health = maxHealth;

            await playerRepository.UpdatePlayerAsync(player);

            if (_announcementChannel != null)
            {
                await _announcementChannel.SendMessageAsync(
                    $"💀 <@{userId}> died in **{dungeon.Name}**"
                );
            }

            _lastDungeonRun[userId] = DateTime.UtcNow;

            return new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithTitle("💀 You Died")
                    .WithDescription("Lost 250 gold and 20% of your current XP.\n❤️ You were restored to full health.")
                    .WithColor(Color.DarkRed)
                    .Build(),
                IsFinished = true
            };
        }

        // =========================
        // COMPLETE
        // =========================
        private async Task<DungeonResult> CompleteDungeon(ulong userId, ActiveDungeon session)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepository = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();
            var petService = scope.ServiceProvider.GetRequiredService<PetService>();
            var levelService = scope.ServiceProvider.GetRequiredService<LevelService>();

            _active.Remove(userId);

            var player = await playerRepository.GetByDiscordIdAsync(userId);
            var dungeon = DungeonRegistry.All[session.DungeonId];

            player.Gold += 250;
            player.XP += 1000;

            // =========================
            // 🐾 PET XP (DUNGEON)
            // =========================
            var (petLeveledUp, petNewLevel) = await petService.AddXPAsync(userId, 50);

            string petLevelMessage = "";

            if (petLeveledUp)
            {
                var pet = await petService.GetEquippedPetAsync(userId);

                if (pet != null && PetRegistry.All.TryGetValue(pet.PetId, out var petDef))
                {
                    petLevelMessage = $"\n\n🐾 **{petDef.Icon} {petDef.Name}** leveled up! It is now **Level {petNewLevel}** 🎉";
                }
            }

            var (levelMessage, levelsGained) = levelService.CheckLevelUp(player);

            var dropText = "";
            var drops = dungeon.Boss?.Drops;
            player.DungeonRunsCompleted++;

            if (drops != null)
            {
                foreach (var drop in drops)
                {
                    int roll = _random.Next(1, 101);

                    if (roll <= drop.ChancePercent)
                    {
                        await inventoryService.GiveItemAsync(userId, drop.ItemId, 1);

                        if (InventoryItemDefinitions.All.TryGetValue(drop.ItemId, out var item))
                        {
                            dropText += $"\n🎁 **{item.Name}**";
                        }
                        else
                        {
                            dropText += $"\n🎁 **{drop.ItemId.Replace("_", " ")}**";
                        }
                    }
                }
            }

            await playerRepository.UpdatePlayerAsync(player);

            var result = new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithTitle("🏆 Dungeon Complete")
                    .WithDescription($"+250 Gold\n+1000 XP and 50 pet XP{dropText}{levelMessage}{petLevelMessage}")
                    .WithColor(Color.Gold)
                    .Build(),
                IsFinished = true
            };

            if (_announcementChannel != null)
            {
                await _announcementChannel.SendMessageAsync(
                    $"🏆 <@{userId}> cleared **{dungeon.Name}**\n" +
                    $"+400 Gold\n+1000 XP{dropText}{levelMessage}{petLevelMessage}"
                );
            }

            _lastDungeonRun[userId] = DateTime.UtcNow;
            return result;
        }

        // =========================
        // RESET DUNGEON COOLDOWN
        // =========================
        public void ResetDungeonCooldown(ulong userId)
        {
            _lastDungeonRun.Remove(userId);
        }

        // =========================
        // UPDATE MESSAGE
        // =========================
        public async Task UpdateDungeonMessageAsync(ulong userId, DungeonResult result)
        {
            ulong messageId;
            ulong channelId;

            if (_active.TryGetValue(userId, out var session))
            {
                messageId = session.MessageId;
                channelId = session.ChannelId;
            }
            else if (_lastMessages.TryGetValue(userId, out var last))
            {
                messageId = last.messageId;
                channelId = last.channelId;
            }
            else
            {
                Console.WriteLine("❌ No message tracking found");
                return;
            }

            var channel = _client.GetChannel(channelId) as IMessageChannel;

            if (channel == null)
            {
                var user = _client.GetUser(userId);

                if (user != null)
                {
                    channel = await user.CreateDMChannelAsync();
                }
            }

            if (channel == null)
            {
                Console.WriteLine("❌ Channel not found");
                return;
            }

            var msg = await channel.GetMessageAsync(messageId) as IUserMessage;

            if (msg == null)
            {
                Console.WriteLine("❌ Dungeon message not found");
                return;
            }

            await msg.ModifyAsync(m =>
            {
                m.Embed = result.Embed;

                if (result.IsFinished)
                {
                    m.Components = new ComponentBuilder().Build();
                }
                else
                {
                    m.Components = new ComponentBuilder()
                        .WithButton("⚔ Attack", "dungeon_attack", ButtonStyle.Danger)
                        .WithButton("🧪 Heal", "dungeon_heal", ButtonStyle.Success)
                        .WithButton("🏃 Flee", "dungeon_flee", ButtonStyle.Secondary)
                        .Build();
                }
            });

            Console.WriteLine($"✅ Dungeon message updated for {userId}");
        }

        // =========================
        // BOSS BEHAVIORS
        // =========================

        private string HandleRage(ActiveDungeon session, DungeonBossDefinition boss, ref int enemyDamage)
        {
            if (!session.RageTriggered && session.EnemyHealth <= boss.MaxHealth * 0.3)
            {
                session.RageTriggered = true;

                int heal = (int)(boss.MaxHealth * 0.50);
                session.EnemyHealth += heal;

                return $"🔥 **{boss.Name} enters SCOOTER ROAD RAGE! (+{heal} HP)**";
            }

            if (session.RageTriggered)
            {
                enemyDamage *= 2;
            }

            return null;
        }

        private string HandleLifestealSmash(ActiveDungeon session, DungeonBossDefinition boss, ref int enemyDamage)
        {
            if (_random.Next(0, 100) < 25)
            {
                enemyDamage = (int)(enemyDamage * 1.8);

                int heal = enemyDamage;
                session.EnemyHealth = Math.Min(session.EnemyMaxHealth, session.EnemyHealth + heal);

                return $"🩸 **{boss.Name} crushes you and absorbs {heal} HP!**";
            }

            return null;
        }

        private string HandleDefensiveCloud(ActiveDungeon session, DungeonBossDefinition boss, ref int enemyDamage)
        {
            if (_random.Next(0, 100) < 30)
            {
                session.CloudActive = true;
                return $"🌫️ **{boss.Name} vanishes into a toxic mist! Incoming damage reduced!**";
            }

            return null;
        }

        private string HandleCrushingBlow(ActiveDungeon session, DungeonBossDefinition boss, ref int enemyDamage)
        {
            if (_random.Next(0, 100) < 20)
            {
                int reducedDefense = session.Defense / 2;

                enemyDamage = (int)(boss.Attack * (100.0 / (100 + reducedDefense)));
                enemyDamage = (int)(enemyDamage * 1.5);

                return $"💥 **{boss.Name} shatters your defenses with a crushing blow!**";
            }

            return null;
        }

        // =========================
        // HELPERS
        // =========================

        private bool IsOnCooldown(ulong userId, int seconds, out int remaining)
        {
            remaining = 0;

            if (!_lastAction.TryGetValue(userId, out var last))
                return false;

            var diff = DateTime.UtcNow - last;

            if (diff.TotalSeconds >= seconds)
                return false;

            remaining = seconds - (int)diff.TotalSeconds;
            return true;
        }

        private string HandleBossBehavior(ActiveDungeon session, DungeonBossDefinition boss, ref int enemyDamage)
        {
            if (boss == null || string.IsNullOrEmpty(boss.BehaviorId))
                return null;

            switch (boss.BehaviorId)
            {
                case "rage":
                    return HandleRage(session, boss, ref enemyDamage);

                case "lifesteal_smash":
                    return HandleLifestealSmash(session, boss, ref enemyDamage);

                case "defensive_cloud":
                    return HandleDefensiveCloud(session, boss, ref enemyDamage);

                case "crushing_blow":
                    return HandleCrushingBlow(session, boss, ref enemyDamage);

                default:
                    return null;
            }
        }

        private DungeonResult Wrap(Embed embed) => new() { Embed = embed };

        private DungeonResult Simple(string text)
            => new() { Embed = new EmbedBuilder().WithDescription(text).Build() };

        private Embed BuildEmbed(ActiveDungeon s, string text, string imageUrl = null)
        {
            var embed = new EmbedBuilder()
                .WithTitle($"🏰 Dungeon - Floor {s.Floor}")
                .WithDescription(text)
                .AddField("❤️ Player HP", $"{Bar(s.PlayerHealth, s.MaxHealth)}\n{s.PlayerHealth}/{s.MaxHealth}", true)
                .AddField("👹 Enemy HP", $"{Bar(s.EnemyHealth, s.EnemyMaxHealth)}\n{s.EnemyHealth}/{s.EnemyMaxHealth}", true)
                .WithColor(s.IsBoss ? Color.DarkRed : Color.DarkBlue);

            if (!string.IsNullOrEmpty(imageUrl))
                embed.WithImageUrl(imageUrl);

            return embed.Build();
        }

        private string Bar(int current, int max)
        {
            int total = 10;

            if (max <= 0)
                max = 1;

            current = Math.Max(0, Math.Min(current, max));

            double percent = (double)current / max;
            int filled = (int)(percent * total);

            filled = Math.Max(0, Math.Min(filled, total));

            return new string('█', filled) + new string('░', total - filled);
        }

        private int GetEnemyAttack(ActiveDungeon session, dynamic dungeon)
        {
            if (session.IsBoss)
                return dungeon.Boss.Attack;

            return dungeon.BaseEnemyAttack +
                   (session.Floor * dungeon.EnemyAttackScaling);
        }

        private int GetEnemyDefense(ActiveDungeon session)
        {
            if (session.IsBoss)
                return DungeonRegistry.All[session.DungeonId].Boss.Defense;

            return 10 + (session.Floor * 2);
        }

        public void SetDungeonMessage(ulong userId, ulong messageId, ulong channelId)
        {
            if (_active.TryGetValue(userId, out var session))
            {
                session.MessageId = messageId;
                session.ChannelId = channelId;
            }

            _lastMessages[userId] = (messageId, channelId);
        }
    }
}

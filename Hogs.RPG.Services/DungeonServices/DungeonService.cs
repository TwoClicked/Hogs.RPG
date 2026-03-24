using Discord;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Discord.WebSocket;
using Hogs.RPG.Services.InventoryServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.Game
{
    public class DungeonService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly IMessageChannel _announcementChannel;
        private readonly DiscordSocketClient _client;
        private readonly StatService _statService;

        private readonly Dictionary<ulong, (ulong messageId, ulong channelId)> _lastMessages = new();
        private readonly Dictionary<ulong, DateTime> _lastAction = new();
        private readonly Dictionary<ulong, DateTime> _lastDungeonRun = new();

        private readonly Dictionary<ulong, ActiveDungeon> _active = new();
        private readonly Random _random = new();

        public DungeonService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            DiscordSocketClient client,
            StatService statService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;

            _client = client;
            _statService = statService;

            _announcementChannel = client.GetChannel(1485357755433750549) as IMessageChannel;
        }

        // =========================
        // START
        // =========================
        public async Task<DungeonResult> StartDungeonAsync(ulong userId, string dungeonId)
        {
            // 🔥 Prevent multiple active sessions
            if (_active.ContainsKey(userId))
                return Simple("You are already in a dungeon.");

            // 🔥 Dungeon cooldown check
            const int COOLDOWN_HOURS = 2;

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

            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return Simple("Use /startadventure first.");

            var dungeon = DungeonRegistry.All[dungeonId];

            if (player.Level < dungeon.RequiredLevel)
                return Simple($"You must be level {dungeon.RequiredLevel}.");

            // 🔥 USE YOUR EXISTING STAT SERVICE
            var (attack, defense, health) = _statService.CalculateStats(player);

            var session = new ActiveDungeon
            {
                PlayerId = userId,
                DungeonId = dungeonId,
                Floor = 1,

                // ✅ SNAPSHOT STATS (critical fix)
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

            // 🔥 IMPORTANT: actually start cooldown
            _lastDungeonRun[userId] = DateTime.UtcNow;

            // 🔥 Announcement (safe)
            if (_announcementChannel != null)
            {
                await _announcementChannel.SendMessageAsync(
                    $"🏰 <@{userId}> entered **{dungeon.Name}**"
                );
            }

            return Wrap(BuildEmbed(session, "🏰 You enter the dungeon..."));
        }

        public async Task<DungeonResult> AttackAsync(ulong userId)
        {
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a dungeon.");

            if (IsOnCooldown(userId, 1, out int remaining))
            {
                return Wrap(BuildEmbed(session,
                    $"⏳ You must wait {remaining}s before acting again.",
                    session.CurrentImageUrl));
            }

            var dungeon = DungeonRegistry.All[session.DungeonId];

            // ✅ USE SESSION STATS
            int playerDamage = (int)(session.Attack * (100.0 / (100 + GetEnemyDefense(session))));
            playerDamage = Math.Max(1, playerDamage);

            session.EnemyHealth = Math.Max(0, session.EnemyHealth - playerDamage);

            var text = $"⚔ You deal {playerDamage} damage!\n";

            if (session.EnemyHealth <= 0)
            {
                _lastAction[userId] = DateTime.UtcNow;
                return await NextFloor(userId, session, text);
            }

            int enemyAttack = GetEnemyAttack(session, dungeon);

            // ✅ USE SESSION DEFENSE
            int enemyDamage = (int)(enemyAttack * (100.0 / (100 + session.Defense)));
            enemyDamage = Math.Max(1, enemyDamage);

            string behaviorText = null;

            if (session.IsBoss)
            {
                behaviorText = HandleBossBehavior(session, dungeon.Boss, ref enemyDamage);
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

            return Wrap(BuildEmbed(session, text, session.CurrentImageUrl));
        }

        // =========================
        // HEAL
        // =========================
        public async Task<DungeonResult> HealAsync(ulong userId)
        {
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a dungeon.");

            // 🔥 COOLDOWN CHECK
            if (IsOnCooldown(userId, 2, out int remaining))
            {
                return Wrap(BuildEmbed(session,
                    $"⏳ You must wait {remaining}s before acting again.",
                    session.CurrentImageUrl));
            }

            if (session.PlayerHealth == session.MaxHealth)
                return Wrap(BuildEmbed(session, "❤️ You are already at full health.", session.CurrentImageUrl));

            var inventory = await _inventoryService.GetInventoryAsync(userId);
            var potion = inventory.Find(i => i.ItemId == "health_potion");

            if (potion == null || potion.Quantity <= 0)
                return Wrap(BuildEmbed(session, "❌ You have no health potions.", session.CurrentImageUrl));

            await _inventoryService.TakeItemAsync(userId, "health_potion", 1);

            session.PlayerHealth = session.MaxHealth;

            // 🔥 APPLY COOLDOWN
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

            // 🔥 Announcement
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

            // NORMAL FLOOR (uses dungeon scaling)
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
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a dungeon.");

            _active.Remove(userId);

            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            var dungeon = DungeonRegistry.All[session.DungeonId];

            // 🔥 Apply penalties
            player.Gold = Math.Max(0, player.Gold - 250);
            player.XP = 0;

            // 🔥 CRITICAL FIX: restore HP INCLUDING gear
            var (attack, defense, maxHealth) = _statService.CalculateStats(player);
            player.Health = maxHealth;

            await _playerRepository.UpdatePlayerAsync(player);

            // 🔥 Announcement
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
                    .WithDescription("Lost 250 gold and all XP.\n❤️ You were restored to full health.")
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
            _active.Remove(userId);

            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            var dungeon = DungeonRegistry.All[session.DungeonId];

            player.Gold += 400;
            player.XP += 200;

            var dropText = "";

            // 🔥 Handle dungeon-specific drops
            foreach (var drop in dungeon.Drops)
            {
                int roll = _random.Next(1, 101); // 1–100

                if (roll <= drop.ChancePercent)
                {
                    await _inventoryService.GiveItemAsync(userId, drop.ItemId, 1);

                    // Get proper item name for display
                    if (InventoryItemDefinitions.All.TryGetValue(drop.ItemId, out var item))
                    {
                        dropText += $"\n🎁 **{item.Name}**";
                    }
                    else
                    {
                        // fallback if not registered
                        dropText += $"\n🎁 **{drop.ItemId.Replace("_", " ")}**";
                    }
                }
            }

            await _playerRepository.UpdatePlayerAsync(player);

            var result = new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithTitle("🏆 Dungeon Complete")
                    .WithDescription($"+400 Gold\n+200 XP{dropText}")
                    .WithColor(Color.Gold)
                    .Build(),
                IsFinished = true
            };

            // 🔥 Announcement (with drops)
            if (_announcementChannel != null)
            {
                await _announcementChannel.SendMessageAsync(
                    $"🏆 <@{userId}> cleared **{dungeon.Name}**\n" +
                    $"+400 Gold\n+200 XP{dropText}"
                );
            }
            _lastDungeonRun[userId] = DateTime.UtcNow;
            return result;
        }

        // =========================
        // UPDATE MESSAGE (FIXED)
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

            // 🔥 FIXED: Proper channel resolution
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

        //Boss ability section
        private string HandleRage(ActiveDungeon session, DungeonBossDefinition boss, ref int enemyDamage)
        {
            // Trigger rage at 50% HP
            if (!session.RageTriggered && session.EnemyHealth <= boss.MaxHealth * 0.3)
            {
                session.RageTriggered = true;

                int heal = (int)(boss.MaxHealth * 0.50);
                session.EnemyHealth += heal;

                return $"🔥 **{boss.Name} enters SCOOTER ROAD RAGE! (+{heal} HP)**";
            }

            // Apply triple damage if enraged
            if (session.RageTriggered)
            {
                enemyDamage *= 3;
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

        private string HandleBossBehavior(ActiveDungeon session,DungeonBossDefinition boss, ref int enemyDamage)
        {
            if (boss == null || string.IsNullOrEmpty(boss.BehaviorId))
                return null;

            switch (boss.BehaviorId)
            {
                case "rage":
                    return HandleRage(session, boss, ref enemyDamage);

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

            // ✅ HARD SAFETY
            if (max <= 0)
                max = 1;

            // clamp current
            current = Math.Max(0, Math.Min(current, max));

            double percent = (double)current / max;
            int filled = (int)(percent * total);

            // ✅ HARD CLAMP AGAIN
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
                return 20; // boss defense

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
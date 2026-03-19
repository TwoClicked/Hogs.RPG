using Discord;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
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

        private readonly Dictionary<ulong, ActiveDungeon> _active = new();
        private readonly Random _random = new();

        public DungeonService(
            PlayerRepository playerRepository,
            InventoryService inventoryService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
        }

        // =========================
        // START
        // =========================
        public async Task<DungeonResult> StartDungeonAsync(ulong userId, string dungeonId)
        {
            if (_active.ContainsKey(userId))
                return Simple("You are already in a dungeon.");

            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return Simple("Use /startadventure first.");

            var dungeon = DungeonRegistry.All[dungeonId];

            if (player.Level < dungeon.RequiredLevel)
                return Simple($"You must be level {dungeon.RequiredLevel}.");

            var session = new ActiveDungeon
            {
                PlayerId = userId,
                DungeonId = dungeonId,
                Floor = 1,
                MaxHealth = player.MaxHealth,
                PlayerHealth = player.MaxHealth,
                EnemyMaxHealth = dungeon.BaseEnemyHealth,
                EnemyHealth = dungeon.BaseEnemyHealth,
                IsBoss = false,
                CurrentImageUrl = null
            };

            _active[userId] = session;

            return Wrap(BuildEmbed(session, "🏰 You enter the dungeon..."));
        }

        // =========================
        // ATTACK
        // =========================
        public async Task<DungeonResult> AttackAsync(ulong userId)
        {
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a dungeon.");

            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            var dungeon = DungeonRegistry.All[session.DungeonId];

            // PLAYER DAMAGE (scaled)
            int playerDamage = (int)(player.Attack * (100.0 / (100 + GetEnemyDefense(session))));
            playerDamage = Math.Max(1, playerDamage);

            session.EnemyHealth -= playerDamage;

            var text = $"⚔ You deal {playerDamage} damage!\n";

            if (session.EnemyHealth <= 0)
                return await NextFloor(userId, session, text);

            // ENEMY DAMAGE (scaled)
            int enemyAttack = GetEnemyAttack(session, dungeon);

            int enemyDamage = (int)(enemyAttack * (100.0 / (100 + player.Defense)));
            enemyDamage = Math.Max(1, enemyDamage);

            session.PlayerHealth -= enemyDamage;

            text += $"👹 Enemy hits you for {enemyDamage}!";

            if (session.PlayerHealth <= 0)
                return await HandleDeath(userId);

            return Wrap(BuildEmbed(session, text, session.CurrentImageUrl));
        }

        // =========================
        // HEAL
        // =========================
        public async Task<DungeonResult> HealAsync(ulong userId)
        {
            if (!_active.TryGetValue(userId, out var session))
                return Simple("You are not in a dungeon.");

            if (session.PlayerHealth == session.MaxHealth)
                return Wrap(BuildEmbed(session, "❤️ You are already at full health.", session.CurrentImageUrl));

            var inventory = await _inventoryService.GetInventoryAsync(userId);
            var potion = inventory.Find(i => i.ItemId == "health_potion");

            if (potion == null || potion.Quantity <= 0)
                return Wrap(BuildEmbed(session, "❌ You have no health potions.", session.CurrentImageUrl));

            await _inventoryService.TakeItemAsync(userId, "health_potion", 1);

            session.PlayerHealth = session.MaxHealth;

            return Wrap(BuildEmbed(session, "🧪 You healed to full!", session.CurrentImageUrl));
        }

        // =========================
        // FLEE
        // =========================
        public Task<DungeonResult> FleeAsync(ulong userId)
        {
            if (!_active.ContainsKey(userId))
                return Task.FromResult(Simple("You are not in a dungeon."));

            _active.Remove(userId);

            return Task.FromResult(new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithDescription("🏃 You fled the dungeon.")
                    .WithColor(Color.DarkGrey)
                    .Build(),
                IsFinished = true
            });
        }

        // =========================
        // NEXT FLOOR
        // =========================
        private async Task<DungeonResult> NextFloor(ulong userId, ActiveDungeon session, string text)
        {
            session.Floor++;

            var dungeon = DungeonRegistry.All[session.DungeonId];

            if (session.Floor > dungeon.Floors)
                return await CompleteDungeon(userId);

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
            _active.Remove(userId);

            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            player.Gold = Math.Max(0, player.Gold - 250);
            player.XP = 0;

            await _playerRepository.UpdatePlayerAsync(player);

            return new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithTitle("💀 You Died")
                    .WithDescription("Lost 250 gold and all XP.")
                    .WithColor(Color.DarkRed)
                    .Build(),
                IsFinished = true
            };
        }

        // =========================
        // COMPLETE
        // =========================
        private async Task<DungeonResult> CompleteDungeon(ulong userId)
        {
            _active.Remove(userId);

            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            player.Gold += 400;
            player.XP += 200;

            await _playerRepository.UpdatePlayerAsync(player);

            return new DungeonResult
            {
                Embed = new EmbedBuilder()
                    .WithTitle("🏆 Dungeon Complete")
                    .WithDescription("+400 Gold\n+200 XP")
                    .WithColor(Color.Gold)
                    .Build(),
                IsFinished = true
            };
        }

        // =========================
        // HELPERS
        // =========================
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
            int filled = (int)((double)current / max * total);
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
    }
}
using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.RelicServices;

namespace Hogs.RPG.Bot.Commands
{
    public class RelicModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RelicService _relicService;
        private readonly PlayerRepository _playerRepository;

        public RelicModule(RelicService relicService, PlayerRepository playerRepository)
        {
            _relicService = relicService;
            _playerRepository = playerRepository;
        }

        private async Task<bool> EnsurePlayerAsync()
        {
            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await RespondAsync("⚠️ You need to start your adventure first with `/startadventure`.", ephemeral: true);
                return false;
            }
            return true;
        }

        // =========================
        // /relics — View all relics
        // =========================
        [SlashCommand("relics", "View your relics and shards")]
        public async Task ViewRelics()
        {
            await DeferAsync(ephemeral: true);
            if (!await EnsurePlayerAsync()) return;

            var relics = await _relicService.GetRelicsAsync(Context.User.Id);

            var embed = new EmbedBuilder()
                .WithTitle("💎 Your Relics")
                .WithColor(new Color(0x7B68EE));

            // Equipped slots
            var equipped = relics.Where(r => r.IsEquipped).OrderBy(r => r.SlotIndex).ToList();

            string slot1 = "Empty";
            string slot2 = "Empty";

            if (equipped.Count > 0)
            {
                var r = equipped[0];
                var def = RelicRegistry.Get(r.RelicId);
                slot1 = $"**{def.Name}** (Rank {r.Rank}) — {_relicService.FormatBonus(r.BonusType)}";
            }
            if (equipped.Count > 1)
            {
                var r = equipped[1];
                var def = RelicRegistry.Get(r.RelicId);
                slot2 = $"**{def.Name}** (Rank {r.Rank}) — {_relicService.FormatBonus(r.BonusType)}";
            }

            embed.AddField("🔮 Slot 1", slot1, inline: false);
            embed.AddField("🔮 Slot 2", slot2, inline: false);

            // Inventory relics
            var unequipped = relics.Where(r => !r.IsEquipped).ToList();
            if (unequipped.Count > 0)
            {
                var lines = unequipped.Select(r =>
                {
                    var def = RelicRegistry.Get(r.RelicId);
                    return $"`ID:{r.Id}` **{def.Name}** (Rank {r.Rank}) [{def.Affinity}] — {_relicService.FormatBonus(r.BonusType)}";
                });
                embed.AddField("📦 Inventory", string.Join("\n", lines), inline: false);
            }

            // Shards
            var shardList = await GetShardsDisplayAsync();
            if (!string.IsNullOrEmpty(shardList))
                embed.AddField("🔶 Shards", shardList, inline: false);

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        // =========================
        // /relic-equip — Equip a relic
        // =========================
        [SlashCommand("relic-equip", "Equip a relic to a slot")]
        public async Task EquipRelic(
            [Summary("relic_id", "The ID of the relic to equip (shown in /relics)")] int relicId,
            [Summary("slot", "Slot 1 or 2")] int slot)
        {
            await DeferAsync(ephemeral: true);
            if (!await EnsurePlayerAsync()) return;

            var result = await _relicService.EquipRelicAsync(Context.User.Id, relicId, slot - 1);
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // /relic-reroll — Reroll a relic's bonus
        // =========================
        [SlashCommand("relic-reroll", "Reroll a relic's passive bonus using a shard")]
        public async Task RerollRelic(
            [Summary("relic_id", "The ID of the relic to reroll (shown in /relics)")] int relicId,
            [Summary("shard_tier", "Which tier shard to consume (1-5)")] int shardTier)
        {
            await DeferAsync(ephemeral: true);
            if (!await EnsurePlayerAsync()) return;

            if (shardTier < 1 || shardTier > 5)
            {
                await FollowupAsync("❌ Shard tier must be between 1 and 5.", ephemeral: true);
                return;
            }

            var result = await _relicService.RerollRelicAsync(Context.User.Id, relicId, shardTier);
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // /relic-upgrade — Upgrade a relic's rank
        // =========================
        [SlashCommand("relic-upgrade", "Upgrade a relic's rank using a shard")]
        public async Task UpgradeRelic(
            [Summary("relic_id", "The ID of the relic to upgrade (shown in /relics)")] int relicId,
            [Summary("shard_tier", "Which tier shard to consume (1-5)")] int shardTier)
        {
            await DeferAsync(ephemeral: true);
            if (!await EnsurePlayerAsync()) return;

            if (shardTier < 1 || shardTier > 5)
            {
                await FollowupAsync("❌ Shard tier must be between 1 and 5.", ephemeral: true);
                return;
            }

            var result = await _relicService.UpgradeRelicAsync(Context.User.Id, relicId, shardTier);
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // HELPER — Shard display
        // =========================
        private async Task<string> GetShardsDisplayAsync()
        {
            var shards = await _relicService.GetShardsAsync(Context.User.Id);

            if (shards.Count == 0)
                return "";

            return string.Join("\n", shards
                .Where(s => s.Quantity > 0)
                .OrderBy(s => s.Tier)
                .Select(s => $"Tier {s.Tier} Shard x{s.Quantity}"));
        }
    }
}
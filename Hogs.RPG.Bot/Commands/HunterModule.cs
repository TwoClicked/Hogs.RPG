using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.Equipment;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    [BossLock]
    public class HunterModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryRepository _inventoryRepository;

        // All 9 hunter gear pieces: itemId → (display name, player slot getter, player slot setter)
        private static readonly (string Id, string Name, Func<Core.Entities.Player, string?> GetSlot, Action<Core.Entities.Player> ClearSlot)[] HunterPieces =
        {
            ("hunter_helm",     "Hunter's Helm",     p => p.Helmet,   p => p.Helmet   = null),
            ("hunter_vest",     "Hunter's Vest",     p => p.Body,     p => p.Body     = null),
            ("hunter_bow",      "Hunter's Bow",      p => p.MainHand, p => p.MainHand = null),
            ("hunter_quiver",   "Hunter's Quiver",   p => p.OffHand,  p => p.OffHand  = null),
            ("hunter_leggings", "Hunter's Leggings", p => p.Legs,     p => p.Legs     = null),
            ("hunter_gloves",   "Hunter's Gloves",   p => p.Gloves,   p => p.Gloves   = null),
            ("hunter_boots",    "Hunter's Boots",    p => p.Boots,    p => p.Boots    = null),
            ("hunter_ring",     "Hunter's Ring",     p => p.Ring,     p => p.Ring     = null),
            ("hunter_amulet",   "Hunter's Amulet",   p => p.Amulet,   p => p.Amulet   = null),
        };

        public HunterModule(PlayerRepository playerRepository, InventoryRepository inventoryRepository)
        {
            _playerRepository = playerRepository;
            _inventoryRepository = inventoryRepository;
        }

        // =========================
        // /hunter-gear
        // Shows which Hunter set pieces are owned (equipped or in inventory)
        // =========================
        [SlashCommand("hunter-gear", "View your Hunter's gear set status")]
        public async Task HunterGear()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("❌ You need to start your adventure first with `/startadventure`.", ephemeral: true);
                return;
            }

            if (player.HasHunterSetBonus)
            {
                var completedEmbed = new EmbedBuilder()
                    .WithTitle("🌟 Hunter's Set — Complete!")
                    .WithColor(Color.Gold)
                    .WithDescription(
                        "You have already consumed all 9 Hunter gear pieces and activated the **permanent set bonus**.\n\n" +
                        "✅ **+4.5% Hunt XP** (always active)\n" +
                        "✅ **+4.5% Hunt Materials** (always active)\n" +
                        "✅ **+5% Rare Drop Rate** (always active)\n\n" +
                        "*You no longer need to equip Hunter gear when hunting.*")
                    .Build();

                await FollowupAsync(embed: completedEmbed, ephemeral: true);
                return;
            }

            var inventory = await _inventoryRepository.GetInventoryAsync(Context.User.Id);
            var inventoryLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            var sb = new StringBuilder();
            int ownedCount = 0;

            foreach (var (id, name, getSlot, _) in HunterPieces)
            {
                string? equippedSlotValue = getSlot(player);
                bool isEquipped = equippedSlotValue == id;
                inventoryLookup.TryGetValue(id, out int invQty);

                string status;
                if (isEquipped)
                {
                    status = "✅ **Equipped**";
                    ownedCount++;
                }
                else if (invQty > 0)
                {
                    status = $"📦 In inventory (×{invQty})";
                    ownedCount++;
                }
                else
                {
                    status = "❌ Not owned";
                }

                sb.AppendLine($"**{name}** — {status}");
            }

            sb.AppendLine();
            sb.AppendLine($"**Owned: {ownedCount}/9**");

            if (ownedCount == 9)
            {
                sb.AppendLine();
                sb.AppendLine("🌟 You have all 9 pieces! Use `/hunter-setcomplete` to permanently activate the set bonus.");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("*Buy missing pieces with Tracker Tokens at `/trail shop`.*");
            }

            var embed = new EmbedBuilder()
                .WithTitle("🏹 Hunter's Gear Set")
                .WithColor(new Color(0x8B4513))
                .WithDescription(sb.ToString().Trim())
                .WithFooter("Full 9-piece set: +4.5% XP, +4.5% materials, +5% rare drop rate on /hunt")
                .Build();

            await FollowupAsync(embed: embed, ephemeral: true);
        }

        // =========================
        // /hunter-setcomplete
        // Consumes all 9 Hunter gear pieces (equipped or in inventory)
        // and grants the permanent set bonus on the player profile.
        // =========================
        [SlashCommand("hunter-setcomplete", "Sacrifice all 9 Hunter gear pieces for a permanent hunt bonus")]
        public async Task HunterSetComplete()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("❌ You need to start your adventure first with `/startadventure`.", ephemeral: true);
                return;
            }

            if (player.HasHunterSetBonus)
            {
                await FollowupAsync("✅ You already have the permanent Hunter Set Bonus active!", ephemeral: true);
                return;
            }

            var inventory = await _inventoryRepository.GetInventoryAsync(Context.User.Id);
            var inventoryLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            // =========================
            // VALIDATE — all 9 pieces must be owned (equipped OR in inventory)
            // =========================
            var missing = new List<string>();

            foreach (var (id, name, getSlot, _) in HunterPieces)
            {
                bool isEquipped = getSlot(player) == id;
                inventoryLookup.TryGetValue(id, out int invQty);

                if (!isEquipped && invQty <= 0)
                    missing.Add(name);
            }

            if (missing.Count > 0)
            {
                var missingList = string.Join("\n", missing.Select(m => $"  ❌ {m}"));
                await FollowupAsync(
                    $"❌ You are missing the following Hunter gear pieces:\n{missingList}\n\n" +
                    $"*Use `/hunter-gear` to see your full set status.*",
                    ephemeral: true);
                return;
            }

            // =========================
            // CONSUME — clear equipped slots and remove from inventory
            // =========================
            bool slotChanged = false;

            foreach (var (id, name, getSlot, clearSlot) in HunterPieces)
            {
                if (getSlot(player) == id)
                {
                    // Equipped — clear the slot (piece is sacrificed, not returned to inventory)
                    clearSlot(player);
                    slotChanged = true;
                }
                else
                {
                    // In inventory — remove 1
                    await _inventoryRepository.RemoveItemAsync(Context.User.Id, id, 1);
                }
            }

            // =========================
            // GRANT PERMANENT BONUS
            // =========================
            player.HasHunterSetBonus = true;
            await _playerRepository.UpdatePlayerAsync(player);

            var embed = new EmbedBuilder()
                .WithTitle("🌟 Hunter Set Bonus Activated!")
                .WithColor(Color.Gold)
                .WithDescription(
                    "All 9 Hunter gear pieces have been **sacrificed to the wild**.\n\n" +
                    "In return, the hunt flows through you permanently:\n\n" +
                    "✅ **+4.5% Hunt XP** — always active\n" +
                    "✅ **+4.5% Hunt Materials** — always active\n" +
                    "✅ **+5% Rare Drop Rate** — always active\n\n" +
                    "*Your profile now shows the Hunter Set Bonus. You no longer need to equip Hunter gear when hunting.*")
                .WithFooter("The hunt is yours. Forever.")
                .Build();

            await FollowupAsync(embed: embed, ephemeral: false); // Public — let the server see it!
        }
    }
}

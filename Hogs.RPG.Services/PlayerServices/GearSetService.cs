// Hogs.RPG.Services/GameplayServices/GearSetService.cs

using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.RelicServices;
using Hogs.RPG.Services.TradeServices;

namespace Hogs.RPG.Services.GameplayServices
{
    public class GearSetService
    {
        private readonly GearSetRepository _gearSetRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly StatService _statService;
        private readonly PetService _petService;
        private readonly RelicService _relicService;
        private readonly TradeService _tradeService;

        private const int GearSwapCooldownSeconds = 120;

        public GearSetService(
            GearSetRepository gearSetRepository,
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            StatService statService,
            PetService petService,
            RelicService relicService,
            TradeService tradeService)
        {
            _gearSetRepository = gearSetRepository;
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _statService = statService;
            _petService = petService;
            _relicService = relicService;
            _tradeService = tradeService;
        }

        // =========================
        // SAVE CURRENT GEAR AS SET
        // =========================
        public async Task<string> SaveSetAsync(ulong userId, int setIndex, string setName)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null) return "You need to start your adventure first.";

            var gearSet = new GearSet
            {
                DiscordId = userId,
                SetIndex = setIndex,
                SetName = setName,
                MainHand = player.MainHand,
                OffHand = player.OffHand,
                Helmet = player.Helmet,
                Body = player.Body,
                Legs = player.Legs,
                Gloves = player.Gloves,
                Boots = player.Boots,
                Ring = player.Ring,
                Amulet = player.Amulet
            };

            // Snapshot equipped relics
            var relics = await _relicService.GetEquippedRelicsAsync(userId);
            gearSet.RelicSlot1Id = relics.FirstOrDefault(r => r.SlotIndex == 0)?.Id;
            gearSet.RelicSlot2Id = relics.FirstOrDefault(r => r.SlotIndex == 1)?.Id;

            // Snapshot equipped pet
            var equippedPet = await _petService.GetEquippedPetAsync(userId);
            gearSet.PetId = equippedPet?.PetId;

            await _gearSetRepository.SaveSetAsync(gearSet);

            var slotCount = CountFilledSlots(gearSet);
            return $"✅ Saved **{setName}** (Set {setIndex}) with {slotCount}/9 gear slot(s) filled.";
        }

        // =========================
        // LOAD SET — SWAP GEAR
        // =========================
        public async Task<string> LoadSetAsync(ulong userId, int setIndex)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null) return "You need to start your adventure first.";

            // =========================
            // TRADE LOCK
            // Blocks gear swaps while the player has an active or pending
            // trade. Prevents stale GearSet snapshots from re-equipping an
            // item that is simultaneously sitting in a trade offer — this
            // was the root cause of the gear duplication exploit.
            // =========================
            if (_tradeService.HasActiveTrade(userId))
            {
                return "📦 You have an active trade — finish or cancel it with `/tradecancel` before swapping gear sets.";
            }

            // =========================
            // COOLDOWN CHECK
            // =========================
            if (player.LastGearSwapAt.HasValue)
            {
                var elapsed = DateTime.UtcNow - player.LastGearSwapAt.Value;
                if (elapsed.TotalSeconds < GearSwapCooldownSeconds)
                {
                    var remaining = GearSwapCooldownSeconds - (int)elapsed.TotalSeconds;
                    return $"⏳ Gear swap on cooldown. Please wait **{remaining}s** before swapping again.";
                }
            }

            var set = await _gearSetRepository.GetSetAsync(userId, setIndex);
            if (set == null) return $"❌ No gear set saved in slot {setIndex}.";

            var changing = BuildSlotList(player, set)
                .Where(s => s.Current != s.Target)
                .ToList();

            // ─────────────────────────────────────────
            // SIMULATE inventory to handle cross-slot
            // swaps (e.g. swap MainHand ↔ OffHand)
            // without false "item not found" errors.
            // ─────────────────────────────────────────
            var inventory = await _inventoryService.GetInventoryAsync(userId);
            var sim = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            // Phase 1 — virtually return all current items that will change
            foreach (var s in changing.Where(s => !string.IsNullOrEmpty(s.Current)))
            {
                sim.TryGetValue(s.Current!, out var qty);
                sim[s.Current!] = qty + 1;
            }

            // Phase 2 — check target availability, mark skipped
            var skipped = new List<string>();
            foreach (var s in changing.Where(s => !string.IsNullOrEmpty(s.Target)))
            {
                sim.TryGetValue(s.Target!, out var available);
                if (available <= 0)
                {
                    s.Skip = true;
                    skipped.Add(s.Name);

                    // Undo the virtual return for this slot — slot stays as-is
                    if (!string.IsNullOrEmpty(s.Current))
                        sim[s.Current!]--;
                }
                else
                {
                    sim[s.Target!] = available - 1;
                }
            }

            // Phase 3 — execute DB changes for non-skipped slots only
            var toExecute = changing.Where(s => !s.Skip).ToList();

            foreach (var s in toExecute.Where(s => !string.IsNullOrEmpty(s.Current)))
                await _inventoryService.GiveItemAsync(userId, s.Current!, 1);

            foreach (var s in toExecute.Where(s => !string.IsNullOrEmpty(s.Target)))
                await _inventoryService.TakeItemAsync(userId, s.Target!, 1);

            // Phase 4 — update player gear slots
            foreach (var s in toExecute)
                s.SetSlot(player, s.Target);

            // =========================
            // RELICS
            // =========================
            var allRelics = await _relicService.GetRelicsAsync(userId);

            // Unequip all current relics
            foreach (var r in allRelics.Where(r => r.IsEquipped))
            {
                r.IsEquipped = false;
                await _relicService.SaveRelicAsync(r);
            }

            // Equip saved relics if they still exist and belong to this player
            if (set.RelicSlot1Id.HasValue)
            {
                var r = allRelics.FirstOrDefault(r => r.Id == set.RelicSlot1Id.Value);
                if (r != null) { r.IsEquipped = true; r.SlotIndex = 0; await _relicService.SaveRelicAsync(r); }
            }
            if (set.RelicSlot2Id.HasValue)
            {
                var r = allRelics.FirstOrDefault(r => r.Id == set.RelicSlot2Id.Value);
                if (r != null) { r.IsEquipped = true; r.SlotIndex = 1; await _relicService.SaveRelicAsync(r); }
            }

            // =========================
            // PET
            // =========================
            if (!string.IsNullOrEmpty(set.PetId))
            {
                var pets = await _petService.GetPetsAsync(userId);
                bool ownsPet = pets.Any(p => p.PetId == set.PetId);
                if (ownsPet)
                    await _petService.EquipPetAsync(userId, set.PetId);
            }

            // Clamp health to new max
            var (_, _, maxHealth) = _statService.CalculateStats(player);
            if (player.Health > maxHealth)
                player.Health = maxHealth;

            // Stamp cooldown
            player.LastGearSwapAt = DateTime.UtcNow;

            await _playerRepository.UpdatePlayerAsync(player);

            var result = $"⚔️ Loaded **{set.SetName}** (Set {setIndex}).";
            if (skipped.Any())
                result += $"\n⚠️ Skipped (item not in inventory): {string.Join(", ", skipped)}";

            return result;
        }

        // =========================
        // VIEW ALL SETS
        // =========================
        public async Task<List<GearSet>> GetSetsAsync(ulong userId)
        {
            return await _gearSetRepository.GetSetsAsync(userId);
        }

        // =========================
        // DELETE SET
        // =========================
        public async Task<string> DeleteSetAsync(ulong userId, int setIndex)
        {
            var set = await _gearSetRepository.GetSetAsync(userId, setIndex);
            if (set == null) return $"❌ No gear set in slot {setIndex}.";

            await _gearSetRepository.DeleteSetAsync(userId, setIndex);
            return $"🗑️ Deleted **{set.SetName}** (Set {setIndex}).";
        }

        // =========================
        // HELPERS
        // =========================
        private List<SlotChange> BuildSlotList(Player player, GearSet set)
        {
            return new List<SlotChange>
            {
                new("Main Hand", player.MainHand, set.MainHand, (p, v) => p.MainHand = v),
                new("Off Hand",  player.OffHand,  set.OffHand,  (p, v) => p.OffHand  = v),
                new("Helmet",    player.Helmet,   set.Helmet,   (p, v) => p.Helmet   = v),
                new("Body",      player.Body,     set.Body,     (p, v) => p.Body     = v),
                new("Legs",      player.Legs,     set.Legs,     (p, v) => p.Legs     = v),
                new("Gloves",    player.Gloves,   set.Gloves,   (p, v) => p.Gloves   = v),
                new("Boots",     player.Boots,    set.Boots,    (p, v) => p.Boots    = v),
                new("Ring",      player.Ring,     set.Ring,     (p, v) => p.Ring     = v),
                new("Amulet",    player.Amulet,   set.Amulet,   (p, v) => p.Amulet   = v),
            };
        }

        private int CountFilledSlots(GearSet set)
        {
            return new[] { set.MainHand, set.OffHand, set.Helmet, set.Body,
                           set.Legs, set.Gloves, set.Boots, set.Ring, set.Amulet }
                .Count(s => !string.IsNullOrEmpty(s));
        }
    }

    // ─────────────────────────────────────────
    // INTERNAL HELPER — not public API
    // ─────────────────────────────────────────
    internal class SlotChange
    {
        public string Name { get; }
        public string? Current { get; }
        public string? Target { get; }
        public Action<Player, string?> SetSlot { get; }
        public bool Skip { get; set; } = false;

        public SlotChange(string name, string? current, string? target,
            Action<Player, string?> setSlot)
        {
            Name = name;
            Current = current;
            Target = target;
            SetSlot = setSlot;
        }
    }
}
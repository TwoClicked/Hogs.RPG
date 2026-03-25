using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Hogs.RPG.Core.Entities.BossDefinition;

namespace Hogs.RPG.Data.Repositories
{
    public class BossRepository
    {
        private readonly IGoogleSheetsService _sheets;

        private List<BossDefinition> _bosses = new();
        private bool _loaded = false;
        private DateTime _lastLoad = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5); // or 10

        public BossRepository(IGoogleSheetsService sheets)
        {
            _sheets = sheets;
        }

        private async Task EnsureLoaded()
        {

            Console.WriteLine("📥 Loading bosses...");
            if (_loaded && DateTime.UtcNow - _lastLoad < _cacheDuration)
                return;



            _bosses.Clear();

            var rows = await _sheets.ReadRangeAsync("Bosses", "A2:I");

            Console.WriteLine($"✅ Rows loaded: {rows.Count}");

            foreach (var row in rows)
            {
                try
                {
                    if (row.Count < 9) continue;

                    var Id = row[0]?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(Id)) continue;

                    int.TryParse(row[4]?.ToString(), out int maxHealth);
                    int.TryParse(row[5]?.ToString(), out int defense);
                    int.TryParse(row[6]?.ToString(), out int rewardGold);

                    BossType type = BossType.Daily;
                    Enum.TryParse(row[7]?.ToString()?.Trim(), true, out type);

                    var boss = new BossDefinition
                    {
                        Id = row[0]?.ToString()?.Trim(),
                        Name = row[1]?.ToString() ?? "Unknown",
                        Description = row[2]?.ToString(),
                        ImageUrl = row[3]?.ToString(),
                        MaxHealth = maxHealth,
                        Defense = defense,
                        RewardGold = rewardGold,
                        Type = type,
                        AbilitiesText = row[8]?.ToString()
                    };

                    _bosses.Add(boss);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to parse boss row: {ex.Message}");
                }
            }

            _loaded = true;
            _lastLoad = DateTime.UtcNow;
        }

        public async Task SaveSpawnEntryAsync(DateTime date, string key)
        {
            await _sheets.AppendRowAsync("BossState", new List<object>
            {
                date.ToString("yyyy-MM-dd"),
                key
            });
        }

        public async Task ClearOldStateAsync(DateTime date)
        {
            var rows = await _sheets.ReadRangeAsync("BossState", "A2:B");

            var today = date.ToString("yyyy-MM-dd");

            var filtered = new List<IList<object>>();

            foreach (var row in rows)
            {
                if (row.Count < 2) continue;

                var rowDate = row[0]?.ToString();

                if (rowDate == today)
                {
                    filtered.Add(new List<object>
            {
                row[0],
                row[1]
            });
                }
            }

            // 🔥 Rewrite sheet (keep header row intact)
            await _sheets.ClearRangeAsync("BossState", "A2:B");

            if (filtered.Any())
            {
                await _sheets.WriteRangeAsync("BossState", "A2", filtered);
            }
        }

        public async Task<HashSet<string>> LoadSpawnStateAsync(DateTime date)
        {
            var result = new HashSet<string>();

            var rows = await _sheets.ReadRangeAsync("BossState", "A2:B");

            foreach (var row in rows)
            {
                if (row.Count < 2) continue;

                var rowDate = row[0]?.ToString();
                var key = row[1]?.ToString();

                if (rowDate == date.ToString("yyyy-MM-dd") && !string.IsNullOrEmpty(key))
                {
                    result.Add(key);
                }
            }

            return result;
        }

        public async Task<List<BossDefinition>> GetAllAsync()
        {
            await EnsureLoaded();
            return _bosses;
        }

        public async Task<List<BossDefinition>> GetByTypeAsync(BossType type)
        {
            await EnsureLoaded();
            return _bosses.Where(b => b.Type == type).ToList();
        }

        public async Task<BossDefinition> GetByIdAsync(string bossId)
        {
            await EnsureLoaded();

            return _bosses.FirstOrDefault(b =>
                !string.IsNullOrWhiteSpace(b.Id) &&
                b.Id.Equals(bossId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
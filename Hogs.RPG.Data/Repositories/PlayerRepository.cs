using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Data.Repositories
{
    public class PlayerRepository
    {
        private readonly IGoogleSheetsService _sheets;

        private List<Player> _players = new();
        private bool _loaded = false;

        public PlayerRepository(IGoogleSheetsService sheets)
        {
            _sheets = sheets;
        }

        private async Task EnsureLoaded()
        {
            if (_loaded)
                return;

            var rows = await _sheets.ReadRangeAsync("Players", "A2:U");

            foreach (var row in rows)
            {
                var player = new Player
                {
                    PlayerId = int.Parse(row[0].ToString()),
                    DiscordId = ulong.Parse(row[1].ToString()),
                    Username = row[2].ToString(),
                    Level = int.Parse(row[3].ToString()),
                    XP = int.Parse(row[4].ToString()),
                    Gold = int.Parse(row[5].ToString()),
                    Attack = int.Parse(row[6].ToString()),
                    Defense = int.Parse(row[7].ToString()),
                    Health = int.Parse(row[8].ToString()),
                    LastHunt = row.Count > 9 ? row[9]?.ToString() : "",

                    MainHand = row.Count > 10 ? row[10]?.ToString() : "",
                    OffHand = row.Count > 11 ? row[11]?.ToString() : "",
                    Helmet = row.Count > 12 ? row[12]?.ToString() : "",
                    Body = row.Count > 13 ? row[13]?.ToString() : "",
                    Legs = row.Count > 14 ? row[14]?.ToString() : "",
                    Gloves = row.Count > 15 ? row[15]?.ToString() : "",
                    Boots = row.Count > 16 ? row[16]?.ToString() : "",
                    Ring = row.Count > 17 ? row[17]?.ToString() : "",
                    Amulet = row.Count > 18 ? row[18]?.ToString() : "",
                    AutoUseXpPotions = row.Count > 20 && bool.TryParse(row[20]?.ToString(), out var autoXp)
                    ? autoXp
                    : false

                };

                // Parse Active Buffs
                if (row.Count > 19 && row[19] != null)
                {
                    var buffData = row[19].ToString();

                    if (!string.IsNullOrWhiteSpace(buffData))
                    {
                        var buffs = buffData.Split(';');

                        foreach (var buff in buffs)
                        {
                            var parts = buff.Split('|');

                            if (parts.Length == 3)
                            {
                                player.ActiveBuffs.Add(new ActiveBuff
                                {
                                    Type = Enum.Parse<BuffType>(parts[0]),
                                    Value = double.Parse(parts[1]),
                                    RemainingUses = int.Parse(parts[2])
                                });
                            }
                        }
                    }
                }


                _players.Add(player);
            }

            _loaded = true;
        }

        public async Task<Player?> GetByDiscordIdAsync(ulong discordId)
        {
            await EnsureLoaded();
            return _players.FirstOrDefault(p => p.DiscordId == discordId);
        }

        public async Task<Player> CreatePlayerAsync(Player player)
        {
            await EnsureLoaded();

            player.PlayerId = _players.Count == 0
                ? 1
                : _players.Max(p => p.PlayerId) + 1;

            _players.Add(player);

            await _sheets.AppendRowAsync("Players", new object[]
            {
                player.PlayerId,
                player.DiscordId.ToString(),
                player.Username,
                player.Level,
                player.XP,
                player.Gold,
                player.Attack,
                player.Defense,
                player.Health,
                player.LastHunt,

                player.MainHand ?? "",
                player.OffHand ?? "",
                player.Helmet ?? "",
                player.Body ?? "",
                player.Legs ?? "",
                player.Gloves ?? "",
                player.Boots ?? "",
                player.Ring ?? "",
                player.Amulet ?? "",

                SerializeBuffs(player.ActiveBuffs),
                player.AutoUseXpPotions
            });

            return player;
        }

        public async Task UpdatePlayerAsync(Player player)
        {
            var rows = await _sheets.ReadRangeAsync("Players", "A2:U");

            int rowIndex = 2;

            foreach (var row in rows)
            {
                if (row.Count < 2)
                {
                    rowIndex++;
                    continue;
                }

                if (ulong.Parse(row[1].ToString()) == player.DiscordId)
                {
                    var values = new List<IList<object>>
                    {
                        new List<object>
                        {
                            player.PlayerId,
                            player.DiscordId.ToString(),
                            player.Username,
                            player.Level,
                            player.XP,
                            player.Gold,
                            player.Attack,
                            player.Defense,
                            player.Health,
                            player.LastHunt,

                            player.MainHand,
                            player.OffHand,
                            player.Helmet,
                            player.Body,
                            player.Legs,
                            player.Gloves,
                            player.Boots,
                            player.Ring,
                            player.Amulet,

                            SerializeBuffs(player.ActiveBuffs),
                            player.AutoUseXpPotions
                        }
                    };

                    await _sheets.UpdateRangeAsync("Players", $"A{rowIndex}:U{rowIndex}", values);
                    return;
                }

                rowIndex++;
            }
        }

        private string SerializeBuffs(List<ActiveBuff> buffs)
        {
            if (buffs == null || buffs.Count == 0)
                return "";

            return string.Join(";", buffs.Select(b =>
                $"{b.Type}|{b.Value}|{b.RemainingUses}"
            ));
        }
    }
}
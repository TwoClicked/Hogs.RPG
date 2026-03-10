using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hogs.RPG.Data.Repositories
{
    public class InventoryRepository
    {
        private readonly IGoogleSheetsService _sheets;

        public InventoryRepository(IGoogleSheetsService sheets)
        {
            _sheets = sheets;
        }

        // Load items the player owns
        public async Task<List<InventoryItem>> GetInventoryAsync(ulong discordId)
        {
            var rows = await _sheets.ReadRangeAsync("Inventory", "A2:C");

            var inventory = new List<InventoryItem>();

            foreach (var row in rows)
            {
                if (row.Count < 3)
                    continue;

                if (!ulong.TryParse(row[0]?.ToString(), out var rowDiscordId))
                    continue;

                if (!int.TryParse(row[2]?.ToString(), out var quantity))
                    quantity = 0;

                var item = new InventoryItem
                {
                    DiscordId = rowDiscordId,
                    ItemId = row[1]?.ToString()?.Trim(),
                    Quantity = quantity
                };

                if (item.DiscordId == discordId)
                    inventory.Add(item);
            }

            return inventory;
        }

        // Add an item to the player (if the player already owns the item, it will take the old and new amounts +=
        public async Task AddItemAsync(ulong discordId, string itemId, int amount)
        {
            if (amount <= 0)
                return;

            var rows = await _sheets.ReadRangeAsync("Inventory", "A2:C");

            int rowIndex = 2;

            foreach (var row in rows)
            {
                if (row.Count < 3)
                {
                    rowIndex++;
                    continue;
                }

                if (!ulong.TryParse(row[0]?.ToString(), out var rowDiscordId))
                {
                    rowIndex++;
                    continue;
                }

                var rowItemId = row[1]?.ToString()?.Trim();

                if (rowDiscordId == discordId && rowItemId == itemId)
                {
                    int quantity = 0;
                    int.TryParse(row[2]?.ToString(), out quantity);

                    quantity += amount;

                    var values = new List<IList<object>>
                    {
                        new List<object> { quantity }
                    };

                    await _sheets.UpdateRangeAsync("Inventory", $"C{rowIndex}", values);
                    return;
                }

                rowIndex++;
            }

            // Append new row if player doesn't have item yet
            await _sheets.AppendRowAsync("Inventory", new List<object>
            {
                discordId.ToString(),
                itemId,
                amount
            });
        }

        // Remove an item from the player
        public async Task RemoveItemAsync(ulong discordId, string itemId, int amount)
        {
            if (amount <= 0)
                return;

            var rows = await _sheets.ReadRangeAsync("Inventory", "A2:C");

            int rowIndex = 2;

            foreach (var row in rows)
            {
                if (row.Count < 3)
                {
                    rowIndex++;
                    continue;
                }

                if (!ulong.TryParse(row[0]?.ToString(), out var rowDiscordId))
                {
                    rowIndex++;
                    continue;
                }

                var rowItemId = row[1]?.ToString()?.Trim();

                if (rowDiscordId == discordId && rowItemId == itemId)
                {
                    int quantity = 0;
                    int.TryParse(row[2]?.ToString(), out quantity);

                    quantity -= amount;

                    if (quantity < 0)
                        quantity = 0;

                    var values = new List<IList<object>>
                    {
                        new List<object> { quantity }
                    };

                    await _sheets.UpdateRangeAsync("Inventory", $"C{rowIndex}", values);
                    return;
                }

                rowIndex++;
            }
        }
    }
}
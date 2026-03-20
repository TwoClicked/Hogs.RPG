using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    public class InventoryModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InventoryService _inventoryService;
        private readonly ItemService _itemService;
        public InventoryModule(InventoryService inventoryService, ItemService itemservice)
        {
            _inventoryService = inventoryService;
            _itemService = itemservice;
        }



        [SlashCommand("inventory", "View your inventory")]
        public async Task Inventory()
        {
            var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);

            if (inventory.Count == 0)
            {
                await RespondAsync("Your inventory is empty.");
                return;
            }

            var builder = new StringBuilder();

            var grouped = inventory
                .Where(i => i.Quantity > 0)
                .Select(i => new
                {
                    Item = InventoryItemDefinitions.All[i.ItemId],
                    Amount = i.Quantity
                })
                .GroupBy(i => i.Item.Type);

            foreach (var group in grouped)
            {
                string header = group.Key switch
                {
                    "Material" => "🧱 **Materials**",
                    "Potion" => "🧪 **Potions**",
                    "Equipment" => "⚔️ **Gear**",
                    _ => "📦 **Other**"
                };

                builder.AppendLine(header);
                builder.AppendLine(); // ONE empty line after header

                foreach (var entry in group.OrderBy(i => i.Item.Name))
                {
                    builder.AppendLine($"{entry.Item.Icon} {entry.Item.Name} x{entry.Amount}");
                }

                builder.AppendLine(); // space between categories
            }

            await RespondAsync(builder.ToString());
        }

    }
}
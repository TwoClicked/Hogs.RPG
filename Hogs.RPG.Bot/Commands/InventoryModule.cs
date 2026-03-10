using Discord;
using Discord.Interactions;
using Hogs.RPG.Core.Entities;
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

        [SlashCommand("giveitem", "Give yourself an item (testing)")]
        public async Task GiveItem(string itemId, int amount)
        {
            var userId = Context.User.Id;

            await _inventoryService.GiveItemAsync(userId, itemId, amount);

            await RespondAsync($"You received {amount}x {itemId}");
        }

        [SlashCommand("inventory", "View your inventory")]
        public async Task Inventory()
        {
            await DeferAsync(ephemeral: true);

            var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);

            if (inventory.Count == 0)
            {
                await FollowupAsync("🎒 Your inventory is empty.", ephemeral: true);
                return;
            }

            var items = new StringBuilder();

            foreach (var item in inventory)
            {
                var definition = _itemService.GetItem(item.ItemId);

                if (definition != null)
                {
                    items.AppendLine($"{definition.Icon} **{definition.Name}** x{item.Quantity}");
                }
                else
                {
                    items.AppendLine($"• {item.ItemId} x{item.Quantity}");
                }
            }

            var embed = new EmbedBuilder()
                .WithTitle($"🎒 Your inventory")
                .WithDescription(items.ToString())
                .WithColor(Color.DarkBlue)
                .WithFooter("HOGS RPG")
                .WithTimestamp(DateTime.UtcNow)
                .Build();

            await FollowupAsync(embed: embed, ephemeral: true);
        }

    }
}
// Hogs.RPG.Bot/Commands/GearSetModule.cs

using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.GameplayServices;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    [Group("gearset", "Manage your gear sets")]
    public class GearSetModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly GearSetService _gearSetService;

        public GearSetModule(GearSetService gearSetService)
        {
            _gearSetService = gearSetService;
        }

        // =========================
        // /gearset save <slot> <name>
        // =========================
        [SlashCommand("save", "Save your current gear as a named set")]
        public async Task SaveSet(
            [Summary("slot", "Set slot to save into (1, 2 or 3)")]
            [MinValue(1), MaxValue(3)] int slot,
            [Summary("name", "A name for this set")]
            string name)
        {
            await DeferAsync(ephemeral: true);

            if (string.IsNullOrWhiteSpace(name))
            {
                await FollowupAsync("❌ Please provide a name for the set.", ephemeral: true);
                return;
            }

            var result = await _gearSetService.SaveSetAsync(Context.User.Id, slot, name.Trim());
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // /gearset load <slot>
        // =========================
        [SlashCommand("load", "Swap to a saved gear set")]
        public async Task LoadSet(
            [Summary("slot", "Set slot to load")]
            [Autocomplete(typeof(GearSetAutocompleteHandler))] int slot)
        {
            await DeferAsync(ephemeral: true);

            var result = await _gearSetService.LoadSetAsync(Context.User.Id, slot);
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // /gearset view
        // =========================
        [SlashCommand("view", "View all your saved gear sets")]
        public async Task ViewSets()
        {
            await DeferAsync(ephemeral: true);

            var sets = await _gearSetService.GetSetsAsync(Context.User.Id);

            var embed = new EmbedBuilder()
                .WithTitle("🗃️ Your Gear Sets")
                .WithColor(new Color(0xB8860B));

            for (int i = 1; i <= 3; i++)
            {
                var set = sets.FirstOrDefault(s => s.SetIndex == i);

                if (set == null)
                {
                    embed.AddField($"Set {i}", $"_Empty — use `/gearset save {i} <name>` to save_", inline: false);
                    continue;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"**{set.SetName}**");
                sb.AppendLine();

                AppendSlotLine(sb, "🗡️ Main Hand", set.MainHand);
                AppendSlotLine(sb, "🏹 Off Hand", set.OffHand);
                AppendSlotLine(sb, "🪖 Helmet", set.Helmet);
                AppendSlotLine(sb, "🛡️ Body", set.Body);
                AppendSlotLine(sb, "👖 Legs", set.Legs);
                AppendSlotLine(sb, "🧤 Gloves", set.Gloves);
                AppendSlotLine(sb, "🥾 Boots", set.Boots);
                AppendSlotLine(sb, "💍 Ring", set.Ring);
                AppendSlotLine(sb, "📿 Amulet", set.Amulet);

                embed.AddField($"Set {i}", sb.ToString(), inline: false);
            }

            embed.WithFooter("Use /gearset load <slot> to swap · /gearset save <slot> <name> to overwrite");

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        // =========================
        // /gearset delete <slot>
        // =========================
        [SlashCommand("delete", "Delete a saved gear set")]
        public async Task DeleteSet(
            [Summary("slot", "Set slot to delete")]
            [Autocomplete(typeof(GearSetAutocompleteHandler))] int slot)
        {
            await DeferAsync(ephemeral: true);

            var result = await _gearSetService.DeleteSetAsync(Context.User.Id, slot);
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // HELPER
        // =========================
        private static void AppendSlotLine(StringBuilder sb, string label, string? itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                sb.AppendLine($"{label}: —");
            }
            else
            {
                var name = EquipmentRegistry.All.TryGetValue(itemId, out var def)
                    ? def.Name
                    : itemId;
                sb.AppendLine($"{label}: {name}");
            }
        }
    }
}
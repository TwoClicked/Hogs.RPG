using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Services.GatheringServices;
using System.Threading.Tasks;

[BossLock]
[GearSwapLock]
public class GatherModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly GatherService _gatherService;

    public GatherModule(GatherService gatherService)
    {
        _gatherService = gatherService;
    }

    [SlashCommand("gather", "Gather materials from an area")]
    public async Task Gather(
        [Autocomplete(typeof(GatherAreaAutocompleteHandler))] string area,
        [Autocomplete(typeof(GatherEnergyAutocompleteHandler))] string energy = "1")
    {
        await DeferAsync(ephemeral: true);

        // =========================
        // VALIDATION
        // =========================
        if (string.IsNullOrWhiteSpace(area))
        {
            await FollowupAsync("Select an area to gather.", ephemeral: true);
            return;
        }

        int energyAmount;

        // =========================
        // PARSE INPUT
        // =========================
        if (!int.TryParse(energy, out energyAmount))
        {
            if (energy.ToLower() == "max")
            {
                energyAmount = -1;
            }
            else
            {
                await FollowupAsync("Invalid energy amount. Use a number or 'max'.", ephemeral: true);
                return;
            }
        }

        // =========================
        // EXECUTE
        // =========================
        var result = await _gatherService.GatherAsync(Context.User.Id, area, energyAmount);

        await FollowupAsync(result, ephemeral: true);
    }
}
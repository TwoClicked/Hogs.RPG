using Discord.Interactions;
using Hogs.RPG.Services.GatheringServices;
using System.Threading.Tasks;

public class GatherModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly GatherService _gatherService;

    public GatherModule(GatherService gatherService)
    {
        _gatherService = gatherService;
    }

    [SlashCommand("gather", "Gather materials from an area")]
    public async Task Gather(
        [Autocomplete(typeof(GatherAreaAutocompleteHandler))]
        string area,
        int energy)
    {
        var result = await _gatherService.GatherAsync(Context.User.Id, area, energy);

        await RespondAsync(result);
    }
}
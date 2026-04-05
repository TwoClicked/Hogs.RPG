using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Registries;

public static class PetPassiveFormatter
{
    public static string Format(PetPassive? passive)
    {
        if (passive == null)
            return "None";

        if (!PetPassiveRegistry.All.TryGetValue(passive.Value, out var def))
            return passive.ToString();

        return $"{def.Name} — {def.Description}";
    }
}
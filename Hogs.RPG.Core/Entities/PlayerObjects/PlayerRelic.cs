using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities.PlayerObjects
{
    public class PlayerRelic
    {
        public int Id { get; set; }

        public ulong DiscordId { get; set; }

        public Player Player { get; set; } = null!;

        public string RelicId { get; set; } = "";

        public int Rank { get; set; } = 1;

        public RelicBonusType BonusType { get; set; }

        public bool IsEquipped { get; set; } = false;

        public int SlotIndex { get; set; } = 0;

        // Set to true while this relic is listed on the player market.
        // Prevents equipping while listed.
        public bool IsListed { get; set; } = false;
    }
}

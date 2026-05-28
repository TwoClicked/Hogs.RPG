namespace Hogs.RPG.Core.Entities
{
    public class PlayerPet
    {
        public int Id { get; set; }

        public ulong DiscordId { get; set; }

        public string PetId { get; set; } = "";

        public int Level { get; set; } = 1;

        public int XP { get; set; } = 0;

        public bool IsEquipped { get; set; } = false;

        public int? Passive1 { get; set; }

        public int? Passive2 { get; set; }

        public string? CustomName { get; set; }

        // Set to true while this pet is listed on the player market.
        // Prevents equipping, trading, or pet-evolve while listed.
        public bool IsListed { get; set; } = false;

        public Player Player { get; set; } = null!;
    }
}

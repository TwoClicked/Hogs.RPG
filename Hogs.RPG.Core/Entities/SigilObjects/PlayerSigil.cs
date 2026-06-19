namespace Hogs.RPG.Core.Entities.SigilObjects
{
    public class PlayerSigil
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public string SigilId { get; set; } = "";
        public int Count { get; set; } = 0;
    }
}

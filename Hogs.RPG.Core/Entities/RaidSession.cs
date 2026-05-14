using Hogs.RPG.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hogs.RPG.Core.Entities
{
    public class RaidSession
    {
        public int Id { get; set; }
        public int Tier { get; set; }
        public RaidStatus Status { get; set; } = RaidStatus.Lobby;
        public ulong LeaderDiscordId { get; set; }
        public ulong LobbyChannelId { get; set; }
        public ulong LobbyMessageId { get; set; }
        public ulong ThreadId { get; set; } = 0;
        public int BossCurrentHp { get; set; }
        public int BossMaxHp { get; set; }
        public int BossAttack { get; set; }
        public int BossDefense { get; set; }
        public int CurrentRound { get; set; } = 0;
        public ulong AggroDiscordId { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Stored in DB as serialized string
        public string ActiveEffectsData { get; set; } = "";

        // Runtime only
        [NotMapped]
        public List<ActiveRaidEffect> ActiveEffects { get; set; } = new();

        public List<RaidParticipant> Participants { get; set; } = new();

        public void DeserializeEffects()
        {
            ActiveEffects.Clear();

            if (string.IsNullOrWhiteSpace(ActiveEffectsData))
                return;

            var effects = ActiveEffectsData.Split(';');

            foreach (var effect in effects)
            {
                var parts = effect.Split('|');

                if (parts.Length == 4)
                {
                    ActiveEffects.Add(new ActiveRaidEffect
                    {
                        EffectType = Enum.Parse<ActiveEffectType>(parts[0]),
                        TargetDiscordId = string.IsNullOrEmpty(parts[1]) ? null : ulong.Parse(parts[1]),
                        RoundsRemaining = int.Parse(parts[2]),
                        Value = double.Parse(parts[3])
                    });
                }
            }
        }

        public void SerializeEffects()
        {
            ActiveEffectsData = string.Join(";", ActiveEffects.Select(e =>
                $"{e.EffectType}|{e.TargetDiscordId?.ToString() ?? ""}|{e.RoundsRemaining}|{e.Value}"
            ));
        }
    }
}
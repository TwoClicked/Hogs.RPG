// Hogs.RPG.Core/Entities/RaidObjects/RaidSession.cs
using Hogs.RPG.Core.Enums.RaidEnums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hogs.RPG.Core.Entities.RaidObjects
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

        // Identifies which RaidParticipant row currently has boss aggro.
        // Was previously a DiscordId, but that breaks for solo raids where all 3
        // participant rows share one DiscordId — Participant.Id is unique per row
        // regardless of mode, so this generalizes cleanly to both group and solo.
        // 0 = unset (mirrors the old DiscordId=0 sentinel).
        public int AggroParticipantId { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime RoundStartedAt { get; set; } = DateTime.UtcNow;
        public ulong RoundStatusMessageId { get; set; } = 0;
        public int LastAggroSwapRound { get; set; } = 0;

        // True when a single player is filling all 3 role slots instead of a real
        // 3-player party. Drives the reward/attempt-tracking and key-cost branches
        // in RaidService — the combat resolution itself doesn't need to care.
        public bool IsSolo { get; set; } = false;

        // Running total of potions consumed by the healer across the entire raid.
        // Settled at raid end (victory or wipe) and split across the party.
        public int PotionsConsumedThisRaid { get; set; } = 0;

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
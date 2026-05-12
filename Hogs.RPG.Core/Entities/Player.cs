using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class Player
    {
        public int PlayerId { get; set; }
        public ulong DiscordId { get; set; }
        public string Username { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public int Gold { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }

        // Hunt
        public int HunterStamina { get; set; }
        public string? LastHunterStaminaUpdate { get; set; }

        // Gear
        public string? MainHand { get; set; }
        public string? OffHand { get; set; }
        public string? Helmet { get; set; }
        public string? Body { get; set; }
        public string? Legs { get; set; }
        public string? Gloves { get; set; }
        public string? Boots { get; set; }
        public string? Ring { get; set; }
        public string? Amulet { get; set; }

        // 🔥 STORED IN DB
        public string ActiveBuffsData { get; set; } = "";

        // 🔥 NOT MAPPED (runtime only)
        [NotMapped]
        public List<ActiveBuff> ActiveBuffs { get; set; } = new();

        public bool AutoUseXpPotions { get; set; }

        // Gathering
        public int Energy { get; set; }
        public string? LastEnergyUpdate { get; set; }

        // Bosses
        public string? LastBossAttack { get; set; }
        // Raids
        public string? LastRaidAt { get; set; }

        //LeaderBoard
        public int DungeonRunsCompleted { get; set; }

        // Shop perks
        public DateTime? XpBoostExpiry { get; set; }
        public DateTime? StaminaBoostExpiry { get; set; }

        // =========================
        // 🔄 BUFF SERIALIZATION
        // =========================

        public void DeserializeBuffs()
        {
            ActiveBuffs.Clear();

            if (string.IsNullOrWhiteSpace(ActiveBuffsData))
                return;

            var buffs = ActiveBuffsData.Split(';');

            foreach (var buff in buffs)
            {
                var parts = buff.Split('|');

                if (parts.Length == 3)
                {
                    ActiveBuffs.Add(new ActiveBuff
                    {
                        Type = Enum.Parse<BuffType>(parts[0]),
                        Value = double.Parse(parts[1]),
                        RemainingUses = int.Parse(parts[2])
                    });
                }
            }
        }

        public void SerializeBuffs()
        {
            ActiveBuffsData = string.Join(";", ActiveBuffs.Select(b =>
                $"{b.Type}|{b.Value}|{b.RemainingUses}"
            ));
        }
    }
}
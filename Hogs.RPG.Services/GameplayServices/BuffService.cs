using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Enums;
using System.Linq;

namespace Hogs.RPG.Services.GameplayServices
{
    public class BuffService
    {
        public double ApplyXpBuff(Player player)
        {
            double multiplier = 1;

            var buff = player.ActiveBuffs
                .FirstOrDefault(b => b.Type == BuffType.XP);

            if (buff == null)
                return multiplier;

            multiplier = buff.Value;

            buff.RemainingUses--;

            if (buff.RemainingUses <= 0)
            {
                player.ActiveBuffs.Remove(buff);
            }

            return multiplier;
        }

        public double ApplyGoldBuff(Player player)
        {
            double multiplier = 1;

            var buff = player.ActiveBuffs
                .FirstOrDefault(b => b.Type == BuffType.Gold);

            if (buff == null)
                return multiplier;

            multiplier = buff.Value;

            buff.RemainingUses--;

            if (buff.RemainingUses <= 0)
            {
                player.ActiveBuffs.Remove(buff);
            }

            return multiplier;
        }

        public double ApplyLootBuff(Player player)
        {
            double multiplier = 1;

            var buff = player.ActiveBuffs
                .FirstOrDefault(b => b.Type == BuffType.Loot);

            if (buff == null)
                return multiplier;

            multiplier = buff.Value;

            buff.RemainingUses--;

            if (buff.RemainingUses <= 0)
            {
                player.ActiveBuffs.Remove(buff);
            }

            return multiplier;
        }

        public double ApplyBossDamageBuff(Player player)
        {
            double multiplier = 1;

            var buff = player.ActiveBuffs
                .FirstOrDefault(b => b.Type == BuffType.BossDamage);

            if (buff == null)
                return multiplier;

            multiplier = buff.Value;

            buff.RemainingUses--;

            if (buff.RemainingUses <= 0)
            {
                player.ActiveBuffs.Remove(buff);
            }

            return multiplier;
        }
    }
}
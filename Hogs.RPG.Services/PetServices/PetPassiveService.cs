using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.PetObjects;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Enums;

public class PetPassiveService
{
    private readonly Random _random = new();

    /// <summary>
    /// Modifies outgoing damage based on the pet's active passives.
    /// Returns modified damage and any trigger text to append to combat log.
    /// </summary>
    public (int damage, string triggerText) ModifyOutgoingDamage(int baseDamage, PlayerPet pet, PetDefinition def, int targetCurrentHp, int targetMaxHp)
    {
        int damage = baseDamage;
        var triggers = new List<string>();

        if (pet == null) return (damage, null);

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.DoubleStrike:
                    if (_random.NextDouble() < 0.20)
                    {
                        damage *= 2;
                        triggers.Add($"⚡ **Double Strike!** You hit twice for **{damage}** total damage!");
                    }
                    break;

                case PetPassive.Executioner:
                    double hpPercent = (double)targetCurrentHp / targetMaxHp;
                    if (hpPercent < 0.30)
                    {
                        int bonus = (int)(damage * 0.25);
                        damage = (int)(damage * 1.25);
                        triggers.Add($"💀 **Executioner!** +25% damage on low HP enemy (+{bonus})!");
                    }
                    break;
            }
        }

        string text = triggers.Count > 0 ? string.Join("\n", triggers) : null;
        return (damage, text);
    }

    /// <summary>
    /// Modifies incoming damage based on the pet's active passives.
    /// Returns modified damage and any trigger text to append to combat log.
    /// </summary>
    public (int damage, string triggerText) ModifyIncomingDamage(int incomingDamage, PlayerPet pet)
    {
        int damage = incomingDamage;
        string triggerText = null;

        if (pet == null) return (damage, null);

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.GuardianShield:
                    if (_random.NextDouble() < 0.15)
                    {
                        int blocked = incomingDamage - (int)(incomingDamage * 0.7);
                        damage = (int)(incomingDamage * 0.7);
                        triggerText = $"🛡️ **Guardian Shield!** Blocked {blocked} damage! ({incomingDamage} → {damage})";
                    }
                    break;
            }
        }

        return (damage, triggerText);
    }

    /// <summary>
    /// Applies on-hit effects after dealing damage (e.g. lifesteal).
    /// Returns the amount of HP the player should recover.
    /// </summary>
    public int ApplyOnHitEffects(int damageDealt, Player player, PlayerPet pet)
    {
        if (pet == null) return 0;

        int healing = 0;

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.Lifesteal:
                    healing += (int)(damageDealt * 0.10);
                    break;
            }
        }

        return healing;
    }

    /// <summary>
    /// Applies on-hit-taken effects after receiving damage (e.g. thorns).
    /// Returns the amount of damage to reflect back to the attacker.
    /// </summary>
    public int ApplyOnHitTaken(int damageTaken, PlayerPet pet)
    {
        if (pet == null) return 0;

        int reflect = 0;

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.Thorns:
                    reflect += (int)(damageTaken * 0.15) + 5;
                    break;
            }
        }

        return reflect;
    }

    private List<PetPassive> GetActivePassives(PlayerPet pet)
    {
        var list = new List<PetPassive>();
        if (pet.Passive1 != null) list.Add(pet.Passive1.Value);
        if (pet.Passive2 != null) list.Add(pet.Passive2.Value);
        return list;
    }
}
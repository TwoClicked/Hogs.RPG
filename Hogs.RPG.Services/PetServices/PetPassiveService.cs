using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

public class PetPassiveService
{
    private readonly Random _random = new();

    /// <summary>
    /// Modifies outgoing damage based on the pet's active passives.
    /// Called before damage is applied to the target.
    /// </summary>
    public int ModifyOutgoingDamage(int baseDamage, PlayerPet pet, PetDefinition def, int targetCurrentHp, int targetMaxHp)
    {
        int damage = baseDamage;

        // No pet equipped — return base damage unmodified
        if (pet == null) return damage;

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.DoubleStrike:
                    // 20% chance to deal double damage
                    if (_random.NextDouble() < 0.20)
                        damage *= 2;
                    break;

                case PetPassive.Executioner:
                    // Deal 25% bonus damage when target is below 30% HP
                    double hpPercent = (double)targetCurrentHp / targetMaxHp;
                    if (hpPercent < 0.30)
                        damage = (int)(damage * 1.25);
                    break;
            }
        }

        return damage;
    }

    /// <summary>
    /// Modifies incoming damage based on the pet's active passives.
    /// Called before damage is applied to the player.
    /// </summary>
    public int ModifyIncomingDamage(int incomingDamage, PlayerPet pet)
    {
        int damage = incomingDamage;

        // No pet equipped — return incoming damage unmodified
        if (pet == null) return damage;

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.GuardianShield:
                    // 15% chance to reduce incoming damage by 30%
                    if (_random.NextDouble() < 0.15)
                        damage = (int)(damage * 0.7);
                    break;
            }
        }

        return damage;
    }

    /// <summary>
    /// Applies on-hit effects after dealing damage (e.g. lifesteal).
    /// Returns the amount of HP the player should recover.
    /// </summary>
    public int ApplyOnHitEffects(int damageDealt, Player player, PlayerPet pet)
    {
        // No pet equipped — no healing to apply
        if (pet == null) return 0;

        int healing = 0;

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.Lifesteal:
                    // Heal the player for 10% of the damage dealt
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
        // No pet equipped — no damage to reflect
        if (pet == null) return 0;

        int reflect = 0;

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.Thorns:
                    // Reflect 15% of damage taken + 5 flat damage back to attacker
                    reflect += (int)(damageTaken * 0.15) + 5;
                    break;
            }
        }

        return reflect;
    }

    /// <summary>
    /// Collects all active (non-null) passives from the pet's two passive slots.
    /// </summary>
    private List<PetPassive> GetActivePassives(PlayerPet pet)
    {
        var list = new List<PetPassive>();

        if (pet.Passive1 != null)
            list.Add(pet.Passive1.Value);

        if (pet.Passive2 != null)
            list.Add(pet.Passive2.Value);

        return list;
    }
}
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

public class PetPassiveService
{
    private readonly Random _random = new();

    public int ModifyOutgoingDamage(int baseDamage, PlayerPet pet, PetDefinition def, int targetCurrentHp, int targetMaxHp)
    {
        int damage = baseDamage;

        if (pet == null) return damage;

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.DoubleStrike:
                    if (_random.NextDouble() < 0.20) // 20% chance
                        damage *= 2;
                    break;

                case PetPassive.Executioner:
                    double hpPercent = (double)targetCurrentHp / targetMaxHp;
                    if (hpPercent < 0.30)
                        damage = (int)(damage * 1.25);
                    break;
            }
        }

        return damage;
    }

    public int ModifyIncomingDamage(int incomingDamage, PlayerPet pet)
    {
        int damage = incomingDamage;

        if (pet == null) return damage;

        foreach (var passive in GetActivePassives(pet))
        {
            switch (passive)
            {
                case PetPassive.GuardianShield:
                    if (_random.NextDouble() < 0.15)
                        damage = (int)(damage * 0.7);
                    break;
            }
        }

        return damage;
    }

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

        if (pet.Passive1 != null)
            list.Add(pet.Passive1.Value);

        if (pet.Passive2 != null)
            list.Add(pet.Passive2.Value);

        return list;
    }
}
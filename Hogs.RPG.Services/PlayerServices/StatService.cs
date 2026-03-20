using Hogs.RPG.Core.Entities;
using Hogs.RPG.Services.GameplayServices;

public class StatService
{
    private readonly EquipmentService _equipmentService;

    public StatService(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    public (int attack, int defense, int health) CalculateStats(Player player)
    {
        int attack = player.Attack;
        int defense = player.Defense;
        int health = player.MaxHealth;

        var equippedItems = new[]
        {
            player.MainHand,
            player.OffHand,
            player.Helmet,
            player.Body,
            player.Legs,
            player.Gloves,
            player.Boots,
            player.Ring,
            player.Amulet
        };

        foreach (var itemId in equippedItems)
        {
            if (string.IsNullOrEmpty(itemId)) continue;

            var item = _equipmentService.GetEquipment(itemId);
            if (item == null) continue;

            attack += item.Attack;
            defense += item.Defense;
            health += item.Health;
        }

        return (attack, defense, health);
    }
}
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.GameplayServices
{
    public class StatService
    {
        private readonly EquipmentService _equipmentService;

        public StatService(EquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
        }

        public CombatStats Calculate(Player player)
        {
            var stats = new CombatStats
            {
                Attack = player.Attack,
                Defense = player.Defense,
                Health = player.Health
            };

            AddItem(stats, player.MainHand);
            AddItem(stats, player.OffHand);
            AddItem(stats, player.Helmet);
            AddItem(stats, player.Body);
            AddItem(stats, player.Legs);
            AddItem(stats, player.Gloves);
            AddItem(stats, player.Boots);
            AddItem(stats, player.Ring);
            AddItem(stats, player.Amulet);

            return stats;
        }

        private void AddItem(CombatStats stats, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return;

            var item = _equipmentService.GetEquipment(itemId);

            if (item == null)
                return;

            stats.Attack += item.Attack;
            stats.Defense += item.Defense;
            stats.Health += item.Health;
        }
    }
}
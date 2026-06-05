using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.EquipmentObjects
{
    public class InventoryItem
    {


        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public string ItemId { get; set; }
        public int Quantity { get; set; }

    }
}

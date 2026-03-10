using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class InventoryItem
    {

        public ulong DiscordId { get; set; }
        public string ItemId { get; set; }
        public int Quantity { get; set; }

    }
}

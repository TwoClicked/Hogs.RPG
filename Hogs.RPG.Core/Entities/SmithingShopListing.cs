using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class SmithingShopListing
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }

        // The smithed item id e.g. "bronze_sword"
        public string ItemId { get; set; }

        // How many are currently listed in the shop
        public int Quantity { get; set; }
    }
}

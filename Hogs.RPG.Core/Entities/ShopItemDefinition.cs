using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class ShopItemDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public ShopCategory Category { get; set; }
        public ShopItemType Type { get; set; }

        // Fixed price items
        public int Price { get; set; }

        // Auction items — minimum bid to START the auction
        public int StartingBid { get; set; }

        // If set, only users with this role can buy/bid
        public ulong? RequiredRoleId { get; set; }

        // For Discord reward items that assign a role
        public ulong? RewardRoleId { get; set; }
    }
}

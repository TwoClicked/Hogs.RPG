using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class ShopPurchase
    {
        public int Id { get; set; }

        public ulong BuyerDiscordId { get; set; }

        public string ItemId { get; set; }

        public string ItemName { get; set; }

        public int GoldPaid { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        public bool IsFulfilled { get; set; } = false;

        public DateTime? FulfilledAt { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class TradeSession
    {
        public ulong Player1Id { get; set; }
        public ulong Player2Id { get; set; }

        public Dictionary<string, int> Player1Offer { get; set; } = new();
        public Dictionary<string, int> Player2Offer { get; set; } = new();

        public bool Player1Confirmed { get; set; }
        public bool Player2Confirmed { get; set; }

        public TradeState State { get; set; } = TradeState.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool WarningSent { get; set; } = false;
        public int Player1Gold { get; set; }
        public int Player2Gold { get; set; }
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class ActiveAuction
    {
        public int Id { get; set; }

        public string ItemId { get; set; }

        public ulong StartedByDiscordId { get; set; }

        public int StartingBid { get; set; }

        public int CurrentBid { get; set; }

        public ulong? CurrentBidderDiscordId { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public bool IsEnded { get; set; } = false;

        public DateTime? EndedAt { get; set; }

        // UI tracking — so the bot can update the embed when a new bid comes in
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
    }
}
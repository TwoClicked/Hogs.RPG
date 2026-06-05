using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.PlayerObjects
{
    public class PlayerRelicShard
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public Player Player { get; set; }
        public int Tier { get; set; }
        public int Quantity { get; set; } = 0;

    }
}

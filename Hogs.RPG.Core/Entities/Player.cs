using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class Player
    {

        public int PlayerId { get; set; }
        public ulong DiscordId { get; set; }
        public string Username { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public int Gold { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Health { get; set; }
        public string LastHunt { get; set; }

    }
}

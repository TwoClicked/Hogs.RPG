using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.RaidServices
{
    public class RaidReward
    {
        public ulong DiscordId { get; set; }
        public RaidRole Role { get; set; }
        public int Gold { get; set; }
        public int PlayerXp { get; set; }
        public int PetXp { get; set; }
        public bool ShardDropped { get; set; }
        public int ShardTier { get; set; }
        public string LevelUpMessage { get; set; } = "";
    }
}

using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities
{
    public class PetDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }
        public int Tier { get; set; }

        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseHealth { get; set; }

        public float Scaling { get; set; }
    }
}

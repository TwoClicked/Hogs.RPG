using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.PetObjects
{
    public class PetDrop
    {
        public string PetId { get; set; }
        public int ChancePercent { get; set; } // e.g. 3 = 3%
    }
}

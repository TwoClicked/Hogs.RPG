using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.Equipment
{
    public class FanculoGear
    {

        public static readonly EquipmentDefinition FanculoHelm = new()
        {
            Id = "fanculo_helm",
            Name = "Fanculo Horned Helm",
            Slot = EquipmentSlot.Helmet,
            Defense = 20,
            Attack = 20,
            Health = 150
        };

    }
}

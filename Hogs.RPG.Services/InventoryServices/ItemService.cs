using Hogs.RPG.Core.Entities;
using System.Collections.Generic;

namespace Hogs.RPG.Services.GameplayServices
{
    public class ItemService
    {
        private readonly Dictionary<string, ItemDefinition> _items = new()
        {
            {
                "fur",
                new ItemDefinition
                {
                    Id = "fur",
                    Name = "Fur",
                    Icon = "🐺",
                    Type = "Material",
                    Description = "Soft fur from hunted animals."
                }
            },
            {
                "meat",
                new ItemDefinition
                {
                    Id = "meat",
                    Name = "Meat",
                    Icon = "🥩",
                    Type = "Material",
                    Description = "Fresh meat from hunted animals."
                }
            },
            {
                "leather",
                new ItemDefinition
                {
                    Id = "leather",
                    Name = "Leather",
                    Icon = "🦬",
                    Type = "Material",
                    Description = "Tough leather used for crafting armor."
                }
            }
        };

        public ItemDefinition GetItem(string id)
        {
            if (_items.TryGetValue(id, out var item))
                return item;

            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.RecipeObjects
{
    public class Recipe
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ResultItem { get; set; }

        public int ResultAmount { get; set; }

        public Dictionary<string, int> Materials { get; set; } = new();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Data
{
    public class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
    {
        public GameDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=HogsRPG;Trusted_Connection=True;");

            return new GameDbContext(optionsBuilder.Options);
        }
    }
}

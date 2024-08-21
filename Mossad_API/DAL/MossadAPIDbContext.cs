using System.Collections.Generic;
using Mossad_API.Moddels;
using Microsoft.EntityFrameworkCore;

namespace Mossad_API.DAL
{
    public class MossadAPIDbContext : DbContext
    {

        public MossadAPIDbContext(DbContextOptions<MossadAPIDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options;
        }

        public DbSet<Agent> agents { get; set; }
        public DbSet<Mission> missions { get; set; }
        public DbSet<Target> targets { get; set; }

    }
}


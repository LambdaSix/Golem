using System;
using System.Data.Entity;
using System.Linq;

namespace Golem.Database
{

    public class GolemDbContext : DbContext
    {
        public DbSet<Component> Components { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<EntityComponent> EntityComponents { get; set; }
        public DbSet<ComponentType> ComponentTypes { get; set; }

        public GolemDbContext()
            : base("name=GolemDb")
        {
        }
    }
}
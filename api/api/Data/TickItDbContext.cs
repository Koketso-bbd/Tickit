using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class TickItDbContext : DbContext
    {
        public TickItDbContext(DbContextOptions options) : base(options) { }

        public virtual DbSet<User> users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}

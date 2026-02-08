using DotNetBlueprint.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetBlueprint.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProjectBlueprint> ProjectBlueprints { get; set; }
        public DbSet<User> Users { get; set; }
    }
}

using DevTools.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace DevTools.Application.Database
{
    public class DevToolsContext : DbContext
    {
        public DevToolsContext(DbContextOptions<DevToolsContext> options) : base(options)
        {
        }

        public DbSet<HueColor> HueColors { get; set; }
        public DbSet<Flags> Flags { get; set; }
        public DbSet<User> Users { get; set; }
        
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Flags>()
                .HasIndex(u => u.Name)
                .IsUnique();
            
            builder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
using DevTools.Application.Models;
using DevTools.Application.Models.Citadels;
using Microsoft.EntityFrameworkCore;

namespace DevTools.Application.Database;

public class DevToolsContext : DbContext
{
    public DevToolsContext(DbContextOptions<DevToolsContext> options) : base(options)
    {
    }

    public DbSet<HueColor> HueColors { get; set; }
    public DbSet<Flags> Flags { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<File> Files { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Hand> Hands { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerResult> PlayerResults { get; set; }
    public DbSet<Turn> Turns { get; set; }
        
        

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Flags>()
            .HasIndex(e => e.Name)
            .IsUnique();
            
        builder.Entity<User>()
            .HasIndex(e => e.Username)
            .IsUnique();
        
        builder.Entity<File>()
            .HasIndex(e => e.Guid)
            .IsUnique();

        // builder.Entity<Game>()
        //     .HasMany<Hand>()
        //     .WithOne(e => e.Game)
        //     .HasForeignKey(e => e.GameId);
        //
        // builder.Entity<Hand>()
        //     .HasMany<Turn>()
        //     .WithOne(e => e.Hand)
        //     .HasForeignKey(e => e.HandId);
        //
        // builder.Entity<Turn>()
        //     .HasOne<Player>()
        //     .WithOne(e => e.Turn)
        //     .HasForeignKey<Turn>(e => e.PlayerId);
        //
        // builder.Entity<Game>()
        //     .HasMany<PlayerResult>()
        //     .WithOne(e => e.Game)
        //     .HasForeignKey(e => e.GameId);
        //
        // builder.Entity<PlayerResult>()
        //     .HasOne<Player>()
        //     .WithOne(e => e.PlayerResult)
        //     .HasForeignKey<PlayerResult>(e => e.PlayerId);
    }
}
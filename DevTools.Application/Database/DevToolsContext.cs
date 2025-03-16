using System;
using DevTools.Application.Models;
using DevTools.Application.Models.Citadels;
using DevTools.Application.Models.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
    public DbSet<QuizShow> QuizShows { get; set; }
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
        
        builder.Entity<Turn>()
            .HasOne(x => x.Player)
            .WithMany(x => x.Turns)
            .HasForeignKey(x => x.PlayerId);
        
        builder.Entity<Turn>()
            .HasOne(x => x.Hand)
            .WithMany(x => x.Turns)
            .HasForeignKey(x => x.HandId);
        
        builder.Entity<Hand>()
            .HasOne(x => x.Game)
            .WithMany(x => x.Hands)
            .HasForeignKey(x => x.GameId);
        
        var converter = new ValueConverter<JokerType, string>(
            v => v.ToString(),
            v => (JokerType)Enum.Parse(typeof(JokerType), v));

        builder.Entity<Joker>()
            .Property(e => e.JokerType)
            .HasMaxLength(20)
            .HasConversion(converter);
        
        builder.Entity<Team>()
            .HasIndex(e => new {e.Name, e.QuizShowId})
            .IsUnique();

        builder.Entity<Team>()
            .HasIndex(e => new {e.TeamId, e.QuizShowId})
            .IsUnique();

        builder.Entity<TeamAnswer>()
            .HasIndex(e => new {e.TeamId, e.QuestionIndex, e.AnswerId})
            .IsUnique();


    }
}
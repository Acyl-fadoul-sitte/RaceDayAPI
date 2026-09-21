using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Event> Events { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Enrolment> Enrolments { get; set; }

    public DbSet<Result> Results { get; set; }

    public DbSet<RaceDayAPI.Models.Route> Routes { get; set; }

    public DbSet<Weather> Weather { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>()
            .Property(c => c.EntryFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Event)
            .WithMany()
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant)
            .WithMany()
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
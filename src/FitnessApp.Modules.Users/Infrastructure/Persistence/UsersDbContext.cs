using FitnessApp.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Users.Infrastructure.Persistence;

public class UsersDbContext : DbContext
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Preference> Preferences => Set<Preference>();

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>(b =>
        {
            b.HasKey(u => u.UserId);
            
            // Configure value objects
            b.OwnsOne(u => u.Name, n =>
            {
                n.Property(fn => fn.FirstName).HasColumnName("FirstName").HasMaxLength(100);
                n.Property(ln => ln.LastName).HasColumnName("LastName").HasMaxLength(100);
            });
            
            b.OwnsOne(u => u.DateOfBirth, dob =>
            {
                dob.Property(d => d.Value).HasColumnName("DateOfBirth");
            });
            
            b.OwnsOne(u => u.PhysicalMeasurements, pm =>
            {
                pm.Property(p => p.HeightCm).HasColumnName("HeightCm").HasPrecision(5, 2);
                pm.Property(p => p.WeightKg).HasColumnName("WeightKg").HasPrecision(5, 2);
                pm.Property(p => p.BMI).HasColumnName("BMI").HasPrecision(4, 2);
            });
            
            // Configure enum properties
            b.Property(u => u.Gender).HasConversion<string>();
            b.Property(u => u.FitnessLevel).HasConversion<string>();
            b.Property(u => u.PrimaryFitnessGoal).HasConversion<string>();

            // Configure Subscription as owned entity
            b.OwnsOne(u => u.Subscription, s =>
            {
                s.Property(sub => sub.Level).HasColumnName("SubscriptionLevel").HasConversion<string>();
                s.Property(sub => sub.StartDate).HasColumnName("SubscriptionStartDate");
                s.Property(sub => sub.EndDate).HasColumnName("SubscriptionEndDate");
            });

            // Configure relationships
            b.HasMany(u => u.Preferences)
             .WithOne()
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(u => u.CreatedAt);
        });

        modelBuilder.Entity<Preference>(b =>
        {
            b.HasKey(p => p.Id);
            
            b.Property(p => p.Category)
             .HasMaxLength(100)
             .IsRequired();
             
            b.Property(p => p.Key)
             .HasMaxLength(100)
             .IsRequired();
             
            b.Property(p => p.Value)
             .IsRequired();

            b.HasIndex(p => new { p.UserId, p.Category, p.Key }).IsUnique();
        });

        modelBuilder.HasDefaultSchema("users");
    }
}
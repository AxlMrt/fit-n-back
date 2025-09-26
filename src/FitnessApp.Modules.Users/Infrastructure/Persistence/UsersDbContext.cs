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
        modelBuilder.HasDefaultSchema("users");

        modelBuilder.Entity<UserProfile>(b =>
        {
            b.HasKey(u => u.UserId);
            
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
                pm.Property(p => p.Height).HasColumnName("Height").HasPrecision(5, 2);
                pm.Property(p => p.Weight).HasColumnName("Weight").HasPrecision(5, 2);
                pm.Property(p => p.BMI).HasColumnName("BMI").HasPrecision(4, 2);
                pm.Property(p => p.HeightUnit).HasColumnName("HeightUnit").HasMaxLength(10);
                pm.Property(p => p.WeightUnit).HasColumnName("WeightUnit").HasMaxLength(10);
            });
            
            b.Property(u => u.Gender).HasConversion<string>();
            b.Property(u => u.FitnessLevel).HasConversion<string>();
            b.Property(u => u.FitnessGoal).HasConversion<string>();

            b.OwnsOne(u => u.Subscription, s =>
            {
                s.Property(sub => sub.Level).HasColumnName("SubscriptionLevel").HasConversion<string>();
                s.Property(sub => sub.StartDate).HasColumnName("SubscriptionStartDate");
                s.Property(sub => sub.EndDate).HasColumnName("SubscriptionEndDate");
            });

            b.HasIndex(u => u.CreatedAt);
        });

        modelBuilder.Entity<Preference>(b =>
        {
            b.HasKey(p => p.Id);
            
            b.Property(p => p.UserId).IsRequired();
            
            b.Property(p => p.Category)
             .HasConversion<string>()
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
using FitnessApp.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Users.Infrastructure.Persistence;

public class UsersDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Preference> Preferences => Set<Preference>();

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            
            // Configure value objects
            b.OwnsOne(u => u.Name, n =>
            {
                n.Property(fn => fn.FirstName).HasColumnName("FirstName").HasMaxLength(100);
                n.Property(ln => ln.LastName).HasColumnName("LastName").HasMaxLength(100);
            });
            
            b.OwnsOne(u => u.Email, e =>
            {
                e.Property(em => em.Value).HasColumnName("Email").HasMaxLength(255);
                e.HasIndex(em => em.Value).IsUnique();
            });
            
            b.OwnsOne(u => u.Username, un =>
            {
                un.Property(u => u.Value).HasColumnName("Username").HasMaxLength(50);
                un.HasIndex(u => u.Value).IsUnique();
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
            
            // Configure enum
            b.Property(u => u.Gender).HasConversion<string>();
            
            // Configure other properties  
            b.Property(u => u.PasswordHash).HasMaxLength(500);
            b.Property(u => u.SecurityStamp).HasMaxLength(100);
            b.Property(u => u.Role).HasConversion<string>();
            b.Property(u => u.FitnessLevel).HasConversion<string>();
            b.Property(u => u.PrimaryFitnessGoal).HasConversion<string>();

            // Configure relationships
            b.HasMany(u => u.Preferences)
             .WithOne(p => p.User)
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
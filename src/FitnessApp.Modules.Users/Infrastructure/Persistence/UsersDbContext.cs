using FitnessApp.Modules.Authorization.Enums;
using FitnessApp.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Users.Infrastructure.Persistence;

public class UsersDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Preference> Preferences => Set<Preference>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.Email).IsUnique();
            b.HasIndex(u => u.UserName).IsUnique();

            b.HasOne(u => u.Profile)
             .WithOne(p => p.User)
             .HasForeignKey<UserProfile>(p => p.UserId)
             .IsRequired();

            b.HasMany(u => u.Preferences)
             .WithOne(p => p.User)
             .HasForeignKey(p => p.UserId)
             .IsRequired();

            b.Property(u => u.Role)
                .HasConversion<string>()           // stocke "Admin", "Coach", ...
                .HasMaxLength(32)
                .IsRequired()
                .HasDefaultValue(Role.Athlete);

            b.HasIndex(u => u.Role);
        });

        modelBuilder.Entity<UserProfile>(b =>
        {
            b.HasKey(p => p.UserId); 
            b.HasOne(p => p.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .IsRequired();
        });

        modelBuilder.Entity<Preference>(b =>
        {
            b.HasIndex(p => new { p.UserId, p.Category, p.Key }).IsUnique();
        });

        modelBuilder.Entity<Subscription>(b =>
        {
            b.HasKey(s => s.Id);
            
            b.HasOne(s => s.User)
             .WithOne(u => u.Subscription)
             .HasForeignKey<Subscription>(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(s => s.Level)
             .HasConversion<string>()
             .HasMaxLength(32)
             .IsRequired();

            b.HasIndex(s => s.UserId);
        });

        modelBuilder.HasDefaultSchema("users");
    }
}
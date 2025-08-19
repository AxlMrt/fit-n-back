using FitnessApp.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Users.Infrastructure.Persistence;

public class UsersDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
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
            b.HasIndex(u => u.Email).IsUnique();
            b.HasIndex(u => u.UserName).IsUnique();
            
            b.HasOne(u => u.Profile)
             .WithOne(p => p.User)
             .HasForeignKey<UserProfile>(p => p.UserId)
             .IsRequired();

            b.HasOne(u => u.Subscription)
             .WithOne(s => s.User)
             .HasForeignKey<Subscription>(s => s.UserId);

            b.HasMany(u => u.Preferences)
             .WithOne(p => p.User)
             .HasForeignKey(p => p.UserId)
             .IsRequired();
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

        modelBuilder.HasDefaultSchema("users");
    }
}
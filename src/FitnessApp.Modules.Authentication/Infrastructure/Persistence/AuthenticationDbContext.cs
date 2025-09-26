using FitnessApp.Modules.Authentication.Domain.Entities;
using FitnessApp.Modules.Authentication.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Authentication.Infrastructure.Persistence;

/// <summary>
/// Database context for Authentication module.
/// Contains only authentication-related entities.
/// </summary>
public class AuthenticationDbContext : DbContext
{
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
    {
    }

    public DbSet<AuthUser> AuthUsers { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema for all entities
        modelBuilder.HasDefaultSchema("auth");

        // Configure AuthUser entity
        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.ToTable("AuthUsers", "auth");
            entity.HasKey(e => e.Id);
            
            // Configure Email value object
            entity.OwnsOne(e => e.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(320)
                    .IsRequired();
                
                email.HasIndex(e => e.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_AuthUsers_Email");
            });

            // Configure Username value object
            entity.OwnsOne(e => e.Username, username =>
            {
                username.Property(e => e.Value)
                    .HasColumnName("Username")
                    .HasMaxLength(30)
                    .IsRequired();
                
                username.HasIndex(e => e.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_AuthUsers_Username");
            });

            // Configure PasswordHash value object
            entity.OwnsOne(e => e.PasswordHash, passwordHash =>
            {
                passwordHash.Property(e => e.Value)
                    .HasColumnName("PasswordHash")
                    .HasMaxLength(500)
                    .IsRequired();
            });

            // Configure properties
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.SecurityStamp)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.PasswordResetToken)
                .HasMaxLength(200);

            entity.Property(e => e.EmailVerificationToken)
                .HasMaxLength(200);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            // Indexes for performance
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_AuthUsers_CreatedAt");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_AuthUsers_IsActive");

            entity.HasIndex(e => e.LastLoginAt)
                .HasDatabaseName("IX_AuthUsers_LastLoginAt");
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", "auth");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.UsedAt);
            
            entity.Property(e => e.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.RevokedAt);
            
            entity.Property(e => e.RevokedReason)
                .HasMaxLength(200);
            
            entity.Property(e => e.CreatedByIpAddress)
                .HasMaxLength(50);

            // Indexes
            entity.HasIndex(e => e.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshTokens_Token");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_RefreshTokens_UserId");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("IX_RefreshTokens_ExpiresAt");
                
            entity.HasIndex(e => e.IsUsed)
                .HasDatabaseName("IX_RefreshTokens_IsUsed");
                
            entity.HasIndex(e => e.IsRevoked)
                .HasDatabaseName("IX_RefreshTokens_IsRevoked");
                
            // Composite index for efficient queries
            entity.HasIndex(e => new { e.UserId, e.IsUsed, e.IsRevoked, e.ExpiresAt })
                .HasDatabaseName("IX_RefreshTokens_Active");
        });
    }
}

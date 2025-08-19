using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Content.Domain.Entities;

namespace FitnessApp.Modules.Content.Infrastructure.Persistence;

public class ContentDbContext : DbContext
{
    public DbSet<MediaAsset> MediaAssets { get; set; }
    // Optional: explicit DbSet for join table (no entity type)
    public DbSet<ExerciseMediaAsset> ExerciseMediaAssets { get; set; }

    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("content");

        modelBuilder.Entity<MediaAsset>(b =>
        {
            b.ToTable("media_assets");
            b.HasKey(a => a.Id);
            b.Property(a => a.Key).IsRequired();
            b.Property(a => a.Url).IsRequired();
            b.Property(a => a.Type).IsRequired().HasMaxLength(50);
            b.Property(a => a.CreatedAt).IsRequired();
            b.HasIndex(a => a.Key).IsUnique();
        });

        // Join table: exercises.exercise_id <-> content.media_assets.media_asset_id
        modelBuilder.Entity<ExerciseMediaAsset>(b =>
        {
            b.HasNoKey();
            b.ToTable("exercise_media_assets", "content");
        });
    }
}

public class ExerciseMediaAsset
{
    public Guid ExerciseId { get; set; }
    public Guid MediaAssetId { get; set; }
}

using FitnessApp.Modules.Exercises.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Exercises.Infrastructure.Persistence
{
    public class ExercisesDbContext : DbContext
    {
        public ExercisesDbContext(DbContextOptions<ExercisesDbContext> options) : base(options)
        {
        }

        public DbSet<Exercise> Exercises { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Exercise>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .IsRequired(false)
                    .HasMaxLength(1000);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.MuscleGroups)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(e => e.ImageContentId)
                    .IsRequired(false);

                entity.Property(e => e.VideoContentId)
                    .IsRequired(false);

                entity.Property(e => e.Difficulty)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(e => e.Equipment)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasConversion(
                        v => v.ToString(),
                        v => Domain.ValueObjects.Equipment.FromString(v));

                entity.Property(e => e.Instructions)
                    .IsRequired(false)
                    .HasMaxLength(2000);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .IsRequired(false);

                // Indexes for performance
                entity.HasIndex(e => e.Name)
                    .IsUnique();

                entity.HasIndex(e => e.Type);

                entity.HasIndex(e => e.Difficulty);

                entity.HasIndex(e => e.IsActive);

                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}

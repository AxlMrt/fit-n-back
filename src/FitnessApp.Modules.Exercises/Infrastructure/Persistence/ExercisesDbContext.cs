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

            modelBuilder.Entity<Exercise>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Name).IsRequired().HasMaxLength(200);
                b.Property(e => e.Type).IsRequired();
                b.Property(e => e.MuscleGroups).IsRequired();
                b.Property(e => e.ImageContentId).IsRequired(false);
                b.Property(e => e.VideoContentId).IsRequired(false);
                b.Property(e => e.Difficulty).HasConversion<int>().IsRequired();
                b.Property(e => e.Equipment)
                    .HasMaxLength(1000)
                    .HasConversion(
                        v => v.ToString(),
                        v => Domain.ValueObjects.Equipment.FromString(v));
            });
        }
    }
}

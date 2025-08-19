using FitnessApp.Modules.Exercises.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Exercises.Infrastructure.Persistence
{
    public class ExercisesDbContext : DbContext
    {
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Tag> Tags { get; set; }
        // media moved to Content module
        public DbSet<MuscleGroup> MuscleGroups { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<ExerciseTag> ExerciseTags { get; set; }
        public DbSet<ExerciseMuscleGroup> ExerciseMuscleGroups { get; set; }
        public DbSet<ExerciseEquipment> ExerciseEquipment { get; set; }
        public DbSet<ExerciseCategory> ExerciseCategories { get; set; }
        public DbSet<ExerciseVariation> ExerciseVariations { get; set; }

        public ExercisesDbContext(DbContextOptions<ExercisesDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("exercises");

            // Exercise
            modelBuilder.Entity<Exercise>(b =>
            {
                b.ToTable("exercises");
                b.HasKey(e => e.Id);
                b.Property(e => e.Name).IsRequired().HasMaxLength(100);
                b.Property(e => e.Description).IsRequired();
                b.Property(e => e.Instructions).IsRequired();
                b.Property(e => e.DifficultyLevel).IsRequired().HasMaxLength(50);
                b.Property(e => e.CommonMistakes).HasMaxLength(500);
                b.Property(e => e.CreatedAt).IsRequired();
                
                // Add relationship with ExerciseCategory
                b.HasOne<ExerciseCategory>()
                 .WithMany(c => c.Exercises)
                 .HasForeignKey("CategoryId")
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // Tag
            modelBuilder.Entity<Tag>(b =>
            {
                b.ToTable("tags");
                b.HasKey(t => t.Id);
                b.Property(t => t.Name).IsRequired().HasMaxLength(50);
                b.HasIndex(t => t.Name).IsUnique();
            });

            // MuscleGroup
            modelBuilder.Entity<MuscleGroup>(b =>
            {
                b.ToTable("muscle_groups");
                b.HasKey(m => m.Id);
                b.Property(m => m.Name).IsRequired().HasMaxLength(50);
                b.Property(m => m.BodyPart).IsRequired().HasMaxLength(50);
                b.HasIndex(m => m.Name).IsUnique();
            });

            // Equipment
            modelBuilder.Entity<Equipment>(b =>
            {
                b.ToTable("equipment");
                b.HasKey(e => e.Id);
                b.Property(e => e.Name).IsRequired().HasMaxLength(50);
                b.HasIndex(e => e.Name).IsUnique();
            });

            // Many-to-many: Exercise <-> Tag
            modelBuilder.Entity<ExerciseTag>(b =>
            {
                b.ToTable("exercise_tags");
                b.HasKey(et => new { et.ExerciseId, et.TagId });
                
                b.HasOne(et => et.Exercise)
                 .WithMany(e => e.ExerciseTags)
                 .HasForeignKey(et => et.ExerciseId);
                
                b.HasOne(et => et.Tag)
                 .WithMany(t => t.ExerciseTags)
                 .HasForeignKey(et => et.TagId);
            });

            // Many-to-many: Exercise <-> MuscleGroup
            modelBuilder.Entity<ExerciseMuscleGroup>(b =>
            {
                b.ToTable("exercise_muscle_groups");
                b.HasKey(emg => new { emg.ExerciseId, emg.MuscleGroupId });
                b.Property(emg => emg.IsPrimary).IsRequired();
                
                b.HasOne(emg => emg.Exercise)
                 .WithMany(e => e.TargetMuscleGroups)
                 .HasForeignKey(emg => emg.ExerciseId);
                
                b.HasOne(emg => emg.MuscleGroup)
                 .WithMany(mg => mg.Exercises)
                 .HasForeignKey(emg => emg.MuscleGroupId);
            });

            // Many-to-many: Exercise <-> Equipment
            modelBuilder.Entity<ExerciseEquipment>(b =>
            {
                b.ToTable("exercise_equipment");
                b.HasKey(ee => new { ee.ExerciseId, ee.EquipmentId });
                
                b.HasOne(ee => ee.Exercise)
                 .WithMany(e => e.RequiredEquipment)
                 .HasForeignKey(ee => ee.ExerciseId);
                
                b.HasOne(ee => ee.Equipment)
                 .WithMany(e => e.Exercises)
                 .HasForeignKey(ee => ee.EquipmentId);
            });

            // ExerciseCategory
            modelBuilder.Entity<ExerciseCategory>(b =>
            {
                b.ToTable("exercise_categories");
                b.HasKey(c => c.Id);
                b.Property(c => c.Name).IsRequired().HasMaxLength(50);
                b.Property(c => c.Description).IsRequired();
                b.Property(c => c.Icon).HasMaxLength(100);
                b.HasIndex(c => c.Name).IsUnique();
            });

            // ExerciseVariation
            modelBuilder.Entity<ExerciseVariation>(b =>
            {
                b.ToTable("exercise_variations");
                b.HasKey(v => v.Id);
                b.Property(v => v.Name).IsRequired().HasMaxLength(100);
                b.Property(v => v.Description).IsRequired();
                b.Property(v => v.ModificationType).IsRequired().HasMaxLength(20);
                
                b.HasOne(v => v.BaseExercise)
                 .WithMany()
                 .HasForeignKey(v => v.BaseExerciseId)
                 .OnDelete(DeleteBehavior.Restrict);
                
                b.HasOne(v => v.VariationExercise)
                 .WithMany()
                 .HasForeignKey(v => v.VariationExerciseId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
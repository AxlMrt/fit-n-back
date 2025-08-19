// namespace FitnessApp.Modules.Workouts.Domain.Entities;
// public class Measurement
// {
//     public Guid Id { get; private set; }
//     public string Type { get; private set; } // "Repetitions", "Time", "Distance", "Weight"
//     public string Unit { get; private set; } // "reps", "seconds", "meters", "kg"
//     public string Description { get; private set; }
    
//     // Foreign key
//     public Guid ExerciseId { get; private set; }
    
//     // Navigation property
//     public Exercise Exercise { get; private set; }
    
//     private Measurement() { } // For EF Core
    
//     public Measurement(string type, string unit, string description, Exercise exercise)
//     {
//         Id = Guid.NewGuid();
//         Type = type;
//         Unit = unit;
//         Description = description;
//         Exercise = exercise ?? throw new ArgumentNullException(nameof(exercise));
//         ExerciseId = exercise.Id;
        
//         Validate();
//     }
    
//     public void Update(string type, string unit, string description)
//     {
//         Type = type;
//         Unit = unit;
//         Description = description;
        
//         Validate();
//     }
    
//     private void Validate()
//     {
//         if (string.IsNullOrWhiteSpace(Type))
//             throw new ArgumentException("Measurement type cannot be empty");
        
//         if (string.IsNullOrWhiteSpace(Unit))
//             throw new ArgumentException("Measurement unit cannot be empty");
//     }
// }

// Measurement
            // modelBuilder.Entity<Measurement>(b =>
            // {
            //     b.ToTable("measurements");
            //     b.HasKey(m => m.Id);
            //     b.Property(m => m.Type).IsRequired().HasMaxLength(20);
            //     b.Property(m => m.Unit).IsRequired().HasMaxLength(20);
            //     b.Property(m => m.Description).HasMaxLength(200);
                
            //     b.HasOne(m => m.Exercise)
            //      .WithMany()
            //      .HasForeignKey(m => m.ExerciseId)
            //      .OnDelete(DeleteBehavior.Cascade);
            // });
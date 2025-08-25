using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.Domain.ValueObjects;

/// <summary>
/// Value object representing exercise parameters (reps, sets, weight, etc.)
/// </summary>
public record ExerciseParameters
{
    public int? Reps { get; init; }
    public int? Sets { get; init; }
    public TimeSpan? Duration { get; init; }
    public double? Weight { get; init; }
    public TimeSpan? RestTime { get; init; }
    public string? Notes { get; init; }

    public ExerciseParameters(
        int? reps = null, 
        int? sets = null, 
        TimeSpan? duration = null, 
        double? weight = null, 
        TimeSpan? restTime = null, 
        string? notes = null)
    {
        if (reps.HasValue && reps <= 0)
            throw new WorkoutDomainException("Reps must be positive");
            
        if (sets.HasValue && sets <= 0)
            throw new WorkoutDomainException("Sets must be positive");
            
        if (weight.HasValue && weight < 0)
            throw new WorkoutDomainException("Weight cannot be negative");

        if (duration.HasValue && duration <= TimeSpan.Zero)
            throw new WorkoutDomainException("Duration must be positive");

        if (restTime.HasValue && restTime < TimeSpan.Zero)
            throw new WorkoutDomainException("Rest time cannot be negative");

        Reps = reps;
        Sets = sets;
        Duration = duration;
        Weight = weight;
        RestTime = restTime;
        Notes = notes?.Trim();
    }

    public static ExerciseParameters ForReps(int reps, int sets, TimeSpan? restTime = null)
        => new(reps: reps, sets: sets, restTime: restTime);

    public static ExerciseParameters ForDuration(TimeSpan duration, int? sets = null, TimeSpan? restTime = null)
        => new(duration: duration, sets: sets, restTime: restTime);

    public static ExerciseParameters ForWeightTraining(int reps, int sets, double weight, TimeSpan? restTime = null)
        => new(reps: reps, sets: sets, weight: weight, restTime: restTime);

    public bool IsEmpty => !Reps.HasValue && !Sets.HasValue && !Duration.HasValue && !Weight.HasValue && !RestTime.HasValue;
}

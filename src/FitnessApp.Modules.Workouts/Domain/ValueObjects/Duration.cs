using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.Domain.ValueObjects;

/// <summary>
/// Value object representing workout duration
/// </summary>
public record Duration
{
    public TimeSpan Value { get; }

    public Duration(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new WorkoutDomainException("Duration must be positive");
            
        if (duration > TimeSpan.FromHours(5))
            throw new WorkoutDomainException("Duration cannot exceed 5 hours");

        Value = duration;
    }

    public static Duration FromMinutes(int minutes)
    {
        return new Duration(TimeSpan.FromMinutes(minutes));
    }

    public static Duration FromHours(double hours)
    {
        return new Duration(TimeSpan.FromHours(hours));
    }

    public int TotalMinutes => (int)Value.TotalMinutes;
    public double TotalHours => Value.TotalHours;

    public static implicit operator TimeSpan(Duration duration) => duration.Value;
    public static explicit operator Duration(TimeSpan timeSpan) => new(timeSpan);

    public override string ToString() => Value.ToString(@"hh\:mm");
}

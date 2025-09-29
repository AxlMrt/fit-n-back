using FitnessApp.Modules.Tracking.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Domain.Entities;

/// <summary>
/// Represents a workout scheduled for a specific date
/// </summary>
public class PlannedWorkout
{
    private PlannedWorkout() { } // For EF Core

    public PlannedWorkout(
        Guid userId,
        Guid workoutId,
        DateTime scheduledDate,
        bool isFromProgram = false,
        Guid? programId = null)
    {
        if (userId == Guid.Empty)
            throw TrackingDomainException.UserIdCannotBeEmpty();
            
        if (workoutId == Guid.Empty)
            throw TrackingDomainException.WorkoutIdCannotBeEmpty();

        Id = Guid.NewGuid();
        UserId = userId;
        WorkoutId = workoutId;
        ScheduledDate = scheduledDate.Date; // Store only the date part
        IsFromProgram = isFromProgram;
        ProgramId = programId;
        Status = WorkoutSessionStatus.Planned;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid WorkoutId { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public WorkoutSessionStatus Status { get; private set; }
    public bool IsFromProgram { get; private set; }
    public Guid? ProgramId { get; private set; }
    public Guid? WorkoutSessionId { get; private set; } // Link to actual session when started
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    #region Status Management

    /// <summary>
    /// Mark as started and link to workout session
    /// </summary>
    public void MarkAsStarted(Guid workoutSessionId)
    {
        if (Status != WorkoutSessionStatus.Planned)
            throw TrackingDomainException.CannotStartPlannedWorkout(Status.ToString());

        if (workoutSessionId == Guid.Empty)
            throw TrackingDomainException.WorkoutSessionIdCannotBeEmpty();

        Status = WorkoutSessionStatus.InProgress;
        WorkoutSessionId = workoutSessionId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark as completed
    /// </summary>
    public void MarkAsCompleted()
    {
        if (Status != WorkoutSessionStatus.InProgress)
            throw TrackingDomainException.CannotCompletePlannedWorkout(Status.ToString());

        Status = WorkoutSessionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark as cancelled
    /// </summary>
    public void Cancel()
    {
        if (Status != WorkoutSessionStatus.Planned)
            throw TrackingDomainException.CannotCancelPlannedWorkout(Status.ToString());

        Status = WorkoutSessionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark as abandoned
    /// </summary>
    public void MarkAsAbandoned()
    {
        if (Status != WorkoutSessionStatus.InProgress)
            throw TrackingDomainException.CannotAbandonPlannedWorkout(Status.ToString());

        Status = WorkoutSessionStatus.Abandoned;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Update Methods

    /// <summary>
    /// Reschedule the workout to a different date
    /// </summary>
    public void Reschedule(DateTime newScheduledDate)
    {
        if (Status != WorkoutSessionStatus.Planned)
            throw TrackingDomainException.CannotRescheduleWorkout(Status.ToString());

        ScheduledDate = newScheduledDate.Date;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if the planned workout is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return Status == WorkoutSessionStatus.Planned && 
               ScheduledDate.Date < DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Check if the planned workout is upcoming
    /// </summary>
    public bool IsUpcoming()
    {
        return Status == WorkoutSessionStatus.Planned && 
               ScheduledDate.Date >= DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Check if the planned workout is scheduled for today
    /// </summary>
    public bool IsScheduledForToday()
    {
        return Status == WorkoutSessionStatus.Planned && 
               ScheduledDate.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Get days until scheduled date (negative if overdue)
    /// </summary>
    public int DaysUntilScheduled()
    {
        return (ScheduledDate.Date - DateTime.UtcNow.Date).Days;
    }

    #endregion
}

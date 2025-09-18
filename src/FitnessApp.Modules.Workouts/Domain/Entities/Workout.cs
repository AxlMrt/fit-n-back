using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Domain.Entities;

/// <summary>
/// Aggregate root representing a complete workout session
/// </summary>
public class Workout
{
    private readonly List<WorkoutPhase> _phases = [];

    public Workout(
        string name,
        WorkoutType type,
        WorkoutCategory category,
        DifficultyLevel difficulty,
        int estimatedDurationMinutes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new WorkoutDomainException("Workout name is required");
            
        if (name.Length > 200)
            throw new WorkoutDomainException("Workout name cannot exceed 200 characters");

        if (estimatedDurationMinutes <= 0 || estimatedDurationMinutes > 300)
            throw new WorkoutDomainException("Duration must be between 1 and 300 minutes");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Type = type;
        Category = category;
        Difficulty = difficulty;
        EstimatedDurationMinutes = estimatedDurationMinutes;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public WorkoutType Type { get; private set; }
    public WorkoutCategory Category { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public int EstimatedDurationMinutes { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Content references
    public Guid? ImageContentId { get; private set; }
    
    // User/Coach references
    public Guid? CreatedByUserId { get; private set; }
    public Guid? CreatedByCoachId { get; private set; }

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public IReadOnlyList<WorkoutPhase> Phases => _phases.AsReadOnly();

    // Computed properties
    public int PhaseCount => _phases.Count;
    public int TotalExercises => _phases.Sum(p => p.Exercises.Count);

    #region Phase Management

    public void AddPhase(WorkoutPhaseType type, string name, int estimatedDurationMinutes)
    {
        if (_phases.Any(p => p.Type == type))
            throw new WorkoutDomainException($"Phase of type {type} already exists");

        var nextOrder = _phases.Count + 1;
        var phase = new WorkoutPhase(type, name, estimatedDurationMinutes, nextOrder);
        _phases.Add(phase);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePhase(WorkoutPhaseType type)
    {
        var phase = GetPhase(type);
        if (phase == null)
            throw new WorkoutDomainException($"Phase of type {type} not found");

        _phases.Remove(phase);
        UpdatePhaseOrders();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MovePhase(WorkoutPhaseType type, int newOrder)
    {
        var phase = GetPhase(type);
        if (phase == null)
            throw new WorkoutDomainException($"Phase of type {type} not found");

        if (newOrder < 1 || newOrder > _phases.Count)
            throw new WorkoutDomainException($"Order must be between 1 and {_phases.Count}");

        var currentOrder = phase.Order;
        
        // If no change needed, return
        if (currentOrder == newOrder)
            return;

        // Temporarily set the phase order to be able to sort properly
        phase.UpdateOrder(newOrder);
        
        // Adjust orders of other phases
        foreach (var otherPhase in _phases.Where(p => p != phase))
        {
            if (currentOrder < newOrder)
            {
                // Moving down: shift phases up
                if (otherPhase.Order > currentOrder && otherPhase.Order <= newOrder)
                {
                    otherPhase.UpdateOrder(otherPhase.Order - 1);
                }
            }
            else
            {
                // Moving up: shift phases down
                if (otherPhase.Order >= newOrder && otherPhase.Order < currentOrder)
                {
                    otherPhase.UpdateOrder(otherPhase.Order + 1);
                }
            }
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdatePhaseOrders()
    {
        var orderedPhases = _phases.OrderBy(p => p.Order).ToList();
        for (int i = 0; i < orderedPhases.Count; i++)
        {
            orderedPhases[i].UpdateOrder(i + 1);
        }
    }

    #endregion

    #region Update Methods

    public void UpdateDetails(
        string name,
        string? description = null,
        DifficultyLevel? difficulty = null,
        int? estimatedDurationMinutes = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Length > 200)
                throw new WorkoutDomainException("Workout name cannot exceed 200 characters");
            Name = name.Trim();
        }

        Description = description?.Trim();

        if (difficulty.HasValue)
            Difficulty = difficulty.Value;

        if (estimatedDurationMinutes.HasValue)
        {
            if (estimatedDurationMinutes.Value <= 0 || estimatedDurationMinutes.Value > 300)
                throw new WorkoutDomainException("Duration must be between 1 and 300 minutes");
            EstimatedDurationMinutes = estimatedDurationMinutes.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public WorkoutPhase? GetPhase(WorkoutPhaseType type) =>
        _phases.FirstOrDefault(p => p.Type == type);

    public bool HasPhase(WorkoutPhaseType type) =>
        _phases.Any(p => p.Type == type);

    public void SetImageContent(Guid? contentId)
    {
        ImageContentId = contentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public int CalculateActualDurationMinutes()
    {
        if (!_phases.Any())
            return 1; // Minimum 1 minute if no phases

        var totalMinutes = _phases.Sum(p => p.EstimatedDurationMinutes);
        return Math.Max(1, totalMinutes); // Ensure at least 1 minute
    }

    #endregion

    #region Factory Methods

    public static Workout CreateUserWorkout(
        string name,
        WorkoutCategory category,
        DifficultyLevel difficulty,
        int estimatedDurationMinutes,
        Guid userId)
    {
        var workout = new Workout(name, WorkoutType.UserCreated, category, difficulty, estimatedDurationMinutes);
        workout.CreatedByUserId = userId;
        return workout;
    }

    public static Workout CreateCoachWorkout(
        string name,
        WorkoutCategory category,
        DifficultyLevel difficulty,
        int estimatedDurationMinutes,
        Guid coachId)
    {
        var workout = new Workout(name, WorkoutType.Template, category, difficulty, estimatedDurationMinutes);
        workout.CreatedByCoachId = coachId;
        return workout;
    }

    public static Workout CreatePresetWorkout(
        string name,
        WorkoutCategory category,
        DifficultyLevel difficulty,
        int estimatedDurationMinutes)
    {
        return new Workout(name, WorkoutType.Template, category, difficulty, estimatedDurationMinutes);
    }

    public static Workout CreateAIGeneratedWorkout(
        string name,
        WorkoutCategory category,
        DifficultyLevel difficulty,
        int estimatedDurationMinutes,
        Guid userId)
    {
        var workout = new Workout(name, WorkoutType.AIGenerated, category, difficulty, estimatedDurationMinutes);
        workout.CreatedByUserId = userId;
        return workout;
    }

    #endregion
}
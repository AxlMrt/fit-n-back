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
        DifficultyLevel difficulty)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw WorkoutDomainException.NameRequired();
            
        if (name.Length > 200)
            throw WorkoutDomainException.NameTooLong(200);

        Id = Guid.NewGuid();
        Name = name.Trim();
        Type = type;
        Category = category;
        Difficulty = difficulty;
        EstimatedDurationMinutes = 1;
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

    // ✅ Méthode moderne - Durée calculée automatiquement
    /// <summary>
    /// Adds a new phase to the workout with automatic duration calculation
    /// </summary>
    public void AddPhase(WorkoutPhaseType type, string name)
    {
        if (_phases.Any(p => p.Type == type))
            throw WorkoutDomainException.DuplicatePhase(type.ToString());

        var nextOrder = _phases.Count + 1;
        var phase = new WorkoutPhase(type, name, nextOrder);
        _phases.Add(phase);
        RecalculateWorkoutDuration();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePhase(WorkoutPhaseType type)
    {
        var phase = GetPhase(type);
        if (phase == null)
            throw WorkoutDomainException.PhaseNotFound(type.ToString());

        _phases.Remove(phase);
        UpdatePhaseOrders();
        RecalculateWorkoutDuration();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MovePhase(WorkoutPhaseType type, int newOrder)
    {
        var phase = GetPhase(type);
        if (phase == null)
            throw WorkoutDomainException.PhaseNotFound(type.ToString());

        if (newOrder < 1 || newOrder > _phases.Count)
            throw WorkoutDomainException.InvalidPhaseOrderRange(_phases.Count);

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

    /// <summary>
    /// Updates workout details with automatic duration recalculation
    /// </summary>
    public void UpdateDetails(
        string name,
        string? description = null,
        DifficultyLevel? difficulty = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Length > 200)
                throw WorkoutDomainException.NameTooLong(200);
            Name = name.Trim();
        }

        Description = description?.Trim();

        if (difficulty.HasValue)
            Difficulty = difficulty.Value;

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

        var totalMinutes = _phases.Sum(p => p.CalculateEstimatedTotalMinutes());
        return Math.Max(1, totalMinutes); // Ensure at least 1 minute
    }

    /// <summary>
    /// Automatically recalculates the total workout duration based on phases and exercises
    /// </summary>
    public void RecalculateWorkoutDuration()
    {
        foreach (var phase in _phases)
        {
            phase.RecalculateDuration();
        }

        EstimatedDurationMinutes = CalculateActualDurationMinutes();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetType(WorkoutType type)
    {
        Type = type;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCreatedBy(Guid userId)
    {
        CreatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Factory Methods

    public static Workout CreateUserWorkout(
        string name,
        WorkoutCategory category,
        DifficultyLevel difficulty,
        Guid userId)
    {
        var workout = new Workout(name, WorkoutType.UserCreated, category, difficulty);
        workout.CreatedByUserId = userId;
        return workout;
    }

    public static Workout CreateCoachWorkout(
        string name,
        WorkoutCategory category,
        DifficultyLevel difficulty,
        Guid coachId)
    {
        var workout = new Workout(name, WorkoutType.Template, category, difficulty);
        workout.CreatedByCoachId = coachId;
        return workout;
    }

    public static Workout CreatePresetWorkout(
        string name,
        WorkoutCategory category,
        DifficultyLevel difficulty)
    {
        return new Workout(name, WorkoutType.Template, category, difficulty);
    }

    public static Workout CreateAIGeneratedWorkout(
        string name,
        WorkoutCategory category,
        DifficultyLevel difficulty,
        Guid userId)
    {
        var workout = new Workout(name, WorkoutType.AIGenerated, category, difficulty);
        workout.CreatedByUserId = userId;
        return workout;
    }

    #endregion
}
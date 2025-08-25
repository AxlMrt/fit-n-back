using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Domain.Exceptions;

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
        DifficultyLevel difficulty,
        Duration estimatedDuration,
        EquipmentType requiredEquipment = EquipmentType.None,
        Guid? createdByUserId = null,
        Guid? createdByCoachId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new WorkoutDomainException("Workout name is required");
            
        if (name.Length > 200)
            throw new WorkoutDomainException("Workout name cannot exceed 200 characters");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Type = type;
        Difficulty = difficulty;
        EstimatedDuration = estimatedDuration ?? throw new ArgumentNullException(nameof(estimatedDuration));
        RequiredEquipment = requiredEquipment;
        CreatedByUserId = createdByUserId;
        CreatedByCoachId = createdByCoachId;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public WorkoutType Type { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public Duration EstimatedDuration { get; private set; } = null!;
    public EquipmentType RequiredEquipment { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Content references
    public Guid? ImageContentId { get; private set; }
    
    // Creator information
    public Guid? CreatedByUserId { get; private set; }
    public Guid? CreatedByCoachId { get; private set; }
    
    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public IReadOnlyList<WorkoutPhase> Phases => _phases.AsReadOnly();

    // Business methods
    public void SetDescription(string? description)
    {
        Description = description?.Trim();
        UpdateTimestamp();
    }

    public void SetImageContent(Guid? imageContentId)
    {
        ImageContentId = imageContentId;
        UpdateTimestamp();
    }

    public void UpdateDifficulty(DifficultyLevel difficulty)
    {
        Difficulty = difficulty;
        UpdateTimestamp();
    }

    public void UpdateRequiredEquipment(EquipmentType equipment)
    {
        RequiredEquipment = equipment;
        UpdateTimestamp();
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new WorkoutDomainException("Workout name is required");
            
        if (name.Length > 200)
            throw new WorkoutDomainException("Workout name cannot exceed 200 characters");

        Name = name.Trim();
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public WorkoutPhase AddPhase(
        WorkoutPhaseType type, 
        string name, 
        Duration estimatedDuration)
    {
        var nextOrder = _phases.Count > 0 ? _phases.Max(p => p.Order) + 1 : 0;
        var phase = new WorkoutPhase(type, name, estimatedDuration, nextOrder);
        _phases.Add(phase);
        
        UpdateEstimatedDuration();
        UpdateTimestamp();
        
        return phase;
    }

    public void RemovePhase(Guid phaseId)
    {
        var phase = _phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        _phases.Remove(phase);
        ReorderPhases();
        UpdateEstimatedDuration();
        UpdateTimestamp();
    }

    public void MovePhase(Guid phaseId, int newOrder)
    {
        var phase = _phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        if (newOrder < 0 || newOrder >= _phases.Count)
            throw new WorkoutDomainException("Invalid phase order position");

        _phases.Remove(phase);
        _phases.Insert(newOrder, phase);
        ReorderPhases();
        UpdateTimestamp();
    }

    private void ReorderPhases()
    {
        for (int i = 0; i < _phases.Count; i++)
        {
            _phases[i].UpdateOrder(i);
        }
    }

    private void UpdateEstimatedDuration()
    {
        if (!_phases.Any())
            return;

        var totalDuration = _phases.Sum(p => p.CalculateTotalDuration().Value.TotalMinutes);
        EstimatedDuration = Duration.FromMinutes((int)totalDuration);
    }

    private void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public WorkoutPhase? GetPhase(WorkoutPhaseType type)
    {
        return _phases.FirstOrDefault(p => p.Type == type);
    }

    public WorkoutPhase? GetPhaseById(Guid phaseId)
    {
        return _phases.FirstOrDefault(p => p.Id == phaseId);
    }

    public bool HasPhase(WorkoutPhaseType type)
    {
        return _phases.Any(p => p.Type == type);
    }

    public int GetTotalExerciseCount()
    {
        return _phases.Sum(p => p.Exercises.Count);
    }

    public bool IsCreatedByUser(Guid userId)
    {
        return CreatedByUserId == userId;
    }

    public bool IsCreatedByCoach(Guid coachId)
    {
        return CreatedByCoachId == coachId;
    }

    public Duration CalculateActualDuration()
    {
        if (!_phases.Any())
            return Duration.FromMinutes(1); // Minimum 1 minute if no phases
            
        var totalMinutes = _phases.Sum(p => p.CalculateTotalDuration().TotalMinutes);
        return Duration.FromMinutes(Math.Max(1, (int)totalMinutes)); // Ensure at least 1 minute
    }

    // Factory methods
    public static Workout CreateUserWorkout(
        string name,
        DifficultyLevel difficulty,
        Duration estimatedDuration,
        EquipmentType requiredEquipment,
        Guid userId)
    {
        return new Workout(name, WorkoutType.UserCreated, difficulty, estimatedDuration, requiredEquipment, userId);
    }

    public static Workout CreateCoachWorkout(
        string name,
        DifficultyLevel difficulty,
        Duration estimatedDuration,
        EquipmentType requiredEquipment,
        Guid coachId)
    {
        return new Workout(name, WorkoutType.Fixed, difficulty, estimatedDuration, requiredEquipment, null, coachId);
    }

    public static Workout CreateDynamicWorkout(
        string name,
        DifficultyLevel difficulty,
        Duration estimatedDuration,
        EquipmentType requiredEquipment)
    {
        return new Workout(name, WorkoutType.Dynamic, difficulty, estimatedDuration, requiredEquipment);
    }
}

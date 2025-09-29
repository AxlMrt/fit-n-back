using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.Domain.Entities;

/// <summary>
/// Represents an exercise within a workout phase with specific parameters
/// </summary>
public class WorkoutExercise
{
    private WorkoutExercise() { } // For EF Core

    // Constructeur principal avec tous les paramètres (nouvelle logique)
    public WorkoutExercise(
        Guid exerciseId, 
        int? sets = null,
        int? reps = null, 
        int? durationSeconds = null,
        double? distance = null,
        double? weight = null,
        int? restSeconds = null,
        int order = 1)
    {
        if (exerciseId == Guid.Empty)
            throw WorkoutDomainException.EmptyExerciseId();
            
        if (order < 1)
            throw WorkoutDomainException.InvalidOrder();

        if (restSeconds.HasValue && restSeconds.Value < 0)
            throw WorkoutDomainException.NegativeRestTime();

        // Validation: au moins un paramètre d'effort doit être spécifié
        if (!sets.HasValue && !reps.HasValue && !durationSeconds.HasValue && !distance.HasValue)
            throw WorkoutDomainException.NoExerciseParameters();

        // Validation pour les exercices basés sur répétitions
        if ((sets.HasValue || reps.HasValue) && (sets <= 0 || reps <= 0))
            throw WorkoutDomainException.InvalidSetsReps();

        // Validation pour la durée
        if (durationSeconds.HasValue && durationSeconds <= 0)
            throw WorkoutDomainException.InvalidDurationValue();

        // Validation pour la distance
        if (distance.HasValue && distance <= 0)
            throw WorkoutDomainException.InvalidDistance();

        // Validation pour le poids
        if (weight.HasValue && weight <= 0)
            throw WorkoutDomainException.InvalidWeight();

        Id = Guid.NewGuid();
        ExerciseId = exerciseId;
        Sets = sets;
        Reps = reps;
        DurationSeconds = durationSeconds;
        Distance = distance;
        Weight = weight;
        RestSeconds = restSeconds;
        Order = order;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Constructeur de compatibilité pour les tests existants (ancienne logique)
    public WorkoutExercise(Guid exerciseId, int sets, int reps, int? restSeconds, int order)
    {
        if (exerciseId == Guid.Empty)
            throw WorkoutDomainException.EmptyExerciseId();
            
        if (order < 1)
            throw WorkoutDomainException.InvalidOrder();

        if (sets <= 0)
            throw WorkoutDomainException.InvalidSets();

        if (reps <= 0)
            throw WorkoutDomainException.InvalidReps();

        if (restSeconds.HasValue && restSeconds.Value < 0)
            throw WorkoutDomainException.NegativeRestTime();

        Id = Guid.NewGuid();
        ExerciseId = exerciseId;
        Sets = sets;
        Reps = reps;
        RestSeconds = restSeconds;
        Order = order;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid ExerciseId { get; private set; }
    public int? Sets { get; private set; }
    public int? Reps { get; private set; }
    public int? DurationSeconds { get; private set; }
    public double? Distance { get; private set; }
    public double? Weight { get; private set; }
    public int? RestSeconds { get; private set; }
    public string? Notes { get; private set; }
    public int Order { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public Guid WorkoutPhaseId { get; private set; }
    public WorkoutPhase WorkoutPhase { get; private set; } = null!;

    #region Update Methods

    // Version surchargée pour la compatibilité avec les tests existants
    public void UpdateParameters(int sets, int reps, int? restSeconds = null)
    {
        if (sets <= 0)
            throw WorkoutDomainException.InvalidSets();

        if (reps <= 0)
            throw WorkoutDomainException.InvalidReps();
            
        if (restSeconds.HasValue && restSeconds.Value < 0)
            throw WorkoutDomainException.NegativeRestTime();
            
        Sets = sets;
        Reps = reps;
        RestSeconds = restSeconds;
        UpdatedAt = DateTime.UtcNow;
    }

    // Version complète avec tous les paramètres
    public void UpdateAllParameters(int? sets = null, int? reps = null, int? durationSeconds = null, 
        double? distance = null, double? weight = null, int? restSeconds = null)
    {
        // Au moins un paramètre d'effort doit être spécifié
        if (!sets.HasValue && !reps.HasValue && !durationSeconds.HasValue && !distance.HasValue)
            throw WorkoutDomainException.NoExerciseParameters();

        // Validation pour les exercices basés sur répétitions
        if ((sets.HasValue || reps.HasValue) && (sets <= 0 || reps <= 0))
            throw WorkoutDomainException.InvalidSetsReps();

        // Validation pour la durée
        if (durationSeconds.HasValue && durationSeconds <= 0)
            throw WorkoutDomainException.InvalidDurationValue();

        // Validation pour la distance
        if (distance.HasValue && distance <= 0)
            throw WorkoutDomainException.InvalidDistance();

        // Validation pour le poids
        if (weight.HasValue && weight <= 0)
            throw WorkoutDomainException.InvalidWeight();
            
        if (restSeconds.HasValue && restSeconds.Value < 0)
            throw WorkoutDomainException.NegativeRestTime();

        Sets = sets ?? Sets;
        Reps = reps ?? Reps;
        DurationSeconds = durationSeconds ?? DurationSeconds;
        Distance = distance ?? Distance;
        Weight = weight ?? Weight;
        RestSeconds = restSeconds ?? RestSeconds;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOrder(int newOrder)
    {
        if (newOrder < 1)
            throw WorkoutDomainException.InvalidOrder();
        Order = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Estimates the total time needed for this exercise in minutes
    /// </summary>
    public int EstimateTimeMinutes()
    {
        // Si une durée spécifique est donnée, l'utiliser
        if (DurationSeconds.HasValue)
        {
            return Math.Max(1, (int)Math.Ceiling(DurationSeconds.Value / 60.0));
        }

        // Si c'est un exercice basé sur la distance (ex: course)
        if (Distance.HasValue)
        {
            // Estimer basé sur un rythme adaptatif selon la distance
            var distanceKm = Distance.Value / 1000.0;
            double paceMinPerKm = EstimateRunningPace(distanceKm);
            var estimatedTimeMinutes = distanceKm * paceMinPerKm;
            return Math.Max(1, (int)Math.Ceiling(estimatedTimeMinutes));
        }

        // Si c'est un exercice basé sur sets/reps
        if (Sets.HasValue && Reps.HasValue)
        {
            var executionTimeSeconds = Sets.Value * Reps.Value * 2; // 2 secondes par rep
            var restTimeSeconds = RestSeconds ?? 45; // 45 secondes par défaut
            var totalRestTimeSeconds = (Sets.Value - 1) * restTimeSeconds;
            var totalSeconds = executionTimeSeconds + totalRestTimeSeconds;
            return Math.Max(1, (int)Math.Ceiling(totalSeconds / 60.0));
        }

        // Par défaut, 1 minute minimum
        return 1;
    }

    /// <summary>
    /// Gets a display string for the exercise parameters
    /// </summary>
    public string GetDisplayParameters()
    {
        if (DurationSeconds.HasValue)
        {
            var minutes = DurationSeconds.Value / 60;
            var seconds = DurationSeconds.Value % 60;
            return seconds > 0 ? $"{minutes}m{seconds}s" : $"{minutes}m";
        }

        if (Distance.HasValue)
        {
            if (Distance.Value >= 1000)
                return $"{Distance.Value / 1000:F1}km";
            else
                return $"{Distance.Value:F0}m";
        }

        if (Sets.HasValue && Reps.HasValue)
        {
            var result = $"{Sets}x{Reps}";
            if (Weight.HasValue)
                result += $" @{Weight}kg";
            if (RestSeconds.HasValue)
                result += $" ({RestSeconds}s rest)";
            return result;
        }

        return "No parameters";
    }

    /// <summary>
    /// Estimates a realistic running pace based on distance and typical fitness levels
    /// </summary>
    private static double EstimateRunningPace(double distanceKm)
    {
        // Adaptation du rythme selon la distance (plus c'est long, plus le rythme ralentit)
        return distanceKm switch
        {
            <= 1.0 => 5.5,    // Sprint/court : 5m30/km
            <= 3.0 => 6.0,    // 1-3km : 6min/km  
            <= 5.0 => 6.5,    // 3-5km : 6m30/km
            <= 10.0 => 7.0,   // 5-10km : 7min/km
            <= 21.0 => 7.5,   // 10-21km : 7m30/km
            _ => 8.0           // Plus de 21km : 8min/km
        };
    }

    #endregion
}

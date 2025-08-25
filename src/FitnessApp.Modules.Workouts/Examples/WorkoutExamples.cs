using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.Examples;

/// <summary>
/// Exemples d'utilisation du module Workouts
/// </summary>
public class WorkoutExamples
{
    /// <summary>
    /// Exemple de création d'un workout complet avec phases et exercices
    /// </summary>
    public static Workout CreateHIITWorkout()
    {
        // Créer le workout principal
        var workout = Workout.CreateCoachWorkout(
            name: "HIIT Full Body Débutant",
            difficulty: DifficultyLevel.Beginner,
            estimatedDuration: Duration.FromMinutes(30),
            requiredEquipment: EquipmentType.None,
            coachId: Guid.NewGuid()
        );

        workout.SetDescription("Séance d'entraînement HIIT complète sans équipement, parfaite pour les débutants.");

        // Phase 1: Échauffement
        var warmupPhase = workout.AddPhase(
            type: WorkoutPhaseType.WarmUp,
            name: "Échauffement Dynamique",
            estimatedDuration: Duration.FromMinutes(5)
        );

        warmupPhase.SetDescription("Préparation du corps pour l'effort principal");

        // Exercices d'échauffement
        warmupPhase.AddExercise(
            exerciseId: Guid.NewGuid(),
            exerciseName: "Marche sur place",
            parameters: ExerciseParameters.ForDuration(
                duration: TimeSpan.FromSeconds(60),
                sets: 1
            )
        );

        warmupPhase.AddExercise(
            exerciseId: Guid.NewGuid(),
            exerciseName: "Rotations des bras",
            parameters: ExerciseParameters.ForReps(
                reps: 10,
                sets: 2,
                restTime: TimeSpan.FromSeconds(15)
            )
        );

        // Phase 2: Effort principal
        var mainPhase = workout.AddPhase(
            type: WorkoutPhaseType.MainEffort,
            name: "Circuit HIIT",
            estimatedDuration: Duration.FromMinutes(20)
        );

        mainPhase.SetDescription("4 tours de 4 exercices, 30s effort / 15s repos");

        // Exercices principaux
        var hiitExercises = new[]
        {
            ("Jumping Jacks", 30, 15),
            ("Squats au poids du corps", 30, 15),
            ("Pompes (genoux si nécessaire)", 30, 15),
            ("Mountain Climbers", 30, 45) // 45s de repos à la fin du tour
        };

        foreach (var (exerciseName, effortTime, restTime) in hiitExercises)
        {
            mainPhase.AddExercise(
                exerciseId: Guid.NewGuid(),
                exerciseName: exerciseName,
                parameters: new ExerciseParameters(
                    sets: 4, // 4 tours
                    duration: TimeSpan.FromSeconds(effortTime),
                    restTime: TimeSpan.FromSeconds(restTime),
                    notes: effortTime == 30 ? "30s effort maximum" : "30s effort maximum, puis 45s de repos entre les tours"
                )
            );
        }

        // Phase 3: Récupération
        var cooldownPhase = workout.AddPhase(
            type: WorkoutPhaseType.Recovery,
            name: "Retour au calme",
            estimatedDuration: Duration.FromMinutes(5)
        );

        cooldownPhase.SetDescription("Récupération active et étirements");

        cooldownPhase.AddExercise(
            exerciseId: Guid.NewGuid(),
            exerciseName: "Marche lente sur place",
            parameters: ExerciseParameters.ForDuration(
                duration: TimeSpan.FromMinutes(2),
                sets: 1
            )
        );

        cooldownPhase.AddExercise(
            exerciseId: Guid.NewGuid(),
            exerciseName: "Étirements des jambes",
            parameters: ExerciseParameters.ForDuration(
                duration: TimeSpan.FromSeconds(30),
                sets: 2
            )
        );

        return workout;
    }

    /// <summary>
    /// Exemple de création d'un workout de musculation avec poids
    /// </summary>
    public static Workout CreateStrengthWorkout()
    {
        var workout = Workout.CreateCoachWorkout(
            name: "Force Haut du Corps",
            difficulty: DifficultyLevel.Advanced,
            estimatedDuration: Duration.FromMinutes(60),
            requiredEquipment: EquipmentType.FreeWeights | EquipmentType.PullUpBar,
            coachId: Guid.NewGuid()
        );

        workout.SetDescription("Séance de musculation ciblée haut du corps avec charges libres.");

        // Échauffement spécifique
        var warmup = workout.AddPhase(
            WorkoutPhaseType.WarmUp,
            "Échauffement Articulaire",
            Duration.FromMinutes(10)
        );

        warmup.AddExercise(
            Guid.NewGuid(),
            "Rotations d'épaules",
            ExerciseParameters.ForReps(15, 2, TimeSpan.FromSeconds(30))
        );

        // Exercices principaux
        var strength = workout.AddPhase(
            WorkoutPhaseType.MainEffort,
            "Exercices de Force",
            Duration.FromMinutes(45)
        );

        // Exercices avec charges progressives
        var strengthExercises = new[]
        {
            ("Développé couché haltères", 8, 4, 25.0, 120),
            ("Tractions prise large", 6, 4, (double?)null, 150),
            ("Développé militaire", 10, 3, 20.0, 90),
            ("Rowing haltère", 10, 3, 22.5, 90)
        };

        foreach (var (name, reps, sets, weight, rest) in strengthExercises)
        {
            var parameters = new ExerciseParameters(
                reps: reps,
                sets: sets,
                weight: weight,
                restTime: TimeSpan.FromSeconds(rest)
            );

            var notes = weight.HasValue ? $"Charge: {weight}kg" : "Poids du corps";
            var parametersWithNotes = new ExerciseParameters(
                reps: parameters.Reps,
                sets: parameters.Sets,
                weight: parameters.Weight,
                restTime: parameters.RestTime,
                notes: notes
            );

            strength.AddExercise(
                Guid.NewGuid(),
                name,
                parametersWithNotes
            );
        }

        // Récupération
        var recovery = workout.AddPhase(
            WorkoutPhaseType.Stretching,
            "Étirements",
            Duration.FromMinutes(10)
        );

        recovery.AddExercise(
            Guid.NewGuid(),
            "Étirement pectoraux",
            new ExerciseParameters(
                duration: TimeSpan.FromSeconds(45), 
                sets: 2
            )
        );

        return workout;
    }

    /// <summary>
    /// Exemple de création d'un workout utilisateur personnalisé
    /// </summary>
    public static Workout CreateUserCustomWorkout(Guid userId)
    {
        var workout = Workout.CreateUserWorkout(
            name: "Ma Routine Matinale",
            difficulty: DifficultyLevel.Beginner,
            estimatedDuration: Duration.FromMinutes(15),
            requiredEquipment: EquipmentType.Mat,
            userId: userId
        );

        workout.SetDescription("Ma routine personnelle pour bien commencer la journée !");

        // Réveil corporel
        var morning = workout.AddPhase(
            WorkoutPhaseType.WarmUp,
            "Réveil du Corps",
            Duration.FromMinutes(15)
        );

        var morningExercises = new[]
        {
            "Étirements chat-vache",
            "Salutation au soleil",
            "Rotations du bassin",
            "Respiration profonde"
        };

        foreach (var exerciseName in morningExercises)
        {
            morning.AddExercise(
                Guid.NewGuid(),
                exerciseName,
                ExerciseParameters.ForDuration(
                    duration: TimeSpan.FromMinutes(3),
                    sets: 1,
                    restTime: TimeSpan.FromSeconds(30)
                )
            );
        }

        return workout;
    }

    /// <summary>
    /// Exemple de modification d'un workout existant
    /// </summary>
    public static void ModifyWorkoutExample(Workout workout)
    {
        // Modifier les propriétés du workout
        workout.UpdateName("HIIT Full Body - Version Modifiée");
        workout.UpdateDifficulty(DifficultyLevel.Intermediate);
        workout.UpdateRequiredEquipment(EquipmentType.ResistanceBands);

        // Ajouter une nouvelle phase
        var stretchingPhase = workout.AddPhase(
            WorkoutPhaseType.Stretching,
            "Étirements Finaux",
            Duration.FromMinutes(5)
        );

        stretchingPhase.AddExercise(
            Guid.NewGuid(),
            "Étirement des quadriceps",
            ExerciseParameters.ForDuration(TimeSpan.FromSeconds(30), 2)
        );

        // Modifier un exercice existant dans une phase
        var mainPhase = workout.GetPhase(WorkoutPhaseType.MainEffort);
        if (mainPhase != null)
        {
            var firstExercise = mainPhase.Exercises.FirstOrDefault();
            if (firstExercise != null)
            {
                // Augmenter l'intensité
                var newParameters = new ExerciseParameters(
                    sets: 5, // Au lieu de 4
                    duration: TimeSpan.FromSeconds(45), // Au lieu de 30
                    restTime: TimeSpan.FromSeconds(15),
                    notes: "Version plus intense - 45s effort"
                );
                
                firstExercise.UpdateParameters(newParameters);
            }
        }

        // Réorganiser les phases
        if (workout.Phases.Count > 2)
        {
            var lastPhase = workout.Phases.Last();
            workout.MovePhase(lastPhase.Id, 1); // Déplacer en 2ème position
        }
    }

    /// <summary>
    /// Exemple de validation métier
    /// </summary>
    public static void ValidationExamples()
    {
        try
        {
            // Ceci lancera une exception - nom vide
            var invalidWorkout = new Workout(
                "", // Nom vide
                WorkoutType.UserCreated,
                DifficultyLevel.Beginner,
                Duration.FromMinutes(30),
                EquipmentType.None
            );
        }
        catch (WorkoutDomainException ex)
        {
            Console.WriteLine($"Erreur de validation: {ex.Message}");
        }

        try
        {
            // Ceci lancera une exception - durée négative
            var invalidDuration = new Duration(TimeSpan.FromMinutes(-10));
        }
        catch (WorkoutDomainException ex)
        {
            Console.WriteLine($"Erreur de durée: {ex.Message}");
        }

        try
        {
            // Paramètres d'exercice invalides
            var invalidParams = new ExerciseParameters(
                reps: -5, // Répétitions négatives
                sets: 3,
                weight: 50
            );
        }
        catch (WorkoutDomainException ex)
        {
            Console.WriteLine($"Erreur de paramètres: {ex.Message}");
        }
    }
}

using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;

namespace FitnessApp.Modules.Workouts.Tests.Helpers;

/// <summary>
/// Factory class for creating test data
/// </summary>
public static class TestDataFactory
{
    public static class Workouts
    {
        public static Workout CreateUserWorkout(
            string name = "Test User Workout",
            DifficultyLevel difficulty = DifficultyLevel.Intermediate,
            int durationMinutes = 30,
            EquipmentType equipment = EquipmentType.None,
            Guid? userId = null)
        {
            return Workout.CreateUserWorkout(
                name,
                difficulty,
                Duration.FromMinutes(durationMinutes),
                equipment,
                userId ?? Guid.NewGuid());
        }

        public static Workout CreateCoachWorkout(
            string name = "Test Coach Workout",
            DifficultyLevel difficulty = DifficultyLevel.Advanced,
            int durationMinutes = 60,
            EquipmentType equipment = EquipmentType.GymEquipment,
            Guid? coachId = null)
        {
            return Workout.CreateCoachWorkout(
                name,
                difficulty,
                Duration.FromMinutes(durationMinutes),
                equipment,
                coachId ?? Guid.NewGuid());
        }

        public static Workout CreateDynamicWorkout(
            string name = "Test Dynamic Workout",
            DifficultyLevel difficulty = DifficultyLevel.Beginner,
            int durationMinutes = 20,
            EquipmentType equipment = EquipmentType.None)
        {
            return Workout.CreateDynamicWorkout(
                name,
                difficulty,
                Duration.FromMinutes(durationMinutes),
                equipment);
        }

        public static Workout CreateWorkoutWithPhases(
            string workoutName = "Test Workout with Phases",
            Guid? userId = null)
        {
            var workout = CreateUserWorkout(workoutName, userId: userId);

            // Add warm-up phase
            var warmUpPhase = workout.AddPhase(
                WorkoutPhaseType.WarmUp,
                "Warm Up",
                Duration.FromMinutes(10));

            warmUpPhase.AddExercise(
                Guid.NewGuid(),
                "Jumping Jacks",
                new ExerciseParameters(
                    duration: TimeSpan.FromSeconds(30),
                    sets: 2,
                    restTime: TimeSpan.FromSeconds(30)));

            // Add main effort phase
            var mainPhase = workout.AddPhase(
                WorkoutPhaseType.MainEffort,
                "Main Effort",
                Duration.FromMinutes(25));

            mainPhase.AddExercise(
                Guid.NewGuid(),
                "Push-ups",
                new ExerciseParameters(
                    reps: 15,
                    sets: 3,
                    restTime: TimeSpan.FromMinutes(1)));

            mainPhase.AddExercise(
                Guid.NewGuid(),
                "Squats",
                new ExerciseParameters(
                    reps: 20,
                    sets: 3,
                    restTime: TimeSpan.FromMinutes(1)));

            // Add cool-down phase
            var coolDownPhase = workout.AddPhase(
                WorkoutPhaseType.Recovery,
                "Cool Down",
                Duration.FromMinutes(5));

            coolDownPhase.AddExercise(
                Guid.NewGuid(),
                "Stretching",
                new ExerciseParameters(
                    duration: TimeSpan.FromMinutes(5),
                    sets: 1));

            return workout;
        }
    }

    public static class ValueObjects
    {
        public static ExerciseParameters CreateRepetitionBasedParameters(
            int repetitions = 10,
            int sets = 3,
            int restTimeSeconds = 60,
            double? weight = null)
        {
            return new ExerciseParameters(
                reps: repetitions,
                sets: sets,
                restTime: TimeSpan.FromSeconds(restTimeSeconds),
                weight: weight);
        }

        public static ExerciseParameters CreateTimeBasedParameters(
            int durationSeconds = 30,
            int sets = 3,
            int restTimeSeconds = 30)
        {
            return new ExerciseParameters(
                duration: TimeSpan.FromSeconds(durationSeconds),
                sets: sets,
                restTime: TimeSpan.FromSeconds(restTimeSeconds));
        }

        public static ExerciseParameters CreateWeightedParameters(
            int repetitions = 8,
            int sets = 4,
            double weight = 20.5,
            int restTimeSeconds = 120)
        {
            return new ExerciseParameters(
                reps: repetitions,
                sets: sets,
                weight: weight,
                restTime: TimeSpan.FromSeconds(restTimeSeconds));
        }
    }

    public static class Users
    {
        public static Guid CreateUserId() => Guid.NewGuid();
        public static Guid CreateCoachId() => Guid.NewGuid();
    }
}

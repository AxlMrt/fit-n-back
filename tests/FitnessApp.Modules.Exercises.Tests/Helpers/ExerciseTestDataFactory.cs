using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Exercises.Tests.Helpers;

public static class ExerciseTestDataFactory
{
    /// <summary>
    /// Données d'exercices réalistes pour les tests
    /// </summary>
    public static class RealExercises
    {
        // ========== BODYWEIGHT EXERCISES ==========
        public static Exercise CreatePushUps()
        {
            var exercise = new Exercise(
                "Push-ups", 
                ExerciseType.Strength, 
                DifficultyLevel.Beginner, 
                MuscleGroup.Chest | MuscleGroup.Arms | MuscleGroup.Core,
                Equipment.None);
            
            exercise.SetDescription("Classic bodyweight chest exercise performed in plank position");
            exercise.SetInstructions("Start in plank position, lower chest to ground, push back up. Keep core engaged and body straight.");
            return exercise;
        }

        public static Exercise CreateBurpees()
        {
            var exercise = new Exercise(
                "Burpees",
                ExerciseType.Cardio,
                DifficultyLevel.Advanced,
                MuscleGroup.Full_Body,
                Equipment.None);
            
            exercise.SetDescription("Full-body explosive movement combining squat, plank, and jump");
            exercise.SetInstructions("Squat down, jump back to plank, do push-up, jump feet forward, explosive jump up.");
            return exercise;
        }

        public static Exercise CreateMountainClimbers()
        {
            var exercise = new Exercise(
                "Mountain Climbers",
                ExerciseType.Cardio,
                DifficultyLevel.Intermediate,
                MuscleGroup.Core | MuscleGroup.Legs | MuscleGroup.Arms,
                Equipment.None);
            
            exercise.SetDescription("Dynamic core exercise in plank position");
            exercise.SetInstructions("Start in plank, alternate bringing knees to chest rapidly. Keep hips level.");
            return exercise;
        }

        // ========== DUMBBELL EXERCISES ==========
        public static Exercise CreateDumbbellRows()
        {
            var exercise = new Exercise(
                "Dumbbell Bent-Over Rows",
                ExerciseType.Strength,
                DifficultyLevel.Intermediate,
                MuscleGroup.Back | MuscleGroup.Arms,
                Equipment.Dumbbells);
            
            exercise.SetDescription("Compound pulling exercise targeting the back muscles");
            exercise.SetInstructions("Bend at hips, pull dumbbells to ribs, squeeze shoulder blades. Control the negative.");
            return exercise;
        }

        public static Exercise CreateDumbbellSquats()
        {
            var exercise = new Exercise(
                "Dumbbell Squats",
                ExerciseType.Strength,
                DifficultyLevel.Beginner,
                MuscleGroup.Legs | MuscleGroup.Core,
                Equipment.Dumbbells);
            
            exercise.SetDescription("Fundamental leg exercise with added resistance");
            exercise.SetInstructions("Hold dumbbells at sides, squat down keeping chest up, drive through heels to stand.");
            return exercise;
        }

        // ========== BARBELL EXERCISES ==========
        public static Exercise CreateDeadlifts()
        {
            var exercise = new Exercise(
                "Deadlifts",
                ExerciseType.Strength,
                DifficultyLevel.Advanced,
                MuscleGroup.Back | MuscleGroup.Legs | MuscleGroup.Core,
                Equipment.Barbells);
            
            exercise.SetDescription("King of all exercises - compound movement targeting posterior chain");
            exercise.SetInstructions("Hip hinge movement, keep bar close to body, drive hips forward to stand tall.");
            return exercise;
        }

        public static Exercise CreateBenchPress()
        {
            var exercise = new Exercise(
                "Bench Press",
                ExerciseType.Strength,
                DifficultyLevel.Intermediate,
                MuscleGroup.Chest | MuscleGroup.Arms,
                Equipment.Barbells | Equipment.Bench);
            
            exercise.SetDescription("Classic upper body strength exercise");
            exercise.SetInstructions("Lie on bench, lower bar to chest with control, press up explosively.");
            return exercise;
        }

        // ========== PULL-UP BAR EXERCISES ==========
        public static Exercise CreatePullUps()
        {
            var exercise = new Exercise(
                "Pull-ups",
                ExerciseType.Strength,
                DifficultyLevel.Advanced,
                MuscleGroup.Back | MuscleGroup.Arms,
                Equipment.PullUpBar);
            
            exercise.SetDescription("Ultimate upper body bodyweight exercise");
            exercise.SetInstructions("Hang from bar, pull chest to bar, control descent. Engage core throughout.");
            return exercise;
        }

        // ========== CARDIO EQUIPMENT ==========
        public static Exercise CreateTreadmillRun()
        {
            var exercise = new Exercise(
                "Treadmill Running",
                ExerciseType.Cardio,
                DifficultyLevel.Beginner,
                MuscleGroup.Legs | MuscleGroup.Core,
                Equipment.Treadmill);
            
            exercise.SetDescription("Cardiovascular endurance training");
            exercise.SetInstructions("Maintain steady pace, land midfoot, keep posture upright, arms relaxed.");
            return exercise;
        }

        // ========== FLEXIBILITY & MOBILITY ==========
        public static Exercise CreateYogaFlow()
        {
            var exercise = new Exercise(
                "Yoga Flow Sequence",
                ExerciseType.Cardio, // Using Cardio as we don't have Flexibility
                DifficultyLevel.Beginner,
                MuscleGroup.Full_Body,
                Equipment.Mat);
            
            exercise.SetDescription("Dynamic stretching sequence for flexibility and mindfulness");
            exercise.SetInstructions("Flow through poses with breath awareness, hold each pose 30-60 seconds.");
            return exercise;
        }
    }

    /// <summary>
    /// Factory methods pour créer des exercices de test personnalisés
    /// </summary>
    public static Exercise CreateCustomExercise(
        string name = "Test Exercise",
        ExerciseType type = ExerciseType.Strength,
        DifficultyLevel difficulty = DifficultyLevel.Beginner,
        MuscleGroup muscleGroups = MuscleGroup.Chest,
        Equipment equipment = Equipment.None,
        string? description = null,
        string? instructions = null)
    {
        var exercise = new Exercise(name, type, difficulty, muscleGroups, equipment);
        
        if (!string.IsNullOrWhiteSpace(description))
            exercise.SetDescription(description);
        if (!string.IsNullOrWhiteSpace(instructions))
            exercise.SetInstructions(instructions);
            
        return exercise;
    }

    /// <summary>
    /// Générateur d'exercices pour tests de performance
    /// </summary>
    public static IEnumerable<Exercise> CreateBulkExercises(int count)
    {
        var exercises = new[]
        {
            RealExercises.CreatePushUps(),
            RealExercises.CreateBurpees(),
            RealExercises.CreateDumbbellRows(),
            RealExercises.CreateDeadlifts(),
            RealExercises.CreatePullUps(),
            RealExercises.CreateTreadmillRun()
        };

        for (int i = 0; i < count; i++)
        {
            var template = exercises[i % exercises.Length];
            yield return CreateCustomExercise(
                $"{template.Name} #{i + 1}",
                template.Type,
                template.Difficulty,
                template.MuscleGroups,
                template.Equipment,
                $"Test variation #{i + 1} of {template.Name}",
                template.Instructions
            );
        }
    }

    /// <summary>
    /// Données pour tests paramétrés
    /// </summary>
    public static class TestData
    {
        public static IEnumerable<object[]> ValidExerciseData =>
            new[]
            {
                new object[] { "Push-ups", ExerciseType.Strength, DifficultyLevel.Beginner, MuscleGroup.Chest, Equipment.None },
                new object[] { "Dumbbell Curls", ExerciseType.Strength, DifficultyLevel.Intermediate, MuscleGroup.Arms, Equipment.Dumbbells },
                new object[] { "Running", ExerciseType.Cardio, DifficultyLevel.Beginner, MuscleGroup.Legs, Equipment.None },
                new object[] { "Bench Press", ExerciseType.Strength, DifficultyLevel.Advanced, MuscleGroup.Chest, Equipment.Barbells | Equipment.Bench }
            };

        public static IEnumerable<object[]> InvalidExerciseNames =>
            new[]
            {
                new object[] { "" },
                new object[] { "   " },
                new object[] { null! },
                new object[] { new string('A', 101) } // Too long
            };

        public static IEnumerable<object[]> EquipmentRequirementTestData =>
            new[]
            {
                new object[] { Equipment.None, false },
                new object[] { Equipment.Dumbbells, true },
                new object[] { Equipment.Barbells | Equipment.Bench, true },
                new object[] { Equipment.Mat, true }
            };
    }
}

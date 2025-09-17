using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Exceptions;
using FitnessApp.Modules.Exercises.Tests.Helpers;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Exercises.Tests.Domain.Entities;

public class ExerciseTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateExercise()
    {
        // Arrange
        var name = "Push-ups";
        var type = ExerciseType.Strength;
        var difficulty = DifficultyLevel.Beginner;
        var muscleGroups = MuscleGroup.Chest | MuscleGroup.Arms;
        var equipment = Equipment.None;

        // Act
        var exercise = new Exercise(name, type, difficulty, muscleGroups, equipment);

        // Assert
        Assert.NotEqual(Guid.Empty, exercise.Id);
        Assert.Equal(name, exercise.Name);
        Assert.Equal(type, exercise.Type);
        Assert.Equal(difficulty, exercise.Difficulty);
        Assert.Equal(muscleGroups, exercise.MuscleGroups);
        Assert.Equal(equipment, exercise.Equipment);
        Assert.True(exercise.IsActive);
        Assert.True(exercise.CreatedAt <= DateTime.UtcNow);
        Assert.Null(exercise.UpdatedAt);
        Assert.Null(exercise.Description);
        Assert.Null(exercise.Instructions);
        Assert.Null(exercise.ImageContentId);
        Assert.Null(exercise.VideoContentId);
    }

    [Theory]
    [MemberData(nameof(ExerciseTestDataFactory.TestData.ValidExerciseData), MemberType = typeof(ExerciseTestDataFactory.TestData))]
    public void Constructor_WithValidVariations_ShouldCreateExercise(
        string name, ExerciseType type, DifficultyLevel difficulty, MuscleGroup muscleGroups, Equipment equipment)
    {
        // Act
        var exercise = new Exercise(name, type, difficulty, muscleGroups, equipment);

        // Assert
        Assert.NotEqual(Guid.Empty, exercise.Id);
        Assert.Equal(name, exercise.Name);
        Assert.Equal(type, exercise.Type);
        Assert.Equal(difficulty, exercise.Difficulty);
        Assert.Equal(muscleGroups, exercise.MuscleGroups);
        Assert.Equal(equipment, exercise.Equipment);
        Assert.True(exercise.IsActive);
    }

    [Theory]
    [MemberData(nameof(ExerciseTestDataFactory.TestData.InvalidExerciseNames), MemberType = typeof(ExerciseTestDataFactory.TestData))]
    public void Constructor_WithInvalidName_ShouldThrowExerciseDomainException(string invalidName)
    {
        // Arrange
        var type = ExerciseType.Strength;
        var difficulty = DifficultyLevel.Beginner;
        var muscleGroups = MuscleGroup.Chest;
        var equipment = Equipment.None;

        // Act & Assert
        var exception = Assert.Throws<ExerciseDomainException>(() =>
            new Exercise(invalidName, type, difficulty, muscleGroups, equipment));
        
        Assert.Contains("name", exception.Message.ToLower());
    }

    #endregion

    #region Business Logic Tests

    [Theory]
    [MemberData(nameof(ExerciseTestDataFactory.TestData.EquipmentRequirementTestData), MemberType = typeof(ExerciseTestDataFactory.TestData))]
    public void RequiresEquipment_ShouldReturnCorrectValue(Equipment equipment, bool expectedRequirement)
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.CreateCustomExercise(equipment: equipment);

        // Act
        var requiresEquipment = exercise.RequiresEquipment();

        // Assert
        Assert.Equal(expectedRequirement, requiresEquipment);
    }

    [Fact]
    public void IsCardioExercise_WithCardioType_ShouldReturnTrue()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreateBurpees();

        // Act & Assert
        Assert.True(exercise.IsCardioExercise());
        Assert.False(exercise.IsStrengthExercise());
    }

    [Fact]
    public void IsStrengthExercise_WithStrengthType_ShouldReturnTrue()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();

        // Act & Assert
        Assert.True(exercise.IsStrengthExercise());
        Assert.False(exercise.IsCardioExercise());
    }

    [Fact]
    public void IsFullBodyExercise_WithFullBodyMuscles_ShouldReturnTrue()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreateBurpees();

        // Act & Assert
        Assert.True(exercise.IsFullBodyExercise());
    }

    #endregion

    #region Property Modification Tests

    [Fact]
    public void SetName_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        var newName = "Modified Push-ups";
        var beforeUpdate = DateTime.UtcNow;

        // Act
        exercise.SetName(newName);

        // Assert
        Assert.Equal(newName, exercise.Name);
        Assert.NotNull(exercise.UpdatedAt);
        Assert.True(exercise.UpdatedAt >= beforeUpdate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SetName_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();

        // Act & Assert
        Assert.Throws<ExerciseDomainException>(() => exercise.SetName(invalidName));
    }

    [Fact]
    public void SetDescription_WithValidDescription_ShouldUpdateDescriptionAndTimestamp()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        var newDescription = "Updated exercise description";
        var beforeUpdate = DateTime.UtcNow;

        // Act
        exercise.SetDescription(newDescription);

        // Assert
        Assert.Equal(newDescription, exercise.Description);
        Assert.NotNull(exercise.UpdatedAt);
        Assert.True(exercise.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public void SetInstructions_WithValidInstructions_ShouldUpdateInstructionsAndTimestamp()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        var newInstructions = "Updated exercise instructions with detailed steps";
        var beforeUpdate = DateTime.UtcNow;

        // Act
        exercise.SetInstructions(newInstructions);

        // Assert
        Assert.Equal(newInstructions, exercise.Instructions);
        Assert.NotNull(exercise.UpdatedAt);
        Assert.True(exercise.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public void SetInstructions_WithTooLongInstructions_ShouldThrowException()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        var tooLongInstructions = new string('A', 2001); // > 2000 chars

        // Act & Assert
        var exception = Assert.Throws<ExerciseDomainException>(() => 
            exercise.SetInstructions(tooLongInstructions));
        
        Assert.Contains("2000", exception.Message);
    }

    [Fact]
    public void SetEquipment_WithNewEquipment_ShouldUpdateEquipmentAndTimestamp()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        var newEquipment = Equipment.Dumbbells | Equipment.Mat;
        var beforeUpdate = DateTime.UtcNow;

        // Act
        exercise.SetEquipment(newEquipment);

        // Assert
        Assert.Equal(newEquipment, exercise.Equipment);
        Assert.NotNull(exercise.UpdatedAt);
        Assert.True(exercise.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public void SetContentReferences_WithValidIds_ShouldUpdateContentAndTimestamp()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        var imageId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        exercise.SetContentReferences(imageId, videoId);

        // Assert
        Assert.Equal(imageId, exercise.ImageContentId);
        Assert.Equal(videoId, exercise.VideoContentId);
        Assert.NotNull(exercise.UpdatedAt);
        Assert.True(exercise.UpdatedAt >= beforeUpdate);
    }

    #endregion

    #region Activation/Deactivation Tests

    [Fact]
    public void Deactivate_WhenActive_ShouldSetInactiveAndUpdateTimestamp()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        Assert.True(exercise.IsActive); // Sanity check
        var beforeUpdate = DateTime.UtcNow;

        // Act
        exercise.Deactivate();

        // Assert
        Assert.False(exercise.IsActive);
        Assert.NotNull(exercise.UpdatedAt);
        Assert.True(exercise.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotUpdateTimestamp()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        exercise.Deactivate(); // First deactivation
        var timestampAfterFirstDeactivation = exercise.UpdatedAt;

        // Act
        exercise.Deactivate(); // Second deactivation

        // Assert
        Assert.False(exercise.IsActive);
        Assert.Equal(timestampAfterFirstDeactivation, exercise.UpdatedAt);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetActiveAndUpdateTimestamp()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        exercise.Deactivate(); // Make it inactive first
        var beforeUpdate = DateTime.UtcNow;

        // Act
        exercise.Activate();

        // Assert
        Assert.True(exercise.IsActive);
        Assert.NotNull(exercise.UpdatedAt);
        Assert.True(exercise.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotUpdateTimestamp()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        Assert.True(exercise.IsActive); // Should be active by default
        var originalTimestamp = exercise.UpdatedAt;

        // Act
        exercise.Activate();

        // Assert
        Assert.True(exercise.IsActive);
        Assert.Equal(originalTimestamp, exercise.UpdatedAt); // Should be null
    }

    #endregion

    #region Real Exercise Data Tests

    [Fact]
    public void RealExerciseData_PushUps_ShouldHaveCorrectProperties()
    {
        // Act
        var pushUps = ExerciseTestDataFactory.RealExercises.CreatePushUps();

        // Assert
        Assert.Equal("Push-ups", pushUps.Name);
        Assert.Equal(ExerciseType.Strength, pushUps.Type);
        Assert.Equal(DifficultyLevel.Beginner, pushUps.Difficulty);
        Assert.True(pushUps.MuscleGroups.HasFlag(MuscleGroup.Chest));
        Assert.True(pushUps.MuscleGroups.HasFlag(MuscleGroup.Arms));
        Assert.True(pushUps.MuscleGroups.HasFlag(MuscleGroup.Core));
        Assert.Equal(Equipment.None, pushUps.Equipment);
        Assert.False(pushUps.RequiresEquipment());
        Assert.NotNull(pushUps.Description);
        Assert.NotNull(pushUps.Instructions);
    }

    [Fact]
    public void RealExerciseData_Deadlifts_ShouldHaveCorrectProperties()
    {
        // Act
        var deadlifts = ExerciseTestDataFactory.RealExercises.CreateDeadlifts();

        // Assert
        Assert.Equal("Deadlifts", deadlifts.Name);
        Assert.Equal(ExerciseType.Strength, deadlifts.Type);
        Assert.Equal(DifficultyLevel.Advanced, deadlifts.Difficulty);
        Assert.True(deadlifts.MuscleGroups.HasFlag(MuscleGroup.Back));
        Assert.True(deadlifts.MuscleGroups.HasFlag(MuscleGroup.Legs));
        Assert.True(deadlifts.MuscleGroups.HasFlag(MuscleGroup.Core));
        Assert.Equal(Equipment.Barbells, deadlifts.Equipment);
        Assert.True(deadlifts.RequiresEquipment());
    }

    [Fact]
    public void RealExerciseData_BenchPress_ShouldRequireMultipleEquipment()
    {
        // Act
        var benchPress = ExerciseTestDataFactory.RealExercises.CreateBenchPress();

        // Assert
        Assert.True(benchPress.Equipment.HasFlag(Equipment.Barbells));
        Assert.True(benchPress.Equipment.HasFlag(Equipment.Bench));
        Assert.True(benchPress.RequiresEquipment());
    }

    #endregion
}

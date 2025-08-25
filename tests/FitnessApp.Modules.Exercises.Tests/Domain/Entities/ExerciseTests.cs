using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Enums;
using FitnessApp.Modules.Exercises.Domain.Exceptions;
using FitnessApp.Modules.Exercises.Domain.ValueObjects;

namespace FitnessApp.Modules.Exercises.Tests.Domain.Entities;
public class ExerciseTests
{
    [Fact]
    public void Exercise_Constructor_ShouldCreateValidExercise()
    {
        // Arrange
        var name = "Push-ups";
        var type = ExerciseType.Strength;
        var difficulty = DifficultyLevel.Intermediate;
        var muscleGroups = MuscleGroup.CHEST | MuscleGroup.ARMS;
        var equipment = new Equipment();

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
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Exercise_Constructor_ShouldThrowException_WhenNameIsInvalid(string invalidName)
    {
        // Arrange
        var type = ExerciseType.Strength;
        var difficulty = DifficultyLevel.Intermediate;
        var muscleGroups = MuscleGroup.CHEST;
        var equipment = new Equipment();

        // Act & Assert
        Assert.Throws<ExerciseDomainException>(() =>
            new Exercise(invalidName, type, difficulty, muscleGroups, equipment));
    }

    [Fact]
    public void Exercise_Constructor_ShouldThrowException_WhenEquipmentIsNull()
    {
        // Arrange
        var name = "Push-ups";
        var type = ExerciseType.Strength;
        var difficulty = DifficultyLevel.Intermediate;
        var muscleGroups = MuscleGroup.CHEST;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Exercise(name, type, difficulty, muscleGroups, null));
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenValidName()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var newName = "Modified Push-ups";
        var originalUpdateTime = exercise.UpdatedAt;

        // Act
        exercise.SetName(newName);

        // Assert
        Assert.Equal(newName, exercise.Name);
        Assert.NotNull(exercise.UpdatedAt);
        // If originally null, it should now have a value; if had a value, it should be greater
        if (originalUpdateTime.HasValue)
            Assert.True(exercise.UpdatedAt > originalUpdateTime);
        else
            Assert.True(exercise.UpdatedAt.HasValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SetName_ShouldThrowException_WhenNameIsInvalid(string invalidName)
    {
        // Arrange
        var exercise = CreateValidExercise();

        // Act & Assert
        Assert.Throws<ExerciseDomainException>(() => exercise.SetName(invalidName));
    }

    [Fact]
    public void SetName_ShouldThrowException_WhenNameTooLong()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var longName = new string('a', 101);

        // Act & Assert
        var exception = Assert.Throws<ExerciseDomainException>(() => exercise.SetName(longName));
        Assert.Contains("cannot exceed 100 characters", exception.Message);
    }

    [Fact]
    public void SetDescription_ShouldUpdateDescription_WhenValidDescription()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var description = "This is a great exercise for building upper body strength";

        // Act
        exercise.SetDescription(description);

        // Assert
        Assert.Equal(description, exercise.Description);
        Assert.NotNull(exercise.UpdatedAt);
    }

    [Fact]
    public void SetDescription_ShouldThrowException_WhenDescriptionTooLong()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var longDescription = new string('a', 1001);

        // Act & Assert
        var exception = Assert.Throws<ExerciseDomainException>(() => exercise.SetDescription(longDescription));
        Assert.Contains("cannot exceed 1000 characters", exception.Message);
    }

    [Fact]
    public void SetDescription_ShouldAllowNull()
    {
        // Arrange
        var exercise = CreateValidExercise();

        // Act
        exercise.SetDescription(null);

        // Assert
        Assert.Null(exercise.Description);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateAllDetails()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var newType = ExerciseType.Cardio;
        var newDifficulty = DifficultyLevel.Advanced;
        var newMuscleGroups = MuscleGroup.FULL_BODY;

        // Act
        exercise.UpdateDetails(newType, newDifficulty, newMuscleGroups);

        // Assert
        Assert.Equal(newType, exercise.Type);
        Assert.Equal(newDifficulty, exercise.Difficulty);
        Assert.Equal(newMuscleGroups, exercise.MuscleGroups);
        Assert.NotNull(exercise.UpdatedAt);
    }

    [Fact]
    public void SetEquipment_ShouldUpdateEquipment_WhenValidEquipment()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var newEquipment = new Equipment(new[] { "Dumbbells", "Bench" });

        // Act
        exercise.SetEquipment(newEquipment);

        // Assert
        Assert.Equal(newEquipment, exercise.Equipment);
        Assert.NotNull(exercise.UpdatedAt);
    }

    [Fact]
    public void SetEquipment_ShouldThrowException_WhenEquipmentIsNull()
    {
        // Arrange
        var exercise = CreateValidExercise();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exercise.SetEquipment(null));
    }

    [Fact]
    public void SetInstructions_ShouldUpdateInstructions_WhenValidInstructions()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var instructions = "1. Start in plank position\n2. Lower your body\n3. Push back up";

        // Act
        exercise.SetInstructions(instructions);

        // Assert
        Assert.Equal(instructions, exercise.Instructions);
        Assert.NotNull(exercise.UpdatedAt);
    }

    [Fact]
    public void SetInstructions_ShouldThrowException_WhenInstructionsTooLong()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var longInstructions = new string('a', 2001);

        // Act & Assert
        var exception = Assert.Throws<ExerciseDomainException>(() => exercise.SetInstructions(longInstructions));
        Assert.Contains("cannot exceed 2000 characters", exception.Message);
    }

    [Fact]
    public void SetContentReferences_ShouldUpdateContentIds()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var imageId = Guid.NewGuid();
        var videoId = Guid.NewGuid();

        // Act
        exercise.SetContentReferences(imageId, videoId);

        // Assert
        Assert.Equal(imageId, exercise.ImageContentId);
        Assert.Equal(videoId, exercise.VideoContentId);
        Assert.NotNull(exercise.UpdatedAt);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue_WhenCurrentlyInactive()
    {
        // Arrange
        var exercise = CreateValidExercise();
        exercise.Deactivate(); // Make it inactive first

        // Act
        exercise.Activate();

        // Assert
        Assert.True(exercise.IsActive);
        Assert.NotNull(exercise.UpdatedAt);
    }

    [Fact]
    public void Activate_ShouldNotChangeUpdatedAt_WhenAlreadyActive()
    {
        // Arrange
        var exercise = CreateValidExercise(); // Already active
        var originalUpdateTime = exercise.UpdatedAt;

        // Act
        exercise.Activate();

        // Assert
        Assert.True(exercise.IsActive);
        Assert.Equal(originalUpdateTime, exercise.UpdatedAt);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_WhenCurrentlyActive()
    {
        // Arrange
        var exercise = CreateValidExercise(); // Active by default

        // Act
        exercise.Deactivate();

        // Assert
        Assert.False(exercise.IsActive);
        Assert.NotNull(exercise.UpdatedAt);
    }

    [Fact]
    public void IsCardioExercise_ShouldReturnTrue_WhenTypeIsCardio()
    {
        // Arrange
        var exercise = new Exercise("Running", ExerciseType.Cardio, DifficultyLevel.Intermediate, MuscleGroup.LEGS, new Equipment());

        // Act & Assert
        Assert.True(exercise.IsCardioExercise());
    }

    [Fact]
    public void IsStrengthExercise_ShouldReturnTrue_WhenTypeIsStrength()
    {
        // Arrange
        var exercise = CreateValidExercise(); // Strength by default

        // Act & Assert
        Assert.True(exercise.IsStrengthExercise());
    }

    [Fact]
    public void RequiresEquipment_ShouldReturnTrue_WhenEquipmentHasItems()
    {
        // Arrange
        var equipment = new Equipment(new[] { "Dumbbells" });
        var exercise = new Exercise("Bicep Curls", ExerciseType.Strength, DifficultyLevel.Beginner, MuscleGroup.ARMS, equipment);

        // Act & Assert
        Assert.True(exercise.RequiresEquipment());
    }

    [Fact]
    public void RequiresEquipment_ShouldReturnFalse_WhenNoEquipment()
    {
        // Arrange
        var exercise = CreateValidExercise(); // No equipment

        // Act & Assert
        Assert.False(exercise.RequiresEquipment());
    }

    [Fact]
    public void IsFullBodyExercise_ShouldReturnTrue_WhenMuscleGroupsIncludeFullBody()
    {
        // Arrange
        var exercise = new Exercise("Burpees", ExerciseType.Cardio, DifficultyLevel.Advanced, MuscleGroup.FULL_BODY, new Equipment());

        // Act & Assert
        Assert.True(exercise.IsFullBodyExercise());
    }

    private static Exercise CreateValidExercise()
    {
        return new Exercise("Push-ups", ExerciseType.Strength, DifficultyLevel.Intermediate, MuscleGroup.CHEST, new Equipment());
    }
}

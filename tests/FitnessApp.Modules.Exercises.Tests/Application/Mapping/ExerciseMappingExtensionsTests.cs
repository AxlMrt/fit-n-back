using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Enums;
using FitnessApp.Modules.Exercises.Domain.ValueObjects;

namespace FitnessApp.Modules.Exercises.Tests.Application.Mapping;
public class ExerciseMappingExtensionsTests
{
    [Fact]
    public void MapToDto_ShouldMapAllProperties_WhenValidExercise()
    {
        // Arrange
        var equipment = new Equipment(new[] { "Dumbbells", "Bench" });
        var exercise = new Exercise(
            "Push-ups", 
            ExerciseType.Strength, 
            DifficultyLevel.Intermediate, 
            MuscleGroup.CHEST | MuscleGroup.ARMS, 
            equipment);
        
        exercise.SetDescription("Basic push-up exercise");
        exercise.SetInstructions("Start in plank position, lower body, push back up");
        
        var imageId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        exercise.SetContentReferences(imageId, videoId);

        // Act
        var dto = exercise.MapToDto();

        // Assert
        Assert.Equal(exercise.Id, dto.Id);
        Assert.Equal(exercise.Name, dto.Name);
        Assert.Equal(exercise.Description, dto.Description);
        Assert.Equal(exercise.Type, dto.Type);
        Assert.Equal(exercise.Difficulty, dto.Difficulty);
        Assert.Equal(exercise.Instructions, dto.Instructions);
        Assert.Equal(exercise.IsActive, dto.IsActive);
        Assert.Equal(exercise.CreatedAt, dto.CreatedAt);
        Assert.Equal(exercise.UpdatedAt, dto.UpdatedAt);
        Assert.Equal(imageId, dto.ImageContentId);
        Assert.Equal(videoId, dto.VideoContentId);

        // Check muscle groups conversion
        Assert.Contains("CHEST", dto.MuscleGroups);
        Assert.Contains("ARMS", dto.MuscleGroups);
        Assert.Equal(2, dto.MuscleGroups.Count);

        // Check equipment conversion
        Assert.Contains("Dumbbells", dto.Equipment);
        Assert.Contains("Bench", dto.Equipment);
        Assert.Equal(2, dto.Equipment.Count);
    }

    [Fact]
    public void MapToDto_ShouldHandleEmptyMuscleGroups()
    {
        // Arrange
        var exercise = new Exercise(
            "Meditation", 
            ExerciseType.Other, 
            DifficultyLevel.Beginner, 
            MuscleGroup.NONE, 
            new Equipment());

        // Act
        var dto = exercise.MapToDto();

        // Assert
        Assert.Empty(dto.MuscleGroups);
    }

    [Fact]
    public void MapToDto_ShouldHandleEmptyEquipment()
    {
        // Arrange
        var exercise = new Exercise(
            "Bodyweight Squats", 
            ExerciseType.Strength, 
            DifficultyLevel.Beginner, 
            MuscleGroup.LEGS, 
            new Equipment());

        // Act
        var dto = exercise.MapToDto();

        // Assert
        Assert.Empty(dto.Equipment);
    }

    [Fact]
    public void MapToListDto_ShouldMapEssentialProperties()
    {
        // Arrange
        var equipment = new Equipment(new[] { "Barbell" });
        var exercise = new Exercise(
            "Deadlift", 
            ExerciseType.Strength, 
            DifficultyLevel.Advanced, 
            MuscleGroup.BACK | MuscleGroup.LEGS, 
            equipment);

        // Act
        var dto = exercise.MapToListDto();

        // Assert
        Assert.Equal(exercise.Id, dto.Id);
        Assert.Equal(exercise.Name, dto.Name);
        Assert.Equal(exercise.Type, dto.Type);
        Assert.Equal(exercise.Difficulty, dto.Difficulty);
        Assert.Equal(exercise.IsActive, dto.IsActive);
        Assert.True(dto.RequiresEquipment);

        // Check muscle groups
        Assert.Contains("BACK", dto.MuscleGroups);
        Assert.Contains("LEGS", dto.MuscleGroups);
        Assert.Equal(2, dto.MuscleGroups.Count);
    }

    [Fact]
    public void MapToListDto_ShouldSetRequiresEquipmentFalse_WhenNoEquipment()
    {
        // Arrange
        var exercise = new Exercise(
            "Push-ups", 
            ExerciseType.Strength, 
            DifficultyLevel.Intermediate, 
            MuscleGroup.CHEST, 
            new Equipment());

        // Act
        var dto = exercise.MapToListDto();

        // Assert
        Assert.False(dto.RequiresEquipment);
    }

    [Theory]
    [InlineData(MuscleGroup.CHEST, new[] { "CHEST" })]
    [InlineData(MuscleGroup.CHEST | MuscleGroup.ARMS, new[] { "CHEST", "ARMS" })]
    [InlineData(MuscleGroup.FULL_BODY, new[] { "FULL_BODY" })]
    [InlineData(MuscleGroup.CHEST | MuscleGroup.BACK | MuscleGroup.LEGS, new[] { "CHEST", "BACK", "LEGS" })]
    public void ConvertMuscleGroupsToList_ShouldConvertCorrectly(MuscleGroup muscleGroups, string[] expectedGroups)
    {
        // Arrange
        var exercise = new Exercise(
            "Test Exercise", 
            ExerciseType.Strength, 
            DifficultyLevel.Intermediate, 
            muscleGroups, 
            new Equipment());

        // Act
        var dto = exercise.MapToDto();

        // Assert
        Assert.Equal(expectedGroups.Length, dto.MuscleGroups.Count);
        foreach (var expectedGroup in expectedGroups)
        {
            Assert.Contains(expectedGroup, dto.MuscleGroups);
        }
    }

    [Fact]
    public void ConvertMuscleGroupsToList_ShouldExcludeNoneGroup()
    {
        // Arrange
        var exercise = new Exercise(
            "Test Exercise", 
            ExerciseType.Strength, 
            DifficultyLevel.Intermediate, 
            MuscleGroup.CHEST | MuscleGroup.NONE, // NONE should be ignored
            new Equipment());

        // Act
        var dto = exercise.MapToDto();

        // Assert
        Assert.Single(dto.MuscleGroups);
        Assert.Contains("CHEST", dto.MuscleGroups);
        Assert.DoesNotContain("NONE", dto.MuscleGroups);
    }
}

using FluentAssertions;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Tests.Domain.Entities;

public class WorkoutTests
{
    [Fact]
    public void Workout_Creation_ShouldSucceed_WithValidData()
    {
        // Arrange
        var name = "Test Workout";
        var type = WorkoutType.Template;
        var category = WorkoutCategory.Cardio;
        var difficulty = DifficultyLevel.Intermediate;

        // Act
        var workout = new Workout(name, type, category, difficulty);

        // Assert
        workout.Should().NotBeNull();
        workout.Name.Should().Be(name);
        workout.Type.Should().Be(type);
        workout.Category.Should().Be(category);
        workout.Difficulty.Should().Be(difficulty);
        workout.EstimatedDurationMinutes.Should().BeGreaterThan(0);
        workout.IsActive.Should().BeTrue();
        workout.Phases.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Workout_Creation_ShouldThrowException_WithInvalidName(string invalidName)
    {
        // Arrange & Act & Assert
        var act = () => new Workout(invalidName, WorkoutType.Template, WorkoutCategory.Cardio, DifficultyLevel.Beginner);
        act.Should().Throw<WorkoutDomainException>();
    }

    [Fact]
    public void UpdateDetails_ShouldSucceed_WithValidData()
    {
        // Arrange
        var workout = CreateValidWorkout();
        var newName = "Updated Workout";
        var newDescription = "Updated description";
        var newDifficulty = DifficultyLevel.Advanced;

        // Act
        workout.UpdateDetails(newName, newDescription, newDifficulty);

        // Assert
        workout.Name.Should().Be(newName);
        workout.Description.Should().Be(newDescription);
        workout.Difficulty.Should().Be(newDifficulty);
    }

    [Fact]
    public void AddPhase_ShouldSucceed_WithValidPhase()
    {
        // Arrange
        var workout = CreateValidWorkout();

        // Act
        workout.AddPhase(WorkoutPhaseType.WarmUp, "Warm Up");

        // Assert
        workout.Phases.Should().HaveCount(1);
        var phase = workout.Phases.First();
        phase.Type.Should().Be(WorkoutPhaseType.WarmUp);
        phase.Name.Should().Be("Warm Up");
        phase.EstimatedDurationMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AddPhase_ShouldThrowException_WithDuplicatePhaseType()
    {
        // Arrange
        var workout = CreateValidWorkout();
        workout.AddPhase(WorkoutPhaseType.WarmUp, "Warm Up");

        // Act & Assert
        var act = () => workout.AddPhase(WorkoutPhaseType.WarmUp, "Another Warm Up");
        act.Should().Throw<WorkoutDomainException>();
    }

    [Fact]
    public void RemovePhase_ShouldSucceed_WithExistingPhase()
    {
        // Arrange
        var workout = CreateValidWorkout();
        workout.AddPhase(WorkoutPhaseType.WarmUp, "Warm Up");

        // Act
        workout.RemovePhase(WorkoutPhaseType.WarmUp);

        // Assert
        workout.Phases.Should().BeEmpty();
    }

    [Fact]
    public void RemovePhase_ShouldThrowException_WithNonExistentPhase()
    {
        // Arrange
        var workout = CreateValidWorkout();

        // Act & Assert
        var act = () => workout.RemovePhase(WorkoutPhaseType.WarmUp);
        act.Should().Throw<WorkoutDomainException>();
    }

    [Fact]
    public void MovePhase_ShouldSucceed_WithValidOrder()
    {
        // Arrange
        var workout = CreateValidWorkout();
        workout.AddPhase(WorkoutPhaseType.WarmUp, "Phase 1");
        workout.AddPhase(WorkoutPhaseType.MainEffort, "Phase 2");
        workout.AddPhase(WorkoutPhaseType.Stretching, "Phase 3");

        // Act
        workout.MovePhase(WorkoutPhaseType.Stretching, 1);

        // Assert
        var orderedPhases = workout.Phases.OrderBy(p => p.Order).ToList();
        orderedPhases[0].Type.Should().Be(WorkoutPhaseType.Stretching);
        orderedPhases[1].Type.Should().Be(WorkoutPhaseType.WarmUp);
        orderedPhases[2].Type.Should().Be(WorkoutPhaseType.MainEffort);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var workout = CreateValidWorkout();

        // Act
        workout.Deactivate();

        // Assert
        workout.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var workout = CreateValidWorkout();
        workout.Deactivate();

        // Act
        workout.Activate();

        // Assert
        workout.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SetImageContent_ShouldSucceed_WithValidContentId()
    {
        // Arrange
        var workout = CreateValidWorkout();
        var contentId = Guid.NewGuid();

        // Act
        workout.SetImageContent(contentId);

        // Assert
        workout.ImageContentId.Should().Be(contentId);
    }

    [Fact]
    public void SetImageContent_ShouldSucceed_WithNull()
    {
        // Arrange
        var workout = CreateValidWorkout();
        var contentId = Guid.NewGuid();
        workout.SetImageContent(contentId);

        // Act
        workout.SetImageContent(null);

        // Assert
        workout.ImageContentId.Should().BeNull();
    }

    [Fact]
    public void CreateUserWorkout_ShouldSucceed_WithValidData()
    {
        // Arrange
        var name = "User Workout";
        var difficulty = DifficultyLevel.Beginner;
        var userId = Guid.NewGuid();

        // Act
        var workout = Workout.CreateUserWorkout(name, WorkoutCategory.Strength, difficulty, userId);

        // Assert
        workout.Should().NotBeNull();
        workout.Name.Should().Be(name);
        workout.Type.Should().Be(WorkoutType.UserCreated);
        workout.Difficulty.Should().Be(difficulty);
        workout.EstimatedDurationMinutes.Should().BeGreaterThan(0);
        workout.CreatedByUserId.Should().Be(userId);
        workout.CreatedByCoachId.Should().BeNull();
    }

    [Fact]
    public void CreateCoachWorkout_ShouldSucceed_WithValidData()
    {
        // Arrange
        var name = "Coach Workout";
        var difficulty = DifficultyLevel.Advanced;
        var coachId = Guid.NewGuid();

        // Act
        var workout = Workout.CreateCoachWorkout(name, WorkoutCategory.HIIT, difficulty, coachId);

        // Assert
        workout.Should().NotBeNull();
        workout.Name.Should().Be(name);
        workout.Type.Should().Be(WorkoutType.Template);
        workout.Category.Should().Be(WorkoutCategory.HIIT);
        workout.Difficulty.Should().Be(difficulty);
        workout.EstimatedDurationMinutes.Should().BeGreaterThan(0);
        workout.CreatedByCoachId.Should().Be(coachId);
        workout.CreatedByUserId.Should().BeNull();
    }

    private static Workout CreateValidWorkout()
    {
        return new Workout(
            "Test Workout",
            WorkoutType.Template,
            WorkoutCategory.Mixed,
            DifficultyLevel.Intermediate
        );
    }
}

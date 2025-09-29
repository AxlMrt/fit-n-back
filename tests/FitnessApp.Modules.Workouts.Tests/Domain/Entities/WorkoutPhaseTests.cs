using FluentAssertions;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Tests.Domain.Entities;

public class WorkoutPhaseTests
{
    [Fact]
    public void WorkoutPhase_Creation_ShouldSucceed_WithValidData()
    {
        // Arrange
        var type = WorkoutPhaseType.WarmUp;
        var name = "Warm Up Phase";
        var order = 1;

        // Act
        var phase = new WorkoutPhase(type, name, order);

        // Assert
        phase.Should().NotBeNull();
        phase.Type.Should().Be(type);
        phase.Name.Should().Be(name);
        phase.EstimatedDurationMinutes.Should().BeGreaterThan(0);
        phase.Order.Should().Be(order);
        phase.Exercises.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void WorkoutPhase_Creation_ShouldThrowException_WithInvalidName(string invalidName)
    {
        // Arrange & Act & Assert
        var act = () => new WorkoutPhase(WorkoutPhaseType.WarmUp, invalidName, 1);
        act.Should().Throw<WorkoutDomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WorkoutPhase_Creation_ShouldThrowException_WithInvalidOrder(int invalidOrder)
    {
        // Arrange & Act & Assert
        var act = () => new WorkoutPhase(WorkoutPhaseType.WarmUp, "Test", invalidOrder);
        act.Should().Throw<WorkoutDomainException>();
    }

    [Fact]
    public void UpdateDetails_ShouldSucceed_WithValidData()
    {
        // Arrange
        var phase = CreateValidPhase();
        var newName = "Updated Phase";
        var newDescription = "Updated description";

        // Act
        phase.UpdateDetails(newName, newDescription);

        // Assert
        phase.Name.Should().Be(newName);
        phase.Description.Should().Be(newDescription);
    }

    [Fact]
    public void UpdateOrder_ShouldSucceed_WithValidOrder()
    {
        // Arrange
        var phase = CreateValidPhase();
        var newOrder = 5;

        // Act
        phase.UpdateOrder(newOrder);

        // Assert
        phase.Order.Should().Be(newOrder);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void UpdateOrder_ShouldThrowException_WithInvalidOrder(int invalidOrder)
    {
        // Arrange
        var phase = CreateValidPhase();

        // Act & Assert
        var act = () => phase.UpdateOrder(invalidOrder);
        act.Should().Throw<WorkoutDomainException>();
    }

    [Fact]
    public void AddExercise_ShouldSucceed_WithValidExercise()
    {
        // Arrange
        var phase = CreateValidPhase();
        var exerciseId = Guid.NewGuid();

        // Act
        phase.AddExercise(exerciseId, 3, 10, 60);

        // Assert
        phase.Exercises.Should().HaveCount(1);
        var exercise = phase.Exercises.First();
        exercise.ExerciseId.Should().Be(exerciseId);
        exercise.Sets.Should().Be(3);
        exercise.Reps.Should().Be(10);
        exercise.RestSeconds.Should().Be(60);
        exercise.Order.Should().Be(1);
    }

    [Fact]
    public void AddExercise_ShouldThrowException_WithDuplicateExercise()
    {
        // Arrange
        var phase = CreateValidPhase();
        var exerciseId = Guid.NewGuid();
        phase.AddExercise(exerciseId, 3, 10, 60);

        // Act & Assert
        var act = () => phase.AddExercise(exerciseId, 3, 12, 60);
        act.Should().Throw<WorkoutDomainException>();
    }

    [Fact]
    public void RemoveExercise_ShouldSucceed_WithExistingExercise()
    {
        // Arrange
        var phase = CreateValidPhase();
        var exerciseId = Guid.NewGuid();
        phase.AddExercise(exerciseId, 3, 10, 60);

        // Act
        phase.RemoveExercise(exerciseId);

        // Assert
        phase.Exercises.Should().BeEmpty();
    }

    [Fact]
    public void RemoveExercise_ShouldThrowException_WithNonExistentExercise()
    {
        // Arrange
        var phase = CreateValidPhase();
        var nonExistentExerciseId = Guid.NewGuid();

        // Act & Assert
        var act = () => phase.RemoveExercise(nonExistentExerciseId);
        act.Should().Throw<WorkoutDomainException>();
    }

    [Fact]
    public void MoveExercise_ShouldSucceed_WithValidOrder()
    {
        // Arrange
        var phase = CreateValidPhase();
        var exercise1Id = Guid.NewGuid();
        var exercise2Id = Guid.NewGuid();
        var exercise3Id = Guid.NewGuid();
        
        phase.AddExercise(exercise1Id, 3, 10, 60);
        phase.AddExercise(exercise2Id, 3, 12, 60);
        phase.AddExercise(exercise3Id, 3, 15, 60);

        // Act
        phase.MoveExercise(exercise3Id, 1);

        // Assert
        var orderedExercises = phase.Exercises.OrderBy(e => e.Order).ToList();
        orderedExercises[0].ExerciseId.Should().Be(exercise3Id);
        orderedExercises[1].ExerciseId.Should().Be(exercise1Id);
        orderedExercises[2].ExerciseId.Should().Be(exercise2Id);
    }

    [Fact]
    public void HasExercise_ShouldReturnTrue_WithExistingExercise()
    {
        // Arrange
        var phase = CreateValidPhase();
        var exerciseId = Guid.NewGuid();
        phase.AddExercise(exerciseId, 3, 10, 60);

        // Act & Assert
        phase.HasExercise(exerciseId).Should().BeTrue();
    }

    [Fact]
    public void HasExercise_ShouldReturnFalse_WithNonExistentExercise()
    {
        // Arrange
        var phase = CreateValidPhase();
        var nonExistentExerciseId = Guid.NewGuid();

        // Act & Assert
        phase.HasExercise(nonExistentExerciseId).Should().BeFalse();
    }

    private static WorkoutPhase CreateValidPhase()
    {
        return new WorkoutPhase(
            WorkoutPhaseType.MainEffort,
            "Test Phase",
            1
        );
    }
}

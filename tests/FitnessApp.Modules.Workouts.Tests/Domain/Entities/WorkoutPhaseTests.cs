using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FluentAssertions;

namespace FitnessApp.Modules.Workouts.Tests.Domain.Entities;

public class WorkoutPhaseTests
{
    [Fact]
    public void Constructor_ValidData_ShouldCreatePhase()
    {
        // Arrange
        var type = WorkoutPhaseType.WarmUp;
        var name = "Warm Up Phase";
        var duration = Duration.FromMinutes(10);
        var order = 0;

        // Act
        var phase = new WorkoutPhase(type, name, duration, order);

        // Assert
        phase.Type.Should().Be(type);
        phase.Name.Should().Be(name);
        phase.EstimatedDuration.Should().Be(duration);
        phase.Order.Should().Be(order);
        phase.Exercises.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidName_ShouldThrowException(string invalidName)
    {
        // Act & Assert
        var act = () => new WorkoutPhase(WorkoutPhaseType.WarmUp, invalidName, Duration.FromMinutes(10), 0);
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*name*");
    }

    [Fact]
    public void Constructor_NegativeOrder_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new WorkoutPhase(WorkoutPhaseType.WarmUp, "Test", Duration.FromMinutes(10), -1);
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*Order*");
    }

    [Fact]
    public void Constructor_NullDuration_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new WorkoutPhase(WorkoutPhaseType.WarmUp, "Test", null!, 0);
        
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetDescription_ValidDescription_ShouldSetDescription()
    {
        // Arrange
        var phase = CreateSamplePhase();
        var description = "Test description";

        // Act
        phase.SetDescription(description);

        // Assert
        phase.Description.Should().Be(description);
    }

    [Fact]
    public void SetDescription_NullDescription_ShouldSetToNull()
    {
        // Arrange
        var phase = CreateSamplePhase();

        // Act
        phase.SetDescription(null);

        // Assert
        phase.Description.Should().BeNull();
    }

    [Fact]
    public void AddExercise_ValidExercise_ShouldAddExercise()
    {
        // Arrange
        var phase = CreateSamplePhase();
        var exerciseId = Guid.NewGuid();
        var exerciseName = "Push-ups";
        var parameters = new ExerciseParameters(reps: 10, sets: 3, restTime: TimeSpan.FromSeconds(60));

        // Act
        var exercise = phase.AddExercise(exerciseId, exerciseName, parameters);

        // Assert
        phase.Exercises.Should().HaveCount(1);
        phase.Exercises.First().Should().Be(exercise);
        exercise.ExerciseId.Should().Be(exerciseId);
        exercise.ExerciseName.Should().Be(exerciseName);
        exercise.Parameters.Should().Be(parameters);
        exercise.Order.Should().Be(0);
    }

    [Fact]
    public void RemoveExercise_ExistingExercise_ShouldRemoveExercise()
    {
        // Arrange
        var phase = CreateSamplePhase();
        var parameters = new ExerciseParameters();
        var exercise = phase.AddExercise(Guid.NewGuid(), "Test", parameters);

        // Act
        phase.RemoveExercise(exercise.Id);

        // Assert
        phase.Exercises.Should().BeEmpty();
    }

    [Fact]
    public void RemoveExercise_NonExistentExercise_ShouldThrowException()
    {
        // Arrange
        var phase = CreateSamplePhase();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var act = () => phase.RemoveExercise(nonExistentId);
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*not found*");
    }

    [Fact]
    public void MoveExercise_ValidPosition_ShouldMoveExercise()
    {
        // Arrange
        var phase = CreateSamplePhase();
        var emptyParams = new ExerciseParameters();
        var exercise1 = phase.AddExercise(Guid.NewGuid(), "Exercise 1", emptyParams);
        var exercise2 = phase.AddExercise(Guid.NewGuid(), "Exercise 2", emptyParams);
        var exercise3 = phase.AddExercise(Guid.NewGuid(), "Exercise 3", emptyParams);

        // Act - Move exercise1 to position 2
        phase.MoveExercise(exercise1.Id, 2);

        // Assert
        var exercises = phase.Exercises.OrderBy(e => e.Order).ToList();
        exercises[0].Should().Be(exercise2);
        exercises[1].Should().Be(exercise3);
        exercises[2].Should().Be(exercise1);
    }

    [Fact]
    public void UpdateOrder_ValidOrder_ShouldUpdateOrder()
    {
        // Arrange
        var phase = CreateSamplePhase();
        var newOrder = 5;

        // Act
        phase.UpdateOrder(newOrder);

        // Assert
        phase.Order.Should().Be(newOrder);
    }

    [Fact]
    public void CalculateTotalDuration_WithExercises_ShouldCalculateCorrectDuration()
    {
        // Arrange
        var phase = CreateSamplePhase();
        
        // Add exercise with duration
        phase.AddExercise(Guid.NewGuid(), "Exercise 1", new ExerciseParameters(
            duration: TimeSpan.FromSeconds(30),
            sets: 3,
            restTime: TimeSpan.FromSeconds(60)));

        // Add exercise with reps (should estimate duration)
        phase.AddExercise(Guid.NewGuid(), "Exercise 2", new ExerciseParameters(
            reps: 10,
            sets: 2,
            restTime: TimeSpan.FromSeconds(45)));

        // Act
        var totalDuration = phase.CalculateTotalDuration();

        // Assert
        // Should calculate total duration based on exercises
        totalDuration.Value.TotalSeconds.Should().BeGreaterThan(0);
    }

    private static WorkoutPhase CreateSamplePhase()
    {
        return new WorkoutPhase(
            WorkoutPhaseType.MainEffort,
            "Main Effort",
            Duration.FromMinutes(20),
            0);
    }
}

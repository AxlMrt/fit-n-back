using FluentAssertions;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.Tests.Domain.Entities;

public class WorkoutExerciseTests
{
    [Fact]
    public void WorkoutExercise_Creation_ShouldSucceed_WithValidData()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var sets = 3;
        var reps = 12;
        var restSeconds = 60;
        var order = 1;

        // Act
        var workoutExercise = new WorkoutExercise(exerciseId, sets, reps, restSeconds, order);

        // Assert
        workoutExercise.Should().NotBeNull();
        workoutExercise.ExerciseId.Should().Be(exerciseId);
        workoutExercise.Sets.Should().Be(sets);
        workoutExercise.Reps.Should().Be(reps);
        workoutExercise.RestSeconds.Should().Be(restSeconds);
        workoutExercise.Order.Should().Be(order);
    }

    [Fact]
    public void WorkoutExercise_Creation_ShouldThrowException_WithEmptyExerciseId()
    {
        // Arrange & Act & Assert
        var act = () => new WorkoutExercise(Guid.Empty, 3, 12, 60, 1);
        act.Should().Throw<WorkoutDomainException>().WithMessage("Exercise ID cannot be empty");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WorkoutExercise_Creation_ShouldThrowException_WithInvalidSets(int invalidSets)
    {
        // Arrange & Act & Assert
        var act = () => new WorkoutExercise(Guid.NewGuid(), invalidSets, 12, 60, 1);
        act.Should().Throw<WorkoutDomainException>().WithMessage("Sets must be greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WorkoutExercise_Creation_ShouldThrowException_WithInvalidReps(int invalidReps)
    {
        // Arrange & Act & Assert
        var act = () => new WorkoutExercise(Guid.NewGuid(), 3, invalidReps, 60, 1);
        act.Should().Throw<WorkoutDomainException>().WithMessage("Reps must be greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WorkoutExercise_Creation_ShouldThrowException_WithInvalidOrder(int invalidOrder)
    {
        // Arrange & Act & Assert
        var act = () => new WorkoutExercise(Guid.NewGuid(), 3, 12, 60, invalidOrder);
        act.Should().Throw<WorkoutDomainException>().WithMessage("Order must be at least 1");
    }

    [Fact]
    public void WorkoutExercise_Creation_ShouldThrowException_WithNegativeRestSeconds()
    {
        // Arrange & Act & Assert
        var act = () => new WorkoutExercise(Guid.NewGuid(), 3, 12, -10, 1);
        act.Should().Throw<WorkoutDomainException>().WithMessage("Rest time cannot be negative");
    }

    [Fact]
    public void WorkoutExercise_Creation_ShouldSucceed_WithNullRestSeconds()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();

        // Act
        var workoutExercise = new WorkoutExercise(exerciseId, 3, 12, null, 1);

        // Assert
        workoutExercise.RestSeconds.Should().BeNull();
    }

    [Fact]
    public void UpdateParameters_ShouldSucceed_WithValidData()
    {
        // Arrange
        var workoutExercise = CreateValidWorkoutExercise();
        var newSets = 4;
        var newReps = 15;
        var newRestSeconds = 90;

        // Act
        workoutExercise.UpdateParameters(newSets, newReps, newRestSeconds);

        // Assert
        workoutExercise.Sets.Should().Be(newSets);
        workoutExercise.Reps.Should().Be(newReps);
        workoutExercise.RestSeconds.Should().Be(newRestSeconds);
    }

    [Fact]
    public void UpdateOrder_ShouldSucceed_WithValidOrder()
    {
        // Arrange
        var workoutExercise = CreateValidWorkoutExercise();
        var newOrder = 5;

        // Act
        workoutExercise.UpdateOrder(newOrder);

        // Assert
        workoutExercise.Order.Should().Be(newOrder);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateOrder_ShouldThrowException_WithInvalidOrder(int invalidOrder)
    {
        // Arrange
        var workoutExercise = CreateValidWorkoutExercise();

        // Act & Assert
        var act = () => workoutExercise.UpdateOrder(invalidOrder);
        act.Should().Throw<WorkoutDomainException>();
    }

    [Fact]
    public void SetNotes_ShouldSucceed_WithValidNotes()
    {
        // Arrange
        var workoutExercise = CreateValidWorkoutExercise();
        var notes = "Focus on form";

        // Act
        workoutExercise.SetNotes(notes);

        // Assert
        workoutExercise.Notes.Should().Be(notes);
    }

    [Fact]
    public void EstimateTimeMinutes_ShouldCalculateCorrectly_WithRestTime()
    {
        // Arrange
        var workoutExercise = new WorkoutExercise(Guid.NewGuid(), 3, 12, 60, 1);

        // Act
        var estimatedTime = workoutExercise.EstimateTimeMinutes();

        // Assert
        // 3 sets * (assumed 2 seconds per rep * 12 reps + 60 seconds rest) = 3 * 84 = 252 seconds = ~4.2 minutes
        estimatedTime.Should().BeGreaterThan(3);
        estimatedTime.Should().BeLessThan(6);
    }

    [Fact]
    public void EstimateTimeMinutes_ShouldCalculateCorrectly_WithoutRestTime()
    {
        // Arrange
        var workoutExercise = new WorkoutExercise(Guid.NewGuid(), 3, 12, null, 1);

        // Act
        var estimatedTime = workoutExercise.EstimateTimeMinutes();

        // Assert
        // Should still provide a reasonable estimate even without explicit rest time
        estimatedTime.Should().BeGreaterThan(1);
        estimatedTime.Should().BeLessThan(4);
    }

    private static WorkoutExercise CreateValidWorkoutExercise()
    {
        return new WorkoutExercise(Guid.NewGuid(), 3, 12, 60, 1);
    }
}

using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FluentAssertions;

namespace FitnessApp.Modules.Workouts.Tests.Domain.ValueObjects;

public class ExerciseParametersTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateParameters()
    {
        // Arrange
        var reps = 12;
        var sets = 3;
        var duration = TimeSpan.FromSeconds(30);
        var weight = 20.5;
        var restTime = TimeSpan.FromSeconds(60);
        var notes = "Test notes";

        // Act
        var parameters = new ExerciseParameters(reps, sets, duration, weight, restTime, notes);

        // Assert
        parameters.Reps.Should().Be(reps);
        parameters.Sets.Should().Be(sets);
        parameters.Duration.Should().Be(duration);
        parameters.Weight.Should().Be(weight);
        parameters.RestTime.Should().Be(restTime);
        parameters.Notes.Should().Be(notes);
    }

    [Fact]
    public void Constructor_NullValues_ShouldAcceptNulls()
    {
        // Act
        var parameters = new ExerciseParameters();

        // Assert
        parameters.Reps.Should().BeNull();
        parameters.Sets.Should().BeNull();
        parameters.Duration.Should().BeNull();
        parameters.Weight.Should().BeNull();
        parameters.RestTime.Should().BeNull();
        parameters.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Constructor_InvalidReps_ShouldThrowException(int invalidReps)
    {
        // Act & Assert
        var act = () => new ExerciseParameters(reps: invalidReps);
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*Reps*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Constructor_InvalidSets_ShouldThrowException(int invalidSets)
    {
        // Act & Assert
        var act = () => new ExerciseParameters(sets: invalidSets);
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*Sets*");
    }

    [Fact]
    public void Constructor_NegativeWeight_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new ExerciseParameters(weight: -1.0);
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*Weight*");
    }

    [Fact]
    public void Constructor_InvalidDuration_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new ExerciseParameters(duration: TimeSpan.FromSeconds(-1));
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*Duration*");
    }

    [Fact]
    public void Constructor_NegativeRestTime_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new ExerciseParameters(restTime: TimeSpan.FromSeconds(-1));
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*Rest time*");
    }

    [Fact]
    public void Equals_SameParameters_ShouldBeEqual()
    {
        // Arrange
        var parameters1 = new ExerciseParameters(10, 3, TimeSpan.FromSeconds(30), 20.0, TimeSpan.FromMinutes(1));
        var parameters2 = new ExerciseParameters(10, 3, TimeSpan.FromSeconds(30), 20.0, TimeSpan.FromMinutes(1));

        // Act & Assert
        parameters1.Should().Be(parameters2);
        parameters1.Equals(parameters2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentParameters_ShouldNotBeEqual()
    {
        // Arrange
        var parameters1 = new ExerciseParameters(10, 3, TimeSpan.FromSeconds(30), 20.0, TimeSpan.FromMinutes(1));
        var parameters2 = new ExerciseParameters(12, 3, TimeSpan.FromSeconds(30), 20.0, TimeSpan.FromMinutes(1));

        // Act & Assert
        parameters1.Should().NotBe(parameters2);
        parameters1.Equals(parameters2).Should().BeFalse();
    }

    [Fact]
    public void ForReps_ValidValues_ShouldCreateParameters()
    {
        // Arrange
        var reps = 15;
        var sets = 4;
        var restTime = TimeSpan.FromSeconds(90);

        // Act
        var parameters = ExerciseParameters.ForReps(reps, sets, restTime);

        // Assert
        parameters.Reps.Should().Be(reps);
        parameters.Sets.Should().Be(sets);
        parameters.RestTime.Should().Be(restTime);
        parameters.Duration.Should().BeNull();
        parameters.Weight.Should().BeNull();
    }

    [Fact]
    public void GetHashCode_SameParameters_ShouldHaveSameHashCode()
    {
        // Arrange
        var parameters1 = new ExerciseParameters(10, 3, TimeSpan.FromSeconds(30), 20.0, TimeSpan.FromMinutes(1));
        var parameters2 = new ExerciseParameters(10, 3, TimeSpan.FromSeconds(30), 20.0, TimeSpan.FromMinutes(1));

        // Act & Assert
        parameters1.GetHashCode().Should().Be(parameters2.GetHashCode());
    }

    [Fact]
    public void Notes_WithWhitespace_ShouldBeTrimmed()
    {
        // Arrange
        var notes = "  Test notes  ";

        // Act
        var parameters = new ExerciseParameters(notes: notes);

        // Assert
        parameters.Notes.Should().Be("Test notes");
    }
}

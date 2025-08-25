using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FluentAssertions;

namespace FitnessApp.Modules.Workouts.Tests.Domain.ValueObjects;

public class DurationTests
{
    [Fact]
    public void FromMinutes_ValidMinutes_ShouldCreateDuration()
    {
        // Arrange
        var minutes = 30;

        // Act
        var duration = Duration.FromMinutes(minutes);

        // Assert
        duration.TotalMinutes.Should().Be(minutes);
        duration.Value.TotalMinutes.Should().Be(minutes);
    }

    [Fact]
    public void FromHours_ValidHours_ShouldCreateDuration()
    {
        // Arrange
        var hours = 1.5;

        // Act
        var duration = Duration.FromHours(hours);

        // Assert
        duration.TotalHours.Should().Be(hours);
        duration.TotalMinutes.Should().Be(90);
    }

    [Fact]
    public void Constructor_NegativeDuration_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new Duration(TimeSpan.FromMinutes(-1));
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*positive*");
    }

    [Fact]
    public void Constructor_ZeroDuration_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new Duration(TimeSpan.Zero);
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*positive*");
    }

    [Fact]
    public void Constructor_TooLongDuration_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new Duration(TimeSpan.FromHours(6));
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*exceed*");
    }

    [Fact]
    public void Equals_SameDuration_ShouldBeEqual()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(30);
        var duration2 = Duration.FromMinutes(30);

        // Act & Assert
        duration1.Should().Be(duration2);
        duration1.Equals(duration2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentDuration_ShouldNotBeEqual()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(30);
        var duration2 = Duration.FromMinutes(45);

        // Act & Assert
        duration1.Should().NotBe(duration2);
        duration1.Equals(duration2).Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_ToTimeSpan_ShouldWork()
    {
        // Arrange
        var duration = Duration.FromMinutes(30);

        // Act
        TimeSpan timeSpan = duration;

        // Assert
        timeSpan.TotalMinutes.Should().Be(30);
    }

    [Fact]
    public void ExplicitConversion_FromTimeSpan_ShouldWork()
    {
        // Arrange
        var timeSpan = TimeSpan.FromMinutes(45);

        // Act
        var duration = (Duration)timeSpan;

        // Assert
        duration.TotalMinutes.Should().Be(45);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var duration = Duration.FromMinutes(90);

        // Act
        var result = duration.ToString();

        // Assert
        result.Should().Be("01:30");
    }

    [Fact]
    public void GetHashCode_SameDurations_ShouldHaveSameHashCode()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(30);
        var duration2 = Duration.FromMinutes(30);

        // Act & Assert
        duration1.GetHashCode().Should().Be(duration2.GetHashCode());
    }
}

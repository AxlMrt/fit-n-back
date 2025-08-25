using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FluentAssertions;

namespace FitnessApp.Modules.Workouts.Tests.Domain.Entities;

public class WorkoutTests
{
    [Fact]
    public void Constructor_ValidData_ShouldCreateWorkout()
    {
        // Arrange
        var name = "Test Workout";
        var type = WorkoutType.Fixed;
        var difficulty = DifficultyLevel.Intermediate;
        var estimatedDuration = Duration.FromMinutes(30);
        var equipment = EquipmentType.None;
        var userId = Guid.NewGuid();

        // Act
        var workout = new Workout(name, type, difficulty, estimatedDuration, equipment, userId);

        // Assert
        workout.Name.Should().Be(name);
        workout.Type.Should().Be(type);
        workout.Difficulty.Should().Be(difficulty);
        workout.EstimatedDuration.Should().Be(estimatedDuration);
        workout.RequiredEquipment.Should().Be(equipment);
        workout.CreatedByUserId.Should().Be(userId);
        workout.IsActive.Should().BeTrue();
        workout.Phases.Should().BeEmpty();
        workout.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidName_ShouldThrowException(string invalidName)
    {
        // Act & Assert
        var act = () => new Workout(invalidName, WorkoutType.Fixed, 
            DifficultyLevel.Beginner, Duration.FromMinutes(30), EquipmentType.None, Guid.NewGuid());
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*name*");
    }

    [Fact]
    public void Constructor_NullDuration_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new Workout("Name", WorkoutType.Fixed, 
            DifficultyLevel.Beginner, null!, EquipmentType.None, Guid.NewGuid());
        
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddPhase_ValidPhase_ShouldAddPhase()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        var phaseType = WorkoutPhaseType.WarmUp;
        var phaseName = "Warm Up";
        var phaseDuration = Duration.FromMinutes(10);

        // Act
        var phase = workout.AddPhase(phaseType, phaseName, phaseDuration);

        // Assert
        workout.Phases.Should().HaveCount(1);
        workout.Phases.First().Should().Be(phase);
        phase.Type.Should().Be(phaseType);
        phase.Name.Should().Be(phaseName);
        phase.EstimatedDuration.Should().Be(phaseDuration);
        workout.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RemovePhase_ExistingPhase_ShouldRemovePhase()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        var phase = workout.AddPhase(WorkoutPhaseType.WarmUp, "Warm Up", Duration.FromMinutes(10));

        // Act
        workout.RemovePhase(phase.Id);

        // Assert
        workout.Phases.Should().BeEmpty();
    }

    [Fact]
    public void RemovePhase_NonExistentPhase_ShouldThrowException()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var act = () => workout.RemovePhase(nonExistentId);
        
        act.Should().Throw<WorkoutDomainException>()
           .WithMessage("*not found*");
    }

    [Fact]
    public void Deactivate_ActiveWorkout_ShouldDeactivate()
    {
        // Arrange
        var workout = CreateSampleWorkout();

        // Act
        workout.Deactivate();

        // Assert
        workout.IsActive.Should().BeFalse();
        workout.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Reactivate_InactiveWorkout_ShouldReactivate()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        workout.Deactivate();

        // Act
        workout.Reactivate();

        // Assert
        workout.IsActive.Should().BeTrue();
        workout.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateName_ValidName_ShouldUpdateWorkout()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        var newName = "Updated Workout";

        // Act
        workout.UpdateName(newName);

        // Assert
        workout.Name.Should().Be(newName);
        workout.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateDifficulty_ValidDifficulty_ShouldUpdateWorkout()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        var newDifficulty = DifficultyLevel.Advanced;

        // Act
        workout.UpdateDifficulty(newDifficulty);

        // Assert
        workout.Difficulty.Should().Be(newDifficulty);
        workout.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateRequiredEquipment_ValidEquipment_ShouldUpdateWorkout()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        var newEquipment = EquipmentType.FreeWeights;

        // Act
        workout.UpdateRequiredEquipment(newEquipment);

        // Assert
        workout.RequiredEquipment.Should().Be(newEquipment);
        workout.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CalculateActualDuration_WithPhases_ShouldReturnCorrectDuration()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        workout.AddPhase(WorkoutPhaseType.WarmUp, "Warm Up", Duration.FromMinutes(10));
        workout.AddPhase(WorkoutPhaseType.MainEffort, "Main", Duration.FromMinutes(30));

        // Act
        var totalDuration = workout.CalculateActualDuration();

        // Assert
        totalDuration.TotalMinutes.Should().Be(40);
    }

    [Fact]
    public void CalculateActualDuration_NoPhases_ShouldReturnMinimumDuration()
    {
        // Arrange
        var workout = CreateSampleWorkout();

        // Act
        var totalDuration = workout.CalculateActualDuration();

        // Assert
        totalDuration.TotalMinutes.Should().Be(1); // Minimum 1 minute when no phases
    }

    [Fact]
    public void CreateUserWorkout_ShouldCreateWorkoutWithUserType()
    {
        // Arrange & Act
        var workout = Workout.CreateUserWorkout(
            "User Workout",
            DifficultyLevel.Beginner,
            Duration.FromMinutes(20),
            EquipmentType.Mat,
            Guid.NewGuid());

        // Assert
        workout.Type.Should().Be(WorkoutType.UserCreated);
        workout.CreatedByUserId.Should().NotBeNull();
        workout.CreatedByCoachId.Should().BeNull();
    }

    [Fact]
    public void CreateCoachWorkout_ShouldCreateWorkoutWithCoachType()
    {
        // Arrange & Act
        var workout = Workout.CreateCoachWorkout(
            "Coach Workout",
            DifficultyLevel.Advanced,
            Duration.FromMinutes(60),
            EquipmentType.GymEquipment,
            Guid.NewGuid());

        // Assert
        workout.Type.Should().Be(WorkoutType.Fixed);
        workout.CreatedByCoachId.Should().NotBeNull();
        workout.CreatedByUserId.Should().BeNull();
    }

    [Fact]
    public void IsCreatedByUser_WithMatchingUserId_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workout = Workout.CreateUserWorkout(
            "Test",
            DifficultyLevel.Beginner,
            Duration.FromMinutes(20),
            EquipmentType.None,
            userId);

        // Act & Assert
        workout.IsCreatedByUser(userId).Should().BeTrue();
        workout.IsCreatedByUser(Guid.NewGuid()).Should().BeFalse();
    }

    private static Workout CreateSampleWorkout()
    {
        return new Workout(
            "Test Workout",
            WorkoutType.Fixed, 
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(30),
            EquipmentType.None, 
            Guid.NewGuid());
    }
}

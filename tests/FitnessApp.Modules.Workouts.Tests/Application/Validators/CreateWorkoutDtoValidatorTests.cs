using FitnessApp.Modules.Workouts.Application.DTOs;
using FitnessApp.Modules.Workouts.Application.Validators;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FluentAssertions;

namespace FitnessApp.Modules.Workouts.Tests.Application.Validators;

public class CreateWorkoutDtoValidatorTests
{
    private readonly CreateWorkoutDtoValidator _validator;

    public CreateWorkoutDtoValidatorTests()
    {
        _validator = new CreateWorkoutDtoValidator();
    }

    [Fact]
    public void Validate_ValidDto_ShouldBeValid()
    {
        // Arrange
        var dto = new CreateWorkoutDto(
            Name: "Test Workout",
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>()
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldBeInvalid(string invalidName)
    {
        // Arrange
        var dto = new CreateWorkoutDto(
            Name: invalidName,
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>()
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.Name));
    }

    [Fact]
    public void Validate_NameTooLong_ShouldBeInvalid()
    {
        // Arrange
        var longName = new string('A', 201); // Max length is 200
        var dto = new CreateWorkoutDto(
            Name: longName,
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>()
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidDuration_ShouldBeInvalid(int invalidDuration)
    {
        // Arrange
        var dto = new CreateWorkoutDto(
            Name: "Test Workout",
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: invalidDuration,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>()
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.EstimatedDurationMinutes));
    }

    [Fact]
    public void Validate_DurationTooLong_ShouldBeInvalid()
    {
        // Arrange
        var dto = new CreateWorkoutDto(
            Name: "Test Workout",
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 481, // Max is 480 minutes (8 hours)
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>()
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.EstimatedDurationMinutes));
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldBeInvalid()
    {
        // Arrange
        var longDescription = new string('A', 1001); // Max length is 1000
        var dto = new CreateWorkoutDto(
            Name: "Test Workout",
            Description: longDescription,
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>()
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.Description));
    }

    [Fact]
    public void Validate_InvalidDifficulty_ShouldBeInvalid()
    {
        // Arrange
        var dto = new CreateWorkoutDto(
            Name: "Test Workout",
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: (DifficultyLevel)999, // Invalid enum value
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>()
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.Difficulty));
    }

    [Fact]
    public void Validate_WithValidPhases_ShouldBeValid()
    {
        // Arrange
        var dto = new CreateWorkoutDto(
            Name: "Test Workout",
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>
            {
                new(
                    Type: WorkoutPhaseType.WarmUp,
                    Name: "Warm Up",
                    Description: "Warm up phase",
                    EstimatedDurationMinutes: 10,
                    Exercises: new List<CreateWorkoutExerciseDto>())
            }
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidPhases_ShouldBeInvalid()
    {
        // Arrange
        var dto = new CreateWorkoutDto(
            Name: "Test Workout",
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>
            {
                new(
                    Type: WorkoutPhaseType.WarmUp,
                    Name: "", // Empty name should be invalid
                    Description: null,
                    EstimatedDurationMinutes: 10,
                    Exercises: new List<CreateWorkoutExerciseDto>())
            }
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Phases"));
    }
}

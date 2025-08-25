using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Application.Validators;
using FitnessApp.Modules.Exercises.Domain.Enums;

namespace FitnessApp.Modules.Exercises.Tests.Application.Validators;
public class ExerciseValidatorsTests
{
    [Fact]
    public void CreateExerciseDtoValidator_ShouldPass_WhenValidDto()
    {
        // Arrange
        var validator = new CreateExerciseDtoValidator();
        var dto = new CreateExerciseDto
        {
            Name = "Push-ups",
            Description = "Basic push-up exercise",
            Type = ExerciseType.Strength,
            MuscleGroups = new List<string> { "CHEST", "ARMS" },
            Difficulty = DifficultyLevel.Intermediate,
            Equipment = new List<string> { "Mat" },
            Instructions = "Start in plank position, lower body, push back up"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateExerciseDtoValidator_ShouldFail_WhenNameIsInvalid(string invalidName)
    {
        // Arrange
        var validator = new CreateExerciseDtoValidator();
        var dto = new CreateExerciseDto
        {
            Name = invalidName,
            Type = ExerciseType.Strength,
            Difficulty = DifficultyLevel.Intermediate
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateExerciseDtoValidator_ShouldFail_WhenNameTooShort()
    {
        // Arrange
        var validator = new CreateExerciseDtoValidator();
        var dto = new CreateExerciseDto
        {
            Name = "A", // Only 1 character
            Type = ExerciseType.Strength,
            Difficulty = DifficultyLevel.Intermediate
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage.Contains("between 2 and 100"));
    }

    [Fact]
    public void CreateExerciseDtoValidator_ShouldFail_WhenNameTooLong()
    {
        // Arrange
        var validator = new CreateExerciseDtoValidator();
        var dto = new CreateExerciseDto
        {
            Name = new string('a', 101), // 101 characters
            Type = ExerciseType.Strength,
            Difficulty = DifficultyLevel.Intermediate
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage.Contains("between 2 and 100"));
    }

    [Fact]
    public void CreateExerciseDtoValidator_ShouldFail_WhenDescriptionTooLong()
    {
        // Arrange
        var validator = new CreateExerciseDtoValidator();
        var dto = new CreateExerciseDto
        {
            Name = "Push-ups",
            Description = new string('a', 1001), // 1001 characters
            Type = ExerciseType.Strength,
            Difficulty = DifficultyLevel.Intermediate
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage.Contains("cannot exceed 1000"));
    }

    [Fact]
    public void CreateExerciseDtoValidator_ShouldFail_WhenInvalidMuscleGroup()
    {
        // Arrange
        var validator = new CreateExerciseDtoValidator();
        var dto = new CreateExerciseDto
        {
            Name = "Push-ups",
            Type = ExerciseType.Strength,
            MuscleGroups = new List<string> { "INVALID_MUSCLE_GROUP" },
            Difficulty = DifficultyLevel.Intermediate
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MuscleGroups" && e.ErrorMessage.Contains("Invalid muscle group"));
    }

    [Fact]
    public void CreateExerciseDtoValidator_ShouldFail_WhenTooManyEquipmentItems()
    {
        // Arrange
        var validator = new CreateExerciseDtoValidator();
        var dto = new CreateExerciseDto
        {
            Name = "Push-ups",
            Type = ExerciseType.Strength,
            Equipment = Enumerable.Range(1, 11).Select(i => $"Equipment{i}").ToList(), // 11 items
            Difficulty = DifficultyLevel.Intermediate
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Equipment" && e.ErrorMessage.Contains("more than 10"));
    }

    [Fact]
    public void CreateExerciseDtoValidator_ShouldFail_WhenEquipmentItemTooLong()
    {
        // Arrange
        var validator = new CreateExerciseDtoValidator();
        var dto = new CreateExerciseDto
        {
            Name = "Push-ups",
            Type = ExerciseType.Strength,
            Equipment = new List<string> { new string('a', 51) }, // 51 characters
            Difficulty = DifficultyLevel.Intermediate
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Equipment" && e.ErrorMessage.Contains("between 1 and 50"));
    }

    [Fact]
    public void CreateExerciseDtoValidator_ShouldFail_WhenInstructionsTooLong()
    {
        // Arrange
        var validator = new CreateExerciseDtoValidator();
        var dto = new CreateExerciseDto
        {
            Name = "Push-ups",
            Type = ExerciseType.Strength,
            Instructions = new string('a', 2001), // 2001 characters
            Difficulty = DifficultyLevel.Intermediate
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Instructions" && e.ErrorMessage.Contains("cannot exceed 2000"));
    }

    [Fact]
    public void UpdateExerciseDtoValidator_ShouldPass_WhenValidDto()
    {
        // Arrange
        var validator = new UpdateExerciseDtoValidator();
        var dto = new UpdateExerciseDto
        {
            Name = "Modified Push-ups",
            Description = "Updated description",
            Type = ExerciseType.Strength,
            MuscleGroups = new List<string> { "CHEST" },
            Difficulty = DifficultyLevel.Advanced,
            Equipment = new List<string> { "Mat", "Weights" }
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void UpdateExerciseDtoValidator_ShouldPass_WhenAllPropertiesNull()
    {
        // Arrange
        var validator = new UpdateExerciseDtoValidator();
        var dto = new UpdateExerciseDto(); // All properties null

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ExerciseQueryDtoValidator_ShouldPass_WhenValidDto()
    {
        // Arrange
        var validator = new ExerciseQueryDtoValidator();
        var dto = new ExerciseQueryDto
        {
            NameFilter = "Push",
            Type = ExerciseType.Strength,
            Difficulty = DifficultyLevel.Intermediate,
            MuscleGroups = new List<string> { "CHEST" },
            RequiresEquipment = false,
            IsActive = true,
            PageNumber = 1,
            PageSize = 10,
            SortBy = "Name",
            SortDescending = false
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ExerciseQueryDtoValidator_ShouldFail_WhenPageNumberInvalid(int invalidPageNumber)
    {
        // Arrange
        var validator = new ExerciseQueryDtoValidator();
        var dto = new ExerciseQueryDto { PageNumber = invalidPageNumber };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PageNumber" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void ExerciseQueryDtoValidator_ShouldFail_WhenPageSizeInvalid(int invalidPageSize)
    {
        // Arrange
        var validator = new ExerciseQueryDtoValidator();
        var dto = new ExerciseQueryDto { PageSize = invalidPageSize };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PageSize" && e.ErrorMessage.Contains("between 1 and 100"));
    }

    [Fact]
    public void ExerciseQueryDtoValidator_ShouldFail_WhenNameFilterTooLong()
    {
        // Arrange
        var validator = new ExerciseQueryDtoValidator();
        var dto = new ExerciseQueryDto
        {
            NameFilter = new string('a', 101) // 101 characters
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "NameFilter" && e.ErrorMessage.Contains("cannot exceed 100"));
    }

    [Fact]
    public void ExerciseQueryDtoValidator_ShouldFail_WhenInvalidSortField()
    {
        // Arrange
        var validator = new ExerciseQueryDtoValidator();
        var dto = new ExerciseQueryDto
        {
            SortBy = "InvalidField"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SortBy" && e.ErrorMessage.Contains("Invalid sort field"));
    }

    [Theory]
    [InlineData("name")]
    [InlineData("type")]
    [InlineData("difficulty")]
    [InlineData("createdat")]
    [InlineData("updatedat")]
    public void ExerciseQueryDtoValidator_ShouldPass_WhenValidSortFields(string sortField)
    {
        // Arrange
        var validator = new ExerciseQueryDtoValidator();
        var dto = new ExerciseQueryDto { SortBy = sortField };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }
}

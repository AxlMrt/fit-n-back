using FluentAssertions;
using FluentValidation.TestHelper;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Tests.Application.Validators;

public class CreateWorkoutDtoValidatorTests
{
    // Note: Les tests de validation seront implémentés une fois que les validateurs seront créés
    // Pour l'instant, nous testons la structure des DTOs
    
    [Fact]
    public void CreateWorkoutDto_ShouldBeValid_WithValidData()
    {
        // Arrange
        var dto = new CreateWorkoutDto
        {
            Name = "Valid Workout",
            Description = "A valid workout description",
            Type = WorkoutType.Template,
            Category = WorkoutCategory.Strength,
            Difficulty = DifficultyLevel.Intermediate,
            EstimatedDurationMinutes = 45,
            Phases = []
        };

        // Act & Assert
        dto.Should().NotBeNull();
        dto.Name.Should().Be("Valid Workout");
        dto.Type.Should().Be(WorkoutType.Template);
        dto.Category.Should().Be(WorkoutCategory.Strength);
        dto.Difficulty.Should().Be(DifficultyLevel.Intermediate);
        dto.EstimatedDurationMinutes.Should().Be(45);
        dto.Phases.Should().BeEmpty();
    }

    [Fact]
    public void CreateWorkoutDto_ShouldHandlePhases_WithValidPhases()
    {
        // Arrange
        var dto = new CreateWorkoutDto
        {
            Name = "Workout with Phases",
            Type = WorkoutType.UserCreated,
            Category = WorkoutCategory.Mixed,
            Difficulty = DifficultyLevel.Intermediate,
            EstimatedDurationMinutes = 60,
            Phases = new List<CreateWorkoutPhaseDto>
            {
                new CreateWorkoutPhaseDto
                {
                    Type = WorkoutPhaseType.WarmUp,
                    Name = "Warm Up",
                    Description = "Preparation phase",
                    EstimatedDurationMinutes = 10,
                    Exercises = []
                },
                new CreateWorkoutPhaseDto
                {
                    Type = WorkoutPhaseType.MainEffort,
                    Name = "Main Workout",
                    Description = "Main effort phase",
                    EstimatedDurationMinutes = 40,
                    Exercises = [
                        new CreateWorkoutExerciseDto
                        {
                            ExerciseId = Guid.NewGuid(),
                            ExerciseName = "Push-ups",
                            Reps = 12,
                            Sets = 3
                        }
                    ]
                },
                new CreateWorkoutPhaseDto
                {
                    Type = WorkoutPhaseType.Stretching,
                    Name = "Cool Down",
                    Description = "Recovery phase",
                    EstimatedDurationMinutes = 10,
                    Exercises = []
                }
            }
        };

        // Act & Assert
        dto.Should().NotBeNull();
        dto.Phases.Should().HaveCount(3);
        dto.Phases.First().Type.Should().Be(WorkoutPhaseType.WarmUp);
        dto.Phases.Skip(1).First().Exercises.Should().HaveCount(1);
    }
}

public class UpdateWorkoutDtoValidatorTests
{
    [Fact]
    public void UpdateWorkoutDto_ShouldAllowPartialUpdates()
    {
        // Arrange
        var dto = new UpdateWorkoutDto
        {
            Name = "Updated Name",
            Description = null, // Allows null for optional updates
            Type = null,
            Difficulty = DifficultyLevel.Advanced,
            EstimatedDurationMinutes = null
        };

        // Act & Assert
        dto.Should().NotBeNull();
        dto.Name.Should().Be("Updated Name");
        dto.Description.Should().BeNull();
        dto.Type.Should().BeNull();
        dto.Difficulty.Should().Be(DifficultyLevel.Advanced);
        dto.EstimatedDurationMinutes.Should().BeNull();
    }
}

public class WorkoutQueryDtoValidatorTests
{
    [Fact]
    public void WorkoutQueryDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new WorkoutQueryDto();

        // Assert
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(20);
        dto.SortBy.Should().Be("Name");
        dto.SortDescending.Should().BeFalse();
        dto.NameFilter.Should().BeNull();
        dto.Type.Should().BeNull();
        dto.Difficulty.Should().BeNull();
        dto.IsActive.Should().BeNull();
    }

    [Fact]
    public void WorkoutQueryDto_ShouldAllowCustomValues()
    {
        // Arrange & Act
        var dto = new WorkoutQueryDto
        {
            PageNumber = 2,
            PageSize = 50,
            SortBy = "CreatedAt",
            SortDescending = true,
            NameFilter = "Running",
            Type = WorkoutType.UserCreated,
            Difficulty = DifficultyLevel.Advanced,
            IsActive = true,
            MinDurationMinutes = 30,
            MaxDurationMinutes = 90
        };

        // Assert
        dto.PageNumber.Should().Be(2);
        dto.PageSize.Should().Be(50);
        dto.SortBy.Should().Be("CreatedAt");
        dto.SortDescending.Should().BeTrue();
        dto.NameFilter.Should().Be("Running");
        dto.Type.Should().Be(WorkoutType.UserCreated);
        dto.Difficulty.Should().Be(DifficultyLevel.Advanced);
        dto.IsActive.Should().BeTrue();
        dto.MinDurationMinutes.Should().Be(30);
        dto.MaxDurationMinutes.Should().Be(90);
    }
}

public class AddWorkoutPhaseDtoValidatorTests
{
    [Fact]
    public void AddWorkoutPhaseDto_ShouldBeValid_WithValidData()
    {
        // Arrange
        var dto = new AddWorkoutPhaseDto
        {
            Type = WorkoutPhaseType.WarmUp,
            Name = "Warm Up Phase",
            Description = "Preparation phase",
            EstimatedDurationMinutes = 10
        };

        // Act & Assert
        dto.Should().NotBeNull();
        dto.Type.Should().Be(WorkoutPhaseType.WarmUp);
        dto.Name.Should().Be("Warm Up Phase");
        dto.Description.Should().Be("Preparation phase");
        dto.EstimatedDurationMinutes.Should().Be(10);
    }
}

public class AddWorkoutExerciseDtoValidatorTests
{
    [Fact]
    public void AddWorkoutExerciseDto_ShouldBeValid_WithSetsAndReps()
    {
        // Arrange
        var dto = new AddWorkoutExerciseDto
        {
            ExerciseId = Guid.NewGuid(),
            Sets = 3,
            Reps = 12,
            DurationSeconds = null
        };

        // Act & Assert
        dto.Should().NotBeNull();
        dto.ExerciseId.Should().NotBeEmpty();
        dto.Sets.Should().Be(3);
        dto.Reps.Should().Be(12);
        dto.DurationSeconds.Should().BeNull();
    }

    [Fact]
    public void AddWorkoutExerciseDto_ShouldBeValid_WithDuration()
    {
        // Arrange
        var dto = new AddWorkoutExerciseDto
        {
            ExerciseId = Guid.NewGuid(),
            Sets = null,
            Reps = null,
            DurationSeconds = 60
        };

        // Act & Assert
        dto.Should().NotBeNull();
        dto.ExerciseId.Should().NotBeEmpty();
        dto.Sets.Should().BeNull();
        dto.Reps.Should().BeNull();
        dto.DurationSeconds.Should().Be(60);
    }
}

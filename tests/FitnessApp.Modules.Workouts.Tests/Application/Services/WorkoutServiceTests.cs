using FluentAssertions;
using AutoMapper;
using Moq;
using FitnessApp.Modules.Workouts.Application.Services;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Repositories;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Tests.Application.Services;

public class WorkoutServiceTests
{
    private readonly Mock<IWorkoutRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly WorkoutService _service;

    public WorkoutServiceTests()
    {
        _mockRepository = new Mock<IWorkoutRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new WorkoutService(_mockRepository.Object, _mockMapper.Object);
    }

    #region CreateWorkoutAsync Tests

    [Fact]
    public async Task CreateWorkoutAsync_ShouldSucceed_WithValidDto()
    {
        // Arrange
        var createDto = new CreateWorkoutDto
        {
            Name = "Test Workout",
            Description = "A test workout",
            Type = WorkoutType.UserCreated,
            Category = WorkoutCategory.Strength,
            Difficulty = DifficultyLevel.Intermediate,
            EstimatedDurationMinutes = 45,
            Phases = []
        };

        var expectedWorkout = new Workout(
            createDto.Name,
            createDto.Type,
            createDto.Category,
            createDto.Difficulty,
            createDto.EstimatedDurationMinutes
        );

        _mockMapper.Setup(m => m.Map<Workout>(It.IsAny<CreateWorkoutDto>()))
                  .Returns(expectedWorkout);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedWorkout);

        // Act
        var result = await _service.CreateWorkoutAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateWorkoutAsync_ShouldThrowException_WithNullDto()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateWorkoutAsync(null!));
    }

    #endregion

    #region GetWorkoutByIdAsync Tests

    [Fact]
    public async Task GetWorkoutByIdAsync_ShouldReturnWorkout_WhenExists()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var expectedWorkout = CreateTestWorkout();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedWorkout);

        // Act
        var result = await _service.GetWorkoutByIdAsync(workoutId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedWorkout);
    }

    [Fact]
    public async Task GetWorkoutByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Workout?)null);

        // Act
        var result = await _service.GetWorkoutByIdAsync(workoutId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdateWorkoutAsync Tests

    [Fact]
    public async Task UpdateWorkoutAsync_ShouldSucceed_WithValidData()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var updateDto = new UpdateWorkoutDto
        {
            Name = "Updated Workout",
            Description = "Updated description",
            Type = WorkoutType.UserCreated,
            Difficulty = DifficultyLevel.Advanced,
            EstimatedDurationMinutes = 60
        };

        var existingWorkout = CreateTestWorkout();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingWorkout);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateWorkoutAsync(workoutId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(updateDto.Name);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateWorkoutAsync_ShouldThrowException_WhenWorkoutNotFound()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var updateDto = new UpdateWorkoutDto { Name = "Updated" };
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Workout?)null);

        // Act & Assert
        await Assert.ThrowsAsync<WorkoutDomainException>(() => _service.UpdateWorkoutAsync(workoutId, updateDto));
    }

    #endregion

    #region DeleteWorkoutAsync Tests

    [Fact]
    public async Task DeleteWorkoutAsync_ShouldReturnTrue_WhenWorkoutExists()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.ExistsAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(workoutId, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteWorkoutAsync(workoutId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(workoutId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteWorkoutAsync_ShouldReturnFalse_WhenWorkoutNotExists()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.ExistsAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteWorkoutAsync(workoutId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task GetActiveWorkoutsAsync_ShouldReturnActiveWorkouts()
    {
        // Arrange
        var activeWorkouts = new List<Workout> { CreateTestWorkout(), CreateTestWorkout() };
        
        _mockRepository.Setup(r => r.GetActiveWorkoutsAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(activeWorkouts);

        // Act
        var result = await _service.GetActiveWorkoutsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(activeWorkouts);
    }

    [Fact]
    public async Task GetWorkoutsByTypeAsync_ShouldReturnWorkoutsOfSpecificType()
    {
        // Arrange
        var workoutType = WorkoutType.Template;
        var workouts = new List<Workout> { CreateTestWorkout() };
        
        _mockRepository.Setup(r => r.GetWorkoutsByTypeAsync(workoutType, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);

        // Act
        var result = await _service.GetWorkoutsByTypeAsync(workoutType);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(workouts);
    }

    [Fact]
    public async Task GetWorkoutsByDifficultyAsync_ShouldReturnWorkoutsOfSpecificDifficulty()
    {
        // Arrange
        var difficulty = DifficultyLevel.Intermediate;
        var workouts = new List<Workout> { CreateTestWorkout() };
        
        _mockRepository.Setup(r => r.GetWorkoutsByDifficultyAsync(difficulty, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);

        // Act
        var result = await _service.GetWorkoutsByDifficultyAsync(difficulty);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(workouts);
    }

    [Fact]
    public async Task GetWorkoutsByCategoryAsync_ShouldReturnWorkoutsOfSpecificCategory()
    {
        // Arrange
        var category = WorkoutCategory.Strength;
        var workouts = new List<Workout> { CreateTestWorkout() };
        
        _mockRepository.Setup(r => r.GetWorkoutsByCategoryAsync(category, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);

        // Act
        var result = await _service.GetWorkoutsByCategoryAsync(category);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(workouts);
    }

    [Fact]
    public async Task GetWorkoutsByCategoryAndDifficultyAsync_ShouldReturnFilteredWorkouts()
    {
        // Arrange
        var category = WorkoutCategory.Cardio;
        var difficulty = DifficultyLevel.Beginner;
        var workouts = new List<Workout> { CreateTestWorkout() };
        
        _mockRepository.Setup(r => r.GetWorkoutsByCategoryAndDifficultyAsync(category, difficulty, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);

        // Act
        var result = await _service.GetWorkoutsByCategoryAndDifficultyAsync(category, difficulty);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(workouts);
    }

    [Fact]
    public async Task GetTemplateWorkoutsAsync_ShouldReturnTemplateWorkouts()
    {
        // Arrange
        var workouts = new List<Workout> { CreateTestWorkout() };
        
        _mockRepository.Setup(r => r.GetTemplateWorkoutsAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);

        // Act
        var result = await _service.GetTemplateWorkoutsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(workouts);
    }

    [Fact]
    public async Task SearchWorkoutsAsync_ShouldReturnMatchingWorkouts()
    {
        // Arrange
        var searchTerm = "cardio";
        var category = WorkoutCategory.Cardio;
        var workouts = new List<Workout> { CreateTestWorkout() };
        
        _mockRepository.Setup(r => r.SearchWorkoutsAsync(searchTerm, category, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);

        // Act
        var result = await _service.SearchWorkoutsAsync(searchTerm, category);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(workouts);
    }

    #endregion

    #region Advanced Operations Tests

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenWorkoutExists()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.ExistsAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(workoutId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnWorkoutCount()
    {
        // Arrange
        var expectedCount = 42;
        
        _mockRepository.Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedCount);

        // Act
        var result = await _service.CountAsync();

        // Assert
        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var workouts = new List<Workout> { CreateTestWorkout() };
        var totalCount = 15;
        var expectedResult = (workouts, totalCount);
        
        _mockRepository.Setup(r => r.GetPagedAsync(page, pageSize, null, null, null, null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetPagedWorkoutsAsync(page, pageSize);

        // Assert
        result.Workouts.Should().BeEquivalentTo(workouts);
        result.TotalCount.Should().Be(totalCount);
    }

    #endregion

    #region Workout Management Tests

    [Fact]
    public async Task DuplicateWorkoutAsync_ShouldSucceed_WithExistingWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var newName = "Duplicated Workout";
        var originalWorkout = CreateTestWorkout();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(originalWorkout);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Workout w, CancellationToken ct) => w);

        // Act
        var result = await _service.DuplicateWorkoutAsync(workoutId, newName);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(newName);
        result.Id.Should().NotBe(originalWorkout.Id);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateWorkoutAsync_ShouldSucceed_WithExistingWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var workout = CreateTestWorkout();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workout);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeactivateWorkoutAsync(workoutId);

        // Assert
        result.Should().BeTrue();
        workout.IsActive.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(workout, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Workout CreateTestWorkout()
    {
        return new Workout(
            "Test Workout",
            WorkoutType.Template,
            WorkoutCategory.Mixed,
            DifficultyLevel.Intermediate,
            45
        );
    }

    #endregion
}

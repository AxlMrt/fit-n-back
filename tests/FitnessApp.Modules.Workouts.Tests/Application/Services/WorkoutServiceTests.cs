using FluentAssertions;
using AutoMapper;
using Moq;
using Microsoft.Extensions.Logging;
using FitnessApp.Modules.Workouts.Application.Services;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Repositories;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Tests.Application.Services;

public class WorkoutServiceTests
{
    private readonly Mock<IWorkoutRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<WorkoutService>> _mockLogger;
    private readonly WorkoutService _service;

    public WorkoutServiceTests()
    {
        _mockRepository = new Mock<IWorkoutRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<WorkoutService>>();
        _service = new WorkoutService(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
    }

    #region CreateUserWorkoutAsync Tests

    [Fact]
    public async Task CreateUserWorkoutAsync_ShouldSucceed_WithValidDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new CreateWorkoutDto
        {
            Name = "Test Workout",
            Description = "A test workout",
            Type = WorkoutType.Template, // Should be overridden to UserCreated
            Category = WorkoutCategory.Strength,
            Difficulty = DifficultyLevel.Intermediate,
            EstimatedDurationMinutes = 45,
            Phases = []
        };

        var expectedWorkout = CreateTestWorkout();
        var expectedDto = CreateTestWorkoutDto();

        _mockMapper.Setup(m => m.Map<Workout>(It.IsAny<CreateWorkoutDto>()))
                  .Returns(expectedWorkout);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedWorkout);
        _mockMapper.Setup(m => m.Map<WorkoutDto>(It.IsAny<Workout>()))
                  .Returns(expectedDto);

        // Act
        var result = await _service.CreateUserWorkoutAsync(createDto, userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        _mockRepository.Verify(r => r.AddAsync(It.Is<Workout>(w => w.Type == WorkoutType.UserCreated), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserWorkoutAsync_ShouldThrowException_WithNullDto()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateUserWorkoutAsync(null!, userId));
    }

    #endregion

    #region CreateTemplateWorkoutAsync Tests

    [Fact]
    public async Task CreateTemplateWorkoutAsync_ShouldSucceed_WithValidDto()
    {
        // Arrange
        var createDto = new CreateWorkoutDto
        {
            Name = "Template Workout",
            Description = "A template workout",
            Type = WorkoutType.UserCreated, // Should be overridden to Template
            Category = WorkoutCategory.Strength,
            Difficulty = DifficultyLevel.Intermediate,
            EstimatedDurationMinutes = 45,
            Phases = []
        };

        var expectedWorkout = CreateTestWorkout();
        var expectedDto = new WorkoutDto(
            Id: Guid.NewGuid(),
            Name: createDto.Name, // Use the actual DTO name
            Description: createDto.Description,
            Type: WorkoutType.Template,
            Category: createDto.Category,
            Difficulty: createDto.Difficulty,
            EstimatedDurationMinutes: createDto.EstimatedDurationMinutes,
            ImageContentId: null,
            IsActive: true,
            PhaseCount: 0,
            TotalExercises: 0,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Phases: new List<WorkoutPhaseDto>()
        );

        _mockMapper.Setup(m => m.Map<Workout>(It.IsAny<CreateWorkoutDto>()))
                  .Returns(expectedWorkout);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedWorkout);
        _mockMapper.Setup(m => m.Map<WorkoutDto>(It.IsAny<Workout>()))
                  .Returns(expectedDto);

        // Act
        var result = await _service.CreateTemplateWorkoutAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        _mockRepository.Verify(r => r.AddAsync(It.Is<Workout>(w => w.Type == WorkoutType.Template), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTemplateWorkoutAsync_ShouldThrowException_WithNullDto()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateTemplateWorkoutAsync(null!));
    }

    #endregion

    #region GetWorkoutByIdAsync Tests

    [Fact]
    public async Task GetWorkoutByIdAsync_ShouldReturnWorkoutDto_WhenExists()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var expectedWorkout = CreateTestWorkout();
        var expectedDto = CreateTestWorkoutDto();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedWorkout);
        _mockMapper.Setup(m => m.Map<WorkoutDto>(It.IsAny<Workout>()))
                  .Returns(expectedDto);

        // Act
        var result = await _service.GetWorkoutByIdAsync(workoutId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedDto);
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

    #region UpdateUserWorkoutAsync Tests

    [Fact]
    public async Task UpdateUserWorkoutAsync_ShouldSucceed_WithValidData()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateDto = new UpdateWorkoutDto
        {
            Name = "Updated Workout",
            Description = "Updated description",
            Type = WorkoutType.UserCreated,
            Difficulty = DifficultyLevel.Advanced,
            EstimatedDurationMinutes = 60
        };

        var existingWorkout = CreateTestUserWorkout(userId);
        var expectedDto = CreateTestWorkoutDto();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingWorkout);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);
        _mockMapper.Setup(m => m.Map<WorkoutDto>(It.IsAny<Workout>()))
                  .Returns(expectedDto);

        // Act
        var result = await _service.UpdateUserWorkoutAsync(workoutId, updateDto, userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedDto);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserWorkoutAsync_ShouldThrowException_WhenWorkoutNotFound()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateDto = new UpdateWorkoutDto { Name = "Updated" };
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Workout?)null);

        // Act & Assert
        await Assert.ThrowsAsync<WorkoutDomainException>(() => _service.UpdateUserWorkoutAsync(workoutId, updateDto, userId));
    }

    #endregion

    #region UpdateWorkoutAsAdminAsync Tests

    [Fact]
    public async Task UpdateWorkoutAsAdminAsync_ShouldSucceed_WithValidData()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var updateDto = new UpdateWorkoutDto
        {
            Name = "Admin Updated Workout",
            Description = "Admin updated description",
            Type = WorkoutType.Template,
            Difficulty = DifficultyLevel.Expert,
            EstimatedDurationMinutes = 90
        };

        var existingWorkout = CreateTestWorkout();
        var expectedDto = CreateTestWorkoutDto();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingWorkout);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);
        _mockMapper.Setup(m => m.Map<WorkoutDto>(It.IsAny<Workout>()))
                  .Returns(expectedDto);

        // Act
        var result = await _service.UpdateWorkoutAsAdminAsync(workoutId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedDto);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region DeleteUserWorkoutAsync Tests

    [Fact]
    public async Task DeleteUserWorkoutAsync_ShouldReturnTrue_WhenWorkoutExists()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingWorkout = CreateTestUserWorkout(userId);
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingWorkout);
        _mockRepository.Setup(r => r.DeleteAsync(workoutId, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteUserWorkoutAsync(workoutId, userId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(workoutId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserWorkoutAsync_ShouldReturnFalse_WhenWorkoutNotExists()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Workout?)null);

        // Act
        var result = await _service.DeleteUserWorkoutAsync(workoutId, userId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region DeleteWorkoutAsAdminAsync Tests

    [Fact]
    public async Task DeleteWorkoutAsAdminAsync_ShouldReturnTrue_WhenWorkoutExists()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.ExistsAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(workoutId, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteWorkoutAsAdminAsync(workoutId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(workoutId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task GetActiveWorkoutsAsync_ShouldReturnActiveWorkouts()
    {
        // Arrange
        var activeWorkouts = new List<Workout> { CreateTestWorkout(), CreateTestWorkout() };
        var expectedDtos = new List<WorkoutListDto> { CreateTestWorkoutListDto(), CreateTestWorkoutListDto() };
        
        _mockRepository.Setup(r => r.GetActiveWorkoutsAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(activeWorkouts);
        _mockMapper.Setup(m => m.Map<IEnumerable<WorkoutListDto>>(It.IsAny<IEnumerable<Workout>>()))
                  .Returns(expectedDtos);

        // Act
        var result = await _service.GetActiveWorkoutsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedDtos);
    }

    [Fact]
    public async Task GetWorkoutsByTypeAsync_ShouldReturnWorkoutsOfSpecificType()
    {
        // Arrange
        var workoutType = WorkoutType.Template;
        var workouts = new List<Workout> { CreateTestWorkout() };
        var expectedDtos = new List<WorkoutListDto> { CreateTestWorkoutListDto() };
        
        _mockRepository.Setup(r => r.GetWorkoutsByTypeAsync(workoutType, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);
        _mockMapper.Setup(m => m.Map<IEnumerable<WorkoutListDto>>(It.IsAny<IEnumerable<Workout>>()))
                  .Returns(expectedDtos);

        // Act
        var result = await _service.GetWorkoutsByTypeAsync(workoutType);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(expectedDtos);
    }

    [Fact]
    public async Task GetWorkoutsByDifficultyAsync_ShouldReturnWorkoutsOfSpecificDifficulty()
    {
        // Arrange
        var difficulty = DifficultyLevel.Intermediate;
        var workouts = new List<Workout> { CreateTestWorkout() };
        var expectedDtos = new List<WorkoutListDto> { CreateTestWorkoutListDto() };
        
        _mockRepository.Setup(r => r.GetWorkoutsByDifficultyAsync(difficulty, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);
        _mockMapper.Setup(m => m.Map<IEnumerable<WorkoutListDto>>(It.IsAny<IEnumerable<Workout>>()))
                  .Returns(expectedDtos);

        // Act
        var result = await _service.GetWorkoutsByDifficultyAsync(difficulty);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(expectedDtos);
    }

    [Fact]
    public async Task GetWorkoutsByCategoryAsync_ShouldReturnWorkoutsOfSpecificCategory()
    {
        // Arrange
        var category = WorkoutCategory.Strength;
        var workouts = new List<Workout> { CreateTestWorkout() };
        var expectedDtos = new List<WorkoutListDto> { CreateTestWorkoutListDto() };
        
        _mockRepository.Setup(r => r.GetWorkoutsByCategoryAsync(category, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);
        _mockMapper.Setup(m => m.Map<IEnumerable<WorkoutListDto>>(It.IsAny<IEnumerable<Workout>>()))
                  .Returns(expectedDtos);

        // Act
        var result = await _service.GetWorkoutsByCategoryAsync(category);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(expectedDtos);
    }

    [Fact]
    public async Task GetWorkoutsByCategoryAndDifficultyAsync_ShouldReturnFilteredWorkouts()
    {
        // Arrange
        var category = WorkoutCategory.Cardio;
        var difficulty = DifficultyLevel.Beginner;
        var workouts = new List<Workout> { CreateTestWorkout() };
        var expectedDtos = new List<WorkoutListDto> { CreateTestWorkoutListDto() };
        
        _mockRepository.Setup(r => r.GetWorkoutsByCategoryAndDifficultyAsync(category, difficulty, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);
        _mockMapper.Setup(m => m.Map<IEnumerable<WorkoutListDto>>(It.IsAny<IEnumerable<Workout>>()))
                  .Returns(expectedDtos);

        // Act
        var result = await _service.GetWorkoutsByCategoryAndDifficultyAsync(category, difficulty);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(expectedDtos);
    }

    [Fact]
    public async Task GetTemplateWorkoutsAsync_ShouldReturnTemplateWorkouts()
    {
        // Arrange
        var workouts = new List<Workout> { CreateTestWorkout() };
        var expectedDtos = new List<WorkoutListDto> { CreateTestWorkoutListDto() };
        
        _mockRepository.Setup(r => r.GetTemplateWorkoutsAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);
        _mockMapper.Setup(m => m.Map<IEnumerable<WorkoutListDto>>(It.IsAny<IEnumerable<Workout>>()))
                  .Returns(expectedDtos);

        // Act
        var result = await _service.GetTemplateWorkoutsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(expectedDtos);
    }

    [Fact]
    public async Task SearchWorkoutsAsync_ShouldReturnMatchingWorkouts()
    {
        // Arrange
        var searchTerm = "cardio";
        var category = WorkoutCategory.Cardio;
        var workouts = new List<Workout> { CreateTestWorkout() };
        var expectedDtos = new List<WorkoutListDto> { CreateTestWorkoutListDto() };
        
        _mockRepository.Setup(r => r.SearchWorkoutsAsync(searchTerm, category, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workouts);
        _mockMapper.Setup(m => m.Map<IEnumerable<WorkoutListDto>>(It.IsAny<IEnumerable<Workout>>()))
                  .Returns(expectedDtos);

        // Act
        var result = await _service.SearchWorkoutsAsync(searchTerm, category);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(expectedDtos);
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
        var expectedDtos = new List<WorkoutListDto> { CreateTestWorkoutListDto() };
        var totalCount = 15;
        var repositoryResult = (workouts, totalCount);
        
        _mockRepository.Setup(r => r.GetPagedAsync(page, pageSize, null, null, null, null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(repositoryResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<WorkoutListDto>>(It.IsAny<IEnumerable<Workout>>()))
                  .Returns(expectedDtos);

        // Act
        var result = await _service.GetPagedWorkoutsAsync(page, pageSize);

        // Assert
        result.Workouts.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(totalCount);
    }

    #endregion

    #region Workout Management Tests

    [Fact]
    public async Task DuplicateUserWorkoutAsync_ShouldSucceed_WithExistingWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var newName = "Duplicated Workout";
        var originalWorkout = CreateTestUserWorkout(userId);
        
        // Mock the CreateUserWorkoutAsync method by setting up the full chain
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(originalWorkout);
        
        // Mock the CreateUserWorkoutAsync internal calls
        var newWorkout = CreateTestUserWorkout(userId);
        var expectedDto = CreateTestWorkoutDto();
        
        _mockMapper.Setup(m => m.Map<Workout>(It.IsAny<CreateWorkoutDto>()))
                  .Returns(newWorkout);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(newWorkout);
        _mockMapper.Setup(m => m.Map<WorkoutDto>(It.IsAny<Workout>()))
                  .Returns(expectedDto);

        // Act
        var result = await _service.DuplicateUserWorkoutAsync(workoutId, newName, userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedDto);
        _mockRepository.Verify(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DuplicateWorkoutAsAdminAsync_ShouldSucceed_WithExistingWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var newName = "Admin Duplicated Workout";
        var originalWorkout = CreateTestWorkout();
        
        // Mock the CreateTemplateWorkoutAsync method by setting up the full chain
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(originalWorkout);
        
        // Mock the CreateTemplateWorkoutAsync internal calls
        var newWorkout = CreateTestWorkout();
        var expectedDto = CreateTestWorkoutDto();
        
        _mockMapper.Setup(m => m.Map<Workout>(It.IsAny<CreateWorkoutDto>()))
                  .Returns(newWorkout);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(newWorkout);
        _mockMapper.Setup(m => m.Map<WorkoutDto>(It.IsAny<Workout>()))
                  .Returns(expectedDto);

        // Act
        var result = await _service.DuplicateWorkoutAsAdminAsync(workoutId, newName);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedDto);
        _mockRepository.Verify(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateUserWorkoutAsync_ShouldSucceed_WithExistingWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workout = CreateTestUserWorkout(userId);
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workout);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeactivateUserWorkoutAsync(workoutId, userId);

        // Assert
        result.Should().BeTrue();
        workout.IsActive.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(workout, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateWorkoutAsAdminAsync_ShouldSucceed_WithExistingWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var workout = CreateTestWorkout();
        
        _mockRepository.Setup(r => r.GetByIdAsync(workoutId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(workout);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeactivateWorkoutAsAdminAsync(workoutId);

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

    private static Workout CreateTestUserWorkout(Guid userId)
    {
        var workout = new Workout(
            "Test User Workout",
            WorkoutType.UserCreated,
            WorkoutCategory.Mixed,
            DifficultyLevel.Intermediate,
            45
        );
        workout.SetCreatedBy(userId);
        return workout;
    }

    private static WorkoutDto CreateTestWorkoutDto()
    {
        return new WorkoutDto(
            Id: Guid.NewGuid(),
            Name: "Test Workout",
            Description: "Test workout description",
            Type: WorkoutType.Template,
            Category: WorkoutCategory.Mixed,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 45,
            ImageContentId: null,
            IsActive: true,
            PhaseCount: 0,
            TotalExercises: 0,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Phases: new List<WorkoutPhaseDto>()
        );
    }

    private static WorkoutListDto CreateTestWorkoutListDto()
    {
        return new WorkoutListDto(
            Id: Guid.NewGuid(),
            Name: "Test Workout",
            Description: "Test workout description",
            Type: WorkoutType.Template,
            Category: WorkoutCategory.Mixed,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 45,
            ImageContentId: null,
            IsActive: true,
            PhaseCount: 0,
            TotalExercises: 0,
            CreatedAt: DateTime.UtcNow
        );
    }

    #endregion
}

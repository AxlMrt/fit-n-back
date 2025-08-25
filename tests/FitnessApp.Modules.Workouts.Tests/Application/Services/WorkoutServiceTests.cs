using FitnessApp.Modules.Workouts.Application.Services;
using FitnessApp.Modules.Workouts.Application.DTOs.Requests;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Infrastructure.Repositories;
using FluentAssertions;
using Moq;

namespace FitnessApp.Modules.Workouts.Tests.Application.Services;

public class WorkoutServiceTests
{
    private readonly Mock<IWorkoutRepository> _workoutRepositoryMock;
    private readonly Mock<IWorkoutAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly WorkoutService _workoutService;

    public WorkoutServiceTests()
    {
        _workoutRepositoryMock = new Mock<IWorkoutRepository>();
        _authorizationServiceMock = new Mock<IWorkoutAuthorizationService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _workoutService = new WorkoutService(
            _workoutRepositoryMock.Object,
            _authorizationServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task CreateWorkoutAsync_ValidRequest_ShouldCreateWorkout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateWorkoutDto(
            Name: "Test Workout",
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>()
        );

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        _workoutRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Workout>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workoutService.CreateWorkoutAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Type.Should().Be(request.Type);
        result.Difficulty.Should().Be(request.Difficulty);

        _workoutRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Workout>()), Times.Once);
    }

    [Fact]
    public async Task CreateWorkoutAsync_WithPhases_ShouldCreateWorkoutWithPhases()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateWorkoutDto(
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
                    EstimatedDurationMinutes: 10,
                    Exercises: new List<CreateWorkoutExerciseDto>()
                )
            }
        );

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        _workoutRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Workout>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workoutService.CreateWorkoutAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Phases.Should().HaveCount(1);
        result.Phases.First().Name.Should().Be("Warm Up");
        result.Phases.First().Type.Should().Be(WorkoutPhaseType.WarmUp);

        _workoutRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Workout>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutByIdAsync_ExistingWorkout_ShouldReturnWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var workout = CreateSampleWorkout();

        _workoutRepositoryMock.Setup(x => x.GetByIdAsync(workoutId))
            .ReturnsAsync(workout);

        // Act
        var result = await _workoutService.GetWorkoutByIdAsync(workoutId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(workout.Id);
        result.Name.Should().Be(workout.Name);
    }

    [Fact]
    public async Task GetWorkoutByIdAsync_NonExistentWorkout_ShouldReturnNull()
    {
        // Arrange
        var workoutId = Guid.NewGuid();

        _workoutRepositoryMock.Setup(x => x.GetByIdAsync(workoutId))
            .ReturnsAsync((Workout?)null);

        // Act
        var result = await _workoutService.GetWorkoutByIdAsync(workoutId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateWorkoutAsync_ValidRequest_ShouldUpdateWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workout = CreateSampleWorkout();
        var request = new UpdateWorkoutDto(
            Name: "Updated Workout",
            Description: "Updated Description",
            Difficulty: DifficultyLevel.Advanced,
            EstimatedDurationMinutes: 45,
            RequiredEquipment: EquipmentType.FreeWeights
        );

        _workoutRepositoryMock.Setup(x => x.GetByIdAsync(workoutId))
            .ReturnsAsync(workout);

        _authorizationServiceMock.Setup(x => x.CanModifyWorkoutAsync(workout))
            .ReturnsAsync(true);

        _workoutRepositoryMock.Setup(x => x.UpdateAsync(workout))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workoutService.UpdateWorkoutAsync(workoutId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Difficulty.Should().Be(request.Difficulty);

        _workoutRepositoryMock.Verify(x => x.UpdateAsync(workout), Times.Once);
    }

    [Fact]
    public async Task UpdateWorkoutAsync_UnauthorizedUser_ShouldThrowException()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var workout = CreateSampleWorkout();
        var request = new UpdateWorkoutDto(
            Name: "Updated Workout",
            Description: "Updated Description",
            Difficulty: DifficultyLevel.Advanced,
            EstimatedDurationMinutes: 45,
            RequiredEquipment: EquipmentType.FreeWeights
        );

        _workoutRepositoryMock.Setup(x => x.GetByIdAsync(workoutId))
            .ReturnsAsync(workout);

        _authorizationServiceMock.Setup(x => x.CanModifyWorkoutAsync(workout))
            .ReturnsAsync(false);

        // Act & Assert
        var act = async () => await _workoutService.UpdateWorkoutAsync(workoutId, request);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        
        _workoutRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Workout>()), Times.Never);
    }

    [Fact]
    public async Task DeleteWorkoutAsync_AuthorizedUser_ShouldDeleteWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var workout = CreateSampleWorkout();

        _workoutRepositoryMock.Setup(x => x.GetByIdAsync(workoutId))
            .ReturnsAsync(workout);

        _authorizationServiceMock.Setup(x => x.CanDeleteWorkoutAsync(workout))
            .ReturnsAsync(true);

        _workoutRepositoryMock.Setup(x => x.DeleteAsync(workout))
            .Returns(Task.CompletedTask);

        // Act
        await _workoutService.DeleteWorkoutAsync(workoutId);

        // Assert
        _workoutRepositoryMock.Verify(x => x.DeleteAsync(workout), Times.Once);
    }

    [Fact]
    public async Task DeleteWorkoutAsync_UnauthorizedUser_ShouldThrowException()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var workout = CreateSampleWorkout();

        _workoutRepositoryMock.Setup(x => x.GetByIdAsync(workoutId))
            .ReturnsAsync(workout);

        _authorizationServiceMock.Setup(x => x.CanDeleteWorkoutAsync(workout))
            .ReturnsAsync(false);

        // Act & Assert
        var act = async () => await _workoutService.DeleteWorkoutAsync(workoutId);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        
        _workoutRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Workout>()), Times.Never);
    }

    [Fact]
    public async Task DuplicateWorkoutAsync_ValidWorkout_ShouldCreateDuplicate()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var originalWorkout = CreateSampleWorkout();
        originalWorkout.AddPhase(WorkoutPhaseType.WarmUp, "Warm Up", Duration.FromMinutes(10));

        _workoutRepositoryMock.Setup(x => x.GetByIdWithPhasesAsync(workoutId))
            .ReturnsAsync(originalWorkout);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        _workoutRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Workout>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workoutService.DuplicateWorkoutAsync(workoutId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Contain("Copy");
        result.Phases.Should().HaveCount(1);
        result.Type.Should().Be(WorkoutType.UserCreated);

        _workoutRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Workout>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutsAsync_ValidQuery_ShouldReturnPagedResults()
    {
        // Arrange
        var query = new WorkoutQueryDto
        {
            Page = 1,
            PageSize = 10,
            Type = WorkoutType.UserCreated
        };

        var workouts = new List<Workout> { CreateSampleWorkout() };
        var totalCount = 1;

        _workoutRepositoryMock.Setup(x => x.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<WorkoutType?>(), 
            It.IsAny<DifficultyLevel?>(), It.IsAny<EquipmentType?>(), 
            It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<bool?>(), 
            It.IsAny<Guid?>(), It.IsAny<Guid?>()))
            .ReturnsAsync((workouts, totalCount));

        // Act
        var result = await _workoutService.GetWorkoutsAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(totalCount);
        result.Page.Should().Be(query.Page);
        result.PageSize.Should().Be(query.PageSize);
    }

    private static Workout CreateSampleWorkout()
    {
        return new Workout(
            "Test Workout",
            WorkoutType.UserCreated,
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(30),
            EquipmentType.None,
            Guid.NewGuid());
    }
}

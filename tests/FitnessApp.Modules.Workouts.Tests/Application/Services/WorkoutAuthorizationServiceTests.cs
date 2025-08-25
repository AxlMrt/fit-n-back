using FitnessApp.Modules.Workouts.Application.Services;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FitnessApp.Modules.Workouts.Tests.Application.Services;

public class WorkoutAuthorizationServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly WorkoutAuthorizationService _authorizationService;

    public WorkoutAuthorizationServiceTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _authorizationService = new WorkoutAuthorizationService(_currentUserServiceMock.Object);
    }

    [Fact]
    public async Task CanViewWorkoutAsync_PublicWorkout_ShouldReturnTrue()
    {
        // Arrange
        var workout = Workout.CreateDynamicWorkout(
            "Public Workout",
            DifficultyLevel.Beginner,
            Duration.FromMinutes(30),
            EquipmentType.None);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Guid.NewGuid());

        // Act
        var canView = await _authorizationService.CanViewWorkoutAsync(workout);

        // Assert
        canView.Should().BeTrue();
    }

    [Fact]
    public async Task CanViewWorkoutAsync_OwnUserWorkout_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workout = Workout.CreateUserWorkout(
            "User Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(45),
            EquipmentType.Mat,
            userId);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        // Act
        var canView = await _authorizationService.CanViewWorkoutAsync(workout);

        // Assert
        canView.Should().BeTrue();
    }

    [Fact]
    public async Task CanViewWorkoutAsync_OtherUserWorkout_ShouldReturnFalse()
    {
        // Arrange
        var workoutOwnerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var workout = Workout.CreateUserWorkout(
            "Other User Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(45),
            EquipmentType.Mat,
            workoutOwnerId);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(currentUserId);

        // Act
        var canView = await _authorizationService.CanViewWorkoutAsync(workout);

        // Assert
        canView.Should().BeFalse();
    }

    [Fact]
    public async Task CanModifyWorkoutAsync_OwnUserWorkout_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workout = Workout.CreateUserWorkout(
            "User Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(45),
            EquipmentType.Mat,
            userId);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        // Act
        var canModify = await _authorizationService.CanModifyWorkoutAsync(workout);

        // Assert
        canModify.Should().BeTrue();
    }

    [Fact]
    public async Task CanModifyWorkoutAsync_OwnCoachWorkout_ShouldReturnTrue()
    {
        // Arrange
        var coachId = Guid.NewGuid();
        var workout = Workout.CreateCoachWorkout(
            "Coach Workout",
            DifficultyLevel.Advanced,
            Duration.FromMinutes(60),
            EquipmentType.GymEquipment,
            coachId);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Guid.NewGuid());

        _currentUserServiceMock.Setup(x => x.GetCurrentCoachId())
            .Returns(coachId);

        // Act
        var canModify = await _authorizationService.CanModifyWorkoutAsync(workout);

        // Assert
        canModify.Should().BeTrue();
    }

    [Fact]
    public async Task CanModifyWorkoutAsync_OtherUserWorkout_ShouldReturnFalse()
    {
        // Arrange
        var workoutOwnerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var workout = Workout.CreateUserWorkout(
            "Other User Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(45),
            EquipmentType.Mat,
            workoutOwnerId);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(currentUserId);

        // Act
        var canModify = await _authorizationService.CanModifyWorkoutAsync(workout);

        // Assert
        canModify.Should().BeFalse();
    }

    [Fact]
    public async Task CanModifyWorkoutAsync_SystemWorkout_ShouldReturnFalse()
    {
        // Arrange
        var workout = Workout.CreateDynamicWorkout(
            "System Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(30),
            EquipmentType.None);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Guid.NewGuid());

        // Act
        var canModify = await _authorizationService.CanModifyWorkoutAsync(workout);

        // Assert
        canModify.Should().BeFalse();
    }

    [Fact]
    public async Task CanDeleteWorkoutAsync_OwnUserWorkout_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workout = Workout.CreateUserWorkout(
            "User Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(45),
            EquipmentType.Mat,
            userId);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        // Act
        var canDelete = await _authorizationService.CanDeleteWorkoutAsync(workout);

        // Assert
        canDelete.Should().BeTrue();
    }

    [Fact]
    public async Task CanDeleteWorkoutAsync_OwnCoachWorkout_ShouldReturnTrue()
    {
        // Arrange
        var coachId = Guid.NewGuid();
        var workout = Workout.CreateCoachWorkout(
            "Coach Workout",
            DifficultyLevel.Advanced,
            Duration.FromMinutes(60),
            EquipmentType.GymEquipment,
            coachId);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Guid.NewGuid());

        _currentUserServiceMock.Setup(x => x.GetCurrentCoachId())
            .Returns(coachId);

        // Act
        var canDelete = await _authorizationService.CanDeleteWorkoutAsync(workout);

        // Assert
        canDelete.Should().BeTrue();
    }

    [Fact]
    public async Task CanDeleteWorkoutAsync_OtherUserWorkout_ShouldReturnFalse()
    {
        // Arrange
        var workoutOwnerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var workout = Workout.CreateUserWorkout(
            "Other User Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(45),
            EquipmentType.Mat,
            workoutOwnerId);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(currentUserId);

        // Act
        var canDelete = await _authorizationService.CanDeleteWorkoutAsync(workout);

        // Assert
        canDelete.Should().BeFalse();
    }

    [Fact]
    public async Task CanDeleteWorkoutAsync_SystemWorkout_ShouldReturnFalse()
    {
        // Arrange
        var workout = Workout.CreateDynamicWorkout(
            "System Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(30),
            EquipmentType.None);

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Guid.NewGuid());

        // Act
        var canDelete = await _authorizationService.CanDeleteWorkoutAsync(workout);

        // Assert
        canDelete.Should().BeFalse();
    }

    [Fact]
    public async Task CanCreateWorkoutAsync_AuthenticatedUser_ShouldReturnTrue()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Guid.NewGuid());

        // Act
        var canCreate = await _authorizationService.CanCreateWorkoutAsync();

        // Assert
        canCreate.Should().BeTrue();
    }

    [Fact]
    public async Task CanCreateWorkoutAsync_AnonymousUser_ShouldReturnFalse()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns((Guid?)null);

        // Act
        var canCreate = await _authorizationService.CanCreateWorkoutAsync();

        // Assert
        canCreate.Should().BeFalse();
    }
}

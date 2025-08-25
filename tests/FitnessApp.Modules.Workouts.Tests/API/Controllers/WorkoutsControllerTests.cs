using FitnessApp.Modules.Workouts.API.Controllers;
using FitnessApp.Modules.Workouts.Application.DTOs;
using FitnessApp.Modules.Workouts.Application.DTOs.Requests;
using FitnessApp.Modules.Workouts.Application.DTOs.Responses;
using FitnessApp.Modules.Workouts.Application.Services;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace FitnessApp.Modules.Workouts.Tests.API.Controllers;

public class WorkoutsControllerTests
{
    private readonly Mock<IWorkoutService> _workoutServiceMock;
    private readonly WorkoutsController _controller;

    public WorkoutsControllerTests()
    {
        _workoutServiceMock = new Mock<IWorkoutService>();
        _controller = new WorkoutsController(_workoutServiceMock.Object);

        // Setup a mock user context
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetWorkouts_ValidQuery_ShouldReturnOkWithResults()
    {
        // Arrange
        var query = new WorkoutQueryDto { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResponseDto<WorkoutSummaryDto>(
            Items: new List<WorkoutSummaryDto>
            {
                new(
                    Id: Guid.NewGuid(),
                    Name: "Test Workout",
                    Type: WorkoutType.UserCreated,
                    Difficulty: DifficultyLevel.Intermediate,
                    EstimatedDurationMinutes: 30,
                    RequiredEquipment: EquipmentType.None,
                    IsActive: true,
                    CreatedAt: DateTime.UtcNow,
                    CreatedByUserId: Guid.NewGuid(),
                    CreatedByCoachId: null)
            },
            Page: 1,
            PageSize: 10,
            TotalCount: 1,
            TotalPages: 1);

        _workoutServiceMock.Setup(x => x.GetWorkoutsAsync(query))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetWorkouts(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult?.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetWorkoutById_ExistingWorkout_ShouldReturnOkWithWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var expectedWorkout = new WorkoutDetailDto(
            Id: workoutId,
            Name: "Test Workout",
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            IsActive: true,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            CreatedByUserId: Guid.NewGuid(),
            CreatedByCoachId: null,
            ImageContentId: null,
            Phases: new List<WorkoutPhaseDto>());

        _workoutServiceMock.Setup(x => x.GetWorkoutByIdAsync(workoutId))
            .ReturnsAsync(expectedWorkout);

        // Act
        var result = await _controller.GetWorkoutById(workoutId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult?.Value.Should().BeEquivalentTo(expectedWorkout);
    }

    [Fact]
    public async Task GetWorkoutById_NonExistentWorkout_ShouldReturnNotFound()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        
        _workoutServiceMock.Setup(x => x.GetWorkoutByIdAsync(workoutId))
            .ReturnsAsync((WorkoutDetailDto?)null);

        // Act
        var result = await _controller.GetWorkoutById(workoutId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateWorkout_ValidRequest_ShouldReturnCreatedWithWorkout()
    {
        // Arrange
        var request = new CreateWorkoutDto(
            Name: "New Workout",
            Description: "New Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            Phases: new List<CreateWorkoutPhaseDto>());

        var createdWorkout = new WorkoutDetailDto(
            Id: Guid.NewGuid(),
            Name: request.Name,
            Description: request.Description,
            Type: request.Type,
            Difficulty: request.Difficulty,
            EstimatedDurationMinutes: request.EstimatedDurationMinutes,
            RequiredEquipment: request.RequiredEquipment,
            IsActive: true,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            CreatedByUserId: Guid.NewGuid(),
            CreatedByCoachId: null,
            ImageContentId: null,
            Phases: new List<WorkoutPhaseDto>());

        _workoutServiceMock.Setup(x => x.CreateWorkoutAsync(request))
            .ReturnsAsync(createdWorkout);

        // Act
        var result = await _controller.CreateWorkout(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult?.Value.Should().BeEquivalentTo(createdWorkout);
        createdResult?.ActionName.Should().Be(nameof(_controller.GetWorkoutById));
        createdResult?.RouteValues?["id"].Should().Be(createdWorkout.Id);
    }

    [Fact]
    public async Task UpdateWorkout_ValidRequest_ShouldReturnOkWithUpdatedWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var request = new UpdateWorkoutDto(
            Name: "Updated Workout",
            Description: "Updated Description",
            Difficulty: DifficultyLevel.Advanced,
            EstimatedDurationMinutes: 45,
            RequiredEquipment: EquipmentType.FreeWeights);

        var updatedWorkout = new WorkoutDetailDto(
            Id: workoutId,
            Name: request.Name,
            Description: request.Description,
            Type: WorkoutType.UserCreated,
            Difficulty: request.Difficulty,
            EstimatedDurationMinutes: request.EstimatedDurationMinutes,
            RequiredEquipment: request.RequiredEquipment,
            IsActive: true,
            CreatedAt: DateTime.UtcNow.AddDays(-1),
            UpdatedAt: DateTime.UtcNow,
            CreatedByUserId: Guid.NewGuid(),
            CreatedByCoachId: null,
            ImageContentId: null,
            Phases: new List<WorkoutPhaseDto>());

        _workoutServiceMock.Setup(x => x.UpdateWorkoutAsync(workoutId, request))
            .ReturnsAsync(updatedWorkout);

        // Act
        var result = await _controller.UpdateWorkout(workoutId, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult?.Value.Should().BeEquivalentTo(updatedWorkout);
    }

    [Fact]
    public async Task UpdateWorkout_UnauthorizedUser_ShouldReturnForbid()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var request = new UpdateWorkoutDto(
            Name: "Updated Workout",
            Description: "Updated Description",
            Difficulty: DifficultyLevel.Advanced,
            EstimatedDurationMinutes: 45,
            RequiredEquipment: EquipmentType.FreeWeights);

        _workoutServiceMock.Setup(x => x.UpdateWorkoutAsync(workoutId, request))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.UpdateWorkout(workoutId, request);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeleteWorkout_ExistingWorkout_ShouldReturnNoContent()
    {
        // Arrange
        var workoutId = Guid.NewGuid();

        _workoutServiceMock.Setup(x => x.DeleteWorkoutAsync(workoutId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteWorkout(workoutId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteWorkout_UnauthorizedUser_ShouldReturnForbid()
    {
        // Arrange
        var workoutId = Guid.NewGuid();

        _workoutServiceMock.Setup(x => x.DeleteWorkoutAsync(workoutId))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.DeleteWorkout(workoutId);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DuplicateWorkout_ExistingWorkout_ShouldReturnCreatedWithDuplicate()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var duplicatedWorkout = new WorkoutDetailDto(
            Id: Guid.NewGuid(),
            Name: "Test Workout (Copy)",
            Description: "Test Description",
            Type: WorkoutType.UserCreated,
            Difficulty: DifficultyLevel.Intermediate,
            EstimatedDurationMinutes: 30,
            RequiredEquipment: EquipmentType.None,
            IsActive: true,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            CreatedByUserId: Guid.NewGuid(),
            CreatedByCoachId: null,
            ImageContentId: null,
            Phases: new List<WorkoutPhaseDto>());

        _workoutServiceMock.Setup(x => x.DuplicateWorkoutAsync(workoutId))
            .ReturnsAsync(duplicatedWorkout);

        // Act
        var result = await _controller.DuplicateWorkout(workoutId);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult?.Value.Should().BeEquivalentTo(duplicatedWorkout);
    }

    [Fact]
    public async Task DeactivateWorkout_ExistingWorkout_ShouldReturnNoContent()
    {
        // Arrange
        var workoutId = Guid.NewGuid();

        _workoutServiceMock.Setup(x => x.DeactivateWorkoutAsync(workoutId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeactivateWorkout(workoutId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ReactivateWorkout_ExistingWorkout_ShouldReturnNoContent()
    {
        // Arrange
        var workoutId = Guid.NewGuid();

        _workoutServiceMock.Setup(x => x.ReactivateWorkoutAsync(workoutId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ReactivateWorkout(workoutId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}

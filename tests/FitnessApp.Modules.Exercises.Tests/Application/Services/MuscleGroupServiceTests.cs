using FitnessApp.Modules.Exercises.Application.DTOs.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Application.Services;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace FitnessApp.Modules.Exercises.Tests.Application.Services;

public class MuscleGroupServiceTests
{
    private readonly Mock<IMuscleGroupRepository> _muscleGroupRepositoryMock;
    private readonly Mock<IExerciseMapper> _mapperMock;
    private readonly MuscleGroupService _sut; // System Under Test

    public MuscleGroupServiceTests()
    {
        _muscleGroupRepositoryMock = new Mock<IMuscleGroupRepository>();
        _mapperMock = new Mock<IExerciseMapper>();

        _sut = new MuscleGroupService(
            _muscleGroupRepositoryMock.Object,
            _mapperMock.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullMuscleGroupRepository_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new MuscleGroupService(null, _mapperMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("muscleGroupRepository");
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new MuscleGroupService(_muscleGroupRepositoryMock.Object, null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("exerciseMapper");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsMuscleGroup()
    {
        // Arrange
        var muscleGroupId = Guid.NewGuid();
        var muscleGroup = CreateTestMuscleGroup();
        var relatedGroups = new List<MuscleGroup> { CreateTestMuscleGroup("Related Group") };
        var expectedResponse = new MuscleGroupResponse { Id = muscleGroupId, Name = "Test Muscle Group" };
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(muscleGroupId))
            .ReturnsAsync(muscleGroup);
        _muscleGroupRepositoryMock.Setup(repo => repo.GetRelatedMuscleGroupsAsync(muscleGroupId))
            .ReturnsAsync(relatedGroups);
        _mapperMock.Setup(mapper => mapper.MapToMuscleGroupList(relatedGroups))
            .Returns(new List<MuscleGroupResponse> { new MuscleGroupResponse { Id = Guid.NewGuid(), Name = "Related Group" } });

        // Act
        var result = await _sut.GetByIdAsync(muscleGroupId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(muscleGroup.Name);
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(muscleGroupId), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.GetRelatedMuscleGroupsAsync(muscleGroupId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(nonExistingId))
            .ReturnsAsync((MuscleGroup?)null);

        // Act
        var result = await _sut.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.GetRelatedMuscleGroupsAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllMuscleGroups()
    {
        // Arrange
        var muscleGroups = new List<MuscleGroup>
        {
            CreateTestMuscleGroup("Muscle Group 1"),
            CreateTestMuscleGroup("Muscle Group 2")
        };
        
        var expectedResponses = new List<MuscleGroupResponse>
        {
            new MuscleGroupResponse { Id = Guid.NewGuid(), Name = "Muscle Group 1" },
            new MuscleGroupResponse { Id = Guid.NewGuid(), Name = "Muscle Group 2" }
        };
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(muscleGroups);
        _mapperMock.Setup(mapper => mapper.MapToMuscleGroupList(muscleGroups))
            .Returns(expectedResponses);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedResponses);
        _muscleGroupRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        _mapperMock.Verify(mapper => mapper.MapToMuscleGroupList(muscleGroups), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoMuscleGroupsExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<MuscleGroup>();
        var emptyResponseList = new List<MuscleGroupResponse>();
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(emptyList);
        _mapperMock.Setup(mapper => mapper.MapToMuscleGroupList(emptyList))
            .Returns(emptyResponseList);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
        _muscleGroupRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        _mapperMock.Verify(mapper => mapper.MapToMuscleGroupList(emptyList), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _muscleGroupRepositoryMock.Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _sut.GetAllAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesMuscleGroupAndReturnsId()
    {
        // Arrange
        var request = new CreateMuscleGroupRequest
        {
            Name = "New Muscle Group",
            Description = "Description"
        };
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByNameAsync(request.Name))
            .ReturnsAsync((MuscleGroup?)null);
        
        MuscleGroup capturedMuscleGroup = null;
        _muscleGroupRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<MuscleGroup>()))
            .Callback<MuscleGroup>(mg => capturedMuscleGroup = mg)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeEmpty();
        capturedMuscleGroup.Should().NotBeNull();
        capturedMuscleGroup.Name.Should().Be(request.Name);
        capturedMuscleGroup.Description.Should().Be(request.Description);
        
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<MuscleGroup>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateMuscleGroupRequest
        {
            Name = "Existing Muscle Group",
            Description = "Description"
        };
        var existingMuscleGroup = CreateTestMuscleGroup(request.Name);
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByNameAsync(request.Name))
            .ReturnsAsync(existingMuscleGroup);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Muscle group with name '{request.Name}' already exists");
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateMuscleGroupRequest
        {
            Name = "",
            Description = "Description"
        };

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name is required*");
    }

    [Fact]
    public async Task CreateAsync_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateMuscleGroupRequest
        {
            Name = null,
            Description = "Description"
        };

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name is required*");
    }

    [Fact]
    public async Task CreateAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Func<Task> act = async () => await _sut.CreateAsync(null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidRequest_UpdatesMuscleGroup()
    {
        // Arrange
        var muscleGroupId = Guid.NewGuid();
        var request = new UpdateMuscleGroupRequest
        {
            Name = "Updated Name",
            Description = "Updated Description"
        };
        
        var existingMuscleGroup = CreateTestMuscleGroup("Original Name");
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(muscleGroupId))
            .ReturnsAsync(existingMuscleGroup);
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByNameAsync(request.Name))
            .ReturnsAsync((MuscleGroup?)null);
        _muscleGroupRepositoryMock.Setup(repo => repo.UpdateAsync(existingMuscleGroup))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(muscleGroupId, request);

        // Assert
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(muscleGroupId), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.UpdateAsync(existingMuscleGroup), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var request = new UpdateMuscleGroupRequest { Name = "Updated Name" };
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(nonExistingId))
            .ReturnsAsync((MuscleGroup?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(nonExistingId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Muscle group with ID {nonExistingId} not found");
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var muscleGroupId = Guid.NewGuid();
        var otherMuscleGroupId = Guid.NewGuid();
        var request = new UpdateMuscleGroupRequest { Name = "Duplicate Name" };
        
        var existingMuscleGroup = CreateTestMuscleGroup("Original Name");
        var duplicateMuscleGroup = CreateTestMuscleGroup("Duplicate Name");
        
        // Set IDs using reflection
        SetMuscleGroupId(existingMuscleGroup, muscleGroupId);
        SetMuscleGroupId(duplicateMuscleGroup, otherMuscleGroupId);
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(muscleGroupId))
            .ReturnsAsync(existingMuscleGroup);
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByNameAsync(request.Name))
            .ReturnsAsync(duplicateMuscleGroup);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(muscleGroupId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Muscle group with name '{request.Name}' already exists");
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(muscleGroupId), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithNoNameChange_DoesNotCheckForDuplicates()
    {
        // Arrange
        var muscleGroupId = Guid.NewGuid();
        var existingName = "Original Name";
        var request = new UpdateMuscleGroupRequest { 
            Description = "Updated Description",
            Name = existingName  // Same name as existing muscle group
        };
        
        var existingMuscleGroup = CreateTestMuscleGroup(existingName);
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(muscleGroupId))
            .ReturnsAsync(existingMuscleGroup);
        _muscleGroupRepositoryMock.Setup(repo => repo.UpdateAsync(existingMuscleGroup))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(muscleGroupId, request);

        // Assert
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(muscleGroupId), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByNameAsync(It.IsAny<string>()), Times.Never);
        _muscleGroupRepositoryMock.Verify(repo => repo.UpdateAsync(existingMuscleGroup), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Func<Task> act = async () => await _sut.UpdateAsync(Guid.NewGuid(), null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingId_DeletesMuscleGroup()
    {
        // Arrange
        var muscleGroupId = Guid.NewGuid();
        var muscleGroup = CreateTestMuscleGroup();
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(muscleGroupId))
            .ReturnsAsync(muscleGroup);
        _muscleGroupRepositoryMock.Setup(repo => repo.DeleteAsync(muscleGroupId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(muscleGroupId);

        // Assert
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(muscleGroupId), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.DeleteAsync(muscleGroupId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(nonExistingId))
            .ReturnsAsync((MuscleGroup?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(nonExistingId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Muscle group with ID {nonExistingId} not found");
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
        _muscleGroupRepositoryMock.Verify(repo => repo.DeleteAsync(nonExistingId), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyGuid_ThrowsArgumentException()
    {
        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(Guid.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*valid ID*");
    }

    #endregion

    // #region GetExercisesByMuscleGroupIdAsync Tests

    // [Fact]
    // public async Task GetExercisesByMuscleGroupIdAsync_WithExistingId_ReturnsExercises()
    // {
    //     // Arrange
    //     var muscleGroupId = Guid.NewGuid();
    //     var muscleGroup = CreateTestMuscleGroup();
        
    //     var exercises = new List<Exercise> { new Exercise("Test Exercise", "Description", "Instructions", "Intermediate", true) };
    //     var exerciseMuscleGroups = new List<ExerciseMuscleGroup>
    //     {
    //         new ExerciseMuscleGroup { Exercise = exercises[0], IsPrimary = true }
    //     };
        
    //     // Set up the muscle group to return exercises via the Exercises navigation property
    //     typeof(MuscleGroup).GetProperty("Exercises")
    //         .SetValue(muscleGroup, exerciseMuscleGroups);
        
    //     var expectedResponses = new List<ExerciseResponse> { new ExerciseResponse { Name = "Test Exercise" } };
        
    //     _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(muscleGroupId))
    //         .ReturnsAsync(muscleGroup);
    //     _mapperMock.Setup(mapper => mapper.MapToExerciseList(It.IsAny<IEnumerable<Exercise>>()))
    //         .Returns(expectedResponses);

    //     // Act
    //     var result = await _sut.GetExercisesByMuscleGroupIdAsync(muscleGroupId);

    //     // Assert
    //     result.Should().BeEquivalentTo(expectedResponses);
    //     _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(muscleGroupId), Times.Once);
    // }

    // [Fact]
    // public async Task GetExercisesByMuscleGroupIdAsync_WithPrimaryFlag_ReturnsOnlyPrimaryExercises()
    // {
    //     // Arrange
    //     var muscleGroupId = Guid.NewGuid();
    //     var muscleGroup = CreateTestMuscleGroup();
        
    //     var exercises = new List<Exercise> 
    //     { 
    //         new Exercise("Primary Exercise", "Description", "Instructions", "Intermediate", true),
    //         new Exercise("Secondary Exercise", "Description", "Instructions", "Intermediate", true)
    //     };
        
    //     var exerciseMuscleGroups = new List<ExerciseMuscleGroup>
    //     {
    //         new ExerciseMuscleGroup { Exercise = exercises[0], IsPrimary = true },
    //         new ExerciseMuscleGroup { Exercise = exercises[1], IsPrimary = false }
    //     };
        
    //     // Set up the muscle group to return exercises via the Exercises navigation property
    //     typeof(MuscleGroup).GetProperty("Exercises")
    //         .SetValue(muscleGroup, exerciseMuscleGroups);
        
    //     var expectedResponses = new List<ExerciseResponse> { new ExerciseResponse { Name = "Primary Exercise" } };
        
    //     _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(muscleGroupId))
    //         .ReturnsAsync(muscleGroup);
    //     _mapperMock.Setup(mapper => mapper.MapToExerciseList(It.IsAny<IEnumerable<Exercise>>()))
    //         .Returns(expectedResponses);

    //     // Act
    //     var result = await _sut.GetExercisesByMuscleGroupIdAsync(muscleGroupId, isPrimary: true);

    //     // Assert
    //     result.Should().BeEquivalentTo(expectedResponses);
    //     _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(muscleGroupId), Times.Once);
    // }

    // [Fact]
    // public async Task GetExercisesByMuscleGroupIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
    // {
    //     // Arrange
    //     var nonExistingId = Guid.NewGuid();
        
    //     _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(nonExistingId))
    //         .ReturnsAsync((MuscleGroup)null);

    //     // Act
    //     Func<Task> act = async () => await _sut.GetExercisesByMuscleGroupIdAsync(nonExistingId);

    //     // Assert
    //     await act.Should().ThrowAsync<KeyNotFoundException>()
    //         .WithMessage($"Muscle group with ID {nonExistingId} not found");
    // }

    // #endregion

    // #region GetRelatedMuscleGroupsAsync Tests

    // [Fact]
    // public async Task GetRelatedMuscleGroupsAsync_WithExistingId_ReturnsRelatedMuscleGroups()
    // {
    //     // Arrange
    //     var muscleGroupId = Guid.NewGuid();
    //     var muscleGroup = CreateTestMuscleGroup();
    //     var relatedGroups = new List<MuscleGroup> { CreateTestMuscleGroup("Related Group") };
    //     var expectedResponses = new List<MuscleGroupResponse> { new MuscleGroupResponse { Name = "Related Group" } };
        
    //     _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(muscleGroupId))
    //         .ReturnsAsync(muscleGroup);
    //     _muscleGroupRepositoryMock.Setup(repo => repo.GetRelatedMuscleGroupsAsync(muscleGroupId))
    //         .ReturnsAsync(relatedGroups);
    //     _mapperMock.Setup(mapper => mapper.MapToMuscleGroupList(relatedGroups))
    //         .Returns(expectedResponses);

    //     // Act
    //     var result = await _sut.GetRelatedMuscleGroupsAsync(muscleGroupId);

    //     // Assert
    //     result.Should().BeEquivalentTo(expectedResponses);
    //     _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(muscleGroupId), Times.Once);
    //     _muscleGroupRepositoryMock.Verify(repo => repo.GetRelatedMuscleGroupsAsync(muscleGroupId), Times.Once);
    // }

    // [Fact]
    // public async Task GetRelatedMuscleGroupsAsync_WithNonExistingId_ThrowsKeyNotFoundException()
    // {
    //     // Arrange
    //     var nonExistingId = Guid.NewGuid();
        
    //     _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(nonExistingId))
    //         .ReturnsAsync((MuscleGroup)null);

    //     // Act
    //     Func<Task> act = async () => await _sut.GetRelatedMuscleGroupsAsync(nonExistingId);

    //     // Assert
    //     await act.Should().ThrowAsync<KeyNotFoundException>()
    //         .WithMessage($"Muscle group with ID {nonExistingId} not found");
    // }

    // #endregion

    #region Helper Methods

    private MuscleGroup CreateTestMuscleGroup(string name = "Test Muscle Group")
    {
        return new MuscleGroup(
            name: name,
            description: "Test Description",
            bodyPart: "Upper Body"
        );
    }

    private void SetMuscleGroupId(MuscleGroup muscleGroup, Guid id)
    {
        // Using reflection to set the private Id property
        var idProperty = typeof(MuscleGroup).GetProperty("Id");
        if (idProperty != null)
        {
            idProperty.SetValue(muscleGroup, id);
        }
    }

    #endregion
}

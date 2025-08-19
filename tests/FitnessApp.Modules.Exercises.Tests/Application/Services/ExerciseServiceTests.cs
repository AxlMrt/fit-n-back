using FitnessApp.Modules.Exercises.Application.Dtos.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;
using FitnessApp.Modules.Exercises.Application.Enums;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Application.Services;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace FitnessApp.Modules.Exercises.Tests.Application.Services;
public class ExerciseServiceTests
{
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<IMuscleGroupRepository> _muscleGroupRepositoryMock;
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IExerciseMapper> _mapperMock;
    private readonly ExerciseService _sut; // System Under Test

    public ExerciseServiceTests()
    {
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        _tagRepositoryMock = new Mock<ITagRepository>();
        _muscleGroupRepositoryMock = new Mock<IMuscleGroupRepository>();
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _mapperMock = new Mock<IExerciseMapper>();

        _sut = new ExerciseService(
            _exerciseRepositoryMock.Object,
            _tagRepositoryMock.Object,
            _muscleGroupRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _mapperMock.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullExerciseRepository_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ExerciseService(
            null,
            _tagRepositoryMock.Object,
            _muscleGroupRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _mapperMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("exerciseRepository");
    }

    [Fact]
    public void Constructor_WithNullTagRepository_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ExerciseService(
            _exerciseRepositoryMock.Object,
            null,
            _muscleGroupRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _mapperMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("tagRepository");
    }

    [Fact]
    public void Constructor_WithNullMuscleGroupRepository_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ExerciseService(
            _exerciseRepositoryMock.Object,
            _tagRepositoryMock.Object,
            null,
            _equipmentRepositoryMock.Object,
            _mapperMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("muscleGroupRepository");
    }

    [Fact]
    public void Constructor_WithNullEquipmentRepository_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ExerciseService(
            _exerciseRepositoryMock.Object,
            _tagRepositoryMock.Object,
            _muscleGroupRepositoryMock.Object,
            null,
            _mapperMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("equipmentRepository");
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ExerciseService(
            _exerciseRepositoryMock.Object,
            _tagRepositoryMock.Object,
            _muscleGroupRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            null
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mapper");
    }
    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsExercise()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = CreateTestExercise();
        var expectedResponse = new ExerciseResponse { Id = exerciseId, Name = "Test Exercise" };
        
        _exerciseRepositoryMock.Setup(repo => repo.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(exercise);
        _mapperMock.Setup(mapper => mapper.MapToExercise(exercise))
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetByIdAsync(exerciseId);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _exerciseRepositoryMock.Verify(repo => repo.GetByIdWithDetailsAsync(exerciseId), Times.Once);
        _mapperMock.Verify(mapper => mapper.MapToExercise(exercise), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        _exerciseRepositoryMock.Setup(repo => repo.GetByIdWithDetailsAsync(nonExistingId))
            .ReturnsAsync(default(Exercise));

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(nonExistingId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Exercise with ID {nonExistingId} not found");
        _exerciseRepositoryMock.Verify(repo => repo.GetByIdWithDetailsAsync(nonExistingId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(emptyId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*valid ID*");
    }
    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            CreateTestExercise("Exercise 1"),
            CreateTestExercise("Exercise 2")
        };
        
        var expectedResponses = new List<ExerciseResponse>
        {
            new ExerciseResponse { Name = "Exercise 1" },
            new ExerciseResponse { Name = "Exercise 2" }
        };
        
        _exerciseRepositoryMock.Setup(repo => repo.GetAllWithDetailsAsync())
            .ReturnsAsync(exercises);
        _mapperMock.Setup(mapper => mapper.MapToExerciseList(exercises))
            .Returns(expectedResponses);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedResponses);
        _exerciseRepositoryMock.Verify(repo => repo.GetAllWithDetailsAsync(), Times.Once);
        _mapperMock.Verify(mapper => mapper.MapToExerciseList(exercises), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoExercisesExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Exercise>();
        var emptyResponseList = new List<ExerciseResponse>();
        
        _exerciseRepositoryMock.Setup(repo => repo.GetAllWithDetailsAsync())
            .ReturnsAsync(emptyList);
        _mapperMock.Setup(mapper => mapper.MapToExerciseList(emptyList))
            .Returns(emptyResponseList);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
        _exerciseRepositoryMock.Verify(repo => repo.GetAllWithDetailsAsync(), Times.Once);
        _mapperMock.Verify(mapper => mapper.MapToExerciseList(emptyList), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _exerciseRepositoryMock.Setup(repo => repo.GetAllWithDetailsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _sut.GetAllAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }
    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WithSearchParameters_ReturnsFilteredExercises()
    {
        // Arrange
        var searchParams = new ExerciseSearchRequest
        {
            Name = "Push",
            TagIds = new List<Guid> { Guid.NewGuid() },
            MuscleGroupIds = new List<Guid> { Guid.NewGuid() },
            EquipmentIds = new List<Guid> { Guid.NewGuid() },
            Difficulty = Enum.Parse<DifficultyLevel>("Intermediate"),
            MaxDurationInSeconds = 300,
            RequiresEquipment = true,
            Skip = 0,
            Take = 10,
            SortBy = "Name",
            SortDescending = false
        };
        
        var filteredExercises = new List<Exercise> { CreateTestExercise("Push-up") };
        var expectedResponses = new List<ExerciseResponse> { new ExerciseResponse { Name = "Push-up" } };
        
        _exerciseRepositoryMock.Setup(repo => repo.SearchAsync(
                searchParams.Name,
                searchParams.TagIds,
                searchParams.MuscleGroupIds,
                searchParams.EquipmentIds,
                searchParams.Difficulty,
                searchParams.MaxDurationInSeconds,
                searchParams.RequiresEquipment,
                searchParams.Skip,
                searchParams.Take,
                searchParams.SortBy,
                searchParams.SortDescending))
            .ReturnsAsync(filteredExercises);
        
        _mapperMock.Setup(mapper => mapper.MapToExerciseList(filteredExercises))
            .Returns(expectedResponses);

        // Act
        var result = await _sut.SearchAsync(searchParams);

        // Assert
        result.Should().BeEquivalentTo(expectedResponses);
        _exerciseRepositoryMock.Verify(repo => repo.SearchAsync(
            searchParams.Name,
            searchParams.TagIds,
            searchParams.MuscleGroupIds,
            searchParams.EquipmentIds,
            searchParams.Difficulty,
            searchParams.MaxDurationInSeconds,
            searchParams.RequiresEquipment,
            searchParams.Skip,
            searchParams.Take,
            searchParams.SortBy,
            searchParams.SortDescending), Times.Once);
        _mapperMock.Verify(mapper => mapper.MapToExerciseList(filteredExercises), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var searchParams = new ExerciseSearchRequest { Name = "NonExistent" };
        var emptyList = new List<Exercise>();
        var emptyResponseList = new List<ExerciseResponse>();
        
        _exerciseRepositoryMock.Setup(repo => repo.SearchAsync(
                searchParams.Name,
                searchParams.TagIds,
                searchParams.MuscleGroupIds,
                searchParams.EquipmentIds,
                searchParams.Difficulty,
                searchParams.MaxDurationInSeconds,
                searchParams.RequiresEquipment,
                searchParams.Skip,
                searchParams.Take,
                searchParams.SortBy,
                searchParams.SortDescending))
            .ReturnsAsync(emptyList);
        
        _mapperMock.Setup(mapper => mapper.MapToExerciseList(emptyList))
            .Returns(emptyResponseList);

        // Act
        var result = await _sut.SearchAsync(searchParams);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Func<Task> act = async () => await _sut.SearchAsync(null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesExerciseAndReturnsId()
    {
        // Arrange
        var request = CreateValidCreateExerciseRequest();
        
        _exerciseRepositoryMock.Setup(repo => repo.GetByNameAsync(request.Name))
            .ReturnsAsync((Exercise?)null);
        
        // Set up mock repositories to return entities for related items
        SetupMockRepositoriesForCreate();
        
        Exercise capturedExercise = null;
        _exerciseRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeEmpty();
        capturedExercise.Should().NotBeNull();
        capturedExercise.Should().NotBeNull();
        capturedExercise!.Name.Should().Be(request.Name);
        
        _exerciseRepositoryMock.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Exercise>()), Times.Once);
        
        // Verify related repositories were called
        _tagRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Exactly(request.TagIds?.Count ?? 0));
        _muscleGroupRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Exactly(request.MuscleGroups?.Count ?? 0));
        _equipmentRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Exactly(request.EquipmentIds?.Count ?? 0));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = CreateValidCreateExerciseRequest();
        var existingExercise = CreateTestExercise(request.Name);
        
        _exerciseRepositoryMock.Setup(repo => repo.GetByNameAsync(request.Name))
            .ReturnsAsync(existingExercise);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Exercise with name '{request.Name}' already exists");
        _exerciseRepositoryMock.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Exercise>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidName_ThrowsArgumentException()
    {
        // Arrange
        var request = CreateValidCreateExerciseRequest();
        request.Name = string.Empty;

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name cannot be empty*");
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
    public async Task UpdateAsync_WithValidRequest_UpdatesExercise()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Instructions = "Updated Instructions",
            CommonMistakes = "Updated Common Mistakes",
            Difficulty = Enum.Parse<DifficultyLevel>("Advanced"),
            CaloriesBurnedPerMinute = 15
        };
        
        var existingExercise = CreateTestExercise("Original Name");
        SetExerciseId(existingExercise, exerciseId);
        
        _exerciseRepositoryMock.Setup(repo => repo.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(existingExercise);
        _exerciseRepositoryMock.Setup(repo => repo.GetByNameAsync(request.Name))
            .ReturnsAsync((Exercise?)null);
        _exerciseRepositoryMock.Setup(repo => repo.UpdateAsync(existingExercise))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(exerciseId, request);

        // Assert
        _exerciseRepositoryMock.Verify(repo => repo.GetByIdWithDetailsAsync(exerciseId), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.UpdateAsync(existingExercise), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var request = new UpdateExerciseRequest { Name = "Updated Name" };
        
        _exerciseRepositoryMock.Setup(repo => repo.GetByIdWithDetailsAsync(nonExistingId))
            .ReturnsAsync((Exercise?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(nonExistingId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Exercise with ID {nonExistingId} not found");
        _exerciseRepositoryMock.Verify(repo => repo.GetByIdWithDetailsAsync(nonExistingId), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Exercise>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var otherExerciseId = Guid.NewGuid();
        var request = new UpdateExerciseRequest { Name = "Duplicate Name" };
        
        var existingExercise = CreateTestExercise("Original Name");
        SetExerciseId(existingExercise, exerciseId);
        
        var duplicateExercise = CreateTestExercise("Duplicate Name");
        SetExerciseId(duplicateExercise, otherExerciseId);
        
        _exerciseRepositoryMock.Setup(repo => repo.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(existingExercise);
        _exerciseRepositoryMock.Setup(repo => repo.GetByNameAsync(request.Name))
            .ReturnsAsync(duplicateExercise);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(exerciseId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Exercise with name '{request.Name}' already exists");
        _exerciseRepositoryMock.Verify(repo => repo.GetByIdWithDetailsAsync(exerciseId), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Exercise>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithNoNameChange_DoesNotCheckForDuplicates()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var existingName = "Original Name";
        var request = new UpdateExerciseRequest { 
            Description = "Updated Description",
            Name = existingName  // Same name as existing exercise
        };
        
        var existingExercise = CreateTestExercise(existingName);
        SetExerciseId(existingExercise, exerciseId);
        
        _exerciseRepositoryMock.Setup(repo => repo.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(existingExercise);
        _exerciseRepositoryMock.Setup(repo => repo.UpdateAsync(existingExercise))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(exerciseId, request);

        // Assert
        _exerciseRepositoryMock.Verify(repo => repo.GetByIdWithDetailsAsync(exerciseId), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.GetByNameAsync(It.IsAny<string>()), Times.Never);
        _exerciseRepositoryMock.Verify(repo => repo.UpdateAsync(existingExercise), Times.Once);
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
    public async Task DeleteAsync_WithExistingId_DeletesExercise()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = CreateTestExercise();
        
        _exerciseRepositoryMock.Setup(repo => repo.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);
        _exerciseRepositoryMock.Setup(repo => repo.DeleteAsync(exerciseId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(exerciseId);

        // Assert
        _exerciseRepositoryMock.Verify(repo => repo.GetByIdAsync(exerciseId), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.DeleteAsync(exerciseId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        
        _exerciseRepositoryMock.Setup(repo => repo.GetByIdAsync(nonExistingId))
            .ReturnsAsync((Exercise?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(nonExistingId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Exercise with ID {nonExistingId} not found");
        _exerciseRepositoryMock.Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
        _exerciseRepositoryMock.Verify(repo => repo.DeleteAsync(nonExistingId), Times.Never);
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

    #region Helper Methods

    private Exercise CreateTestExercise(string name = "Test Exercise")
    {
        return new Exercise(
            name: name,
            description: "Test Description",
            instructions: "Test Instructions",
            difficultyLevel: "Intermediate",
            isBodyweightExercise: true
        );
    }

    private void SetExerciseId(Exercise exercise, Guid id)
    {
        // Using reflection to set the private Id property
        var idProperty = typeof(Exercise).GetProperty("Id");
        if (idProperty != null)
        {
            idProperty.SetValue(exercise, id, null);
        }
        else
        {
            throw new InvalidOperationException("Property 'Id' not found on type 'Exercise'.");
        }
    }

    private CreateExerciseRequest CreateValidCreateExerciseRequest()
    {
        return new CreateExerciseRequest
        {
            Name = "New Exercise",
            Description = "Description",
            Instructions = "Instructions",
            Difficulty = Enum.Parse<DifficultyLevel>("Intermediate"),
            TagIds = new List<Guid> { Guid.NewGuid() },
            MuscleGroups = new List<MuscleGroupAssignment> 
            {
                new MuscleGroupAssignment { MuscleGroupId = Guid.NewGuid(), IsPrimary = true }
            },
            EquipmentIds = new List<Guid> { Guid.NewGuid() },
            CaloriesBurnedPerMinute = 10,
            CommonMistakes = "Common mistakes"
        };
    }

    private void SetupMockRepositoriesForCreate()
    {
        var tag = new Tag("Test Tag");
        _tagRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(tag);
            
        var muscleGroup = new MuscleGroup("Test Muscle Group", "Test Description", "Upper Body");
        _muscleGroupRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(muscleGroup);
            
        var equipment = new Equipment("Test Equipment");
        _equipmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(equipment);
    }
    #endregion
}
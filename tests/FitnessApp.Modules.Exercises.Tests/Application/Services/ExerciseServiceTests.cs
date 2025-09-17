using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using FitnessApp.Modules.Exercises.Application.Services;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Exceptions;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Tests.Helpers;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FitnessApp.Modules.Exercises.Tests.Application.Services;

public class ExerciseServiceTests
{
    private readonly Mock<IExerciseRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateExerciseDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateExerciseDto>> _updateValidatorMock;
    private readonly Mock<IValidator<ExerciseQueryDto>> _queryValidatorMock;
    private readonly Mock<ILogger<ExerciseService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ExerciseService _service;

    public ExerciseServiceTests()
    {
        _repositoryMock = new Mock<IExerciseRepository>();
        _createValidatorMock = new Mock<IValidator<CreateExerciseDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateExerciseDto>>();
        _queryValidatorMock = new Mock<IValidator<ExerciseQueryDto>>();
        _loggerMock = new Mock<ILogger<ExerciseService>>();
        _mapperMock = new Mock<IMapper>();

        _service = new ExerciseService(
            _repositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _queryValidatorMock.Object,
            _loggerMock.Object,
            _mapperMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnExerciseDto()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        var expectedDto = new ExerciseDto(
            exercise.Id,
            exercise.Name,
            exercise.Description,
            exercise.Type,
            new List<string> { "CHEST", "ARMS" }, // MuscleGroups as strings
            exercise.ImageContentId,
            exercise.VideoContentId,
            exercise.Difficulty,
            new List<string> { "None" }, // Equipment as strings
            exercise.Instructions,
            exercise.IsActive,
            exercise.CreatedAt,
            exercise.UpdatedAt
        );

        _repositoryMock.Setup(r => r.GetByIdAsync(exercise.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(exercise);
        _mapperMock.Setup(m => m.Map<ExerciseDto>(exercise))
                   .Returns(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(exercise.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Type, result.Type);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Exercise?)null);

        // Act
        var result = await _service.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetByIdAsync(Guid.Empty));
        Assert.Contains("Exercise ID cannot be empty", exception.Message);
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_WithValidName_ShouldReturnExerciseDto()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreateDeadlifts();
        var expectedDto = new ExerciseDto(
            exercise.Id,
            exercise.Name,
            exercise.Description,
            exercise.Type,
            new List<string> { "BACK", "LEGS" },
            exercise.ImageContentId,
            exercise.VideoContentId,
            exercise.Difficulty,
            new List<string> { "Barbells" },
            exercise.Instructions,
            exercise.IsActive,
            exercise.CreatedAt,
            exercise.UpdatedAt
        );

        _repositoryMock.Setup(r => r.GetByNameAsync("Deadlifts", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(exercise);
        _mapperMock.Setup(m => m.Map<ExerciseDto>(exercise))
                   .Returns(expectedDto);

        // Act
        var result = await _service.GetByNameAsync("Deadlifts");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Deadlifts", result.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetByNameAsync_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetByNameAsync(invalidName));
        Assert.Contains("Exercise name cannot be empty", exception.Message);
    }

    #endregion

    #region GetPagedAsync Tests

    [Fact]
    public async Task GetPagedAsync_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var query = new ExerciseQueryDto { PageNumber = 1, PageSize = 10 };
        var exercises = new List<Exercise> 
        { 
            ExerciseTestDataFactory.RealExercises.CreatePushUps(),
            ExerciseTestDataFactory.RealExercises.CreateBurpees()
        };
        var exerciseListDtos = exercises.Select(e => new ExerciseListDto(
            e.Id, 
            e.Name, 
            e.Type,
            e.Difficulty,
            new List<string> { "CHEST", "ARMS" },
            e.Equipment != Equipment.None,
            e.IsActive
        )).ToList();

        var validationResult = new ValidationResult();
        _queryValidatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);
        
        _repositoryMock.Setup(r => r.GetPagedAsync(query, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((exercises, 20)); // Total count = 20
        
        _mapperMock.Setup(m => m.Map<List<ExerciseListDto>>(exercises))
                   .Returns(exerciseListDtos);

        // Act
        var result = await _service.GetPagedAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(20, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalPages); // Math.Ceiling(20/10)
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidQuery_ShouldThrowValidationException()
    {
        // Arrange
        var query = new ExerciseQueryDto { PageNumber = 0, PageSize = -1 };
        var validationFailures = new List<ValidationFailure>
        {
            new("PageNumber", "Page number must be greater than 0"),
            new("PageSize", "Page size must be greater than 0")
        };
        var validationResult = new ValidationResult(validationFailures);

        _queryValidatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetPagedAsync(query));
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldReturnCreatedExerciseDto()
    {
        // Arrange
        var createDto = new CreateExerciseDto
        {
            Name = "New Push-ups Variation",
            Description = "A new variation of push-ups",
            Type = ExerciseType.Strength,
            MuscleGroups = new List<string> { "CHEST", "ARMS" },
            Difficulty = DifficultyLevel.Intermediate,
            Equipment = new List<string> { "None" }
        };

        var exercise = ExerciseTestDataFactory.CreateCustomExercise(
            createDto.Name, 
            createDto.Type, 
            createDto.Difficulty);

        var expectedDto = new ExerciseDto(
            exercise.Id,
            exercise.Name,
            exercise.Description,
            exercise.Type,
            new List<string> { "CHEST", "ARMS" },
            exercise.ImageContentId,
            exercise.VideoContentId,
            exercise.Difficulty,
            new List<string> { "None" },
            exercise.Instructions,
            exercise.IsActive,
            exercise.CreatedAt,
            exercise.UpdatedAt
        );

        var validationResult = new ValidationResult();
        _createValidatorMock.Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(validationResult);
        
        _repositoryMock.Setup(r => r.ExistsWithNameAsync(createDto.Name, null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);
        
        _mapperMock.Setup(m => m.Map<Exercise>(createDto))
                   .Returns(exercise);
        
        _mapperMock.Setup(m => m.Map<ExerciseDto>(exercise))
                   .Returns(expectedDto);

        _repositoryMock.Setup(r => r.AddAsync(exercise, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Type, result.Type);

        _repositoryMock.Verify(r => r.AddAsync(exercise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ShouldThrowExerciseDomainException()
    {
        // Arrange
        var createDto = new CreateExerciseDto
        {
            Name = "Push-ups",
            Type = ExerciseType.Strength
        };

        var validationResult = new ValidationResult();
        _createValidatorMock.Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(validationResult);
        
        _repositoryMock.Setup(r => r.ExistsWithNameAsync(createDto.Name, null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true); // Name already exists

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ExerciseDomainException>(
            () => _service.CreateAsync(createDto));
        Assert.Contains("already exists", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidDto_ShouldReturnUpdatedExerciseDto()
    {
        // Arrange
        var existingExercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        var updateDto = new UpdateExerciseDto
        {
            Name = "Advanced Push-ups",
            Description = "Updated description"
        };

        var expectedDto = new ExerciseDto(
            existingExercise.Id,
            updateDto.Name!,
            updateDto.Description,
            existingExercise.Type,
            new List<string> { "CHEST", "ARMS" },
            existingExercise.ImageContentId,
            existingExercise.VideoContentId,
            existingExercise.Difficulty,
            new List<string> { "None" },
            existingExercise.Instructions,
            existingExercise.IsActive,
            existingExercise.CreatedAt,
            existingExercise.UpdatedAt
        );

        var validationResult = new ValidationResult();
        _updateValidatorMock.Setup(v => v.ValidateAsync(updateDto, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(validationResult);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(existingExercise.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingExercise);
        
        _repositoryMock.Setup(r => r.ExistsWithNameAsync(updateDto.Name, existingExercise.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);
        
        _repositoryMock.Setup(r => r.UpdateAsync(existingExercise, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);
        
        _mapperMock.Setup(m => m.Map<ExerciseDto>(existingExercise))
                   .Returns(expectedDto);

        // Act
        var result = await _service.UpdateAsync(existingExercise.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Description, result.Description);

        _repositoryMock.Verify(r => r.UpdateAsync(existingExercise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateExerciseDto { Name = "New Name" };

        var validationResult = new ValidationResult();
        _updateValidatorMock.Setup(v => v.ValidateAsync(updateDto, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(validationResult);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Exercise?)null);

        // Act
        var result = await _service.UpdateAsync(nonExistentId, updateDto);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region ActivateAsync/DeactivateAsync Tests

    [Fact]
    public async Task ActivateAsync_WithValidId_ShouldActivateExercise()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        exercise.Deactivate(); // Start deactivated

        _repositoryMock.Setup(r => r.GetByIdAsync(exercise.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(exercise);
        _repositoryMock.Setup(r => r.UpdateAsync(exercise, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ActivateAsync(exercise.Id);

        // Assert
        Assert.True(result);
        Assert.True(exercise.IsActive);
        _repositoryMock.Verify(r => r.UpdateAsync(exercise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WithValidId_ShouldDeactivateExercise()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        Assert.True(exercise.IsActive); // Should start active

        _repositoryMock.Setup(r => r.GetByIdAsync(exercise.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(exercise);
        _repositoryMock.Setup(r => r.UpdateAsync(exercise, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeactivateAsync(exercise.Id);

        // Assert
        Assert.True(result);
        Assert.False(exercise.IsActive);
        _repositoryMock.Verify(r => r.UpdateAsync(exercise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("ActivateAsync")]
    [InlineData("DeactivateAsync")]
    public async Task ActivateDeactivateAsync_WithNonExistentId_ShouldReturnFalse(string methodName)
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Exercise?)null);

        // Act
        var result = methodName == "ActivateAsync" 
            ? await _service.ActivateAsync(nonExistentId)
            : await _service.DeactivateAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteExercise()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();

        _repositoryMock.Setup(r => r.GetByIdAsync(exercise.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(exercise);
        _repositoryMock.Setup(r => r.DeleteAsync(exercise, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(exercise.Id);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(exercise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Exercise?)null);

        // Act
        var result = await _service.DeleteAsync(nonExistentId);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExistsAsync(existingId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(existingId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        const string existingName = "Push-ups";
        _repositoryMock.Setup(r => r.ExistsWithNameAsync(existingName, null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsWithNameAsync(existingName);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExistsWithNameAsync_WithInvalidName_ShouldReturnFalse(string invalidName)
    {
        // Act
        var result = await _service.ExistsWithNameAsync(invalidName);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WithValidTerm_ShouldReturnMatchingExercises()
    {
        // Arrange
        const string searchTerm = "Push";
        var exercises = new List<Exercise> 
        { 
            ExerciseTestDataFactory.RealExercises.CreatePushUps() 
        };
        var exerciseListDtos = exercises.Select(e => new ExerciseListDto(
            e.Id, 
            e.Name,
            e.Type,
            e.Difficulty,
            new List<string> { "CHEST", "ARMS" },
            e.Equipment != Equipment.None,
            e.IsActive
        )).ToList();

        _repositoryMock.Setup(r => r.SearchByNameAsync(searchTerm, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(exercises);
        _mapperMock.Setup(m => m.Map<IEnumerable<ExerciseListDto>>(exercises))
                   .Returns(exerciseListDtos);

        // Act
        var result = await _service.SearchAsync(searchTerm);

        // Assert
        Assert.Single(result);
        Assert.Equal("Push-ups", result.First().Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchAsync_WithInvalidTerm_ShouldReturnEmpty(string invalidTerm)
    {
        // Act
        var result = await _service.SearchAsync(invalidTerm);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            ExerciseTestDataFactory.RealExercises.CreatePushUps(),
            ExerciseTestDataFactory.RealExercises.CreateBurpees()
        };
        var exerciseListDtos = exercises.Select(e => new ExerciseListDto(
            e.Id, 
            e.Name,
            e.Type,
            e.Difficulty,
            new List<string> { "CHEST", "ARMS" },
            e.Equipment != Equipment.None,
            e.IsActive
        )).ToList();

        _repositoryMock.Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(exercises);
        _mapperMock.Setup(m => m.Map<IEnumerable<ExerciseListDto>>(exercises))
                   .Returns(exerciseListDtos);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, dto => dto.Name == "Push-ups");
        Assert.Contains(result, dto => dto.Name == "Burpees");
    }

    #endregion
}

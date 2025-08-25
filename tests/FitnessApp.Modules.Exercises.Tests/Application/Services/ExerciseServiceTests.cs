using FluentValidation;
using FluentValidation.Results;
using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Application.Services;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Enums;
using FitnessApp.Modules.Exercises.Domain.Exceptions;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Domain.ValueObjects;
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
    private readonly ExerciseService _service;

    public ExerciseServiceTests()
    {
        _repositoryMock = new Mock<IExerciseRepository>();
        _createValidatorMock = new Mock<IValidator<CreateExerciseDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateExerciseDto>>();
        _queryValidatorMock = new Mock<IValidator<ExerciseQueryDto>>();
        _loggerMock = new Mock<ILogger<ExerciseService>>();

        _service = new ExerciseService(
            _repositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _queryValidatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnExerciseDto_WhenExerciseExists()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = CreateValidExercise();
        _repositoryMock.Setup(r => r.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(exercise);

        // Act
        var result = await _service.GetByIdAsync(exerciseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exercise.Id, result.Id);
        Assert.Equal(exercise.Name, result.Name);
        Assert.Equal(exercise.Type, result.Type);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Exercise)null);

        // Act
        var result = await _service.GetByIdAsync(exerciseId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnExerciseDto_WhenExerciseExists()
    {
        // Arrange
        var exerciseName = "Push-ups";
        var exercise = CreateValidExercise();
        _repositoryMock.Setup(r => r.GetByNameAsync(exerciseName, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(exercise);

        // Act
        var result = await _service.GetByNameAsync(exerciseName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exercise.Name, result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByNameAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByNameAsync("   "));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByNameAsync(null));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateExercise_WhenValidDto()
    {
        // Arrange
        var createDto = new CreateExerciseDto
        {
            Name = "Push-ups",
            Type = ExerciseType.Strength,
            Difficulty = DifficultyLevel.Intermediate,
            MuscleGroups = new List<string> { "CHEST", "ARMS" },
            Equipment = new List<string>(),
            Description = "Basic push-up exercise",
            Instructions = "Start in plank position, lower body, push back up"
        };

        _createValidatorMock.Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.ExistsWithNameAsync(createDto.Name, null, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.Type, result.Type);
        Assert.Equal(createDto.Difficulty, result.Difficulty);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var createDto = new CreateExerciseDto { Name = "" };
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Exercise name is required")
        };
        var validationResult = new ValidationResult(validationFailures);

        _createValidatorMock.Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowDomainException_WhenExerciseNameAlreadyExists()
    {
        // Arrange
        var createDto = new CreateExerciseDto
        {
            Name = "Push-ups",
            Type = ExerciseType.Strength,
            Difficulty = DifficultyLevel.Intermediate
        };

        _createValidatorMock.Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.ExistsWithNameAsync(createDto.Name, null, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ExerciseDomainException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExercise_WhenValidDto()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var existingExercise = CreateValidExercise();
        var updateDto = new UpdateExerciseDto
        {
            Name = "Modified Push-ups",
            Description = "Updated description"
        };

        _updateValidatorMock.Setup(v => v.ValidateAsync(updateDto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(existingExercise);

        _repositoryMock.Setup(r => r.ExistsWithNameAsync(updateDto.Name, exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(exerciseId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Name, result.Name);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var updateDto = new UpdateExerciseDto { Name = "Updated Name" };

        _updateValidatorMock.Setup(v => v.ValidateAsync(updateDto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Exercise)null);

        // Act
        var result = await _service.UpdateAsync(exerciseId, updateDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteExercise_WhenExerciseExists()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = CreateValidExercise();

        _repositoryMock.Setup(r => r.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(exercise);

        _repositoryMock.Setup(r => r.DeleteAsync(exercise, It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(exerciseId);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(exercise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenExerciseDoesNotExist()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Exercise)null);

        // Act
        var result = await _service.DeleteAsync(exerciseId);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivateAsync_ShouldActivateExercise_WhenExerciseExists()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = CreateValidExercise();
        exercise.Deactivate(); // Make it inactive first

        _repositoryMock.Setup(r => r.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(exercise);

        _repositoryMock.Setup(r => r.UpdateAsync(exercise, It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ActivateAsync(exerciseId);

        // Assert
        Assert.True(result);
        Assert.True(exercise.IsActive);
        _repositoryMock.Verify(r => r.UpdateAsync(exercise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldDeactivateExercise_WhenExerciseExists()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = CreateValidExercise(); // Active by default

        _repositoryMock.Setup(r => r.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(exercise);

        _repositoryMock.Setup(r => r.UpdateAsync(exercise, It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeactivateAsync(exerciseId);

        // Assert
        Assert.True(result);
        Assert.False(exercise.IsActive);
        _repositoryMock.Verify(r => r.UpdateAsync(exercise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenExerciseExists()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExistsAsync(exerciseId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(exerciseId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsWithNameAsync_ShouldReturnTrue_WhenExerciseWithNameExists()
    {
        // Arrange
        var exerciseName = "Push-ups";
        _repositoryMock.Setup(r => r.ExistsWithNameAsync(exerciseName, null, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsWithNameAsync(exerciseName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsWithNameAsync_ShouldReturnFalse_WhenNameIsEmpty()
    {
        // Act & Assert
        Assert.False(await _service.ExistsWithNameAsync(""));
        Assert.False(await _service.ExistsWithNameAsync("   "));
        Assert.False(await _service.ExistsWithNameAsync(null));
    }

    private static Exercise CreateValidExercise()
    {
        return new Exercise("Push-ups", ExerciseType.Strength, DifficultyLevel.Intermediate, MuscleGroup.CHEST, new Equipment());
    }
}

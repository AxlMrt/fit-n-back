using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Enums;
using FitnessApp.Modules.Exercises.Domain.ValueObjects;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using FitnessApp.Modules.Exercises.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Exercises.Tests.Infrastructure.Repositories;
public class ExerciseRepositoryTests : IDisposable
{
    private readonly ExercisesDbContext _context;
    private readonly ExerciseRepository _repository;

    public ExerciseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ExercisesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ExercisesDbContext(options);
        _repository = new ExerciseRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddExercise_WhenValidExercise()
    {
        // Arrange
        var exercise = CreateValidExercise();

        // Act
        await _repository.AddAsync(exercise);

        // Assert
        var savedExercise = await _context.Exercises.FindAsync(exercise.Id);
        Assert.NotNull(savedExercise);
        Assert.Equal(exercise.Name, savedExercise.Name);
        Assert.Equal(exercise.Type, savedExercise.Type);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnExercise_WhenExerciseExists()
    {
        // Arrange
        var exercise = CreateValidExercise();
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(exercise.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exercise.Id, result.Id);
        Assert.Equal(exercise.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnExercise_WhenExerciseExists()
    {
        // Arrange
        var exercise = CreateValidExercise();
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync(exercise.Name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exercise.Name, result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var exercise = CreateValidExercise();
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync(exercise.Name.ToUpper());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exercise.Name, result.Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveExercises()
    {
        // Arrange
        var activeExercise = CreateValidExercise();
        var inactiveExercise = new Exercise("Inactive Exercise", ExerciseType.Strength, DifficultyLevel.Beginner, MuscleGroup.ARMS, new Equipment());
        inactiveExercise.Deactivate();

        _context.Exercises.AddRange(activeExercise, inactiveExercise);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        Assert.Single(results);
        Assert.Equal(activeExercise.Id, results.First().Id);
        Assert.True(results.First().IsActive);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExercise_WhenValidExercise()
    {
        // Arrange
        var exercise = CreateValidExercise();
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        var newName = "Updated Exercise Name";
        exercise.SetName(newName);

        // Act
        await _repository.UpdateAsync(exercise);

        // Assert
        var updatedExercise = await _context.Exercises.FindAsync(exercise.Id);
        Assert.NotNull(updatedExercise);
        Assert.Equal(newName, updatedExercise.Name);
        Assert.NotNull(updatedExercise.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveExercise_WhenExerciseExists()
    {
        // Arrange
        var exercise = CreateValidExercise();
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(exercise);

        // Assert
        var deletedExercise = await _context.Exercises.FindAsync(exercise.Id);
        Assert.Null(deletedExercise);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenExerciseExists()
    {
        // Arrange
        var exercise = CreateValidExercise();
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(exercise.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenExerciseDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsWithNameAsync_ShouldReturnTrue_WhenExerciseWithNameExists()
    {
        // Arrange
        var exercise = CreateValidExercise();
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsWithNameAsync(exercise.Name);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsWithNameAsync_ShouldExcludeSpecifiedId()
    {
        // Arrange
        var exercise = CreateValidExercise();
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsWithNameAsync(exercise.Name, exercise.Id);

        // Assert
        Assert.False(result); // Should exclude the exercise with the specified ID
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new("Exercise A", ExerciseType.Strength, DifficultyLevel.Beginner, MuscleGroup.ARMS, new Equipment()),
            new("Exercise B", ExerciseType.Cardio, DifficultyLevel.Intermediate, MuscleGroup.LEGS, new Equipment()),
            new("Exercise C", ExerciseType.Strength, DifficultyLevel.Advanced, MuscleGroup.CHEST, new Equipment()),
            new("Exercise D", ExerciseType.Mobility, DifficultyLevel.Beginner, MuscleGroup.BACK, new Equipment()),
            new("Exercise E", ExerciseType.Cardio, DifficultyLevel.Expert, MuscleGroup.FULL_BODY, new Equipment())
        };

        _context.Exercises.AddRange(exercises);
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(1, 3, null, "Name", false);

        // Assert
        Assert.Equal(5, totalCount);
        Assert.Equal(3, items.Count());
        Assert.Equal("Exercise A", items.First().Name);
        Assert.Equal("Exercise C", items.Last().Name);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldSortDescending_WhenSortDescendingIsTrue()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new("Alpha", ExerciseType.Strength, DifficultyLevel.Beginner, MuscleGroup.ARMS, new Equipment()),
            new("Beta", ExerciseType.Cardio, DifficultyLevel.Intermediate, MuscleGroup.LEGS, new Equipment()),
            new("Charlie", ExerciseType.Strength, DifficultyLevel.Advanced, MuscleGroup.CHEST, new Equipment())
        };

        _context.Exercises.AddRange(exercises);
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(1, 10, null, "Name", true);

        // Assert
        Assert.Equal(3, totalCount);
        Assert.Equal("Charlie", items.First().Name);
        Assert.Equal("Alpha", items.Last().Name);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new("Exercise 1", ExerciseType.Strength, DifficultyLevel.Beginner, MuscleGroup.ARMS, new Equipment()),
            new("Exercise 2", ExerciseType.Cardio, DifficultyLevel.Intermediate, MuscleGroup.LEGS, new Equipment())
        };

        _context.Exercises.AddRange(exercises);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync();

        // Assert
        Assert.Equal(2, count);
    }

    private static Exercise CreateValidExercise()
    {
        return new Exercise("Push-ups", ExerciseType.Strength, DifficultyLevel.Intermediate, MuscleGroup.CHEST, new Equipment());
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

using AutoMapper;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using FitnessApp.Modules.Exercises.Infrastructure.Repositories;
using FitnessApp.Modules.Exercises.Tests.Helpers;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FitnessApp.Modules.Exercises.Tests.Infrastructure.Repositories;

public class ExerciseRepositoryTests : IDisposable
{
    private readonly ExercisesDbContext _context;
    private readonly ExerciseRepository _repository;
    private readonly Mock<IMapper> _mockMapper;

    public ExerciseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ExercisesDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ExercisesDbContext(options);
        _mockMapper = new Mock<IMapper>();
        _repository = new ExerciseRepository(_context, _mockMapper.Object);
    }

    #region CRUD Operations Tests

    [Fact]
    public async Task AddAsync_WithValidExercise_ShouldAddToDatabase()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();

        // Act
        await _repository.AddAsync(exercise);

        // Assert
        var savedExercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == exercise.Id);
        Assert.NotNull(savedExercise);
        Assert.Equal(exercise.Name, savedExercise.Name);
        Assert.Equal(exercise.Type, savedExercise.Type);
        Assert.Equal(exercise.Equipment, savedExercise.Equipment);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnExercise()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreateDeadlifts();
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(exercise.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exercise.Id, result.Id);
        Assert.Equal(exercise.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithModifiedExercise_ShouldUpdateInDatabase()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();

        var newName = "Advanced Push-ups";
        exercise.SetName(newName);

        // Act
        await _repository.UpdateAsync(exercise);

        // Assert
        var updatedExercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == exercise.Id);
        Assert.NotNull(updatedExercise);
        Assert.Equal(newName, updatedExercise.Name);
        Assert.NotNull(updatedExercise.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingExercise_ShouldRemoveFromDatabase()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreateBurpees();
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(exercise);

        // Assert
        var deletedExercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == exercise.Id);
        Assert.Null(deletedExercise);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task GetAllAsync_WithNoFilters_ShouldReturnAllActiveExercises()
    {
        // Arrange
        await SeedTestExercises();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.True(result.Count() >= 3); // We seeded at least 3 active exercises
        Assert.All(result, ex => Assert.True(ex.IsActive));
        Assert.True(result.OrderBy(e => e.Name).SequenceEqual(result)); // Should be ordered by name
    }

    [Fact]
    public async Task GetAllAsync_WithIncludeInactiveTrue_ShouldReturnAllExercises()
    {
        // Arrange
        await SeedTestExercises();
        
        // Add an inactive exercise
        var inactiveExercise = ExerciseTestDataFactory.CreateCustomExercise("Inactive Exercise", ExerciseType.Strength);
        inactiveExercise.Deactivate();
        await _context.Exercises.AddAsync(inactiveExercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(includeInactive: true);

        // Assert
        Assert.Contains(result, ex => !ex.IsActive);
        Assert.Contains(result, ex => ex.Id == inactiveExercise.Id);
    }

    [Fact]
    public async Task SearchByNameAsync_WithValidSearchTerm_ShouldReturnMatchingExercises()
    {
        // Arrange
        await SeedTestExercises();

        // Act
        var result = await _repository.SearchByNameAsync("Push");

        // Assert
        Assert.All(result, ex => Assert.Contains("Push", ex.Name, StringComparison.OrdinalIgnoreCase));
        Assert.True(result.Any()); // Should find push-ups
    }

    [Fact]
    public async Task GetPagedAsync_WithValidQuery_ShouldReturnPagedResults()
    {
        // Arrange
        await SeedBulkTestExercises(10); // Seed 10 exercises
        
        var query = new ExerciseQueryDto
        {
            PageNumber = 2,
            PageSize = 3
        };

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        // Assert
        Assert.Equal(3, items.Count()); // Should return page size items
        Assert.Equal(10, totalCount); // Total count should be 10
    }

    [Fact]
    public async Task GetPagedAsync_WithTypeFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestExercises();
        
        var query = new ExerciseQueryDto
        {
            Type = ExerciseType.Strength,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        // Assert
        Assert.All(items, ex => Assert.Equal(ExerciseType.Strength, ex.Type));
        Assert.True(items.Any()); // Should have strength exercises
    }

    [Fact]
    public async Task GetPagedAsync_WithDifficultyFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestExercises();
        
        var query = new ExerciseQueryDto
        {
            Difficulty = DifficultyLevel.Beginner,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        // Assert
        Assert.All(items, ex => Assert.Equal(DifficultyLevel.Beginner, ex.Difficulty));
        Assert.True(items.Any()); // Should have beginner exercises
    }

    [Fact]
    public async Task GetPagedAsync_WithEquipmentFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestExercises();
        
        var query = new ExerciseQueryDto
        {
            RequiresEquipment = false, // Bodyweight only
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        // Assert
        Assert.All(items, ex => Assert.Equal(Equipment.None, ex.Equipment));
        Assert.True(items.Any()); // Should have bodyweight exercises
    }

    [Fact]
    public async Task GetPagedAsync_WithNameFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestExercises();
        
        var query = new ExerciseQueryDto
        {
            NameFilter = "Push",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        // Assert
        Assert.All(items, ex => Assert.Contains("Push", ex.Name, StringComparison.OrdinalIgnoreCase));
        Assert.True(items.Any()); // Should find push-ups
    }

    [Fact]
    public async Task GetPagedAsync_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        await SeedTestExercises();
        
        var query = new ExerciseQueryDto
        {
            SortBy = "name",
            SortDescending = true,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        // Assert
        var sortedNames = items.Select(e => e.Name).ToList();
        var expectedOrder = sortedNames.OrderByDescending(n => n).ToList();
        Assert.Equal(expectedOrder, sortedNames);
    }

    #endregion

    #region Specialized Query Tests

    [Fact]
    public async Task GetByNameAsync_WithExistingName_ShouldReturnExercise()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Push-ups");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Push-ups", result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_WithNonExistingName_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("Non-existing Exercise");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingExercise_ShouldReturnTrue()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(exercise.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingExercise_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var exists = await _repository.ExistsAsync(nonExistingId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsWithNameAsync("Push-ups");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExcludedId_ShouldReturnFalseWhenOnlyExcludedExists()
    {
        // Arrange
        var exercise = ExerciseTestDataFactory.RealExercises.CreatePushUps();
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsWithNameAsync("Push-ups", exercise.Id);

        // Assert
        Assert.False(exists); // Should return false because the only match is excluded
    }

    #endregion

    #region Helper Methods

    private async Task SeedTestExercises()
    {
        var exercises = new[]
        {
            ExerciseTestDataFactory.RealExercises.CreatePushUps(),
            ExerciseTestDataFactory.RealExercises.CreateBurpees(),
            ExerciseTestDataFactory.RealExercises.CreateDeadlifts(),
            ExerciseTestDataFactory.RealExercises.CreateDumbbellRows(),
            ExerciseTestDataFactory.RealExercises.CreatePullUps()
        };

        foreach (var exercise in exercises)
        {
            await _context.Exercises.AddAsync(exercise);
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task SeedBulkTestExercises(int count)
    {
        var exercises = ExerciseTestDataFactory.CreateBulkExercises(count);
        
        foreach (var exercise in exercises)
        {
            await _context.Exercises.AddAsync(exercise);
        }
        
        await _context.SaveChangesAsync();
    }

    #endregion

    [Fact]
    public async Task GetPagedAsync_WithMuscleGroupsFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestExercises();
        
        // Configure mock mapper to convert string list to MuscleGroup enum
        _mockMapper.Setup(m => m.Map<MuscleGroup>(It.IsAny<List<string>>()))
                  .Returns(MuscleGroup.Chest);
        
        var query = new ExerciseQueryDto
        {
            MuscleGroups = new List<string> { "Chest" },
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        // Assert
        Assert.All(items, ex => Assert.True(ex.MuscleGroups.HasFlag(MuscleGroup.Chest)));
        Assert.True(items.Any()); // Should have chest exercises
    }

    [Fact]
    public async Task GetPagedAsync_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        await SeedTestExercises();
        
        var query = new ExerciseQueryDto
        {
            Type = ExerciseType.Strength,
            Difficulty = DifficultyLevel.Beginner,
            RequiresEquipment = false,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        // Assert
        Assert.All(items, ex =>
        {
            Assert.Equal(ExerciseType.Strength, ex.Type);
            Assert.Equal(DifficultyLevel.Beginner, ex.Difficulty);
            Assert.Equal(Equipment.None, ex.Equipment);
        });
    }

    #region Performance & Edge Case Tests

    [Fact]
    public async Task GetPagedAsync_WithLargeDataset_ShouldPerformEfficiently()
    {
        // Arrange
        const int largeCount = 1000;
        await SeedBulkTestExercises(largeCount);
        
        var query = new ExerciseQueryDto
        {
            PageNumber = 10,
            PageSize = 50
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var (items, totalCount) = await _repository.GetPagedAsync(query);
        stopwatch.Stop();

        // Assert
        Assert.Equal(50, items.Count()); // Should return page size
        Assert.Equal(largeCount, totalCount); // Should have total count
        Assert.True(stopwatch.ElapsedMilliseconds < 1000); // Should complete within 1 second
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page number
    [InlineData(-1, 10)] // Negative page number
    [InlineData(1, 0)] // Invalid page size
    [InlineData(1, -5)] // Negative page size
    public async Task GetPagedAsync_WithInvalidPagination_ShouldHandleGracefully(int pageNumber, int pageSize)
    {
        // Arrange
        await SeedTestExercises();
        
        var query = new ExerciseQueryDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Act & Assert - Should not throw exception
        var (items, totalCount) = await _repository.GetPagedAsync(query);
        
        // The repository should handle invalid pagination gracefully
        Assert.NotNull(items);
        Assert.True(totalCount >= 0);
    }

    [Fact]
    public async Task GetPagedAsync_WithVeryLargePageSize_ShouldReturnAllAvailable()
    {
        // Arrange
        await SeedTestExercises();
        
        var query = new ExerciseQueryDto
        {
            PageNumber = 1,
            PageSize = int.MaxValue
        };

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        // Assert
        Assert.Equal(totalCount, items.Count()); // Should return all items
    }

    [Fact]
    public async Task GetAllAsync_WithConcurrentAccess_ShouldMaintainConsistency()
    {
        // Arrange
        await SeedTestExercises();

        // Act - Multiple concurrent reads
        var tasks = Enumerable.Range(0, 10)
            .Select(async _ => await _repository.GetAllAsync())
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - All results should be consistent
        var firstResult = results.First().ToList();
        Assert.All(results, result => 
        {
            var resultList = result.ToList();
            Assert.Equal(firstResult.Count, resultList.Count);
            Assert.True(firstResult.Select(e => e.Id).SequenceEqual(resultList.Select(e => e.Id)));
        });
    }

    [Fact]
    public async Task SearchByNameAsync_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var specialExercise = ExerciseTestDataFactory.CreateCustomExercise(
            "Push-ups (Beginner's Version)", 
            ExerciseType.Strength);
        await _context.Exercises.AddAsync(specialExercise);
        await _context.SaveChangesAsync();

        // Act
        var result1 = await _repository.SearchByNameAsync("Push-ups");
        var result2 = await _repository.SearchByNameAsync("Beginner's");
        var result3 = await _repository.SearchByNameAsync("(Beginner");

        // Assert
        Assert.Contains(result1, ex => ex.Id == specialExercise.Id);
        Assert.Contains(result2, ex => ex.Id == specialExercise.Id);
        Assert.Contains(result3, ex => ex.Id == specialExercise.Id);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}

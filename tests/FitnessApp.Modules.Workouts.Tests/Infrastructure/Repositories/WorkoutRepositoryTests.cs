using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Infrastructure.Persistence;
using FitnessApp.Modules.Workouts.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Workouts.Tests.Infrastructure.Repositories;

public class WorkoutRepositoryTests : IDisposable
{
    private readonly WorkoutsDbContext _context;
    private readonly WorkoutRepository _repository;

    public WorkoutRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<WorkoutsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new WorkoutsDbContext(options);
        _repository = new WorkoutRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ValidWorkout_ShouldAddToDatabase()
    {
        // Arrange
        var workout = CreateSampleWorkout();

        // Act
        await _repository.AddAsync(workout);
        await _context.SaveChangesAsync();

        // Assert
        var savedWorkout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == workout.Id);
        savedWorkout.Should().NotBeNull();
        savedWorkout.Name.Should().Be(workout.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingWorkout_ShouldReturnWorkout()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(workout.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(workout.Id);
        result.Name.Should().Be(workout.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentWorkout_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithPhasesAsync_WorkoutWithPhases_ShouldReturnWorkoutWithPhases()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        workout.AddPhase(WorkoutPhaseType.WarmUp, "Warm Up", Duration.FromMinutes(10));
        workout.AddPhase(WorkoutPhaseType.MainEffort, "Main Effort", Duration.FromMinutes(30));

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithPhasesAsync(workout.Id);

        // Assert
        result.Should().NotBeNull();
        result.Phases.Should().HaveCount(2);
        result.Phases.Should().Contain(p => p.Type == WorkoutPhaseType.WarmUp);
        result.Phases.Should().Contain(p => p.Type == WorkoutPhaseType.MainEffort);
    }

    [Fact]
    public async Task UpdateAsync_ExistingWorkout_ShouldUpdateWorkout()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        var newName = "Updated Workout";
        workout.UpdateName(newName);

        // Act
        await _repository.UpdateAsync(workout);
        await _context.SaveChangesAsync();

        // Assert
        var updatedWorkout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == workout.Id);
        updatedWorkout.Should().NotBeNull();
        updatedWorkout.Name.Should().Be(newName);
        updatedWorkout.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingWorkout_ShouldRemoveFromDatabase()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(workout);
        await _context.SaveChangesAsync();

        // Assert
        var deletedWorkout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == workout.Id);
        deletedWorkout.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var workout1 = CreateSampleWorkout();
        workout1.UpdateName("Beginner Workout");
        workout1.UpdateDifficulty(DifficultyLevel.Beginner);

        var workout2 = new Workout(
            "Advanced Workout",
            WorkoutType.Fixed,
            DifficultyLevel.Advanced,
            Duration.FromMinutes(60),
            EquipmentType.FreeWeights,
            Guid.NewGuid());

        _context.Workouts.AddRange(workout1, workout2);
        await _context.SaveChangesAsync();

        // Act
        var (results, totalCount) = await _repository.GetPagedAsync(
            page: 1,
            pageSize: 10,
            type: null,
            difficulty: DifficultyLevel.Beginner,
            equipment: null,
            maxDurationMinutes: null,
            searchTerm: null,
            isActive: true,
            createdByUserId: null,
            createdByCoachId: null);

        // Assert
        results.Should().HaveCount(1);
        results.First().Difficulty.Should().Be(DifficultyLevel.Beginner);
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_WithSearchTerm_ShouldReturnMatchingResults()
    {
        // Arrange
        var workout1 = CreateSampleWorkout();
        workout1.UpdateName("HIIT Training");

        var workout2 = new Workout(
            "Yoga Session",
            WorkoutType.Fixed,
            DifficultyLevel.Beginner,
            Duration.FromMinutes(45),
            EquipmentType.Mat,
            Guid.NewGuid());

        _context.Workouts.AddRange(workout1, workout2);
        await _context.SaveChangesAsync();

        // Act
        var (results, totalCount) = await _repository.GetPagedAsync(
            page: 1,
            pageSize: 10,
            type: null,
            difficulty: null,
            equipment: null,
            maxDurationMinutes: null,
            searchTerm: "HIIT",
            isActive: true,
            createdByUserId: null,
            createdByCoachId: null);

        // Assert
        results.Should().HaveCount(1);
        results.First().Name.Should().Contain("HIIT");
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_UserWorkouts_ShouldReturnUserWorkouts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var userWorkout = Workout.CreateUserWorkout(
            "User Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(30),
            EquipmentType.None,
            userId);

        var otherWorkout = Workout.CreateUserWorkout(
            "Other Workout",
            DifficultyLevel.Intermediate,
            Duration.FromMinutes(30),
            EquipmentType.None,
            otherUserId);

        _context.Workouts.AddRange(userWorkout, otherWorkout);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByUserIdAsync(userId);

        // Assert
        results.Should().HaveCount(1);
        results.First().CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByCoachIdAsync_CoachWorkouts_ShouldReturnCoachWorkouts()
    {
        // Arrange
        var coachId = Guid.NewGuid();
        var otherCoachId = Guid.NewGuid();

        var coachWorkout = Workout.CreateCoachWorkout(
            "Coach Workout",
            DifficultyLevel.Advanced,
            Duration.FromMinutes(60),
            EquipmentType.GymEquipment,
            coachId);

        var otherWorkout = Workout.CreateCoachWorkout(
            "Other Workout",
            DifficultyLevel.Advanced,
            Duration.FromMinutes(60),
            EquipmentType.GymEquipment,
            otherCoachId);

        _context.Workouts.AddRange(coachWorkout, otherWorkout);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByCoachIdAsync(coachId);

        // Assert
        results.Should().HaveCount(1);
        results.First().CreatedByCoachId.Should().Be(coachId);
    }

    [Fact]
    public async Task ExistsAsync_ExistingWorkout_ShouldReturnTrue()
    {
        // Arrange
        var workout = CreateSampleWorkout();
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(workout.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistentWorkout_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var exists = await _repository.ExistsAsync(nonExistentId);

        // Assert
        exists.Should().BeFalse();
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

    public void Dispose()
    {
        _context.Dispose();
    }
}

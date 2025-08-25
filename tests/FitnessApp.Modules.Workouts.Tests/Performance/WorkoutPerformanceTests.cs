using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Infrastructure.Persistence;
using FitnessApp.Modules.Workouts.Infrastructure.Repositories;
using FitnessApp.Modules.Workouts.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FitnessApp.Modules.Workouts.Tests.Performance;

public class WorkoutPerformanceTests : IDisposable
{
    private readonly WorkoutsDbContext _context;
    private readonly WorkoutRepository _repository;

    public WorkoutPerformanceTests()
    {
        var options = new DbContextOptionsBuilder<WorkoutsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new WorkoutsDbContext(options);
        _repository = new WorkoutRepository(_context);
    }

    [Fact]
    public async Task GetPagedAsync_With1000Workouts_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var workouts = new List<Workout>();
        for (int i = 0; i < 1000; i++)
        {
            var workout = TestDataFactory.Workouts.CreateUserWorkout(
                $"Workout {i}",
                (DifficultyLevel)(i % 3),
                30 + (i % 60),
                (EquipmentType)(1 << (i % 8)));
            workouts.Add(workout);
        }

        _context.Workouts.AddRange(workouts);
        await _context.SaveChangesAsync();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var (results, totalCount) = await _repository.GetPagedAsync(
            page: 1,
            pageSize: 50,
            type: null,
            difficulty: null,
            equipment: null,
            maxDurationMinutes: null,
            searchTerm: null,
            isActive: true,
            createdByUserId: null,
            createdByCoachId: null);

        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(50);
        totalCount.Should().Be(1000);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Query should complete within 1 second");
    }

    [Fact]
    public async Task CreateWorkoutWithManyPhases_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var workout = TestDataFactory.Workouts.CreateUserWorkout("Complex Workout");
        var stopwatch = Stopwatch.StartNew();

        // Act - Add 50 phases with exercises
        for (int i = 0; i < 50; i++)
        {
            var phase = workout.AddPhase(
                (WorkoutPhaseType)(i % 4),
                $"Phase {i}",
                Duration.FromMinutes(5));

            // Add 10 exercises per phase
            for (int j = 0; j < 10; j++)
            {
                phase.AddExercise(
                    Guid.NewGuid(),
                    $"Exercise {j}",
                    TestDataFactory.ValueObjects.CreateRepetitionBasedParameters(
                        repetitions: 10 + j,
                        sets: 3,
                        restTimeSeconds: 30 + j * 5));
            }
        }

        await _repository.AddAsync(workout);
        await _context.SaveChangesAsync();

        stopwatch.Stop();

        // Assert
        workout.Phases.Should().HaveCount(50);
        workout.GetTotalExerciseCount().Should().Be(500);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "Complex workout creation should complete within 2 seconds");
    }

    [Fact]
    public async Task SearchWorkouts_WithComplexFilters_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var workouts = new List<Workout>();
        var userId = Guid.NewGuid();

        // Create diverse workouts
        for (int i = 0; i < 500; i++)
        {
            var workout = i % 2 == 0
                ? TestDataFactory.Workouts.CreateUserWorkout(
                    $"User Workout {i} HIIT Training",
                    (DifficultyLevel)(i % 3),
                    20 + (i % 40),
                    (EquipmentType)(1 << (i % 3)),
                    userId)
                : TestDataFactory.Workouts.CreateCoachWorkout(
                    $"Coach Workout {i} Strength Training",
                    (DifficultyLevel)(i % 3 + 1),
                    40 + (i % 60),
                    (EquipmentType)(1 << (i % 4 + 3)));
            
            workouts.Add(workout);
        }

        _context.Workouts.AddRange(workouts);
        await _context.SaveChangesAsync();

        var stopwatch = Stopwatch.StartNew();

        // Act - Complex search with multiple filters
        var (results, totalCount) = await _repository.GetPagedAsync(
            page: 1,
            pageSize: 20,
            type: WorkoutType.UserCreated,
            difficulty: DifficultyLevel.Intermediate,
            equipment: EquipmentType.Mat,
            maxDurationMinutes: 45,
            searchTerm: "HIIT",
            isActive: true,
            createdByUserId: userId,
            createdByCoachId: null);

        stopwatch.Stop();

        // Assert
        results.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "Complex search should complete within 500ms");
    }

    [Fact]
    public async Task CalculateActualDuration_ForComplexWorkout_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var workout = TestDataFactory.Workouts.CreateWorkoutWithPhases("Performance Test Workout");
        
        // Add more exercises to make it complex
        var mainPhase = workout.GetPhase(WorkoutPhaseType.MainEffort);
        if (mainPhase != null)
        {
            for (int i = 0; i < 100; i++)
            {
                mainPhase.AddExercise(
                    Guid.NewGuid(),
                    $"Exercise {i}",
                    TestDataFactory.ValueObjects.CreateRepetitionBasedParameters(
                        repetitions: 10 + (i % 20),
                        sets: 2 + (i % 3),
                        restTimeSeconds: 30 + (i % 60)));
            }
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        var totalDuration = workout.CalculateActualDuration();

        stopwatch.Stop();

        // Assert
        totalDuration.TotalMinutes.Should().BeGreaterThan(0);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "Duration calculation should complete within 100ms");
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

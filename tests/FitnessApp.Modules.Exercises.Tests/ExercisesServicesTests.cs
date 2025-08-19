using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Application.Services;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using Moq;
using AwesomeAssertions;

namespace FitnessApp.Modules.Exercises.Tests;

public class ExerciseServiceTests
{
    private readonly Mock<IExerciseRepository> _repo = new();
    private readonly ExerciseService _sut;

    public ExerciseServiceTests()
    {
        _sut = new ExerciseService(_repo.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_ExerciseDto_When_Exercise_Exists()
    {
        var id = Guid.NewGuid();
        var exercise = new Exercise { Id = id, Name = "Push Up" };
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(exercise);

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Push Up");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Exercise_Does_Not_Exist()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Exercise?)null);

        var result = await _sut.GetByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Add_Exercise_And_Return_ExerciseDto()
    {
        var dto = new CreateExerciseDto
        {
            Name = "Squat",
            Type = Domain.Enums.ExerciseType.Strength,
            MuscleGroups = new List<string> { "Legs" },
            Difficulty = (DifficultyLevelDto)2,
            Equipment = new List<string> { "Barbell" }
        };

        Exercise? capturedExercise = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Squat");
        capturedExercise.Should().NotBeNull();
        capturedExercise!.Name.Should().Be("Squat");
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Exercise()
    {
        var id = Guid.NewGuid();
        var existingExercise = new Exercise { Id = id, Name = "Bench Press" };
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingExercise);

        var dto = new ExerciseDto
        {
            Id = id,
            Name = "Incline Bench Press",
            Type = Domain.Enums.ExerciseType.Strength,
            MuscleGroups = new List<string> { "Chest" },
            Difficulty = (DifficultyLevelDto)3,
            Equipment = new List<string> { "Dumbbell" }
        };

        var result = await _sut.UpdateAsync(id, dto);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Incline Bench Press");
        _repo.Verify(r => r.UpdateAsync(existingExercise), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Exercise_When_It_Exists()
    {
        var id = Guid.NewGuid();
        var existingExercise = new Exercise { Id = id, Name = "Deadlift" };
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingExercise);

        await _sut.DeleteAsync(id);

        _repo.Verify(r => r.DeleteAsync(existingExercise), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Do_Nothing_When_Exercise_Does_Not_Exist()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Exercise?)null);

        await _sut.DeleteAsync(id);

        _repo.Verify(r => r.DeleteAsync(It.IsAny<Exercise>()), Times.Never);
    }
}

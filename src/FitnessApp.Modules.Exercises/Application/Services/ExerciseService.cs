
using FitnessApp.Modules.Exercises.Application.Dtos.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;

namespace FitnessApp.Modules.Exercises.Application.Services;

public class ExerciseService : IExerciseService
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMuscleGroupRepository _muscleGroupRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IExerciseMapper _mapper;

    public ExerciseService(
        IExerciseRepository exerciseRepository,
        ITagRepository tagRepository,
        IMuscleGroupRepository muscleGroupRepository,
        IEquipmentRepository equipmentRepository,
        IExerciseMapper mapper)
    {
        _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _muscleGroupRepository = muscleGroupRepository ?? throw new ArgumentNullException(nameof(muscleGroupRepository));
        _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ExerciseResponse> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A valid ID must be provided.", nameof(id));
        }
        
        var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id);
        if (exercise == null)
        {
            throw new KeyNotFoundException($"Exercise with ID {id} not found");
        }

        return _mapper.MapToExercise(exercise);
    }

    public async Task<IEnumerable<ExerciseResponse>> GetAllAsync()
    {
        var exercises = await _exerciseRepository.GetAllWithDetailsAsync();
        return _mapper.MapToExerciseList(exercises);
    }

    public async Task<IEnumerable<ExerciseResponse>> SearchAsync(ExerciseSearchRequest searchParams)
    {
        if (searchParams == null)
        {
            throw new ArgumentNullException(nameof(searchParams), "Search parameters cannot be null");
        }

        var exercises = await _exerciseRepository.SearchAsync(
            name: searchParams.Name,
            tagIds: searchParams.TagIds,
            muscleGroupIds: searchParams.MuscleGroupIds,
            equipmentIds: searchParams.EquipmentIds,
            difficulty: searchParams.Difficulty,
            maxDuration: searchParams.MaxDurationInSeconds,
            requiresEquipment: searchParams.RequiresEquipment,
            skip: searchParams.Skip,
            take: searchParams.Take,
            sortBy: searchParams.SortBy,
            sortDescending: searchParams.SortDescending
        );

        return _mapper.MapToExerciseList(exercises);
    }

    public async Task<Guid> CreateAsync(CreateExerciseRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null");
        }

        var existingExercise = await _exerciseRepository.GetByNameAsync(request.Name);
        if (existingExercise != null)
        {
            throw new InvalidOperationException($"Exercise with name '{request.Name}' already exists");
        }

        var exercise = new Exercise(
            name: request.Name,
            description: request.Description,
            instructions: request.Instructions,
            difficultyLevel: request.Difficulty.ToString(),
            isBodyweightExercise: request.EquipmentIds == null || !request.EquipmentIds.Any()
        );

        if (!string.IsNullOrEmpty(request.CommonMistakes))
        {
            exercise.Update(
                exercise.Name,
                exercise.Description,
                exercise.Instructions,
                request.CommonMistakes,
                exercise.DifficultyLevel,
                request.CaloriesBurnedPerMinute,
                exercise.IsBodyweightExercise
            );
        }

        if (request.TagIds != null && request.TagIds.Any())
        {
            foreach (var tagId in request.TagIds)
            {
                var tag = await _tagRepository.GetByIdAsync(tagId);
                if (tag != null)
                {
                    exercise.AddTag(tag);
                }
            }
        }

        if (request.MuscleGroups != null && request.MuscleGroups.Any())
        {
            foreach (var muscleGroupAssignment in request.MuscleGroups)
            {
                var muscleGroup = await _muscleGroupRepository.GetByIdAsync(muscleGroupAssignment.MuscleGroupId);
                if (muscleGroup != null)
                {
                    exercise.AddMuscleGroup(muscleGroup, muscleGroupAssignment.IsPrimary);
                }
            }
        }

        if (request.EquipmentIds != null && request.EquipmentIds.Any())
        {
            foreach (var equipmentId in request.EquipmentIds)
            {
                var equipment = await _equipmentRepository.GetByIdAsync(equipmentId);
                if (equipment != null)
                {
                    exercise.AddEquipment(equipment);
                }
            }
        }

        await _exerciseRepository.AddAsync(exercise);
        return exercise.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateExerciseRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null");
        }

        var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id);
        if (exercise == null)
        {
            throw new KeyNotFoundException($"Exercise with ID {id} not found");
        }

        if (request.Name != null && request.Name != exercise.Name)
        {
            var existingExercise = await _exerciseRepository.GetByNameAsync(request.Name);
            if (existingExercise != null && existingExercise.Id != id)
            {
                throw new InvalidOperationException($"Exercise with name '{request.Name}' already exists");
            }
        }

        exercise.Update(
            name: request.Name ?? exercise.Name,
            description: request.Description ?? exercise.Description,
            instructions: request.Instructions ?? exercise.Instructions,
            commonMistakes: request.CommonMistakes ?? exercise.CommonMistakes,
            difficultyLevel: request.Difficulty?.ToString() ?? exercise.DifficultyLevel,
            estimatedCaloriesBurn: request.CaloriesBurnedPerMinute ?? exercise.EstimatedCaloriesBurn,
            isBodyweightExercise: exercise.IsBodyweightExercise
        );

        await _exerciseRepository.UpdateAsync(exercise);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A valid ID must be provided.", nameof(id));
        }

        var exercise = await _exerciseRepository.GetByIdAsync(id);

        if (exercise == null)
        {
            throw new KeyNotFoundException($"Exercise with ID {id} not found");
        }

        await _exerciseRepository.DeleteAsync(id);
    }
}
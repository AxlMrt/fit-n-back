using AutoMapper;
using FluentValidation;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Exceptions;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using Microsoft.Extensions.Logging;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Exercises.Application.Services;
public class ExerciseService : IExerciseService
{
    private readonly IExerciseRepository _repository;
    private readonly IValidator<CreateExerciseDto> _createValidator;
    private readonly IValidator<UpdateExerciseDto> _updateValidator;
    private readonly IValidator<ExerciseQueryDto> _queryValidator;
    private readonly ILogger<ExerciseService> _logger;
    private readonly IMapper _mapper;

    public ExerciseService(
        IExerciseRepository repository,
        IValidator<CreateExerciseDto> createValidator,
        IValidator<UpdateExerciseDto> updateValidator,
        IValidator<ExerciseQueryDto> queryValidator,
        ILogger<ExerciseService> logger,
        IMapper mapper)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _queryValidator = queryValidator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ExerciseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Exercise ID cannot be empty", nameof(id));

        var exercise = await _repository.GetByIdAsync(id, cancellationToken);
        return exercise != null ? _mapper.Map<ExerciseDto>(exercise) : null;
    }

    public async Task<ExerciseDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exercise name cannot be empty", nameof(name));

        var exercise = await _repository.GetByNameAsync(name.Trim(), cancellationToken);
        return exercise != null ? _mapper.Map<ExerciseDto>(exercise) : null;
    }

    public async Task<PagedResult<ExerciseListDto>> GetPagedAsync(ExerciseQueryDto query, CancellationToken cancellationToken = default)
    {
        var validationResult = await _queryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        var exerciseListDtos = _mapper.Map<List<ExerciseListDto>>(items);

        return new PagedResult<ExerciseListDto>(
            exerciseListDtos,
            totalCount,
            query.PageNumber,
            query.PageSize,
            totalPages);
    }

    public async Task<IEnumerable<ExerciseListDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var exercises = await _repository.GetAllAsync(includeInactive, cancellationToken);
        return _mapper.Map<IEnumerable<ExerciseListDto>>(exercises);
    }

    public async Task<ExerciseDto> CreateAsync(CreateExerciseDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Check for duplicate name
        if (await _repository.ExistsWithNameAsync(dto.Name, cancellationToken: cancellationToken))
        {
            throw new ExerciseDomainException($"An exercise with the name '{dto.Name}' already exists");
        }

        var exercise = _mapper.Map<Exercise>(dto);

        await _repository.AddAsync(exercise, cancellationToken);

        _logger.LogInformation("Created new exercise: {ExerciseName} (ID: {ExerciseId})", 
            exercise.Name, exercise.Id);

        return _mapper.Map<ExerciseDto>(exercise);
    }

    public async Task<ExerciseDto?> UpdateAsync(Guid id, UpdateExerciseDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var exercise = await _repository.GetByIdAsync(id, cancellationToken);
        if (exercise == null)
            return null;

        // Check for duplicate name (excluding current exercise)
        if (!string.IsNullOrWhiteSpace(dto.Name) && 
            await _repository.ExistsWithNameAsync(dto.Name, id, cancellationToken))
        {
            throw new ExerciseDomainException($"An exercise with the name '{dto.Name}' already exists");
        }

        // Apply updates
        if (!string.IsNullOrWhiteSpace(dto.Name))
            exercise.SetName(dto.Name);

        exercise.SetDescription(dto.Description);

        if (dto.Type.HasValue || dto.Difficulty.HasValue || dto.MuscleGroups != null)
        {
            var type = dto.Type ?? exercise.Type;
            var difficulty = dto.Difficulty ?? exercise.Difficulty;
            var muscleGroups = dto.MuscleGroups != null 
                ? _mapper.Map<MuscleGroup>(dto.MuscleGroups) 
                : exercise.MuscleGroups;
            
            exercise.UpdateDetails(type, difficulty, muscleGroups);
        }

        if (dto.Equipment != null)
            exercise.SetEquipment(_mapper.Map<Equipment>(dto.Equipment));

        exercise.SetInstructions(dto.Instructions);

        if (dto.ImageContentId.HasValue || dto.VideoContentId.HasValue)
            exercise.SetContentReferences(dto.ImageContentId, dto.VideoContentId);

        await _repository.UpdateAsync(exercise, cancellationToken);

        _logger.LogInformation("Updated exercise: {ExerciseName} (ID: {ExerciseId})", 
            exercise.Name, exercise.Id);

        return _mapper.Map<ExerciseDto>(exercise);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exercise = await _repository.GetByIdAsync(id, cancellationToken);
        if (exercise == null)
            return false;

        await _repository.DeleteAsync(exercise, cancellationToken);

        _logger.LogInformation("Deleted exercise: {ExerciseName} (ID: {ExerciseId})", 
            exercise.Name, exercise.Id);

        return true;
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exercise = await _repository.GetByIdAsync(id, cancellationToken);
        if (exercise == null)
            return false;

        exercise.Activate();
        await _repository.UpdateAsync(exercise, cancellationToken);

        _logger.LogInformation("Activated exercise: {ExerciseName} (ID: {ExerciseId})", 
            exercise.Name, exercise.Id);

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exercise = await _repository.GetByIdAsync(id, cancellationToken);
        if (exercise == null)
            return false;

        exercise.Deactivate();
        await _repository.UpdateAsync(exercise, cancellationToken);

        _logger.LogInformation("Deactivated exercise: {ExerciseName} (ID: {ExerciseId})", 
            exercise.Name, exercise.Id);

        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(id, cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _repository.ExistsWithNameAsync(name.Trim(), excludeId, cancellationToken);
    }

    public async Task<IEnumerable<ExerciseListDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<ExerciseListDto>();

        var exercises = await _repository.SearchByNameAsync(searchTerm.Trim(), false, cancellationToken);
        return _mapper.Map<IEnumerable<ExerciseListDto>>(exercises);
    }
}

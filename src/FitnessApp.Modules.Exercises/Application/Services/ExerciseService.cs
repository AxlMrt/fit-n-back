using FluentValidation;
using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Enums;
using FitnessApp.Modules.Exercises.Domain.Exceptions;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Domain.Specifications;
using FitnessApp.Modules.Exercises.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Exercises.Application.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _repository;
        private readonly IValidator<CreateExerciseDto> _createValidator;
        private readonly IValidator<UpdateExerciseDto> _updateValidator;
        private readonly IValidator<ExerciseQueryDto> _queryValidator;
        private readonly ILogger<ExerciseService> _logger;

        public ExerciseService(
            IExerciseRepository repository,
            IValidator<CreateExerciseDto> createValidator,
            IValidator<UpdateExerciseDto> updateValidator,
            IValidator<ExerciseQueryDto> queryValidator,
            ILogger<ExerciseService> logger)
        {
            _repository = repository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _queryValidator = queryValidator;
            _logger = logger;
        }

        public async Task<ExerciseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Exercise ID cannot be empty", nameof(id));

            var exercise = await _repository.GetByIdAsync(id, cancellationToken);
            return exercise?.MapToDto();
        }

        public async Task<ExerciseDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Exercise name cannot be empty", nameof(name));

            var exercise = await _repository.GetByNameAsync(name.Trim(), cancellationToken);
            return exercise?.MapToDto();
        }

        public async Task<PagedResultDto<ExerciseListDto>> GetPagedAsync(ExerciseQueryDto query, CancellationToken cancellationToken = default)
        {
            var validationResult = await _queryValidator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var specification = BuildSpecification(query);

            var (items, totalCount) = await _repository.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                specification,
                query.SortBy,
                query.SortDescending,
                cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            var exerciseListDtos = items.Select(exercise => exercise.MapToListDto()).ToList();

            return new PagedResultDto<ExerciseListDto>(
                exerciseListDtos,
                totalCount,
                query.PageNumber,
                query.PageSize,
                totalPages);
        }

        public async Task<IEnumerable<ExerciseListDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            Specification<Exercise>? specification = includeInactive ? null : new ActiveExerciseSpecification();
            
            var exercises = await _repository.GetBySpecificationAsync(specification ?? new ActiveExerciseSpecification(), cancellationToken);
            return exercises.Select(e => e.MapToListDto());
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

            var equipment = new Equipment(dto.Equipment);
            var muscleGroups = ConvertMuscleGroupsList(dto.MuscleGroups);
            
            var exercise = new Exercise(dto.Name, dto.Type, dto.Difficulty, muscleGroups, equipment);

            // Set optional properties
            if (!string.IsNullOrWhiteSpace(dto.Description))
                exercise.SetDescription(dto.Description);
            
            if (!string.IsNullOrWhiteSpace(dto.Instructions))
                exercise.SetInstructions(dto.Instructions);
            
            if (dto.ImageContentId.HasValue || dto.VideoContentId.HasValue)
                exercise.SetContentReferences(dto.ImageContentId, dto.VideoContentId);

            await _repository.AddAsync(exercise, cancellationToken);

            _logger.LogInformation("Created new exercise: {ExerciseName} (ID: {ExerciseId})", 
                exercise.Name, exercise.Id);

            return exercise.MapToDto();
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
                    ? ConvertMuscleGroupsList(dto.MuscleGroups) 
                    : exercise.MuscleGroups;
                
                exercise.UpdateDetails(type, difficulty, muscleGroups);
            }

            if (dto.Equipment != null)
                exercise.SetEquipment(new Equipment(dto.Equipment));

            exercise.SetInstructions(dto.Instructions);

            if (dto.ImageContentId.HasValue || dto.VideoContentId.HasValue)
                exercise.SetContentReferences(dto.ImageContentId, dto.VideoContentId);

            await _repository.UpdateAsync(exercise, cancellationToken);

            _logger.LogInformation("Updated exercise: {ExerciseName} (ID: {ExerciseId})", 
                exercise.Name, exercise.Id);

            return exercise.MapToDto();
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

            var specification = new ExerciseByNameSpecification(searchTerm.Trim())
                .And(new ActiveExerciseSpecification());

            var exercises = await _repository.GetBySpecificationAsync(specification, cancellationToken);
            return exercises.Select(e => e.MapToListDto());
        }

        private static Specification<Exercise> BuildSpecification(ExerciseQueryDto query)
        {
            Specification<Exercise>? specification = null;

            if (!string.IsNullOrWhiteSpace(query.NameFilter))
            {
                specification = new ExerciseByNameSpecification(query.NameFilter);
            }

            if (query.Type.HasValue)
            {
                var typeSpec = new ExerciseByTypeSpecification(query.Type.Value);
                specification = specification?.And(typeSpec) ?? typeSpec;
            }

            if (query.Difficulty.HasValue)
            {
                var difficultySpec = new ExerciseByDifficultySpecification(query.Difficulty.Value);
                specification = specification?.And(difficultySpec) ?? difficultySpec;
            }

            if (query.MuscleGroups != null && query.MuscleGroups.Any())
            {
                var muscleGroups = ConvertMuscleGroupsList(query.MuscleGroups);
                var muscleGroupSpec = new ExerciseByMuscleGroupSpecification(muscleGroups);
                specification = specification?.And(muscleGroupSpec) ?? muscleGroupSpec;
            }

            if (query.RequiresEquipment.HasValue)
            {
                var equipmentSpec = new ExerciseRequiresEquipmentSpecification(query.RequiresEquipment.Value);
                specification = specification?.And(equipmentSpec) ?? equipmentSpec;
            }

            // Default to active exercises only unless explicitly specified
            if (!query.IsActive.HasValue || query.IsActive.Value)
            {
                var activeSpec = new ActiveExerciseSpecification();
                specification = specification?.And(activeSpec) ?? activeSpec;
            }

            return specification ?? new ActiveExerciseSpecification();
        }

        private static MuscleGroup ConvertMuscleGroupsList(List<string> muscleGroups)
        {
            if (muscleGroups == null || !muscleGroups.Any())
                return MuscleGroup.NONE;

            var result = MuscleGroup.NONE;
            foreach (var mg in muscleGroups)
            {
                if (Enum.TryParse<MuscleGroup>(mg, true, out var parsed))
                    result |= parsed;
            }

            return result;
        }
    }
}

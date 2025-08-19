using FitnessApp.Modules.Exercises.Application.DTOs.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;

namespace FitnessApp.Modules.Exercises.Application.Services;

public class MuscleGroupService : IMuscleGroupService
{
    private readonly IMuscleGroupRepository _muscleGroupRepository;
    private readonly IExerciseMapper _exerciseMapper;

    public MuscleGroupService(
        IMuscleGroupRepository muscleGroupRepository,
        IExerciseMapper exerciseMapper)
    {
        _muscleGroupRepository = muscleGroupRepository ?? throw new ArgumentNullException(nameof(muscleGroupRepository));
        _exerciseMapper = exerciseMapper ?? throw new ArgumentNullException(nameof(exerciseMapper));
    }

    public async Task<MuscleGroupResponse> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A valid ID must be provided.", nameof(id));
        }

        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
        if (muscleGroup == null)
            return null;

        var relatedGroups = await _muscleGroupRepository.GetRelatedMuscleGroupsAsync(id);
        var response = new MuscleGroupResponse
        {
            Id = muscleGroup.Id,
            Name = muscleGroup.Name,
            Description = muscleGroup.Description,
            RelatedMuscleGroups = _exerciseMapper.MapToMuscleGroupList(relatedGroups)
        };
        
        return response;
    }

    public async Task<IEnumerable<MuscleGroupResponse>> GetAllAsync()
    {
        var muscleGroups = await _muscleGroupRepository.GetAllAsync();
        return _exerciseMapper.MapToMuscleGroupList(muscleGroups);
    }

    public async Task<Guid> CreateAsync(CreateMuscleGroupRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null");
        }
        // Validate request
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Muscle group name is required");
        }
            
        // Check if muscle group with same name already exists
        var existingGroup = await _muscleGroupRepository.GetByNameAsync(request.Name);
        if (existingGroup != null)
        {
            throw new InvalidOperationException($"Muscle group with name '{request.Name}' already exists");
        }
            
        // Create new muscle group
        var muscleGroup = new MuscleGroup(
            request.Name,
            request.Description ?? string.Empty,
            "General" // Default body part - you may want to add this to the request DTO
        );
        
        await _muscleGroupRepository.AddAsync(muscleGroup);
        
        return muscleGroup.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateMuscleGroupRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null");
        }
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Muscle group with ID {id} not found");
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != muscleGroup.Name)
        {
            var existingGroup = await _muscleGroupRepository.GetByNameAsync(request.Name);
            if (existingGroup != null && existingGroup.Id != id)
                throw new InvalidOperationException($"Muscle group with name '{request.Name}' already exists");
        }
        
        // Update properties
        muscleGroup.Update(
            request.Name ?? muscleGroup.Name,
            request.Description ?? muscleGroup.Description,
            muscleGroup.BodyPart // Keep existing body part
        );
        
        await _muscleGroupRepository.UpdateAsync(muscleGroup);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A valid ID must be provided.", nameof(id));
        }
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Muscle group with ID {id} not found");
        await _muscleGroupRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ExerciseResponse>> GetExercisesByMuscleGroupIdAsync(Guid muscleGroupId, bool? isPrimary = null)
    {
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(muscleGroupId) ?? throw new KeyNotFoundException($"Muscle group with ID {muscleGroupId} not found");

        // Filter exercises based on isPrimary flag if provided
        var exercises = muscleGroup.Exercises
            .Where(e => !isPrimary.HasValue || e.IsPrimary == isPrimary.Value)
            .Select(e => e.Exercise);
            
        return _exerciseMapper.MapToExerciseList(exercises);
    }

    public async Task<IEnumerable<MuscleGroupResponse>> GetRelatedMuscleGroupsAsync(Guid muscleGroupId)
    {
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(muscleGroupId) ?? throw new KeyNotFoundException($"Muscle group with ID {muscleGroupId} not found");
        var relatedGroups = await _muscleGroupRepository.GetRelatedMuscleGroupsAsync(muscleGroupId);
        return _exerciseMapper.MapToMuscleGroupList(relatedGroups);
    }
}
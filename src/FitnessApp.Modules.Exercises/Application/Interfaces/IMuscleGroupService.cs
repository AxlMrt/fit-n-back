using FitnessApp.Modules.Exercises.Application.DTOs.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;

namespace FitnessApp.Modules.Exercises.Application.Interfaces;

public interface IMuscleGroupService
{
    Task<MuscleGroupResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<MuscleGroupResponse>> GetAllAsync();
    Task<Guid> CreateAsync(CreateMuscleGroupRequest muscleGroupDto);
    Task UpdateAsync(Guid id, UpdateMuscleGroupRequest muscleGroupDto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<ExerciseResponse>> GetExercisesByMuscleGroupIdAsync(Guid muscleGroupId, bool? isPrimary = null);
    Task<IEnumerable<MuscleGroupResponse>> GetRelatedMuscleGroupsAsync(Guid muscleGroupId);
}
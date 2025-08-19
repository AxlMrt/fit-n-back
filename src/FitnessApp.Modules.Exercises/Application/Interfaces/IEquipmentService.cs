using FitnessApp.Modules.Exercises.Application.DTOs.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;

namespace FitnessApp.Modules.Exercises.Application.Interfaces;

public interface IEquipmentService
{
    Task<EquipmentResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<EquipmentResponse>> GetAllAsync();
    Task<Guid> CreateAsync(CreateEquipmentRequest equipmentDto);
    Task UpdateAsync(Guid id, UpdateEquipmentRequest equipmentDto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<ExerciseResponse>> GetExercisesByEquipmentIdAsync(Guid equipmentId);
}
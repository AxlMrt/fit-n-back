using FitnessApp.Modules.Exercises.Application.Dtos.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;

namespace FitnessApp.Modules.Exercises.Application.Interfaces;

public interface IExerciseService
{
    Task<ExerciseResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<ExerciseResponse>> GetAllAsync();
    Task<IEnumerable<ExerciseResponse>> SearchAsync(ExerciseSearchRequest searchParams);
    Task<Guid> CreateAsync(CreateExerciseRequest exerciseDto);
    Task UpdateAsync(Guid id, UpdateExerciseRequest exerciseDto);
    Task DeleteAsync(Guid id);
}
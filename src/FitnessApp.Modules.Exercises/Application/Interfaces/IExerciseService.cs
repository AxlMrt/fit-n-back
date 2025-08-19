using FitnessApp.Modules.Exercises.Application.DTOs;

namespace FitnessApp.Modules.Exercises.Application.Interfaces
{
    public interface IExerciseService
    {
        Task<ExerciseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ExerciseDto>> GetAllAsync();
        Task<ExerciseDto> CreateAsync(CreateExerciseDto dto);
        Task<ExerciseDto?> UpdateAsync(Guid id, ExerciseDto dto);
        Task DeleteAsync(Guid id);
    }
}

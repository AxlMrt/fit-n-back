using FitnessApp.Modules.Exercises.Application.DTOs;

namespace FitnessApp.Modules.Exercises.Application.Interfaces
{
    public interface IExerciseService
    {
        Task<ExerciseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ExerciseDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<PagedResultDto<ExerciseListDto>> GetPagedAsync(ExerciseQueryDto query, CancellationToken cancellationToken = default);
        Task<IEnumerable<ExerciseListDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<ExerciseDto> CreateAsync(CreateExerciseDto dto, CancellationToken cancellationToken = default);
        Task<ExerciseDto?> UpdateAsync(Guid id, UpdateExerciseDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<ExerciseListDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    }
}

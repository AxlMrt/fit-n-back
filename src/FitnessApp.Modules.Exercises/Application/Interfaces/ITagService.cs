using FitnessApp.Modules.Exercises.Application.DTOs.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;

namespace FitnessApp.Modules.Exercises.Application.Interfaces;

public interface ITagService
{
    Task<TagResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<TagResponse>> GetAllAsync();
    Task<IEnumerable<TagResponse>> SearchAsync(string searchTerm);
    Task<Guid> CreateAsync(CreateTagRequest tagDto);
    Task UpdateAsync(Guid id, UpdateTagRequest tagDto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<ExerciseResponse>> GetExercisesByTagIdAsync(Guid tagId);
}
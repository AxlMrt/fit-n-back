using FitnessApp.Modules.Exercises.Application.DTOs.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;

namespace FitnessApp.Modules.Exercises.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IExerciseMapper _exerciseMapper;

    public TagService(ITagRepository tagRepository, IExerciseMapper exerciseMapper)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _exerciseMapper = exerciseMapper ?? throw new ArgumentNullException(nameof(exerciseMapper));
    }

    public async Task<TagResponse> GetByIdAsync(Guid id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        return tag != null ? _exerciseMapper.MapToTag(tag) : null;
    }

    public async Task<IEnumerable<TagResponse>> GetAllAsync()
    {
        var tags = await _tagRepository.GetAllAsync();
        return tags.Select(_exerciseMapper.MapToTag);
    }

    public async Task<IEnumerable<TagResponse>> SearchAsync(string searchTerm)
    {
        var tags = await _tagRepository.SearchAsync(searchTerm);
        return tags.Select(_exerciseMapper.MapToTag);
    }

    public async Task<Guid> CreateAsync(CreateTagRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Check for duplicate tag name
        var existingTag = await _tagRepository.GetByNameAsync(request.Name);
        if (existingTag != null)
            throw new InvalidOperationException($"A tag with the name '{request.Name}' already exists.");

        var tag = new Tag(request.Name, request.Description);
        await _tagRepository.AddAsync(tag);

        return tag.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateTagRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var tag = await _tagRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Tag with ID {id} not found.");

        // Check if the new name already exists but belongs to a different tag
        if (tag.Name != request.Name)
        {
            var existingTag = await _tagRepository.GetByNameAsync(request.Name);
            if (existingTag != null && existingTag.Id != id)
                throw new InvalidOperationException($"A tag with the name '{request.Name}' already exists.");
        }

        tag.Update(request.Name, request.Description);
        await _tagRepository.UpdateAsync(tag);
    }

    public async Task DeleteAsync(Guid id)
    {
        var tag = await _tagRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Tag with ID {id} not found.");
        await _tagRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ExerciseResponse>> GetExercisesByTagIdAsync(Guid tagId)
    {
        var tag = await _tagRepository.GetByIdAsync(tagId) ?? throw new KeyNotFoundException($"Tag with ID {tagId} not found.");

        // Note: This implementation assumes that the ExerciseTags navigation property
        // is loaded with their related Exercise entities.
        // In a real-world scenario, you would likely need to enhance the repository
        // to provide a dedicated method for this query.
        if (tag.ExerciseTags != null && tag.ExerciseTags.Any() && tag.ExerciseTags.First().Exercise != null)
        {
            var exercises = tag.ExerciseTags
                .Select(et => et.Exercise)
                .ToList();

            return _exerciseMapper.MapToExerciseList(exercises);
        }

        return Enumerable.Empty<ExerciseResponse>();
    }
}
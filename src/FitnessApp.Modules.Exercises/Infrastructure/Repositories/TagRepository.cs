using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Exercises.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ExercisesDbContext _dbContext;

    public TagRepository(ExercisesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tag?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Tags.FindAsync(id);
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _dbContext.Tags.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<IEnumerable<Tag>> SearchAsync(string searchTerm)
    {
        return await _dbContext.Tags
            .Where(t => t.Name.Contains(searchTerm))
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _dbContext.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task AddAsync(Tag tag)
    {
        await _dbContext.Tags.AddAsync(tag);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Tag tag)
    {
        _dbContext.Tags.Update(tag);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var tag = await GetByIdAsync(id);
        if (tag != null)
        {
            _dbContext.Tags.Remove(tag);
            await _dbContext.SaveChangesAsync();
        }
    }
}
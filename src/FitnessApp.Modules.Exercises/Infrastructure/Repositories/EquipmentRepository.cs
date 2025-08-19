
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Exercises.Infrastructure.Repositories;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly ExercisesDbContext _dbContext;

    public EquipmentRepository(ExercisesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Equipment?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Equipment.FindAsync(id);
    }

    public async Task<IEnumerable<Equipment>> GetAllAsync()
    {
        return await _dbContext.Equipment.OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<Equipment?> GetByNameAsync(string name)
    {
        return await _dbContext.Equipment
            .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());
    }

    public async Task AddAsync(Equipment equipment)
    {
        await _dbContext.Equipment.AddAsync(equipment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Equipment equipment)
    {
        _dbContext.Equipment.Update(equipment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var equipment = await GetByIdAsync(id);
        if (equipment != null)
        {
            _dbContext.Equipment.Remove(equipment);
            await _dbContext.SaveChangesAsync();
        }
    }
}
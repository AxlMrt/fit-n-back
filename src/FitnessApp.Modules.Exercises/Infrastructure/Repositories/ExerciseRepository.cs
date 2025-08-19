using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Exercises.Infrastructure.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly ExercisesDbContext _context;

        public ExerciseRepository(ExercisesDbContext context)
        {
            _context = context;
        }

        public async Task<Exercise?> GetByIdAsync(Guid id)
        {
            return await _context.Exercises.FindAsync(id);
        }

        public async Task<IEnumerable<Exercise>> GetAllAsync()
        {
            return await _context.Exercises.OrderBy(e => e.Name).ToListAsync();
        }

        public async Task AddAsync(Exercise entity)
        {
            await _context.Exercises.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Exercise entity)
        {
            _context.Exercises.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Exercise entity)
        {
            _context.Exercises.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}

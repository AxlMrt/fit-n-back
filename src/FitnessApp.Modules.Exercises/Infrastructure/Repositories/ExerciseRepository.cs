using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Domain.Specifications;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FitnessApp.Modules.Exercises.Infrastructure.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly ExercisesDbContext _context;

        public ExerciseRepository(ExercisesDbContext context)
        {
            _context = context;
        }

        public async Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Exercise?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        public async Task<IEnumerable<Exercise>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Exercise>> GetBySpecificationAsync(
            Specification<Exercise> specification, 
            CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .Where(specification.ToExpression())
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Exercise> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Specification<Exercise>? specification = null,
            string sortBy = "Name", 
            bool sortDescending = false, 
            CancellationToken cancellationToken = default)
        {
            var query = _context.Exercises.AsQueryable();

            if (specification != null)
            {
                query = query.Where(specification.ToExpression());
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, sortBy, sortDescending);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<int> CountAsync(Specification<Exercise>? specification = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Exercises.AsQueryable();

            if (specification != null)
            {
                query = query.Where(specification.ToExpression());
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises.AnyAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Exercises.Where(e => e.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task AddAsync(Exercise entity, CancellationToken cancellationToken = default)
        {
            await _context.Exercises.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Exercise entity, CancellationToken cancellationToken = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Exercise entity, CancellationToken cancellationToken = default)
        {
            _context.Exercises.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static IQueryable<Exercise> ApplySorting(IQueryable<Exercise> query, string sortBy, bool sortDescending)
        {
            Expression<Func<Exercise, object>> selector = sortBy.ToLowerInvariant() switch
            {
                "name" => e => e.Name,
                "type" => e => e.Type,
                "difficulty" => e => e.Difficulty,
                "createdat" => e => e.CreatedAt,
                "updatedat" => e => e.UpdatedAt ?? e.CreatedAt,
                _ => e => e.Name
            };

            return sortDescending
                ? query.OrderByDescending(selector)
                : query.OrderBy(selector);
        }
    }
}

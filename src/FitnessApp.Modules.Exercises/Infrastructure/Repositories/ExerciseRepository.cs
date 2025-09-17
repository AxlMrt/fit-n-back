using AutoMapper;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FitnessApp.Modules.Exercises.Infrastructure.Repositories;
public class ExerciseRepository : IExerciseRepository
{
    private readonly ExercisesDbContext _context;
    private readonly IMapper _mapper;

    public ExerciseRepository(ExercisesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

    public async Task<IEnumerable<Exercise>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Exercises.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }
        
        return await query
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Exercise>> SearchByNameAsync(string searchTerm, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Exercises
            .Where(e => e.Name.ToLower().Contains(searchTerm.ToLower()));
            
        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }
        
        return await query
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Exercise> Items, int TotalCount)> GetPagedAsync(
        ExerciseQueryDto query, 
        CancellationToken cancellationToken = default)
    {
        var queryable = _context.Exercises.AsQueryable();

        queryable = ApplyFilters(queryable, query);

        var totalCount = await queryable.CountAsync(cancellationToken);

        queryable = ApplySorting(queryable, query.SortBy, query.SortDescending);

        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
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

    private IQueryable<Exercise> ApplyFilters(IQueryable<Exercise> query, ExerciseQueryDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.NameFilter))
        {
            query = query.Where(e => e.Name.ToLower().Contains(filters.NameFilter.ToLower()));
        }

        if (filters.Type.HasValue)
        {
            query = query.Where(e => e.Type == filters.Type.Value);
        }

        if (filters.Difficulty.HasValue)
        {
            query = query.Where(e => e.Difficulty == filters.Difficulty.Value);
        }

        if (filters.MuscleGroups != null && filters.MuscleGroups.Any())
        {
            var muscleGroups = _mapper.Map<MuscleGroup>(filters.MuscleGroups);
            query = query.Where(e => e.MuscleGroups.HasFlag(muscleGroups));
        }

        if (filters.RequiresEquipment.HasValue)
        {
            if (filters.RequiresEquipment.Value)
            {
                query = query.Where(e => e.Equipment != Equipment.None);
            }
            else
            {
                query = query.Where(e => e.Equipment == Equipment.None);
            }
        }

        if (!filters.IsActive.HasValue || filters.IsActive.Value)
        {
            query = query.Where(e => e.IsActive);
        }

        return query;
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

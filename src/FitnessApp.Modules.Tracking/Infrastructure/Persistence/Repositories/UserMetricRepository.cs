using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.Modules.Tracking.Domain.Repositories;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for UserMetric entity
/// </summary>
internal class UserMetricRepository : IUserMetricRepository
{
    private readonly TrackingDbContext _context;

    public UserMetricRepository(TrackingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserMetrics
            .FirstOrDefaultAsync(um => um.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<UserMetric>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserMetrics
            .Where(um => um.UserId == userId)
            .OrderByDescending(um => um.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserMetric>> GetByUserAndTypeAsync(Guid userId, UserMetricType metricType, CancellationToken cancellationToken = default)
    {
        return await _context.UserMetrics
            .Where(um => um.UserId == userId && um.MetricType == metricType)
            .OrderByDescending(um => um.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserMetric>> GetInPeriodAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.UserMetrics
            .Where(um => um.UserId == userId &&
                        um.RecordedAt.Date >= startDate.Date &&
                        um.RecordedAt.Date <= endDate.Date)
            .OrderByDescending(um => um.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserMetric>> GetLatestByTypeAsync(Guid userId, UserMetricType metricType, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.UserMetrics
            .Where(um => um.UserId == userId && um.MetricType == metricType)
            .OrderByDescending(um => um.RecordedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserMetric?> GetLatestMetricAsync(Guid userId, UserMetricType metricType, CancellationToken cancellationToken = default)
    {
        return await _context.UserMetrics
            .Where(um => um.UserId == userId && um.MetricType == metricType)
            .OrderByDescending(um => um.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<double?> GetLatestValueAsync(Guid userId, UserMetricType metricType, CancellationToken cancellationToken = default)
    {
        var metric = await GetLatestMetricAsync(userId, metricType, cancellationToken);
        return metric?.Value;
    }

    public async Task AddAsync(UserMetric metric, CancellationToken cancellationToken = default)
    {
        await _context.UserMetrics.AddAsync(metric, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserMetric metric, CancellationToken cancellationToken = default)
    {
        _context.UserMetrics.Update(metric);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserMetric metric, CancellationToken cancellationToken = default)
    {
        _context.UserMetrics.Remove(metric);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

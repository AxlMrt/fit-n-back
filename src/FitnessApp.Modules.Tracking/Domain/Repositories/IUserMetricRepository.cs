using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Domain.Repositories;

/// <summary>
/// Repository interface for UserMetric entity
/// </summary>
public interface IUserMetricRepository
{
    Task<UserMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserMetric>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserMetric>> GetByUserAndTypeAsync(Guid userId, UserMetricType metricType, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserMetric>> GetInPeriodAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserMetric>> GetLatestByTypeAsync(Guid userId, UserMetricType metricType, int count = 10, CancellationToken cancellationToken = default);
    Task<UserMetric?> GetLatestMetricAsync(Guid userId, UserMetricType metricType, CancellationToken cancellationToken = default);
    Task<double?> GetLatestValueAsync(Guid userId, UserMetricType metricType, CancellationToken cancellationToken = default);
    Task AddAsync(UserMetric metric, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserMetric metric, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserMetric metric, CancellationToken cancellationToken = default);
}

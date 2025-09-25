using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Entities;

public class Subscription
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public SubscriptionLevel Level { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    public UserProfile UserProfile { get; private set; } = null!;

    private Subscription() { }

    public Subscription(UserProfile userProfile, SubscriptionLevel level, DateTime startDate, DateTime endDate)
    {
        Id = Guid.NewGuid();
        UserProfile = userProfile;
        Level = level;
        
        StartDate = startDate.Kind == DateTimeKind.Utc ? startDate : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        EndDate = endDate.Kind == DateTimeKind.Utc ? endDate : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
    }

    public void UpdateSubscription(SubscriptionLevel level, DateTime endDate)
    {
        Level = level;
        EndDate = endDate.Kind == DateTimeKind.Utc ? endDate : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
    }

    public void Renew(DateTime newEndDate)
    {
        EndDate = newEndDate.Kind == DateTimeKind.Utc ? newEndDate : DateTime.SpecifyKind(newEndDate, DateTimeKind.Utc);
    }

    public void Cancel()
    {
        EndDate = DateTime.UtcNow;
    }
}

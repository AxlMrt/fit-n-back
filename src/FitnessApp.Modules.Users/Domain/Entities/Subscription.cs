namespace FitnessApp.Modules.Users.Domain.Entities;

public class Subscription
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Plan { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    private Subscription() { }

    public Subscription(Guid userId, string plan, DateTime startDate, DateTime endDate)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Plan = plan;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = DateTime.UtcNow <= endDate;
    }

    public void Renew(DateTime newEndDate)
    {
        EndDate = newEndDate;
        IsActive = true;
    }

    public void Cancel()
    {
        IsActive = false;
    }
}
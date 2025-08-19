namespace FitnessApp.Modules.Users.Domain.Entities;

public class Preference
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;

    private Preference() { }

    public Preference(Guid userId, string category, string key, string value)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Category = category;
        Key = key;
        Value = value;
    }

    public void UpdateValue(string value)
    {
        Value = value;
    }
}
namespace FitnessApp.Modules.Users.Application.DTOs.Responses;

public record UserGoalsResponse(
    IEnumerable<UserGoalItem> Goals
);

public record UserGoalItem(
    string Type,
    string Target,
    DateTime? DueDate
);

namespace FitnessApp.Modules.Users.Application.DTOs.Responses;

public record UserStatsResponse(
    int Workouts,
    int Minutes,
    int Calories
);

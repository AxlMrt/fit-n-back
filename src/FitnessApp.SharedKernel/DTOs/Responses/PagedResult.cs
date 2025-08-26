namespace FitnessApp.SharedKernel.DTOs.Responses;

public sealed record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
) where T : class;
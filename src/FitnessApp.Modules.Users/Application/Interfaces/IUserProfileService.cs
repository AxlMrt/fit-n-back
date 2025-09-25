using FitnessApp.SharedKernel.DTOs.Users.Requests;
using FitnessApp.SharedKernel.DTOs.Users.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Application.Interfaces;

/// <summary>
/// Service interface for user profile operations.
/// </summary>
public interface IUserProfileService
{
    // Profile Management
    Task<UserProfileResponse?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileSummaryResponse?> GetUserProfileSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> CreateUserProfileAsync(Guid userId, CreateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> UpdatePersonalInfoAsync(Guid userId, UpdatePersonalInfoRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> UpdatePhysicalMeasurementsAsync(Guid userId, UpdatePhysicalMeasurementsRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> UpdateFitnessProfileAsync(Guid userId, UpdateFitnessProfileRequest request, CancellationToken cancellationToken = default);
    Task<ProfileOperationResponse> DeleteUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    // Subscription Management
    Task<SubscriptionResponse?> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SubscriptionResponse> UpdateSubscriptionAsync(Guid userId, UpdateSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<ProfileOperationResponse> CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SubscriptionResponse> RenewSubscriptionAsync(Guid userId, DateTime newEndDate, CancellationToken cancellationToken = default);

    // Utility Methods
    Task<bool> UserProfileExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}
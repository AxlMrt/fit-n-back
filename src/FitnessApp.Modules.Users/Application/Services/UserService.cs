using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.Modules.Users.Application.DTOs.Responses;
using FitnessApp.Modules.Users.Application.Mapping;
using FitnessApp.Modules.Users.Application.Exceptions;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.Modules.Users.Domain.Exceptions;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Authorization.Enums;

namespace FitnessApp.Modules.Users.Application.Services;

/// <summary>
/// Application service for user management operations.
/// Orchestrates business logic and coordinates between domain and infrastructure layers.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateUserRequest> _createUserValidator;
    private readonly IValidator<UpdateUserProfileRequest> _updateProfileValidator;
    private readonly IValidator<UpdateUserEmailRequest> _updateEmailValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<UserQueryRequest> _queryValidator;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IValidator<CreateUserRequest> createUserValidator,
        IValidator<UpdateUserProfileRequest> updateProfileValidator,
        IValidator<UpdateUserEmailRequest> updateEmailValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator,
        IValidator<UserQueryRequest> queryValidator,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _createUserValidator = createUserValidator ?? throw new ArgumentNullException(nameof(createUserValidator));
        _updateProfileValidator = updateProfileValidator ?? throw new ArgumentNullException(nameof(updateProfileValidator));
        _updateEmailValidator = updateEmailValidator ?? throw new ArgumentNullException(nameof(updateEmailValidator));
        _changePasswordValidator = changePasswordValidator ?? throw new ArgumentNullException(nameof(changePasswordValidator));
        _queryValidator = queryValidator ?? throw new ArgumentNullException(nameof(queryValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("Invalid user ID");

        _logger.LogDebug("Getting user by ID: {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.MapToDto();
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email is required");

        _logger.LogDebug("Getting user by email: {Email}", email);
        
        var user = await _userRepository.GetByEmailAsync(email);
        return user?.MapToDto();
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username is required");

        _logger.LogDebug("Getting user by username: {Username}", username);
        
        var user = await _userRepository.GetByUsernameAsync(username);
        return user?.MapToDto();
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        _logger.LogDebug("Creating new user with email: {Email}", request.Email);
        
        // Validate request
        await _createUserValidator.ValidateAndThrowAsync(request);

        // Check for duplicates
        if (await _userRepository.ExistsWithEmailAsync(request.Email))
        {
            throw new UserDomainException($"User with email '{request.Email}' already exists");
        }

        if (await _userRepository.ExistsWithUsernameAsync(request.Username))
        {
            throw new UserDomainException($"User with username '{request.Username}' already exists");
        }

        try
        {
            // Create value objects
            var email = Email.Create(request.Email);
            var username = Username.Create(request.Username);

            // Create user entity
            var user = new User(email, username);

            // Set initial profile if provided
            if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName))
            {
                var name = FullName.Create(request.FirstName, request.LastName);
                user.UpdatePersonalInfo(name, null, null);
            }

            // Set password hash (in real implementation, this would be hashed)
            user.SetPasswordHash(HashPassword(request.Password));

            // Save to repository
            var savedUser = await _userRepository.AddAsync(user);
            
            _logger.LogInformation("User created successfully with ID: {UserId}", savedUser.Id);
            
            return savedUser.MapToDto();
        }
        catch (Exception ex) when (!(ex is ValidationException || ex is UserDomainException))
        {
            _logger.LogError(ex, "Error creating user with email: {Email}", request.Email);
            throw new ApplicationException("An error occurred while creating the user", ex);
        }
    }

    public async Task<UserDto> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request)
    {
        _logger.LogDebug("Updating profile for user: {UserId}", userId);
        
        // Validate request
        await _updateProfileValidator.ValidateAndThrowAsync(request);

        var user = await GetUserOrThrowAsync(userId);

        try
        {
            // Update personal information
            var name = FullName.Create(request.FirstName, request.LastName);
            var dateOfBirth = request.DateOfBirth.HasValue ? DateOfBirth.Create(request.DateOfBirth.Value) : null;
            
            user.UpdatePersonalInfo(name, dateOfBirth, request.Gender);

            // Update physical measurements
            if (request.HeightCm.HasValue || request.WeightKg.HasValue)
            {
                var measurements = PhysicalMeasurements.Create(request.HeightCm, request.WeightKg);
                user.UpdatePhysicalMeasurements(measurements);
            }

            // Update fitness profile
            user.UpdateFitnessProfile(request.FitnessLevel, request.PrimaryFitnessGoal);

            // Save changes
            var updatedUser = await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
            
            return updatedUser.MapToDto();
        }
        catch (Exception ex) when (!(ex is ValidationException || ex is UserDomainException))
        {
            _logger.LogError(ex, "Error updating profile for user: {UserId}", userId);
            throw new ApplicationException("An error occurred while updating the user profile", ex);
        }
    }

    public async Task<UserDto> UpdateUserEmailAsync(Guid userId, UpdateUserEmailRequest request)
    {
        _logger.LogDebug("Updating email for user: {UserId}", userId);
        
        // Validate request
        await _updateEmailValidator.ValidateAndThrowAsync(request);

        var user = await GetUserOrThrowAsync(userId);

        // Check if new email already exists
        if (await _userRepository.ExistsWithEmailAsync(request.NewEmail))
        {
            throw new UserDomainException($"User with email '{request.NewEmail}' already exists");
        }

        try
        {
            var newEmail = Email.Create(request.NewEmail);
            user.UpdateEmail(newEmail);

            var updatedUser = await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("Email updated successfully for user: {UserId}", userId);
            
            return updatedUser.MapToDto();
        }
        catch (Exception ex) when (!(ex is ValidationException || ex is UserDomainException))
        {
            _logger.LogError(ex, "Error updating email for user: {UserId}", userId);
            throw new ApplicationException("An error occurred while updating the user email", ex);
        }
    }

    public async Task<PagedUserResult> GetUsersAsync(UserQueryRequest request)
    {
        _logger.LogDebug("Getting users with query: {@Query}", request);
        
        // Validate request
        await _queryValidator.ValidateAndThrowAsync(request);

        try
        {
            var (users, totalCount) = await _userRepository.GetPagedAsync(
                request.EmailFilter,
                request.NameFilter,
                request.GenderFilter,
                request.FitnessLevelFilter,
                request.IsActiveFilter,
                request.SortBy,
                request.SortDescending,
                request.Page,
                request.PageSize);

            return users.MapToPagedResult(totalCount, request.Page, request.PageSize);
        }
        catch (Exception ex) when (!(ex is ValidationException))
        {
            _logger.LogError(ex, "Error getting users with query: {@Query}", request);
            throw new ApplicationException("An error occurred while retrieving users", ex);
        }
    }

    public async Task<UserStatsDto> GetUserStatsAsync()
    {
        _logger.LogDebug("Getting user statistics");
        
        try
        {
            var users = await _userRepository.GetAllAsync();
            return users.MapToStatsDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            throw new ApplicationException("An error occurred while retrieving user statistics", ex);
        }
    }

    public async Task<bool> ExistsWithEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email is required");

        return await _userRepository.ExistsWithEmailAsync(email);
    }

    public async Task<bool> ExistsWithUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username is required");

        return await _userRepository.ExistsWithUsernameAsync(username);
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        _logger.LogDebug("Deactivating user: {UserId}", userId);
        
        var user = await GetUserOrThrowAsync(userId);

        try
        {
            user.Deactivate();
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("User deactivated successfully: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
            throw new ApplicationException("An error occurred while deactivating the user", ex);
        }
    }

    public async Task<bool> ReactivateUserAsync(Guid userId)
    {
        _logger.LogDebug("Reactivating user: {UserId}", userId);
        
        var user = await GetUserOrThrowAsync(userId);

        try
        {
            user.Reactivate();
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("User reactivated successfully: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating user: {UserId}", userId);
            throw new ApplicationException("An error occurred while reactivating the user", ex);
        }
    }

    public async Task UpdateUserPreferencesAsync(Guid userId, UpdatePreferencesRequest request)
    {
        _logger.LogDebug("Updating preferences for user: {UserId}", userId);
        
        try
        {
            // Verify user exists
            if (!await _userRepository.ExistsAsync(userId))
            {
                throw new NotFoundException($"User with ID '{userId}' not found");
            }

            // Use the existing UpsertPreferencesAsync method which works correctly
            var preferences = request.Preferences
                .SelectMany(category => category.Value.Select(pref => 
                    new Preference(userId, category.Key, pref.Key, pref.Value)))
                .ToList();

            await _userRepository.UpsertPreferencesAsync(userId, preferences);
            
            _logger.LogInformation("Preferences updated successfully for user: {UserId}", userId);
        }
        catch (Exception ex) when (!(ex is ValidationException || ex is UserDomainException || ex is NotFoundException))
        {
            _logger.LogError(ex, "Error updating preferences for user: {UserId}", userId);
            throw new ApplicationException("An error occurred while updating user preferences", ex);
        }
    }

    /// <summary>
    /// Updates a user's role (admin only operation)
    /// </summary>
    public async Task<UserDto> UpdateUserRoleAsync(Guid userId, Role role)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        user.SetRole(role);
        var updatedUser = await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Updated role for user {UserId} to {Role}", userId, role);
        return updatedUser.MapToDto();
    }

    // Private helper methods
    private async Task<User> GetUserOrThrowAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID '{userId}' not found");
        }
        return user;
    }

    private static string HashPassword(string password)
    {
        // In a real implementation, use a proper password hashing library like BCrypt
        // This is just a placeholder
        return $"HASHED_{password}_{DateTime.UtcNow.Ticks}";
    }
}
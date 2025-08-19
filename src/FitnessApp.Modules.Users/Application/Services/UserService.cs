using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.Modules.Users.Application.DTOs.Responses;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Application.Mappers;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.SharedKernel.Interfaces;

namespace FitnessApp.Modules.Users.Application.Services;

public class UserService : IUserService
{
    private readonly IValidationService _validationService;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository,
                       IValidationService validationService)
    {
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user == null ? null : UserMapper.MapToUserDto(user);
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user == null ? null : UserMapper.MapToUserDto(user);
    }

    public async Task<UserResponse> UpdateUserProfileAsync(Guid userId, UpdateProfileRequest profileDto)
    {
        await _validationService.ValidateAsync(profileDto);
        User user = await _userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");

        user.Profile.UpdatePersonalInfo(
            profileDto.FirstName,
            profileDto.LastName,
            profileDto.DateOfBirth,
            profileDto.Gender
        );

        user.Profile.UpdatePhysicalInfo(
            profileDto.Height,
            profileDto.Weight
        );

        user.Profile.UpdateFitnessInfo(
            profileDto.FitnessLevel,
            profileDto.FitnessGoal
        );

        await _userRepository.UpdateAsync(user);

        return UserMapper.MapToUserDto(user);
    }

    public async Task UpdatePreferencesAsync(Guid userId, PreferencesUpdateRequest request)
    {
        await _validationService.ValidateAsync(request);

        var existingPrefs = await _userRepository.GetPreferencesAsync(userId);
        var toUpsert = new List<Preference>();
        var toDelete = new List<(string Category, string Key)>();

        // Build maps for quick lookup
        var existingMap = existingPrefs.ToDictionary(p => (p.Category, p.Key));
        var incomingMap = request.Items.ToDictionary(i => (i.Category, i.Key));

        // Preferences to add or update
        foreach (var item in request.Items)
        {
            if (existingMap.TryGetValue((item.Category, item.Key), out var existing))
            {
                if (existing.Value != item.Value)
                {
                    existing.UpdateValue(item.Value);
                    toUpsert.Add(existing);
                }
            }
            else
            {
                toUpsert.Add(new Preference(userId, item.Category, item.Key, item.Value));
            }
        }

        // Preferences to delete (present in DB but not in request)
        foreach (var existing in existingPrefs)
        {
            if (!incomingMap.ContainsKey((existing.Category, existing.Key)))
            {
                toDelete.Add((existing.Category, existing.Key));
            }
        }

        if (toUpsert.Count > 0)
            await _userRepository.UpsertPreferencesAsync(userId, toUpsert);

        if (toDelete.Count > 0)
            await _userRepository.DeletePreferencesAsync(userId, toDelete);
    }

    public Task<UserGoalsResponse> GetGoalsAsync(Guid userId)
    {
        // Placeholder: integrate with Objectives module later
        return Task.FromResult(new UserGoalsResponse(Array.Empty<UserGoalItem>()));
    }

    public Task<UserStatsResponse> GetStatsAsync(Guid userId)
    {
        // Placeholder: aggregate from Tracking/Workouts modules later
        return Task.FromResult(new UserStatsResponse(0, 0, 0));
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        User user = await _userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");

        await _userRepository.DeleteAsync(userId);
        return true;
    }

    public Task<bool> IsServiceHealthyAsync()
    {
        return Task.FromResult(true);
    }
}


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
        User user = await _userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");

        foreach (var item in request.Items)
        {
            var existing = user.Preferences.FirstOrDefault(p => p.Category == item.Category && p.Key == item.Key);
            if (existing is null)
            {
                user.AddPreference(new Preference(user.Id, item.Category, item.Key, item.Value));
            }
            else
            {
                existing.UpdateValue(item.Value);
            }
        }

        await _userRepository.UpdateAsync(user);
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


using Xunit;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.Modules.Users.Domain.Enums;
using FitnessApp.Modules.Users.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;
using FitnessApp.Modules.Authorization.Enums;

namespace FitnessApp.Modules.Users.Tests.Domain.Entities;

public class UserTests
{
    private User CreateValidUser()
    {
        var email = Email.Create("test@example.com");
        var username = Username.Create("testuser");
        return new User(email, username);
    }

    [Fact]
    public void User_Constructor_ShouldCreateValidUser()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var username = Username.Create("testuser");

        // Act
        var user = new User(email, username);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(username, user.Username);
        Assert.Equal(Role.Athlete, user.Role);
        Assert.True(user.IsActive);
        Assert.False(user.EmailConfirmed);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.Null(user.UpdatedAt);
        Assert.NotNull(user.SecurityStamp);
        Assert.NotEmpty(user.SecurityStamp);
    }

    [Fact]
    public void User_Constructor_ShouldThrowException_WhenEmailIsNull()
    {
        // Arrange
        var username = Username.Create("testuser");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new User(null!, username));
    }

    [Fact]
    public void User_Constructor_ShouldThrowException_WhenUsernameIsNull()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new User(email, null!));
    }

    [Fact]
    public void SetPasswordHash_ShouldUpdatePasswordHash_WhenValidHash()
    {
        // Arrange
        var user = CreateValidUser();
        var passwordHash = "hashedPassword123";

        // Act
        user.SetPasswordHash(passwordHash);

        // Assert
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.NotNull(user.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SetPasswordHash_ShouldThrowException_WhenHashIsInvalid(string invalidHash)
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Throws<UserDomainException>(() => user.SetPasswordHash(invalidHash));
    }

    [Fact]
    public void ConfirmEmail_ShouldSetEmailConfirmedToTrue()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.ConfirmEmail();

        // Assert
        Assert.True(user.EmailConfirmed);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void UpdatePersonalInfo_ShouldUpdateUserInfo()
    {
        // Arrange
        var user = CreateValidUser();
        var name = FullName.Create("John", "Doe");
        var dateOfBirth = DateOfBirth.Create(new DateTime(1990, 1, 1));
        var gender = Gender.Male;

        // Act
        user.UpdatePersonalInfo(name, dateOfBirth, gender);

        // Assert
        Assert.Equal(name, user.Name);
        Assert.Equal(dateOfBirth, user.DateOfBirth);
        Assert.Equal(gender, user.Gender);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void UpdatePhysicalMeasurements_ShouldUpdateMeasurements()
    {
        // Arrange
        var user = CreateValidUser();
        var measurements = PhysicalMeasurements.Create(180, 75);

        // Act
        user.UpdatePhysicalMeasurements(measurements);

        // Assert
        Assert.Equal(measurements, user.PhysicalMeasurements);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void UpdateFitnessProfile_ShouldUpdateFitnessInfo()
    {
        // Arrange
        var user = CreateValidUser();
        var fitnessLevel = FitnessLevel.Advanced;
        var fitnessGoal = FitnessGoal.MUSCLE_GAIN;

        // Act
        user.UpdateFitnessProfile(fitnessLevel, fitnessGoal);

        // Assert
        Assert.Equal(fitnessLevel, user.FitnessLevel);
        Assert.Equal(fitnessGoal, user.PrimaryFitnessGoal);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void IncrementAccessFailedCount_ShouldIncrementCounter()
    {
        // Arrange
        var user = CreateValidUser();
        var initialCount = user.AccessFailedCount;

        // Act
        user.IncrementAccessFailedCount();

        // Assert
        Assert.Equal(initialCount + 1, user.AccessFailedCount);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void IncrementAccessFailedCount_ShouldLockAccount_WhenLimitReached()
    {
        // Arrange
        var user = CreateValidUser();

        // Act - increment 5 times to trigger lockout
        for (int i = 0; i < 5; i++)
        {
            user.IncrementAccessFailedCount();
        }

        // Assert
        Assert.True(user.IsLockedOut());
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.LockoutEnd > DateTime.UtcNow);
    }

    [Fact]
    public void ResetAccessFailedCount_ShouldResetCounter()
    {
        // Arrange
        var user = CreateValidUser();
        user.IncrementAccessFailedCount();
        user.IncrementAccessFailedCount();

        // Act
        user.ResetAccessFailedCount();

        // Assert
        Assert.Equal(0, user.AccessFailedCount);
    }

    [Fact]
    public void LockAccount_ShouldLockUser_WhenLockoutEnabled()
    {
        // Arrange
        var user = CreateValidUser();
        var duration = TimeSpan.FromMinutes(30);

        // Act
        user.LockAccount(duration);

        // Assert
        Assert.True(user.IsLockedOut());
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.LockoutEnd > DateTime.UtcNow);
    }

    [Fact]
    public void UnlockAccount_ShouldUnlockUser()
    {
        // Arrange
        var user = CreateValidUser();
        user.LockAccount(TimeSpan.FromMinutes(30));

        // Act
        user.UnlockAccount();

        // Assert
        Assert.False(user.IsLockedOut());
        Assert.Null(user.LockoutEnd);
        Assert.Equal(0, user.AccessFailedCount);
    }

    [Fact]
    public void AddOrUpdatePreference_ShouldAddNewPreference()
    {
        // Arrange
        var user = CreateValidUser();
        var category = "notifications";
        var key = "email";
        var value = "true";

        // Act
        user.AddOrUpdatePreference(category, key, value);

        // Assert
        Assert.Single(user.Preferences);
        var preference = user.Preferences.First();
        Assert.Equal(category, preference.Category);
        Assert.Equal(key, preference.Key);
        Assert.Equal(value, preference.Value);
    }

    [Fact]
    public void AddOrUpdatePreference_ShouldUpdateExistingPreference()
    {
        // Arrange
        var user = CreateValidUser();
        var category = "notifications";
        var key = "email";
        user.AddOrUpdatePreference(category, key, "false");

        // Act
        user.AddOrUpdatePreference(category, key, "true");

        // Assert
        Assert.Single(user.Preferences);
        var preference = user.Preferences.First();
        Assert.Equal("true", preference.Value);
    }

    [Fact]
    public void GetPreference_ShouldReturnValue_WhenPreferenceExists()
    {
        // Arrange
        var user = CreateValidUser();
        var category = "notifications";
        var key = "email";
        var value = "true";
        user.AddOrUpdatePreference(category, key, value);

        // Act
        var result = user.GetPreference(category, key);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetPreference_ShouldReturnNull_WhenPreferenceDoesNotExist()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var result = user.GetPreference("nonexistent", "key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RemovePreference_ShouldRemovePreference_WhenExists()
    {
        // Arrange
        var user = CreateValidUser();
        var category = "notifications";
        var key = "email";
        user.AddOrUpdatePreference(category, key, "true");

        // Act
        user.RemovePreference(category, key);

        // Assert
        Assert.Empty(user.Preferences);
    }

    [Fact]
    public void RegisterLogin_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var user = CreateValidUser();
        var beforeLogin = DateTime.UtcNow;

        // Act
        user.RegisterLogin();

        // Assert
        Assert.NotNull(user.LastLoginAt);
        Assert.True(user.LastLoginAt >= beforeLogin);
        Assert.Equal(0, user.AccessFailedCount); // Should reset failed count
    }

    [Fact]
    public void HasCompletedProfile_ShouldReturnTrue_WhenAllFieldsCompleted()
    {
        // Arrange
        var user = CreateValidUser();
        user.UpdatePersonalInfo(
            FullName.Create("John", "Doe"),
            DateOfBirth.Create(new DateTime(1990, 1, 1)),
            Gender.Male);
        user.UpdatePhysicalMeasurements(PhysicalMeasurements.Create(180, 75));
        user.UpdateFitnessProfile(FitnessLevel.Advanced, FitnessGoal.MUSCLE_GAIN);

        // Act
        var result = user.HasCompletedProfile();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasCompletedProfile_ShouldReturnFalse_WhenFieldsMissing()
    {
        // Arrange
        var user = CreateValidUser();
        // Only set some fields

        // Act
        var result = user.HasCompletedProfile();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void Reactivate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = CreateValidUser();
        user.Deactivate();

        // Act
        user.Reactivate();

        // Assert
        Assert.True(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AddOrUpdatePreference_ShouldThrowException_WhenCategoryIsInvalid(string invalidCategory)
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Throws<UserDomainException>(() => 
            user.AddOrUpdatePreference(invalidCategory, "key", "value"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AddOrUpdatePreference_ShouldThrowException_WhenKeyIsInvalid(string invalidKey)
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Throws<UserDomainException>(() => 
            user.AddOrUpdatePreference("category", invalidKey, "value"));
    }
}

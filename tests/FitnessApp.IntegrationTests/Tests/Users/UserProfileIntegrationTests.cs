using FluentAssertions;
using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.IntegrationTests.Helpers;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessApp.IntegrationTests.Tests.Users;

/// <summary>
/// Tests d'intégration pour l'entité UserProfile
/// </summary>
public class UserProfileIntegrationTests : IntegrationTestBase
{
    public UserProfileIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateUserProfile_ShouldPersistCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile(userId);

        // Act
        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        // Assert
        var savedProfile = await UsersContext.UserProfiles
            .FirstOrDefaultAsync(u => u.UserId == userId);

        savedProfile.Should().NotBeNull();
        savedProfile!.UserId.Should().Be(userId);
        savedProfile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        savedProfile.UpdatedAt.Should().BeNull();
        savedProfile.Name.Should().Be(FullName.Empty);
        savedProfile.DateOfBirth.Should().BeNull();
        savedProfile.Gender.Should().BeNull();
        savedProfile.PhysicalMeasurements.Should().Be(PhysicalMeasurements.Empty);
        savedProfile.FitnessLevel.Should().BeNull();
        savedProfile.PrimaryFitnessGoal.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePersonalInfo_ShouldModifyExistingData()
    {
        // Arrange
        var userProfile = TestDataBuilder.CreateUser()
            .WithName("Initial", "Name")
            .Build();

        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        var newName = FullName.Create("Updated", "Name");
        var dateOfBirth = DateOfBirth.Create(new DateTime(1990, 5, 15));
        var gender = Gender.Male;

        // Act
        userProfile.UpdatePersonalInfo(newName, dateOfBirth, gender);
        await UsersContext.SaveChangesAsync();

        // Assert
        await RefreshContextAsync(UsersContext);
        var updatedProfile = await UsersContext.UserProfiles
            .FirstAsync(u => u.UserId == userProfile.UserId);

        updatedProfile.Name.FirstName.Should().Be("Updated");
        updatedProfile.Name.LastName.Should().Be("Name");
        updatedProfile.DateOfBirth.Should().Be(dateOfBirth);
        updatedProfile.Gender.Should().Be(gender);
        updatedProfile.UpdatedAt.Should().NotBeNull();
        updatedProfile.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdatePhysicalMeasurements_ShouldPersistCorrectly()
    {
        // Arrange
        var userProfile = TestDataBuilder.CreateUser().Build();
        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        var measurements = PhysicalMeasurements.Create(175.5m, 70.2m);

        // Act
        userProfile.UpdatePhysicalMeasurements(measurements);
        await UsersContext.SaveChangesAsync();

        // Assert
        await RefreshContextAsync(UsersContext);
        var updatedProfile = await UsersContext.UserProfiles
            .FirstAsync(u => u.UserId == userProfile.UserId);

        updatedProfile.PhysicalMeasurements.HeightCm.Should().Be(175.5m);
        updatedProfile.PhysicalMeasurements.WeightKg.Should().Be(70.2m);
        updatedProfile.GetBMI().Should().BeApproximately(22.8m, 0.1m);
        updatedProfile.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateFitnessProfile_ShouldPersistChanges()
    {
        // Arrange
        var userProfile = TestDataBuilder.CreateUser().Build();
        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        var fitnessLevel = FitnessLevel.Enthousiast;
        var primaryGoal = FitnessGoal.Muscle_Gain;

        // Act
        userProfile.UpdateFitnessProfile(fitnessLevel, primaryGoal);
        await UsersContext.SaveChangesAsync();

        // Assert
        await RefreshContextAsync(UsersContext);
        var updatedProfile = await UsersContext.UserProfiles
            .FirstAsync(u => u.UserId == userProfile.UserId);

        updatedProfile.FitnessLevel.Should().Be(fitnessLevel);
        updatedProfile.PrimaryFitnessGoal.Should().Be(primaryGoal);
        updatedProfile.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserProfile_ShouldReturnCompleteData()
    {
        // Arrange
        var userProfile = TestDataBuilder.CreateUser()
            .WithName("Complete", "Profile")
            .WithDateOfBirth(new DateTime(1985, 3, 20))
            .WithGender(Gender.Female)
            .WithPhysicalMeasurements(165m, 60m)
            .WithFitnessProfile(FitnessLevel.Advanced, FitnessGoal.Weight_Loss)
            .Build();

        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        // Act
        var retrievedProfile = await UsersContext.UserProfiles
            .FirstAsync(u => u.UserId == userProfile.UserId);

        // Assert
        retrievedProfile.Should().NotBeNull();
        retrievedProfile.Name.FirstName.Should().Be("Complete");
        retrievedProfile.Name.LastName.Should().Be("Profile");
        retrievedProfile.DateOfBirth.Should().NotBeNull();
        retrievedProfile.Gender.Should().Be(Gender.Female);
        retrievedProfile.PhysicalMeasurements.HeightCm.Should().Be(165m);
        retrievedProfile.PhysicalMeasurements.WeightKg.Should().Be(60m);
        retrievedProfile.FitnessLevel.Should().Be(FitnessLevel.Advanced);
        retrievedProfile.PrimaryFitnessGoal.Should().Be(FitnessGoal.Weight_Loss);
        retrievedProfile.HasCompletedProfile().Should().BeTrue();
        retrievedProfile.GetAge().Should().BeGreaterThan(35);
    }

    [Fact]
    public async Task DeleteUserProfile_ShouldRemoveFromDatabase()
    {
        // Arrange
        var userProfile = TestDataBuilder.CreateUser()
            .WithName("To", "Delete")
            .Build();

        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        var userId = userProfile.UserId;

        // Act
        UsersContext.UserProfiles.Remove(userProfile);
        await UsersContext.SaveChangesAsync();

        // Assert
        var deletedProfile = await UsersContext.UserProfiles
            .FirstOrDefaultAsync(u => u.UserId == userId);

        deletedProfile.Should().BeNull();
    }

    [Fact]
    public async Task UserProfile_WithPhysicalMeasurements_ShouldCalculateBMI()
    {
        // Arrange
        var userProfile = TestDataBuilder.CreateUser()
            .WithPhysicalMeasurements(180m, 80m) // BMI should be ~24.7
            .Build();

        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        // Act
        var savedProfile = await UsersContext.UserProfiles
            .FirstAsync(u => u.UserId == userProfile.UserId);

        // Assert
        var bmi = savedProfile.GetBMI();
        bmi.Should().NotBeNull();
        bmi.Should().BeApproximately(24.69m, 0.01m);
    }

    [Fact]
    public async Task UserProfile_WithIncompleteData_ShouldNotBeCompleted()
    {
        // Arrange - Créer un profil incomplet manuellement
        var incompleteProfile = new UserProfile(Guid.NewGuid());
        incompleteProfile.UpdatePersonalInfo(
            FullName.Create("Incomplete", "Profile"),
            DateOfBirth.Create(new DateTime(1990, 1, 1)),
            null); // Pas de Gender
        // Manque PhysicalMeasurements, FitnessLevel, PrimaryFitnessGoal

        await UsersContext.UserProfiles.AddAsync(incompleteProfile);
        await UsersContext.SaveChangesAsync();

        // Act
        var savedProfile = await UsersContext.UserProfiles
            .FirstAsync(u => u.UserId == incompleteProfile.UserId);

        // Assert
        savedProfile.HasCompletedProfile().Should().BeFalse();
    }

    [Fact]
    public async Task MultipleUserProfiles_ShouldPersistIndependently()
    {
        // Arrange
        var users = TestDataBuilder.TestScenarios.CreateMultipleUsers(3);

        // Act
        await UsersContext.UserProfiles.AddRangeAsync(users);
        await UsersContext.SaveChangesAsync();

        // Assert
        var savedUsers = await UsersContext.UserProfiles
            .Where(u => users.Select(usr => usr.UserId).Contains(u.UserId))
            .ToListAsync();

        savedUsers.Should().HaveCount(3);
        savedUsers.Select(u => u.UserId).Should().BeEquivalentTo(users.Select(u => u.UserId));

        // Vérifier que chaque utilisateur est unique
        savedUsers.Select(u => u.UserId).Should().OnlyHaveUniqueItems();
    }
}




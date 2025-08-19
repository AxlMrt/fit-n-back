using FluentAssertions;
using Moq;
using FitnessApp.Modules.Users.Application.Services;
using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.SharedKernel.Interfaces;

namespace FitnessApp.Modules.Users.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly Mock<IValidationService> _validator = new();

    [Fact]
    public async Task UpdateProfile_Should_Update_Profile_And_Return_UserResponse()
    {
        var id = Guid.NewGuid();
        var user = new User("john@doe.com", "john");
        user.SetProfile(new UserProfile(id));
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
        _repo.Setup(r => r.UpdateAsync(user)).ReturnsAsync(user);

        var sut = new UserService(_repo.Object, _validator.Object);
        var req = new UpdateProfileRequest("John", "Doe", new DateTime(1990,1,1), "M", 180, 80, "Intermediate", "LoseWeight");

        var result = await sut.UpdateUserProfileAsync(id, req);

        result.Should().NotBeNull();
        result.Profile!.FirstName.Should().Be("John");
        _repo.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdatePreferences_Should_Upsert_Values()
    {
        var id = Guid.NewGuid();
        var user = new User("john@doe.com", "john");
        user.SetProfile(new UserProfile(id));
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
        _repo.Setup(r => r.UpdateAsync(user)).ReturnsAsync(user);

        var sut = new UserService(_repo.Object, _validator.Object);
        var request = new PreferencesUpdateRequest(new []
        {
            new PreferenceItem("general", "theme", "dark"),
            new PreferenceItem("general", "units", "metric")
        });

        await sut.UpdatePreferencesAsync(id, request);

        user.Preferences.Should().HaveCount(2);
        user.Preferences.Should().Contain(p => p.Key == "theme" && p.Value == "dark");
        _repo.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetGoals_Should_Return_Placeholder()
    {
        var sut = new UserService(_repo.Object, _validator.Object);
        var output = await sut.GetGoalsAsync(Guid.NewGuid());
        output.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStats_Should_Return_Placeholder()
    {
        var sut = new UserService(_repo.Object, _validator.Object);
        var output = await sut.GetStatsAsync(Guid.NewGuid());
        output.Should().NotBeNull();
    }
}

using Xunit;
using FluentValidation.TestHelper;
using FitnessApp.Modules.Users.Application.Validators;
using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.Modules.Users.Domain.Enums;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Tests.Application.Validators;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenValidRequest()
    {
        // Arrange
        var request = new CreateUserRequest(
            "test@example.com",
            "testuser",
            "Password123!",
            "John",
            "Doe"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    public void ShouldFail_WhenEmailInvalid(string invalidEmail)
    {
        // Arrange
        var request = new CreateUserRequest(
            invalidEmail,
            "testuser",
            "Password123!",
            "John",
            "Doe"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("this_is_a_very_long_username_that_exceeds_30_characters")]
    [InlineData("user@name")]
    public void ShouldFail_WhenUsernameInvalid(string invalidUsername)
    {
        // Arrange
        var request = new CreateUserRequest(
            "test@example.com",
            invalidUsername,
            "Password123!",
            "John",
            "Doe"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("password")]
    [InlineData("PASSWORD")]
    [InlineData("Password")]
    [InlineData("Password123")]
    public void ShouldFail_WhenPasswordInvalid(string invalidPassword)
    {
        // Arrange
        var request = new CreateUserRequest(
            "test@example.com",
            "testuser",
            invalidPassword,
            "John",
            "Doe"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ShouldFail_WhenFirstNameTooLong()
    {
        // Arrange
        var request = new CreateUserRequest(
            "test@example.com",
            "testuser",
            "Password123!",
            new string('a', 51), // too long
            "Doe"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("John<script>")]
    [InlineData("John>alert")]
    [InlineData("John&amp")]
    public void ShouldFail_WhenFirstNameContainsInvalidCharacters(string invalidFirstName)
    {
        // Arrange
        var request = new CreateUserRequest(
            "test@example.com",
            "testuser",
            "Password123!",
            invalidFirstName,
            "Doe"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }
}

public class UpdateUserProfileRequestValidatorTests
{
    private readonly UpdateUserProfileRequestValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenValidRequest()
    {
        // Arrange
        var request = new UpdateUserProfileRequest(
            "John",
            "Doe",
            new DateTime(1990, 1, 1),
            Gender.Male,
            180,
            75,
            FitnessLevel.Advanced,
            FitnessGoal.MUSCLE_GAIN
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldPass_WhenAllFieldsNull()
    {
        // Arrange
        var request = new UpdateUserProfileRequest(
            null, null, null, null, null, null, null, null
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldFail_WhenDateOfBirthInFuture()
    {
        // Arrange
        var request = new UpdateUserProfileRequest(
            "John",
            "Doe",
            DateTime.UtcNow.AddDays(1), // future date
            Gender.Male,
            180,
            75,
            FitnessLevel.Advanced,
            FitnessGoal.MUSCLE_GAIN
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void ShouldFail_WhenDateOfBirthTooOld()
    {
        // Arrange
        var request = new UpdateUserProfileRequest(
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-125), // too old
            Gender.Male,
            180,
            75,
            FitnessLevel.Advanced,
            FitnessGoal.MUSCLE_GAIN
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void ShouldFail_WhenUserTooYoung()
    {
        // Arrange
        var request = new UpdateUserProfileRequest(
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10), // 10 years old
            Gender.Male,
            180,
            75,
            FitnessLevel.Advanced,
            FitnessGoal.MUSCLE_GAIN
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Theory]
    [InlineData(49)]
    [InlineData(251)]
    public void ShouldFail_WhenHeightOutOfRange(decimal invalidHeight)
    {
        // Arrange
        var request = new UpdateUserProfileRequest(
            "John",
            "Doe",
            new DateTime(1990, 1, 1),
            Gender.Male,
            invalidHeight,
            75,
            FitnessLevel.Advanced,
            FitnessGoal.MUSCLE_GAIN
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Height);
    }

    [Theory]
    [InlineData(19)]
    [InlineData(301)]
    public void ShouldFail_WhenWeightOutOfRange(decimal invalidWeight)
    {
        // Arrange
        var request = new UpdateUserProfileRequest(
            "John",
            "Doe",
            new DateTime(1990, 1, 1),
            Gender.Male,
            180,
            invalidWeight,
            FitnessLevel.Advanced,
            FitnessGoal.MUSCLE_GAIN
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Weight);
    }
}

public class UserQueryRequestValidatorTests
{
    private readonly UserQueryRequestValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenValidRequest()
    {
        // Arrange
        var request = new UserQueryRequest(
            "test@example.com",
            "John Doe",
            Gender.Male,
            FitnessLevel.Advanced,
            true,
            "Email",
            true,
            1,
            20
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldFail_WhenEmailFilterInvalid()
    {
        // Arrange
        var request = new UserQueryRequest(EmailFilter: "invalid-email");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmailFilter);
    }

    [Fact]
    public void ShouldFail_WhenSortByInvalid()
    {
        // Arrange
        var request = new UserQueryRequest(SortBy: "InvalidField");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ShouldFail_WhenPageInvalid(int invalidPage)
    {
        // Arrange
        var request = new UserQueryRequest(Page: invalidPage);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void ShouldFail_WhenPageSizeInvalid(int invalidPageSize)
    {
        // Arrange
        var request = new UserQueryRequest(PageSize: invalidPageSize);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}

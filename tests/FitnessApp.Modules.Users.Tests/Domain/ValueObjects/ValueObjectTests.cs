using Xunit;
using FitnessApp.Modules.Users.Domain.ValueObjects;

namespace FitnessApp.Modules.Users.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("test+tag@example.org")]
    [InlineData("user123@test-domain.com")]
    public void Create_ShouldCreateEmail_WhenValidFormat(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        Assert.Equal(validEmail.ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("@domain.com")]
    [InlineData("test@")]
    [InlineData("test.domain.com")]
    [InlineData("test@domain")]
    public void Create_ShouldThrowException_WhenInvalidFormat(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Create(invalidEmail));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenEmailTooLong()
    {
        // Arrange
        var longEmail = new string('a', 300) + "@example.com"; // > 320 chars

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Create(longEmail));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenSameEmail()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@EXAMPLE.COM");

        // Act & Assert
        Assert.True(email1.Equals(email2));
        Assert.True(email1 == email2);
        Assert.False(email1 != email2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDifferentEmail()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        Assert.False(email1.Equals(email2));
        Assert.False(email1 == email2);
        Assert.True(email1 != email2);
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var emailValue = "test@example.com";
        var email = Email.Create(emailValue);

        // Act & Assert
        Assert.Equal(emailValue, email.ToString());
    }

    [Fact]
    public void ImplicitConversion_ShouldWork()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string emailString = email;

        // Assert
        Assert.Equal("test@example.com", emailString);
    }
}

public class UsernameTests
{
    [Theory]
    [InlineData("testuser")]
    [InlineData("test123")]
    [InlineData("user.name")]
    [InlineData("user_name")]
    [InlineData("user-name")]
    [InlineData("a1b")]
    public void Create_ShouldCreateUsername_WhenValid(string validUsername)
    {
        // Act
        var username = Username.Create(validUsername);

        // Assert
        Assert.Equal(validUsername, username.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("ab")] // too short
    [InlineData("a")] // too short
    [InlineData("this_is_a_very_long_username_that_exceeds_30_chars")] // too long
    [InlineData("user@name")] // invalid character
    [InlineData("user name")] // space not allowed
    [InlineData(".username")] // starts with dot
    [InlineData("username.")] // ends with dot
    [InlineData("user..name")] // consecutive dots
    public void Create_ShouldThrowException_WhenInvalid(string invalidUsername)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Username.Create(invalidUsername));
    }

    [Fact]
    public void Equals_ShouldBeCaseInsensitive()
    {
        // Arrange
        var username1 = Username.Create("TestUser");
        var username2 = Username.Create("testuser");

        // Act & Assert
        Assert.True(username1.Equals(username2));
        Assert.True(username1 == username2);
    }
}

public class PhysicalMeasurementsTests
{
    [Fact]
    public void Create_ShouldCreateMeasurements_WhenValidValues()
    {
        // Arrange
        var height = 180m;
        var weight = 75m;

        // Act
        var measurements = PhysicalMeasurements.Create(height, weight);

        // Assert
        Assert.Equal(height, measurements.HeightCm);
        Assert.Equal(weight, measurements.WeightKg);
        Assert.NotNull(measurements.BMI);
        Assert.Equal(23.15m, measurements.BMI); // 75 / (1.8^2)
    }

    [Fact]
    public void Create_ShouldCreateEmpty_WhenNoValues()
    {
        // Act
        var measurements = PhysicalMeasurements.Create();

        // Assert
        Assert.Null(measurements.HeightCm);
        Assert.Null(measurements.WeightKg);
        Assert.Null(measurements.BMI);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(49)]
    [InlineData(251)]
    public void Create_ShouldThrowException_WhenHeightInvalid(decimal invalidHeight)
    {
        // Act & Assert
        Assert.Throws<Exception>(() => PhysicalMeasurements.Create(invalidHeight, 75));
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(19)]
    [InlineData(301)]
    public void Create_ShouldThrowException_WhenWeightInvalid(decimal invalidWeight)
    {
        // Act & Assert
        Assert.Throws<Exception>(() => PhysicalMeasurements.Create(180, invalidWeight));
    }

    [Theory]
    [InlineData(18.4, "Underweight")]
    [InlineData(22.0, "Normal weight")]
    [InlineData(27.0, "Overweight")]
    [InlineData(35.0, "Obese")]
    public void GetBMICategory_ShouldReturnCorrectCategory(decimal bmi, string expectedCategory)
    {
        // Arrange
        // Calculate height and weight that give the desired BMI
        var height = 180m;
        var weight = bmi * (height / 100m) * (height / 100m);
        var measurements = PhysicalMeasurements.Create(height, weight);

        // Act
        var category = measurements.GetBMICategory();

        // Assert
        Assert.Equal(expectedCategory, category);
    }

    [Fact]
    public void UpdateHeight_ShouldReturnNewInstance_WithUpdatedHeight()
    {
        // Arrange
        var original = PhysicalMeasurements.Create(180, 75);
        var newHeight = 175m;

        // Act
        var updated = original.UpdateHeight(newHeight);

        // Assert
        Assert.Equal(newHeight, updated.HeightCm);
        Assert.Equal(original.WeightKg, updated.WeightKg);
        Assert.NotEqual(original.BMI, updated.BMI);
    }

    [Fact]
    public void UpdateWeight_ShouldReturnNewInstance_WithUpdatedWeight()
    {
        // Arrange
        var original = PhysicalMeasurements.Create(180, 75);
        var newWeight = 80m;

        // Act
        var updated = original.UpdateWeight(newWeight);

        // Assert
        Assert.Equal(original.HeightCm, updated.HeightCm);
        Assert.Equal(newWeight, updated.WeightKg);
        Assert.NotEqual(original.BMI, updated.BMI);
    }
}

public class FullNameTests
{
    [Fact]
    public void Create_ShouldCreateFullName_WhenValidNames()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var fullName = FullName.Create(firstName, lastName);

        // Assert
        Assert.Equal(firstName, fullName.FirstName);
        Assert.Equal(lastName, fullName.LastName);
        Assert.Equal("John Doe", fullName.DisplayName);
        Assert.True(fullName.IsComplete);
    }

    [Fact]
    public void Create_ShouldCreatePartialName_WhenOnlyFirstName()
    {
        // Arrange
        var firstName = "John";

        // Act
        var fullName = FullName.Create(firstName, null);

        // Assert
        Assert.Equal(firstName, fullName.FirstName);
        Assert.Null(fullName.LastName);
        Assert.Equal("John", fullName.DisplayName);
        Assert.False(fullName.IsComplete);
    }

    [Fact]
    public void Create_ShouldCreateEmpty_WhenNoNames()
    {
        // Act
        var fullName = FullName.Create();

        // Assert
        Assert.Null(fullName.FirstName);
        Assert.Null(fullName.LastName);
        Assert.Equal("User", fullName.DisplayName);
        Assert.False(fullName.IsComplete);
    }

    [Theory]
    [InlineData("John<script>")]
    [InlineData("John>alert")]
    [InlineData("John&amp")]
    public void Create_ShouldThrowException_WhenNamesContainInvalidCharacters(string invalidName)
    {
        // Act & Assert
        Assert.Throws<Exception>(() => FullName.Create(invalidName, "Doe"));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenNameTooLong()
    {
        // Arrange
        var longName = new string('a', 51);

        // Act & Assert
        Assert.Throws<Exception>(() => FullName.Create(longName, "Doe"));
    }
}

public class DateOfBirthTests
{
    [Fact]
    public void Create_ShouldCreateDateOfBirth_WhenValidDate()
    {
        // Arrange
        var birthDate = new DateTime(1990, 1, 1);

        // Act
        var dateOfBirth = DateOfBirth.Create(birthDate);

        // Assert
        Assert.Equal(birthDate.Date, dateOfBirth.Value);
        Assert.True(dateOfBirth.Age >= 30); // Should be around 34-35 in 2025
        Assert.True(dateOfBirth.IsAdult);
        Assert.False(dateOfBirth.IsSenior);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDateInFuture()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<Exception>(() => DateOfBirth.Create(futureDate));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenAgeTooYoung()
    {
        // Arrange
        var tooYoungDate = DateTime.UtcNow.AddYears(-10); // 10 years old

        // Act & Assert
        Assert.Throws<Exception>(() => DateOfBirth.Create(tooYoungDate));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenAgeTooOld()
    {
        // Arrange
        var tooOldDate = DateTime.UtcNow.AddYears(-125); // 125 years old

        // Act & Assert
        Assert.Throws<Exception>(() => DateOfBirth.Create(tooOldDate));
    }

    [Theory]
    [InlineData(-10, "Teen")] // 15 years old
    [InlineData(-25, "Young Adult")]
    [InlineData(-35, "Adult")]
    [InlineData(-55, "Middle-aged")]
    [InlineData(-70, "Senior")]
    public void GetAgeGroup_ShouldReturnCorrectGroup(int yearsAgo, string expectedGroup)
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(yearsAgo);
        if (yearsAgo == -10) birthDate = DateTime.UtcNow.AddYears(-15); // Adjust for minimum age
        var dateOfBirth = DateOfBirth.Create(birthDate);

        // Act
        var ageGroup = dateOfBirth.GetAgeGroup();

        // Assert
        Assert.Equal(expectedGroup, ageGroup);
    }
}

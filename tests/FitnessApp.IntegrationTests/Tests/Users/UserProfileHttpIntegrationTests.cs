using FitnessApp.IntegrationTests.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace FitnessApp.IntegrationTests.Tests.Users;

/// <summary>
/// Tests d'intégration HTTP pour le module Users
/// </summary>
public class UserProfileHttpIntegrationTests : IntegrationTestBase
{
    public UserProfileHttpIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region Profile Management Tests

    [Fact]
    public async Task CreateProfile_WithValidData_ShouldCreate()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var requestJson = """
        {
            "firstName": "John",
            "lastName": "Doe",
            "dateOfBirth": "1990-05-15T00:00:00Z",
            "gender": "Male",
            "height": 180.0,
            "weight": 75.0,
            "fitnessLevel": "Enthousiast",
            "fitnessGoal": "Muscle_Gain",
            "preferredWorkoutDuration": 45,
            "hasEquipment": true
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/users/profile",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Request failed with status {response.StatusCode}. Response: {errorContent}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"firstName\":\"John\"");
        content.Should().Contain("\"lastName\":\"Doe\"");
        content.Should().Contain("\"gender\":\"Male\"");
        content.Should().Contain("\"height\":180");
        content.Should().Contain("\"weight\":75");
        content.Should().Contain("\"fitnessLevel\":\"Enthousiast\"");
        content.Should().Contain("\"fitnessGoal\":\"Muscle_Gain\"");
        content.Should().Contain("\"hasCompletedProfile\":true");
    }

    [Fact]
    public async Task CreateProfile_WhenAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var requestJson = """
        {
            "firstName": "Jane",
            "lastName": "Smith",
            "dateOfBirth": "1985-03-20T00:00:00Z",
            "gender": "Female",
            "height": 165.0,
            "weight": 60.0,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        // Create profile first time
        var firstResponse = await Client.PostAsync("/api/v1/users/profile",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Try to create again
        var secondResponse = await Client.PostAsync("/api/v1/users/profile",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await secondResponse.Content.ReadAsStringAsync();
        content.Should().Contain("Profile already exists");
    }

    [Fact]
    public async Task GetProfile_WhenExists_ShouldReturnProfile()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Alice",
            "lastName": "Johnson",
            "dateOfBirth": "1992-08-10T00:00:00Z",
            "gender": "Female",
            "height": 170.0,
            "weight": 65.0,
            "fitnessLevel": "Advanced",
            "fitnessGoal": "Endurance",
            "preferredWorkoutDuration": 60,
            "hasEquipment": true
        }
        """;

        await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        // Act
        var response = await Client.GetAsync("/api/v1/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"firstName\":\"Alice\"");
        content.Should().Contain("\"lastName\":\"Johnson\"");
        content.Should().Contain("\"fitnessLevel\":\"Advanced\"");
        content.Should().Contain("\"fitnessGoal\":\"Endurance\"");
        
        // Verify BMI calculation
        content.Should().Contain("\"bmi\":");
        
        // Parse JSON to verify BMI calculation (65kg / (1.7m)^2 ≈ 22.49)
        var doc = JsonDocument.Parse(content);
        var bmi = doc.RootElement.GetProperty("bmi").GetDecimal();
        bmi.Should().BeApproximately(22.49m, 0.1m);
    }

    [Fact]
    public async Task GetProfile_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Act - Try to get profile without creating it first
        var response = await Client.GetAsync("/api/v1/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WithValidData_ShouldUpdate()
    {
        // Arrange - Create profile first
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Original",
            "lastName": "Name",
            "dateOfBirth": "1990-01-01T00:00:00Z",
            "gender": "Male",
            "height": 175.0,
            "weight": 70.0,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        // Act - Update personal info
        var updateRequestJson = """
        {
            "firstName": "Updated",
            "lastName": "Person",
            "dateOfBirth": "1985-12-25T00:00:00Z",
            "gender": "Female"
        }
        """;

        var response = await Client.PatchAsync("/api/v1/users/profile/personal",
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"firstName\":\"Updated\"");
        content.Should().Contain("\"lastName\":\"Person\"");
        content.Should().Contain("\"gender\":\"Female\"");
        content.Should().Contain("\"updatedAt\":");
    }

    [Fact]
    public async Task UpdatePhysicalMeasurements_WithValidData_ShouldUpdate()
    {
        // Arrange - Create profile first
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Test",
            "lastName": "User",
            "dateOfBirth": "1990-01-01T00:00:00Z",
            "gender": "Male",
            "height": 175,
            "weight": 75,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        // Act - Update measurements
        var updateRequestJson = """
        {
            "height": 180,
            "weight": 80,
            "units": {
                "heightUnit": "cm",
                "weightUnit": "kg"
            }
        }
        """;

        var response = await Client.PatchAsync("/api/v1/users/profile/measurements",
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"height\":180");
        content.Should().Contain("\"weight\":80");
        
        // Verify BMI recalculation (80kg / (1.8m)^2 ≈ 24.69)
        var doc = JsonDocument.Parse(content);
        var bmi = doc.RootElement.GetProperty("bmi").GetDecimal();
        bmi.Should().BeApproximately(24.69m, 0.1m);
    }

    [Fact]
    public async Task UpdateFitnessProfile_WithValidData_ShouldUpdate()
    {
        // Arrange - Create profile first
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Fitness",
            "lastName": "Enthusiast",
            "dateOfBirth": "1988-06-30T00:00:00Z",
            "gender": "Female",
            "height": 168,
            "weight": 62,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        // Act - Update fitness profile
        var updateRequestJson = """
        {
            "fitnessLevel": "Advanced",
            "fitnessGoal": "Muscle_Gain"
        }
        """;

        var response = await Client.PatchAsync("/api/v1/users/profile/fitness",
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"fitnessLevel\":\"Advanced\"");
        content.Should().Contain("\"fitnessGoal\":\"Muscle_Gain\"");
        content.Should().Contain("\"updatedAt\":");
    }

    [Fact]
    public async Task DeleteProfile_WhenExists_ShouldDelete()
    {
        // Arrange - Create profile first
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "To",
            "lastName": "Delete",
            "dateOfBirth": "1995-01-01T00:00:00Z",
            "gender": "Male",
            "height": 175,
            "weight": 70,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        // Act
        var response = await Client.DeleteAsync("/api/v1/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"success\":true");
        content.Should().Contain("\"message\":\"Profile deleted successfully\"");

        // Verify profile is deleted
        var getResponse = await Client.GetAsync("/api/v1/users/profile");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task CreateProfile_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var invalidRequestJson = """
        {
            "firstName": "J",
            "lastName": "",
            "dateOfBirth": "2020-01-01T00:00:00Z",
            "height": -10,
            "weight": 0,
            "fitnessLevel": "InvalidLevel"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/users/profile",
            new StringContent(invalidRequestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePhysicalMeasurements_WithNegativeValues_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Create profile first
        var createRequestJson = """
        {
            "firstName": "Test",
            "lastName": "User",
            "dateOfBirth": "1990-01-01T00:00:00Z",
            "gender": "Male",
            "height": 175,
            "weight": 75,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        var invalidUpdateJson = """
        {
            "height": -180,
            "weight": -80
        }
        """;

        // Act
        var response = await Client.PatchAsync("/api/v1/users/profile/measurements",
            new StringContent(invalidUpdateJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_WithNonExistentProfile_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var updateRequestJson = """
        {
            "firstName": "New",
            "lastName": "Name"
        }
        """;

        // Act - Try to update without creating profile first
        var response = await Client.PatchAsync("/api/v1/users/profile/personal",
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Unit System Tests

    [Fact]
    public async Task UpdatePhysicalMeasurements_WithImperialUnits_ShouldConvertCorrectly()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Imperial",
            "lastName": "User",
            "dateOfBirth": "1990-01-01T00:00:00Z",
            "gender": "Male",
            "height": 175,
            "weight": 70,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        // Act - Update with imperial units (5.9 ft ≈ 179.83 cm, 165 lbs ≈ 74.84 kg)
        var updateRequestJson = """
        {
            "height": 5.9,
            "weight": 165,
            "units": {
                "heightUnit": "ft",
                "weightUnit": "lbs"
            }
        }
        """;

        var response = await Client.PatchAsync("/api/v1/users/profile/measurements",
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        // Should be converted to metric in storage
        var doc = JsonDocument.Parse(content);
        var height = doc.RootElement.GetProperty("height").GetDecimal();
        var weight = doc.RootElement.GetProperty("weight").GetDecimal();
        
        height.Should().BeInRange(179m, 181m); // Allow for conversion rounding (5.9 ft ≈ 179.83 cm)
        weight.Should().BeApproximately(74.84m, 1m);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task CreateProfile_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange - No authentication

        var requestJson = """
        {
            "firstName": "Test",
            "lastName": "User",
            "dateOfBirth": "1990-01-01T00:00:00Z",
            "gender": "Male",
            "height": 175,
            "weight": 70,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/users/profile",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task CompleteProfileWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Step 1: Create complete profile
        var createRequestJson = """
        {
            "firstName": "Complete",
            "lastName": "User",
            "dateOfBirth": "1987-04-12T00:00:00Z",
            "gender": "Female",
            "height": 165,
            "weight": 58,
            "fitnessLevel": "Enthousiast",
            "fitnessGoal": "Muscle_Gain",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 2: Update personal info
        var updatePersonalJson = """
        {
            "firstName": "Updated",
            "lastName": "Complete"
        }
        """;

        var updatePersonalResponse = await Client.PatchAsync("/api/v1/users/profile/personal",
            new StringContent(updatePersonalJson, Encoding.UTF8, "application/json"));
        updatePersonalResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Update measurements
        var updateMeasurementsJson = """
        {
            "height": 168,
            "weight": 60
        }
        """;

        var updateMeasurementsResponse = await Client.PatchAsync("/api/v1/users/profile/measurements",
            new StringContent(updateMeasurementsJson, Encoding.UTF8, "application/json"));
        updateMeasurementsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Update fitness profile
        var updateFitnessJson = """
        {
            "fitnessLevel": "Advanced",
            "fitnessGoal": "Endurance"
        }
        """;

        var updateFitnessResponse = await Client.PatchAsync("/api/v1/users/profile/fitness",
            new StringContent(updateFitnessJson, Encoding.UTF8, "application/json"));
        updateFitnessResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Verify final state
        var getResponse = await Client.GetAsync("/api/v1/users/profile");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await getResponse.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"firstName\":\"Updated\"");
        content.Should().Contain("\"lastName\":\"Complete\"");
        content.Should().Contain("\"height\":168");
        content.Should().Contain("\"weight\":60");
        content.Should().Contain("\"fitnessLevel\":\"Advanced\"");
        content.Should().Contain("\"fitnessGoal\":\"Endurance\"");
        content.Should().Contain("\"hasCompletedProfile\":true");

        // Verify BMI calculation (60kg / (1.68m)^2 ≈ 21.26)
        var doc = JsonDocument.Parse(content);
        var bmi = doc.RootElement.GetProperty("bmi").GetDecimal();
        bmi.Should().BeApproximately(21.26m, 0.1m);
    }

    [Fact]
    public async Task PartialProfileUpdate_ShouldOnlyUpdateSpecifiedFields()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Original",
            "lastName": "User",
            "dateOfBirth": "1990-01-01T00:00:00Z",
            "gender": "Male",
            "height": 175,
            "weight": 70,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        // Act - Update only first name
        var updateRequestJson = """
        {
            "firstName": "Updated"
        }
        """;

        var response = await Client.PatchAsync("/api/v1/users/profile/personal",
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        // Updated field
        content.Should().Contain("\"firstName\":\"Updated\"");
        
        // Unchanged fields
        content.Should().Contain("\"lastName\":\"User\"");
        content.Should().Contain("\"gender\":\"Male\"");
    }

    [Fact]
    public async Task ProfileAgeCalculation_ShouldBeCorrect()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var birthDate = DateTime.Now.AddYears(-30).ToString("yyyy-MM-ddTHH:mm:ssZ");

        var createRequestJson = $$"""
        {
            "firstName": "Age",
            "lastName": "Test",
            "dateOfBirth": "{{birthDate}}",
            "gender": "Male",
            "height": 175,
            "weight": 70,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss",
            "preferredWorkoutDuration": 30,
            "hasEquipment": false
        }
        """;

        await Client.PostAsync("/api/v1/users/profile",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        // Act
        var response = await Client.GetAsync("/api/v1/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        var doc = JsonDocument.Parse(content);
        var age = doc.RootElement.GetProperty("age").GetInt32();
        
        // Age should be approximately 30 (allowing for date differences)
        age.Should().BeInRange(29, 31);
    }

    #endregion
}




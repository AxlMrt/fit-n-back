using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.IntegrationTests.Helpers;
using System.Net;
using System.Text;
using System.Text.Json;

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

        // Act
        var response = await Client.PostAsync(ApiEndpoints.Users.Profile,
            ApiJsonTemplates.CreateUserProfile("John", "Doe", "1990-05-15T00:00:00Z", "Male", 180.0, 75.0, "Enthousiast", "Muscle_Gain").ToStringContent());

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

        var requestJson = ApiJsonTemplates.CreateUserProfile("Jane", "Smith", "1985-03-20T00:00:00Z", "Female", 165.0, 60.0, "Beginner", "Weight_Loss");

        // Create profile first time
        var firstResponse = await Client.PostAsync(ApiEndpoints.Users.Profile,
            requestJson.ToStringContent());
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Try to create again
        var secondResponse = await Client.PostAsync(ApiEndpoints.Users.Profile,
            requestJson.ToStringContent());

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

        var createRequestJson = ApiJsonTemplates.CreateUserProfile("Alice", "Johnson", "1992-08-10T00:00:00Z", "Female", 170.0, 65.0, "Advanced", "Endurance");

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());

        // Act
        var response = await Client.GetAsync(ApiEndpoints.Users.Profile);

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
        var response = await Client.GetAsync(ApiEndpoints.Users.Profile);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WithValidData_ShouldUpdate()
    {
        // Arrange - Create profile first
        await AuthenticateAsUserAsync();

        var createRequestJson = ApiJsonTemplates.CreateUserProfile("Original", "Name", "1990-01-01T00:00:00Z", "Male", 175.0, 70.0, "Beginner", "Weight_Loss");

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());

        // Act - Update personal info
        var updateRequestJson = ApiJsonTemplates.UpdatePersonalInfoComplete("Updated", "Person", "1985-12-25T00:00:00Z", "Female");

        var response = await Client.PatchAsync(ApiEndpoints.Users.ProfilePersonal,
            updateRequestJson.ToStringContent());

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

        var createRequestJson = ApiJsonTemplates.CreateUserProfile("Test", "User", "1990-01-01T00:00:00Z", "Male", 175, 75, "Beginner", "Weight_Loss");

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());

        // Act - Update measurements
        var updateRequestJson = ApiJsonTemplates.UpdatePhysicalMeasurements(180, 80, "cm", "kg");

        var response = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            updateRequestJson.ToStringContent());

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

        var createRequestJson = ApiJsonTemplates.CreateUserProfile("Fitness", "Enthusiast", "1988-06-30T00:00:00Z", "Female", 168, 62, "Beginner", "Weight_Loss");

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());

        // Act - Update fitness profile
        var updateRequestJson = ApiJsonTemplates.UpdateFitnessPreferences("Advanced", "Muscle_Gain");

        var response = await Client.PatchAsync(ApiEndpoints.Users.ProfileFitness,
            updateRequestJson.ToStringContent());

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

        var createRequestJson = ApiJsonTemplates.CreateUserProfile("To", "Delete", "1995-01-01T00:00:00Z", "Male", 175, 70, "Beginner", "Weight_Loss");

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());

        // Act
        var response = await Client.DeleteAsync(ApiEndpoints.Users.Profile);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"success\":true");
        content.Should().Contain("\"message\":\"Profile deleted successfully\"");

        // Verify profile is deleted
        var getResponse = await Client.GetAsync(ApiEndpoints.Users.Profile);
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
        var response = await Client.PostAsync(ApiEndpoints.Users.Profile,
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
        var createRequestJson = ApiJsonTemplates.CreateUserProfile("Test", "User", "1990-01-01T00:00:00Z", "Male", 175, 75, "Beginner", "Weight_Loss");

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());

        var invalidUpdateJson = """
        {
            "height": -180,
            "weight": -80
        }
        """;

        // Act
        var response = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(invalidUpdateJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_WithNonExistentProfile_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var updateRequestJson = ApiJsonTemplates.UpdatePersonalInfoComplete("New", "Name", "1990-01-01T00:00:00Z", "Male");

        // Act - Try to update without creating profile first
        var response = await Client.PatchAsync(ApiEndpoints.Users.ProfilePersonal,
            updateRequestJson.ToStringContent());

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

        var createRequestJson = ApiJsonTemplates.CreateUserProfile("Imperial", "User", "1990-01-01T00:00:00Z", "Male", 175, 70, "Beginner", "Weight_Loss");

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());

        // Act - Update with imperial units (5.9 ft ≈ 179.83 cm, 165 lbs ≈ 74.84 kg)
        var updateRequestJson = ApiJsonTemplates.UpdatePhysicalMeasurements(5.9, 165, "ft", "lbs");

        var response = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            updateRequestJson.ToStringContent());

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

        var requestJson = ApiJsonTemplates.CreateUserProfile("Test", "User", "1990-01-01T00:00:00Z", "Male", 175, 70, "Beginner", "Weight_Loss");

        // Act
        var response = await Client.PostAsync(ApiEndpoints.Users.Profile,
            requestJson.ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiEndpoints.Users.Profile);

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
        var createRequestJson = ApiJsonTemplates.CreateUserProfile("Complete", "User", "1987-04-12T00:00:00Z", "Female", 165, 58, "Enthousiast", "Muscle_Gain");

        var createResponse = await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 2: Update personal info
        var updatePersonalJson = ApiJsonTemplates.UpdatePersonalInfoComplete("Updated", "Complete", "1987-04-12T00:00:00Z", "Female");

        var updatePersonalResponse = await Client.PatchAsync(ApiEndpoints.Users.ProfilePersonal,
            updatePersonalJson.ToStringContent());
        updatePersonalResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Update measurements
        var updateMeasurementsJson = ApiJsonTemplates.UpdatePhysicalMeasurements(168, 60);

        var updateMeasurementsResponse = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            updateMeasurementsJson.ToStringContent());
        updateMeasurementsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Update fitness profile
        var updateFitnessJson = ApiJsonTemplates.UpdateFitnessPreferences("Advanced", "Endurance");

        var updateFitnessResponse = await Client.PatchAsync(ApiEndpoints.Users.ProfileFitness,
            updateFitnessJson.ToStringContent());
        updateFitnessResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Verify final state
        var getResponse = await Client.GetAsync(ApiEndpoints.Users.Profile);
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

        var createRequestJson = ApiJsonTemplates.CreateUserProfile("Original", "User", "1990-01-01T00:00:00Z", "Male", 175, 70, "Beginner", "Weight_Loss");

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());

        // Act - Update only first name
        var updateRequestJson = ApiJsonTemplates.UpdatePersonalInfo(firstName: "Updated");

        var response = await Client.PatchAsync(ApiEndpoints.Users.ProfilePersonal,
            updateRequestJson.ToStringContent());

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

        var createRequestJson = ApiJsonTemplates.CreateUserProfile("Age", "Test", birthDate, "Male", 175, 70, "Beginner", "Weight_Loss");

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            createRequestJson.ToStringContent());

        // Act
        var response = await Client.GetAsync(ApiEndpoints.Users.Profile);

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




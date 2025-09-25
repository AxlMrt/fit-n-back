using FitnessApp.IntegrationTests.Infrastructure;
using System.Net;

namespace FitnessApp.IntegrationTests.Tests.Exercises;

public class ExerciseIntegrationTests : IntegrationTestBase
{
    public ExerciseIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetExercises_WithoutAuthentication_ShouldReturn401()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/exercises");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExercises_WithAuthentication_ShouldReturnPagedResults()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Act
        var response = await Client.GetAsync("/api/v1/exercises");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        content.Should().Contain("items");
        content.Should().Contain("totalCount");
        content.Should().Contain("pageNumber");
        content.Should().Contain("pageSize");
    }

    [Fact]
    public async Task GetExercises_WithFilters_ShouldFilterCorrectly()
    {
        // Arrange
        await AuthenticateAsUserAsync();
        
        // Create test exercises via admin API first
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "Push-ups Integration Test",
            "description": "A basic push-up exercise",
            "type": "Strength",
            "muscleGroups": ["Chest", "Shoulders"],
            "difficulty": "Beginner",
            "equipment": ["None"]
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/exercises", 
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Switch back to regular user
        await AuthenticateAsUserAsync();

        // Act - Filter by strength exercises 
        var response = await Client.GetAsync("/api/v1/exercises?type=Strength&difficulty=Beginner");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        content.Should().Contain("Push-ups Integration Test");
        content.Should().Contain("\"type\":\"Strength\"");
        content.Should().Contain("\"difficulty\":\"Beginner\"");
    }

    [Fact]
    public async Task CreateExercise_WithAdminRole_ShouldCreate()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "New Exercise",
            "description": "A test exercise", 
            "type": "Strength",
            "muscleGroups": ["Arms"],
            "difficulty": "Intermediate",
            "equipment": ["Dumbbells"],
            "instructions": "Perform the exercise carefully"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        content.Should().Contain("\"name\":\"New Exercise\"");
        content.Should().Contain("\"type\":\"Strength\"");
        content.Should().Contain("\"muscleGroups\":[\"Arms\"]");
        content.Should().Contain("\"description\":\"A test exercise\"");
        content.Should().Contain("\"difficulty\":\"Intermediate\"");
        content.Should().Contain("\"isActive\":true");
        content.Should().Contain("\"id\":\"");
        
        // Verify Location header exists
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateExercise_WithRegularUser_ShouldReturn403()
    {
        // Arrange
        await AuthenticateAsUserAsync();
        
        var createRequestJson = """
        {
            "name": "Unauthorized Exercise",
            "type": "Strength",
            "muscleGroups": ["Arms"]
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateExercise_WithInvalidData_ShouldReturn400()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        var invalidRequestJson = """
        {
            "name": "",
            "type": "Strength",
            "muscleGroups": ["Arms"]
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/exercises",
            new StringContent(invalidRequestJson, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("validation");
    }

    [Fact]
    public async Task GetExerciseById_WithValidId_ShouldReturnExercise()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        // Create exercise first
        var createRequestJson = """
        {
            "name": "Get Test Exercise",
            "type": "Cardio",
            "muscleGroups": ["Legs"],
            "difficulty": "Advanced"
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        createContent.Should().Contain("\"id\":\"");
        
        var exerciseId = ExtractIdFromJsonResponse(createContent);

        // Switch to regular user
        await AuthenticateAsUserAsync();

        // Act
        var response = await Client.GetAsync($"/api/v1/exercises/{exerciseId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain($"\"id\":\"{exerciseId}\"");
        content.Should().Contain("\"name\":\"Get Test Exercise\"");
        content.Should().Contain("\"type\":\"Cardio\"");
    }

    [Fact]
    public async Task GetExerciseById_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        await AuthenticateAsUserAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/exercises/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateExercise_WithValidData_ShouldUpdate()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        // Create exercise first
        var createRequestJson = """
        {
            "name": "Original Exercise",
            "type": "Strength",
            "muscleGroups": ["Arms"],
            "difficulty": "Beginner"
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);

        // Update request
        var updateRequestJson = """
        {
            "name": "Updated Exercise",
            "description": "Updated description",
            "difficulty": "Advanced"
        }
        """;

        // Act
        var response = await Client.PutAsync($"/api/v1/exercises/{exerciseId}",
            new StringContent(updateRequestJson, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("\"name\":\"Updated Exercise\"");
        content.Should().Contain("\"description\":\"Updated description\"");
        content.Should().Contain("\"difficulty\":\"Advanced\"");
        content.Should().Contain("\"updatedAt\":");
    }

    [Fact]
    public async Task SearchExercises_WithValidTerm_ShouldReturnMatches()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        // Create searchable exercise
        var createRequestJson = """
        {
            "name": "Unique Push-up Variation",
            "type": "Strength",
            "muscleGroups": ["Chest"]
        }
        """;

        await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        
        // Switch to regular user
        await AuthenticateAsUserAsync();

        // Act - Query string
        var response = await Client.GetAsync("/api/v1/exercises/search?term=Unique");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Unique");
    }

    [Fact]
    public async Task ActivateExercise_WithValidId_ShouldActivate()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        // Create and deactivate exercise first
        var createRequestJson = """
        {
            "name": "Exercise to Activate",
            "type": "Stretching",
            "muscleGroups": ["Core"]
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);

        // Deactivate first
        await Client.PostAsync($"/api/v1/exercises/{exerciseId}/deactivate", null);

        // Act - Reactivate
        var response = await Client.PostAsync($"/api/v1/exercises/{exerciseId}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify activation
        var getResponse = await Client.GetAsync($"/api/v1/exercises/{exerciseId}");
        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("\"isActive\":true");
    }

    [Fact]
    public async Task DeactivateExercise_WithValidId_ShouldDeactivate()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        // JSON
        var createRequestJson = """
        {
            "name": "Exercise to Deactivate",
            "type": "Strength",
            "muscleGroups": ["Legs"]
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);

        // Act
        var response = await Client.PostAsync($"/api/v1/exercises/{exerciseId}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify deactivation
        var getResponse = await Client.GetAsync($"/api/v1/exercises/{exerciseId}");
        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("\"isActive\":false");
    }

    [Fact]
    public async Task DeleteExercise_WithAdminRole_ShouldDelete()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        // JSON
        var createRequestJson = """
        {
            "name": "Exercise to Delete",
            "type": "Cardio",
            "muscleGroups": ["Legs"]
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);

        // Act
        var response = await Client.DeleteAsync($"/api/v1/exercises/{exerciseId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify deletion
        var getResponse = await Client.GetAsync($"/api/v1/exercises/{exerciseId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExerciseExists_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        // JSON
        var createRequestJson = """
        {
            "name": "Exists Test Exercise",
            "type": "Strength",
            "muscleGroups": ["Arms"]
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);

        // Switch to regular user
        await AuthenticateAsUserAsync();

        // Act
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"/api/v1/exercises/{exerciseId}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    private static string ExtractIdFromJsonResponse(string jsonContent)
    {
        var idStart = jsonContent.IndexOf("\"id\":\"") + 6;
        var idEnd = jsonContent.IndexOf("\"", idStart);
        return jsonContent.Substring(idStart, idEnd - idStart);
    }
}




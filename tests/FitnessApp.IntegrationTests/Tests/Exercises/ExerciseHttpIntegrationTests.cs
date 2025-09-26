using FitnessApp.IntegrationTests.Infrastructure;
using System.Net;

namespace FitnessApp.IntegrationTests.Tests.Exercises;

public class ExerciseHttpIntegrationTests : IntegrationTestBase
{
    public ExerciseHttpIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetExercises_WithoutAuthentication_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/exercises");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExercises_WithAuthentication_ShouldReturnPagedResults()
    {
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync("/api/v1/exercises");

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
        await AuthenticateAsUserAsync();
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

        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync("/api/v1/exercises?type=Strength&difficulty=Beginner");

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

        var response = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));

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
        
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateExercise_WithRegularUser_ShouldReturn403()
    {
        await AuthenticateAsUserAsync();
        
        var createRequestJson = """
        {
            "name": "Unauthorized Exercise",
            "type": "Strength",
            "muscleGroups": ["Arms"]
        }
        """;

        var response = await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateExercise_WithInvalidData_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();
        
        var invalidRequestJson = """
        {
            "name": "",
            "type": "Strength",
            "muscleGroups": ["Arms"]
        }
        """;

        var response = await Client.PostAsync("/api/v1/exercises",
            new StringContent(invalidRequestJson, System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("validation");
    }

    [Fact]
    public async Task GetExerciseById_WithValidId_ShouldReturnExercise()
    {
        await AuthenticateAsAdminAsync();
        
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

        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync($"/api/v1/exercises/{exerciseId}");

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
        await AuthenticateAsUserAsync();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/v1/exercises/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateExercise_WithValidData_ShouldUpdate()
    {
        await AuthenticateAsAdminAsync();
        
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

        var updateRequestJson = """
        {
            "name": "Updated Exercise",
            "description": "Updated description",
            "difficulty": "Advanced"
        }
        """;

        var response = await Client.PutAsync($"/api/v1/exercises/{exerciseId}",
            new StringContent(updateRequestJson, System.Text.Encoding.UTF8, "application/json"));

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
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "Unique Push-up Variation",
            "type": "Strength",
            "muscleGroups": ["Chest"]
        }
        """;

        await Client.PostAsync("/api/v1/exercises",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync("/api/v1/exercises/search?term=Unique");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Unique");
    }

    [Fact]
    public async Task ActivateExercise_WithValidId_ShouldActivate()
    {
        await AuthenticateAsAdminAsync();
        
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

        await Client.PostAsync($"/api/v1/exercises/{exerciseId}/deactivate", null);

        var response = await Client.PostAsync($"/api/v1/exercises/{exerciseId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getResponse = await Client.GetAsync($"/api/v1/exercises/{exerciseId}");
        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("\"isActive\":true");
    }

    [Fact]
    public async Task DeactivateExercise_WithValidId_ShouldDeactivate()
    {
        await AuthenticateAsAdminAsync();
        
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

        var response = await Client.PostAsync($"/api/v1/exercises/{exerciseId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getResponse = await Client.GetAsync($"/api/v1/exercises/{exerciseId}");
        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("\"isActive\":false");
    }

    [Fact]
    public async Task DeleteExercise_WithAdminRole_ShouldDelete()
    {
        await AuthenticateAsAdminAsync();
        
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

        var response = await Client.DeleteAsync($"/api/v1/exercises/{exerciseId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync($"/api/v1/exercises/{exerciseId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExerciseExists_WithValidId_ShouldReturnOk()
    {
        await AuthenticateAsAdminAsync();
        
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

        await AuthenticateAsUserAsync();

        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"/api/v1/exercises/{exerciseId}"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    private static string ExtractIdFromJsonResponse(string jsonContent)
    {
        var idStart = jsonContent.IndexOf("\"id\":\"") + 6;
        var idEnd = jsonContent.IndexOf("\"", idStart);
        return jsonContent.Substring(idStart, idEnd - idStart);
    }
}




using FitnessApp.IntegrationTests.Infrastructure;
using System.Net;

namespace FitnessApp.IntegrationTests.Tests.Workouts;

public class WorkoutHttpIntegrationTests : IntegrationTestBase
{
    public WorkoutHttpIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory) {}

    [Fact]
    public async Task GetWorkouts_WithoutAuthentication_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/workouts/templates");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTemplateWorkouts_WithAuthentication_ShouldReturnWorkouts()
    {
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync("/api/v1/workouts/templates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        content.Should().StartWith("[");
        content.Should().EndWith("]");
    }

    [Fact]
    public async Task CreateTemplateWorkout_WithAdminRole_ShouldCreate()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "Strength Training - Intermediate Level",
            "description": "Bodyweight strength training program for intermediate level",
            "type": "Template",
            "category": "Strength",
            "difficulty": "Intermediate",
            "estimatedDurationMinutes": 40,
            "phases": [
                {
                    "type": "WarmUp",
                    "name": "Dynamic warm-up",
                    "description": "Muscle activation and light cardio",
                    "estimatedDurationMinutes": 8,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440100",
                            "durationSeconds": 60,
                            "sets": 2,
                            "restTimeSeconds": 30
                        }
                    ]
                },
                {
                    "type": "MainEffort",
                    "name": "Strength Circuit",
                    "description": "3 rounds of 4 exercises, focus on form",
                    "estimatedDurationMinutes": 27,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440202",
                            "reps": 12,
                            "sets": 3,
                            "restTimeSeconds": 60
                        }
                    ]
                }
            ]
        }
        """;

        var response = await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        content.Should().Contain("\"name\":\"Strength Training - Intermediate Level\"");
        content.Should().Contain("\"type\":\"Template\"");
        content.Should().Contain("\"category\":\"Strength\"");
        content.Should().Contain("\"difficulty\":\"Intermediate\"");
        content.Should().Contain("\"estimatedDurationMinutes\":40");
        content.Should().Contain("\"id\":\"");
        
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTemplateWorkout_WithRegularUser_ShouldReturn403()
    {
        await AuthenticateAsUserAsync();
        
        var createRequestJson = """
        {
            "name": "Unauthorized Workout",
            "type": "Template",
            "category": "Strength",
            "difficulty": "Beginner",
            "estimatedDurationMinutes": 30
        }
        """;

        var response = await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUserWorkout_WithUser_ShouldCreate()
    {
        await AuthenticateAsUserAsync();
        
        var createRequestJson = """
        {
            "name": "My Personal Workout",
            "description": "Custom workout created by user",
            "type": "UserCreated",
            "category": "Cardio",
            "difficulty": "Beginner",
            "estimatedDurationMinutes": 25
        }
        """;

        var response = await Client.PostAsync("/api/v1/workouts/my-workouts",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        content.Should().Contain("\"name\":\"My Personal Workout\"");
        content.Should().Contain("\"type\":\"UserCreated\"");
        content.Should().Contain("\"category\":\"Cardio\"");
        content.Should().Contain("\"difficulty\":\"Beginner\"");
        content.Should().Contain("\"id\":\"");
    }

    [Fact]
    public async Task GetWorkoutById_WithValidId_ShouldReturnWorkout()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "Test Workout to Get",
            "description": "Workout for testing GET by ID",
            "type": "Template",
            "category": "Flexibility",
            "difficulty": "Beginner",
            "estimatedDurationMinutes": 20
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var workoutId = ExtractIdFromJsonResponse(createContent);

        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync($"/api/v1/workouts/{workoutId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain($"\"id\":\"{workoutId}\"");
        content.Should().Contain("\"name\":\"Test Workout to Get\"");
        content.Should().Contain("\"category\":\"Flexibility\"");
    }

    [Fact]
    public async Task GetWorkoutById_WithInvalidId_ShouldReturn404()
    {
        await AuthenticateAsUserAsync();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/v1/workouts/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWorkoutAsAdmin_WithValidData_ShouldUpdate()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "Original Workout",
            "description": "Original description",
            "type": "Template",
            "category": "Strength",
            "difficulty": "Beginner",
            "estimatedDurationMinutes": 30
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var workoutId = ExtractIdFromJsonResponse(createContent);

        var updateRequestJson = """
        {
            "name": "Updated Workout Name",
            "description": "Updated workout description",
            "difficulty": "Advanced",
            "estimatedDurationMinutes": 45
        }
        """;

        var response = await Client.PutAsync($"/api/v1/workouts/admin/{workoutId}",
            new StringContent(updateRequestJson, System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("\"name\":\"Updated Workout Name\"");
        content.Should().Contain("\"description\":\"Updated workout description\"");
        content.Should().Contain("\"difficulty\":\"Advanced\"");
        content.Should().Contain("\"estimatedDurationMinutes\":45");
    }

    [Fact]
    public async Task SearchWorkouts_WithValidTerm_ShouldReturnMatches()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "Unique HIIT Training",
            "description": "High intensity interval training",
            "type": "Template",
            "category": "Cardio",
            "difficulty": "Advanced",
            "estimatedDurationMinutes": 30
        }
        """;

        await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync("/api/v1/workouts/search?searchTerm=Unique");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Unique");
    }

    [Fact]
    public async Task GetWorkoutsByCategory_ShouldFilterCorrectly()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "Stretching Session",
            "type": "Template",
            "category": "Flexibility",
            "difficulty": "Beginner",
            "estimatedDurationMinutes": 15
        }
        """;

        await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync("/api/v1/workouts/category/Flexibility");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Flexibility");
    }

    [Fact]
    public async Task GetActiveWorkouts_ShouldReturnOnlyActiveWorkouts()
    {
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync("/api/v1/workouts/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        content.Should().StartWith("[");
        content.Should().EndWith("]");
    }

    [Fact]
    public async Task DeleteWorkoutAsAdmin_ShouldDelete()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "Workout to Delete",
            "type": "Template",
            "category": "Strength",
            "difficulty": "Beginner",
            "estimatedDurationMinutes": 30
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var workoutId = ExtractIdFromJsonResponse(createContent);

        var response = await Client.DeleteAsync($"/api/v1/workouts/admin/{workoutId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync($"/api/v1/workouts/{workoutId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WorkoutExists_WithValidId_ShouldReturnOk()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = """
        {
            "name": "Exists Test Workout",
            "type": "Template",
            "category": "Cardio",
            "difficulty": "Intermediate",
            "estimatedDurationMinutes": 35
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var workoutId = ExtractIdFromJsonResponse(createContent);

        await AuthenticateAsUserAsync();

        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"/api/v1/workouts/{workoutId}"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateWorkout_WithInvalidData_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();
        
        var invalidRequestJson = """
        {
            "name": "",
            "type": "Template",
            "category": "Strength",
            "difficulty": "Beginner",
            "estimatedDurationMinutes": -5
        }
        """;

        var response = await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(invalidRequestJson, System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("validation");
    }

    [Fact]
    public async Task GetMyWorkouts_WithUser_ShouldReturnUserWorkouts()
    {
        await AuthenticateAsUserAsync();
        
        var createRequestJson = """
        {
            "name": "My Custom Training",
            "type": "UserCreated",
            "category": "Strength",
            "difficulty": "Intermediate",
            "estimatedDurationMinutes": 40
        }
        """;

        await Client.PostAsync("/api/v1/workouts/my-workouts",
            new StringContent(createRequestJson, System.Text.Encoding.UTF8, "application/json"));

        var response = await Client.GetAsync("/api/v1/workouts/my-workouts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("My Custom Training");
    }

    [Fact]
    public async Task CreateCardioWorkout_WithComplexStructure_ShouldCreate()
    {
        await AuthenticateAsAdminAsync();
        
        var cardioWorkoutJson = """
        {
            "name": "Express Cardio - 15 minutes",
            "description": "Intensive 15-minute cardio workout to burn maximum calories",
            "type": "Template",
            "category": "Cardio",
            "difficulty": "Advanced",
            "estimatedDurationMinutes": 15,
            "phases": [
                {
                    "type": "WarmUp",
                    "name": "Quick activation",
                    "estimatedDurationMinutes": 2,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440100",
                            "durationSeconds": 60,
                            "sets": 2,
                            "restTimeSeconds": 0
                        }
                    ]
                },
                {
                    "type": "MainEffort",
                    "name": "Intense Cardio",
                    "description": "Tabata - 20s all-out / 10s rest, 4 rounds",
                    "estimatedDurationMinutes": 12,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440101",
                            "durationSeconds": 20,
                            "sets": 8,
                            "restTimeSeconds": 10,
                            "notes": "Maximum intensity"
                        }
                    ]
                },
                {
                    "type": "Recovery",
                    "name": "Cool down",
                    "estimatedDurationMinutes": 1,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440100",
                            "durationSeconds": 30,
                            "sets": 1,
                            "notes": "Very slow tempo for recovery"
                        }
                    ]
                }
            ]
        }
        """;

        var response = await Client.PostAsync("/api/v1/workouts/templates",
            new StringContent(cardioWorkoutJson, System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("\"name\":\"Express Cardio - 15 minutes\"");
        content.Should().Contain("\"category\":\"Cardio\"");
        content.Should().Contain("\"difficulty\":\"Advanced\"");
        content.Should().Contain("\"estimatedDurationMinutes\":15");
    }
    
    private static string ExtractIdFromJsonResponse(string jsonContent)
    {
        var idStart = jsonContent.IndexOf("\"id\":\"") + 6;
        var idEnd = jsonContent.IndexOf("\"", idStart);
        return jsonContent.Substring(idStart, idEnd - idStart);
    }
}

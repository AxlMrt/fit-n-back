using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.IntegrationTests.Helpers;
using System.Net;
using static FitnessApp.IntegrationTests.Helpers.ApiJsonTemplates;

namespace FitnessApp.IntegrationTests.Tests.Workouts;

public class WorkoutHttpIntegrationTests : IntegrationTestBase
{
    public WorkoutHttpIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory) {}

    [Fact]
    public async Task GetWorkouts_WithoutAuthentication_ShouldReturn401()
    {
        var response = await Client.GetAsync(ApiEndpoints.Workouts.Templates);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTemplateWorkouts_WithAuthentication_ShouldReturnWorkouts()
    {
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync(ApiEndpoints.Workouts.Templates);

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
        
        var createRequestJson = CreateComplexWorkoutTemplate(
            "Strength Training - Intermediate Level",
            "Bodyweight strength training program for intermediate level");

        var response = await Client.PostAsync(ApiEndpoints.Workouts.Templates, createRequestJson.ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        content.Should().Contain("\"name\":\"Strength Training - Intermediate Level\"");
        content.Should().Contain("\"type\":\"Template\"");
        content.Should().Contain("\"category\":\"Strength\"");
        content.Should().Contain("\"difficulty\":\"Intermediate\"");
        content.Should().Contain("\"estimatedDurationMinutes\":");
        content.Should().Contain("\"id\":\"");
        
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTemplateWorkout_WithRegularUser_ShouldReturn403()
    {
        await AuthenticateAsUserAsync();
        
        var createRequestJson = CreateSimpleWorkoutTemplate("Unauthorized Workout");

        var response = await Client.PostAsync(ApiEndpoints.Workouts.Templates, createRequestJson.ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUserWorkout_WithUser_ShouldCreate()
    {
        await AuthenticateAsUserAsync();
        
        var createRequestJson = CreateUserWorkout("My Personal Workout");

        var response = await Client.PostAsync(ApiEndpoints.Workouts.MyWorkouts, createRequestJson.ToStringContent());

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
        
        var createRequestJson = CreateSimpleWorkoutTemplate(
            "Test Workout to Get", 
            "Workout for testing GET by ID",
            category: "Flexibility",
            difficulty: "Beginner",
            estimatedDurationMinutes: 20);

        var createResponse = await Client.PostAsync(ApiEndpoints.Workouts.Templates, createRequestJson.ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var workoutId = ExtractIdFromJsonResponse(createContent);

        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync(ApiEndpoints.Workouts.GetById(Guid.Parse(workoutId)));

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

        var response = await Client.GetAsync(ApiEndpoints.Workouts.GetById(nonExistentId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWorkoutAsAdmin_WithValidData_ShouldUpdate()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = CreateSimpleWorkoutTemplate(
            "Original Workout",
            "Original description");

        var createResponse = await Client.PostAsync(ApiEndpoints.Workouts.Templates, createRequestJson.ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var workoutId = ExtractIdFromJsonResponse(createContent);

        var updateRequestJson = UpdateWorkout(
            name: "Updated Workout Name",
            description: "Updated workout description",
            difficulty: "Advanced",
            estimatedDurationMinutes: 45);

        var response = await Client.PutAsync(ApiEndpoints.Workouts.AdminUpdate(Guid.Parse(workoutId)), 
            updateRequestJson.ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("\"name\":\"Updated Workout Name\"");
        content.Should().Contain("\"description\":\"Updated workout description\"");
        content.Should().Contain("\"difficulty\":\"Advanced\"");
        content.Should().Contain("\"estimatedDurationMinutes\":");
    }

    [Fact]
    public async Task SearchWorkouts_WithValidTerm_ShouldReturnMatches()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = CreateSimpleWorkoutTemplate(
            "Unique HIIT Training",
            "High intensity interval training",
            category: "Cardio",
            difficulty: "Advanced");

        await Client.PostAsync(ApiEndpoints.Workouts.Templates, createRequestJson.ToStringContent());
        
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync(ApiEndpoints.Workouts.Search("Unique"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Unique");
    }

    [Fact]
    public async Task GetWorkoutsByCategory_ShouldFilterCorrectly()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = CreateSimpleWorkoutTemplate(
            "Stretching Session",
            category: "Flexibility",
            estimatedDurationMinutes: 15);

        await Client.PostAsync(ApiEndpoints.Workouts.Templates, createRequestJson.ToStringContent());
        
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync(ApiEndpoints.Workouts.GetByCategory("Flexibility"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Flexibility");
    }

    [Fact]
    public async Task GetActiveWorkouts_ShouldReturnOnlyActiveWorkouts()
    {
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync(ApiEndpoints.Workouts.GetActive);

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
        
        var createRequestJson = CreateSimpleWorkoutTemplate("Workout to Delete");

        var createResponse = await Client.PostAsync(ApiEndpoints.Workouts.Templates, createRequestJson.ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var workoutId = ExtractIdFromJsonResponse(createContent);

        var response = await Client.DeleteAsync(ApiEndpoints.Workouts.AdminDelete(Guid.Parse(workoutId)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync(ApiEndpoints.Workouts.GetById(Guid.Parse(workoutId)));
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WorkoutExists_WithValidId_ShouldReturnOk()
    {
        await AuthenticateAsAdminAsync();
        
        var createRequestJson = CreateSimpleWorkoutTemplate(
            "Exists Test Workout",
            category: "Cardio",
            difficulty: "Intermediate",
            estimatedDurationMinutes: 35);

        var createResponse = await Client.PostAsync(ApiEndpoints.Workouts.Templates, createRequestJson.ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var workoutId = ExtractIdFromJsonResponse(createContent);

        await AuthenticateAsUserAsync();

        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, ApiEndpoints.Workouts.Head(Guid.Parse(workoutId))));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateWorkout_WithInvalidData_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();
        
        var invalidRequestJson = CreateInvalidWorkout();

        var response = await Client.PostAsync(ApiEndpoints.Workouts.Templates, invalidRequestJson.ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("validation");
    }

    [Fact]
    public async Task GetMyWorkouts_WithUser_ShouldReturnUserWorkouts()
    {
        await AuthenticateAsUserAsync();
        
        var createRequestJson = CreateUserWorkout(
            "My Custom Training",
            category: "Strength",
            difficulty: "Intermediate",
            estimatedDurationMinutes: 40);

        await Client.PostAsync(ApiEndpoints.Workouts.MyWorkouts, createRequestJson.ToStringContent());

        var response = await Client.GetAsync(ApiEndpoints.Workouts.MyWorkouts);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("My Custom Training");
    }

    [Fact]
    public async Task CreateCardioWorkout_WithComplexStructure_ShouldCreate()
    {
        await AuthenticateAsAdminAsync();
        
        var cardioWorkoutJson = CreateCardioWorkout();

        var response = await Client.PostAsync(ApiEndpoints.Workouts.Templates, cardioWorkoutJson.ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("\"name\":\"Express Cardio - 15 minutes\"");
        content.Should().Contain("\"category\":\"Cardio\"");
        content.Should().Contain("\"difficulty\":\"Advanced\"");
        content.Should().Contain("\"estimatedDurationMinutes\":");
    }
    
    private static string ExtractIdFromJsonResponse(string jsonContent)
    {
        var idStart = jsonContent.IndexOf("\"id\":\"") + 6;
        var idEnd = jsonContent.IndexOf("\"", idStart);
        return jsonContent.Substring(idStart, idEnd - idStart);
    }
}

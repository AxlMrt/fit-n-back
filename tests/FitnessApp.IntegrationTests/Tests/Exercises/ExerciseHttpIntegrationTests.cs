using FitnessApp.IntegrationTests.Infrastructure;
using System.Net;
using FitnessApp.IntegrationTests.Helpers;
using static FitnessApp.IntegrationTests.Helpers.ApiJsonTemplates;

namespace FitnessApp.IntegrationTests.Tests.Exercises;

public class ExerciseHttpIntegrationTests : IntegrationTestBase
{
    public ExerciseHttpIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetExercises_WithoutAuthentication_ShouldReturn401()
    {
        var response = await Client.GetAsync(ApiEndpoints.Exercises.List);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExercises_WithAuthentication_ShouldReturnPagedResults()
    {
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync(ApiEndpoints.Exercises.List);

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
        
        var createResponse = await Client.PostAsync(ApiEndpoints.Exercises.Create, 
            CreateExercise("Push-ups Integration Test", "A basic push-up exercise", "Strength", 
                new[] { "Chest", "Shoulders" }, "Beginner", new[] { "None" }).ToStringContent());
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync(ApiEndpoints.Exercises.ListWithFilters("Strength", "Beginner"));

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
        
        var response = await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("New Exercise", "A test exercise", "Strength", 
                new[] { "Arms" }, "Intermediate", new[] { "Dumbbells" }, "Perform the exercise carefully").ToStringContent());

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Exercise creation failed with status {response.StatusCode}: {errorContent}");
        }

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
        
        var response = await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("Unauthorized Exercise", "Test description", "Strength", new[] { "Arms" }, "Beginner", new[] { "None" }).ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateExercise_WithInvalidData_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();
        
        var response = await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("", "", "Strength", new[] { "Arms" }, "Beginner", new[] { "None" }).ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("validation");
    }

    [Fact]
    public async Task GetExerciseById_WithValidId_ShouldReturnExercise()
    {
        await AuthenticateAsAdminAsync();
        
        var createResponse = await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("Get Test Exercise", "", "Cardio", new[] { "Legs" }, "Advanced", new string[0]).ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        createContent.Should().Contain("\"id\":\"");
        
        var exerciseId = ExtractIdFromJsonResponse(createContent);

        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync(ApiEndpoints.Exercises.GetById(Guid.Parse(exerciseId)));

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

        var response = await Client.GetAsync(ApiEndpoints.Exercises.GetById(nonExistentId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateExercise_WithValidData_ShouldUpdate()
    {
        await AuthenticateAsAdminAsync();
        
        var createResponse = await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("Original Exercise", "", "Strength", new[] { "Arms" }, "Beginner", new string[0]).ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);

        var response = await Client.PutAsync(ApiEndpoints.Exercises.Update(Guid.Parse(exerciseId)),
            UpdateExercise(name: "Updated Exercise", description: "Updated description", difficulty: "Advanced").ToStringContent());

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
        
        await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("Unique Push-up Variation", "A unique variation", "Strength", new[] { "Chest" }, "Beginner", new[] { "None" }).ToStringContent());
        
        await AuthenticateAsUserAsync();

        var response = await Client.GetAsync(ApiEndpoints.Exercises.Search("Unique"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Unique");
    }

    [Fact]
    public async Task ActivateExercise_WithValidId_ShouldActivate()
    {
        await AuthenticateAsAdminAsync();
        
        var createResponse = await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("Exercise to Activate", "Exercise to test activation", "Stretching", new[] { "Core" }, "Beginner", new[] { "None" }).ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);
        var exerciseGuid = Guid.Parse(exerciseId);

        await Client.PostAsync(ApiEndpoints.Exercises.Deactivate(exerciseGuid), null);

        var response = await Client.PostAsync(ApiEndpoints.Exercises.Activate(exerciseGuid), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getResponse = await Client.GetAsync(ApiEndpoints.Exercises.GetById(exerciseGuid));
        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("\"isActive\":true");
    }

    [Fact]
    public async Task DeactivateExercise_WithValidId_ShouldDeactivate()
    {
        await AuthenticateAsAdminAsync();
        
        var createResponse = await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("Exercise to Deactivate", "Exercise to test deactivation", "Strength", new[] { "Legs" }, "Beginner", new[] { "None" }).ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);
        var exerciseGuid = Guid.Parse(exerciseId);

        var response = await Client.PostAsync(ApiEndpoints.Exercises.Deactivate(exerciseGuid), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getResponse = await Client.GetAsync(ApiEndpoints.Exercises.GetById(exerciseGuid));
        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("\"isActive\":false");
    }

    [Fact]
    public async Task DeleteExercise_WithAdminRole_ShouldDelete()
    {
        await AuthenticateAsAdminAsync();
        
        var createResponse = await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("Exercise to Delete", "Exercise to test deletion", "Cardio", new[] { "Legs" }, "Beginner", new[] { "None" }).ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);
        var exerciseGuid = Guid.Parse(exerciseId);

        var response = await Client.DeleteAsync(ApiEndpoints.Exercises.Delete(exerciseGuid));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync(ApiEndpoints.Exercises.GetById(exerciseGuid));
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExerciseExists_WithValidId_ShouldReturnOk()
    {
        await AuthenticateAsAdminAsync();
        
        var createResponse = await Client.PostAsync(ApiEndpoints.Exercises.Create,
            CreateExercise("Exists Test Exercise", "Exercise to test existence", "Strength", new[] { "Arms" }, "Beginner", new[] { "None" }).ToStringContent());
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var exerciseId = ExtractIdFromJsonResponse(createContent);
        var exerciseGuid = Guid.Parse(exerciseId);

        await AuthenticateAsUserAsync();

        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, ApiEndpoints.Exercises.GetById(exerciseGuid)));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    private static string ExtractIdFromJsonResponse(string jsonContent)
    {
        // Try different patterns for ID extraction
        var patterns = new[] { "\"id\":\"", "\"Id\":\"", "\"id\": \"", "\"Id\": \"" };
        
        foreach (var pattern in patterns)
        {
            var idStartIndex = jsonContent.IndexOf(pattern);
            if (idStartIndex != -1)
            {
                var idStart = idStartIndex + pattern.Length;
                var idEnd = jsonContent.IndexOf("\"", idStart);
                if (idEnd != -1)
                {
                    return jsonContent.Substring(idStart, idEnd - idStart);
                }
            }
        }
        
        throw new InvalidOperationException($"Could not extract ID from JSON response: {jsonContent}");
    }
}




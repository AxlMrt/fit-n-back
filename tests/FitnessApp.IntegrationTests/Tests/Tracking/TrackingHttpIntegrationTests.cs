using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.IntegrationTests.Helpers;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FitnessApp.IntegrationTests.Tests.Tracking;

/// <summary>
/// Tests d'intégration HTTP pour le module Tracking
/// </summary>
public class TrackingHttpIntegrationTests : IntegrationTestBase
{
    public TrackingHttpIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region User Metrics CRUD Tests

    [Fact]
    public async Task RecordUserMetric_WithValidData_ShouldCreate()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Act
        var response = await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric,
            ApiJsonTemplates.CreateUserMetric("Weight", 75.5, "kg", "2024-09-01T08:00:00Z", "After morning workout").ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"value\":75.5");
        content.Should().Contain("\"unit\":\"kg\"");
        content.Should().Contain("\"notes\":\"After morning workout\"");
        content.Should().Contain("\"metricType\":\"Weight\"");
    }

    [Fact]
    public async Task UpdateUserMetric_WithValidData_ShouldUpdate()
    {
        // Arrange - Create a metric first
        await AuthenticateAsUserAsync();

        var createResponse = await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric,
            ApiJsonTemplates.CreateHeightMetric(175.0, "Initial measurement").ToStringContent());

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createContent);
        var metricId = createDoc.RootElement.GetProperty("id").GetString();

        // Act - Update the metric
        var updateResponse = await Client.PutAsync(ApiEndpoints.Tracking.UpdateMetric(Guid.Parse(metricId!)),
            ApiJsonTemplates.UpdateUserMetric("Height", 176.5, "Updated measurement after growth").ToStringContent());

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updateContent = await updateResponse.Content.ReadAsStringAsync();
        
        updateContent.Should().Contain("\"value\":176.5");
        updateContent.Should().Contain("\"notes\":\"Updated measurement after growth\"");
    }

    [Fact]
    public async Task GetUserMetrics_ShouldReturnUserMetrics()
    {
        // Arrange - Create some metrics
        await AuthenticateAsUserAsync();

        var metrics = new[]
        {
            ApiJsonTemplates.CreateWeightMetric(75.0),
            ApiJsonTemplates.CreateHeightMetric(175.0)
        };

        foreach (var metricJson in metrics)
        {
            await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric, metricJson.ToStringContent());
        }

        // Act
        var response = await Client.GetAsync(ApiEndpoints.Tracking.GetMetrics);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"metricType\":\"Weight\"");
        content.Should().Contain("\"value\":75");
        content.Should().Contain("\"metricType\":\"Height\"");
        content.Should().Contain("\"value\":175");
    }

    [Fact]
    public async Task GetUserMetricsByType_ShouldFilterCorrectly()
    {
        // Arrange - Create multiple metrics of different types
        await AuthenticateAsUserAsync();

        var metrics = new[]
        {
            ApiJsonTemplates.CreateUserMetric("Weight", 75.0, recordedAt: "2024-09-01T08:00:00Z"),
            ApiJsonTemplates.CreateUserMetric("Weight", 74.5, recordedAt: "2024-09-02T08:00:00Z"),
            ApiJsonTemplates.CreateUserMetric("Height", 175.0, recordedAt: "2024-09-02T08:00:00Z")
        };

        foreach (var metricJson in metrics)
        {
            await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric, metricJson.ToStringContent());
        }

        // Act
        var response = await Client.GetAsync(ApiEndpoints.Tracking.GetMetricsByType("Weight"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        // Should only contain Weight metrics
        content.Should().Contain("\"metricType\":\"Weight\"");
        content.Should().NotContain("\"metricType\":\"Height\"");
        
        // Should contain both weight values
        content.Should().Contain("\"value\":75");
        content.Should().Contain("\"value\":74.5");
    }

    [Fact]
    public async Task GetLatestMetric_ShouldReturnMostRecent()
    {
        // Arrange - Create multiple weight metrics
        await AuthenticateAsUserAsync();

        var metrics = new[]
        {
            ApiJsonTemplates.CreateUserMetric("Weight", 76.0, recordedAt: "2024-08-28T08:00:00Z"),
            ApiJsonTemplates.CreateUserMetric("Weight", 75.5, recordedAt: "2024-08-30T08:00:00Z"),
            ApiJsonTemplates.CreateUserMetric("Weight", 75.8, recordedAt: "2024-08-26T08:00:00Z")
        };

        foreach (var metricJson in metrics)
        {
            await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric, metricJson.ToStringContent());
        }

        // Act
        var response = await Client.GetAsync(ApiEndpoints.Tracking.GetLatestMetric("Weight"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        // Should return the most recent metric (75.5 from Aug 30)
        content.Should().Contain("\"value\":75.5");
        content.Should().Contain("\"metricType\":\"Weight\"");
    }

    [Fact]
    public async Task DeleteUserMetric_ShouldRemoveMetric()
    {
        // Arrange - Create a metric first
        await AuthenticateAsUserAsync();

        var createRequestJson = ApiJsonTemplates.CreateUserMetric("Weight", 75.0, notes: "To be deleted");

        var createResponse = await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric,
            createRequestJson.ToStringContent());

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createContent);
        var metricId = createDoc.RootElement.GetProperty("id").GetString();

        // Act
        var deleteResponse = await Client.DeleteAsync(ApiEndpoints.Tracking.DeleteMetric(Guid.Parse(metricId!)));

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the metric is deleted by trying to get it
        var getResponse = await Client.GetAsync(ApiEndpoints.Tracking.GetMetrics);
        var getContent = await getResponse.Content.ReadAsStringAsync();
        
        getContent.Should().NotContain("\"notes\":\"To be deleted\"");
    }

    [Fact]
    public async Task RecordUserMetric_WithDifferentUnits_ShouldPersistCorrectly()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var metrics = new[]
        {
            ApiJsonTemplates.CreateUserMetric("Weight", 75.0, unit: "kg"),
            ApiJsonTemplates.CreateUserMetric("Weight", 165.3, unit: "lbs"),
            ApiJsonTemplates.CreateUserMetric("Height", 175.0, unit: "cm"),
            ApiJsonTemplates.CreateUserMetric("Height", 68.9, unit: "in")
        };

        // Act
        foreach (var metricJson in metrics)
        {
            var response = await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric,
                metricJson.ToStringContent());
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert
        var getResponse = await Client.GetAsync(ApiEndpoints.Tracking.GetMetrics);
        var content = await getResponse.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"unit\":\"kg\"");
        content.Should().Contain("\"unit\":\"lbs\"");
        content.Should().Contain("\"unit\":\"cm\"");
        content.Should().Contain("\"unit\":\"in\"");
        content.Should().Contain("\"value\":75");
        content.Should().Contain("\"value\":165.3");
        content.Should().Contain("\"value\":175");
        content.Should().Contain("\"value\":68.9");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task RecordUserMetric_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var invalidRequestJson = """
        {
            "metricType": "Weight",
            "value": -10.0
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/tracking/metrics",
            new StringContent(invalidRequestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserMetric_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var nonExistentId = Guid.NewGuid();
        var updateRequestJson = ApiJsonTemplates.UpdateUserMetric("Weight", 75.0);

        // Act
        var response = await Client.PutAsync(ApiEndpoints.Tracking.UpdateMetric(nonExistentId),
            updateRequestJson.ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLatestMetric_WithNoMetricsOfType_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Act
        var response = await Client.GetAsync(ApiEndpoints.Tracking.GetLatestMetric("PersonalRecord"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUserMetric_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync(ApiEndpoints.Tracking.DeleteMetric(nonExistentId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task RecordUserMetric_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange - No authentication

        var requestJson = ApiJsonTemplates.CreateUserMetric("Weight", 75.0);

        // Act
        var response = await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric,
            requestJson.ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserMetrics_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiEndpoints.Tracking.GetMetrics);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task CompleteUserMetricWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Act & Assert 1: Create initial metric
        var createRequestJson = ApiJsonTemplates.CreateUserMetric("Weight", 80.0, notes: "Starting weight");

        var createResponse = await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric,
            createRequestJson.ToStringContent());

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createContent);
        var metricId = createDoc.RootElement.GetProperty("id").GetString();

        // Act & Assert 2: Update the metric
        var updateRequestJson = ApiJsonTemplates.UpdateUserMetric("Weight", 78.5, notes: "Week 2 progress");

        var updateResponse = await Client.PutAsync(ApiEndpoints.Tracking.UpdateMetric(Guid.Parse(metricId!)),
            updateRequestJson.ToStringContent());

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert 3: Get latest metric
        var latestResponse = await Client.GetAsync(ApiEndpoints.Tracking.GetLatestMetric("Weight"));
        latestResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var latestContent = await latestResponse.Content.ReadAsStringAsync();
        
        latestContent.Should().Contain("\"value\":78.5");
        latestContent.Should().Contain("\"notes\":\"Week 2 progress\"");

        // Act & Assert 4: Get all metrics
        var allResponse = await Client.GetAsync(ApiEndpoints.Tracking.GetMetrics);
        allResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var allContent = await allResponse.Content.ReadAsStringAsync();
        
        allContent.Should().Contain("\"metricType\":\"Weight\"");

        // Act & Assert 5: Delete the metric
        var deleteResponse = await Client.DeleteAsync(ApiEndpoints.Tracking.DeleteMetric(Guid.Parse(metricId!)));
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MultipleMetricTypes_ShouldBeHandledCorrectly()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Create different metric types
        var metrics = new[]
        {
            ApiJsonTemplates.CreateUserMetric("Weight", 75.0, unit: "kg"),
            ApiJsonTemplates.CreateUserMetric("Height", 175.0, unit: "cm"), 
            ApiJsonTemplates.CreateUserMetric("PersonalRecord", 15.5, unit: "reps")
        };

        // Act - Create different metric types
        foreach (var metricJson in metrics)
        {
            var response = await Client.PostAsync(ApiEndpoints.Tracking.CreateMetric,
                metricJson.ToStringContent());

            // Debug: Capture error response if not OK
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Request failed with status {response.StatusCode}. Request JSON: {metricJson}. Error: {errorContent}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert - Verify all metrics exist
        var getResponse = await Client.GetAsync(ApiEndpoints.Tracking.GetMetrics);
        var content = await getResponse.Content.ReadAsStringAsync();
        
        content.Should().Contain("\"metricType\":\"Weight\"");
        content.Should().Contain("\"metricType\":\"Height\"");
        content.Should().Contain("\"metricType\":\"PersonalRecord\"");
        content.Should().Contain("\"value\":75");
        content.Should().Contain("\"value\":175");
        content.Should().Contain("\"value\":15.5");
    }

    #endregion
}

using FitnessApp.IntegrationTests.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;

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

        var requestJson = """
        {
            "metricType": "Weight",
            "value": 75.5,
            "recordedAt": "2024-09-01T08:00:00Z",
            "notes": "After morning workout",
            "unit": "kg"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/tracking/metrics",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

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

        var createRequestJson = """
        {
            "metricType": "Height",
            "value": 175.0,
            "notes": "Initial measurement",
            "unit": "cm"
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/tracking/metrics",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createContent);
        var metricId = createDoc.RootElement.GetProperty("id").GetString();

        // Act - Update the metric
        var updateRequestJson = """
        {
            "metricType": "Height",
            "value": 176.5,
            "notes": "Updated measurement after growth"
        }
        """;

        var updateResponse = await Client.PutAsync($"/api/v1/tracking/metrics/{metricId}",
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

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
            """
            {
                "metricType": "Weight",
                "value": 75.0,
                "unit": "kg"
            }
            """,
            """
            {
                "metricType": "Height", 
                "value": 175.0,
                "unit": "cm"
            }
            """
        };

        foreach (var metricJson in metrics)
        {
            await Client.PostAsync("/api/v1/tracking/metrics",
                new StringContent(metricJson, Encoding.UTF8, "application/json"));
        }

        // Act
        var response = await Client.GetAsync("/api/v1/tracking/metrics");

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
            """
            {
                "metricType": "Weight",
                "value": 75.0,
                "recordedAt": "2024-09-01T08:00:00Z"
            }
            """,
            """
            {
                "metricType": "Weight",
                "value": 74.5,
                "recordedAt": "2024-09-02T08:00:00Z"
            }
            """,
            """
            {
                "metricType": "Height",
                "value": 175.0,
                "recordedAt": "2024-09-02T08:00:00Z"
            }
            """
        };

        foreach (var metricJson in metrics)
        {
            await Client.PostAsync("/api/v1/tracking/metrics",
                new StringContent(metricJson, Encoding.UTF8, "application/json"));
        }

        // Act
        var response = await Client.GetAsync("/api/v1/tracking/metrics/Weight");

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
            """
            {
                "metricType": "Weight",
                "value": 76.0,
                "recordedAt": "2024-08-28T08:00:00Z"
            }
            """,
            """
            {
                "metricType": "Weight",
                "value": 75.5,
                "recordedAt": "2024-08-30T08:00:00Z"
            }
            """,
            """
            {
                "metricType": "Weight",
                "value": 75.8,
                "recordedAt": "2024-08-26T08:00:00Z"
            }
            """
        };

        foreach (var metricJson in metrics)
        {
            await Client.PostAsync("/api/v1/tracking/metrics",
                new StringContent(metricJson, Encoding.UTF8, "application/json"));
        }

        // Act
        var response = await Client.GetAsync("/api/v1/tracking/metrics/Weight/latest");

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

        var createRequestJson = """
        {
            "metricType": "Weight",
            "value": 75.0,
            "notes": "To be deleted"
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/tracking/metrics",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createContent);
        var metricId = createDoc.RootElement.GetProperty("id").GetString();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/v1/tracking/metrics/{metricId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the metric is deleted by trying to get it
        var getResponse = await Client.GetAsync("/api/v1/tracking/metrics");
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
            """
            {
                "metricType": "Weight",
                "value": 75.0,
                "unit": "kg"
            }
            """,
            """
            {
                "metricType": "Weight",
                "value": 165.3,
                "unit": "lbs"
            }
            """,
            """
            {
                "metricType": "Height",
                "value": 175.0,
                "unit": "cm"
            }
            """,
            """
            {
                "metricType": "Height",
                "value": 68.9,
                "unit": "in"
            }
            """
        };

        // Act
        foreach (var metricJson in metrics)
        {
            var response = await Client.PostAsync("/api/v1/tracking/metrics",
                new StringContent(metricJson, Encoding.UTF8, "application/json"));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert
        var getResponse = await Client.GetAsync("/api/v1/tracking/metrics");
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
        var updateRequestJson = """
        {
            "metricType": "Weight",
            "value": 75.0
        }
        """;

        // Act
        var response = await Client.PutAsync($"/api/v1/tracking/metrics/{nonExistentId}",
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLatestMetric_WithNoMetricsOfType_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Act
        var response = await Client.GetAsync("/api/v1/tracking/metrics/PersonalRecord/latest");

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
        var response = await Client.DeleteAsync($"/api/v1/tracking/metrics/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task RecordUserMetric_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange - No authentication

        var requestJson = """
        {
            "metricType": "Weight",
            "value": 75.0
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/tracking/metrics",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserMetrics_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/tracking/metrics");

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
        var createRequestJson = """
        {
            "metricType": "Weight",
            "value": 80.0,
            "notes": "Starting weight"
        }
        """;

        var createResponse = await Client.PostAsync("/api/v1/tracking/metrics",
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createContent);
        var metricId = createDoc.RootElement.GetProperty("id").GetString();

        // Act & Assert 2: Update the metric
        var updateRequestJson = """
        {
            "metricType": "Weight",
            "value": 78.5,
            "notes": "Week 2 progress"
        }
        """;

        var updateResponse = await Client.PutAsync($"/api/v1/tracking/metrics/{metricId}",
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert 3: Get latest metric
        var latestResponse = await Client.GetAsync("/api/v1/tracking/metrics/Weight/latest");
        latestResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var latestContent = await latestResponse.Content.ReadAsStringAsync();
        
        latestContent.Should().Contain("\"value\":78.5");
        latestContent.Should().Contain("\"notes\":\"Week 2 progress\"");

        // Act & Assert 4: Get all metrics
        var allResponse = await Client.GetAsync("/api/v1/tracking/metrics");
        allResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var allContent = await allResponse.Content.ReadAsStringAsync();
        
        allContent.Should().Contain("\"metricType\":\"Weight\"");

        // Act & Assert 5: Delete the metric
        var deleteResponse = await Client.DeleteAsync($"/api/v1/tracking/metrics/{metricId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MultipleMetricTypes_ShouldBeHandledCorrectly()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // UserMetricType enum values: Weight = 0, Height = 1, PersonalRecord = 2
        var metrics = new[]
        {
            (0, 75.0, "kg"),        // Weight
            (1, 175.0, "cm"),       // Height
            (2, 15.5, "reps")       // PersonalRecord
        };

        // Act - Create different metric types
        foreach (var (typeValue, value, unit) in metrics)
        {
            // Use invariant culture to ensure proper decimal formatting (15.5 not 15,5)
            var valueString = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            
            var requestJson = $$"""
            {
                "metricType": {{typeValue}},
                "value": {{valueString}},
                "unit": "{{unit}}"
            }
            """;

            var response = await Client.PostAsync("/api/v1/tracking/metrics",
                new StringContent(requestJson, Encoding.UTF8, "application/json"));

            // Debug: Capture error response if not OK
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Request failed for MetricType {typeValue} with status {response.StatusCode}. Request JSON: {requestJson}. Error: {errorContent}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert - Verify all metrics exist
        var getResponse = await Client.GetAsync("/api/v1/tracking/metrics");
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

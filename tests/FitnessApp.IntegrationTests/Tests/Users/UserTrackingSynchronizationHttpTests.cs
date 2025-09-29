using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.IntegrationTests.Helpers;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FitnessApp.IntegrationTests.Tests.Users;

/// <summary>
/// Tests d'intégration HTTP pour la synchronisation cross-module Users ↔ Tracking
/// </summary>
public class UserTrackingSynchronizationHttpTests : IntegrationTestBase
{
    public UserTrackingSynchronizationHttpTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region User Profile Update Synchronization Tests

    [Fact]
    public async Task CreateProfile_ShouldSyncInitialMetricsToTracking()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = ApiJsonTemplates.CreateUserProfile(
            firstName: "Sync",
            lastName: "Test",
            dateOfBirth: "1990-05-15T00:00:00Z",
            gender: "Male",
            height: 180,
            weight: 75,
            fitnessLevel: "Enthousiast",
            fitnessGoal: "Muscle_Gain"
        );

        // Act - Create profile via HTTP
        var createResponse = await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        // Assert - Profile created successfully
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Wait for async sync to complete
        await Task.Delay(1000);

        // Verify metrics synced to Tracking module
        var metricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        metricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await metricsResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var metrics = doc.RootElement.EnumerateArray().ToList();

        // Should have height and weight metrics
        var heightMetric = metrics.FirstOrDefault(m => 
            m.GetProperty("metricType").GetString() == "Height");
        var weightMetric = metrics.FirstOrDefault(m => 
            m.GetProperty("metricType").GetString() == "Weight");

        heightMetric.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        heightMetric.GetProperty("value").GetDouble().Should().Be(180.0);
        heightMetric.GetProperty("unit").GetString().Should().Be("cm");
        heightMetric.GetProperty("notes").GetString().Should().Contain("Auto-sync");

        weightMetric.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        weightMetric.GetProperty("value").GetDouble().Should().Be(75.0);
        weightMetric.GetProperty("unit").GetString().Should().Be("kg");
        weightMetric.GetProperty("notes").GetString().Should().Contain("Auto-sync");
    }

    [Fact]
    public async Task UpdatePhysicalMeasurements_ShouldSyncToTrackingModule()
    {
        // Arrange - Create initial profile
        await AuthenticateAsUserAsync();

        var createRequestJson = ApiJsonTemplates.CreateUserProfile(
            firstName: "Update",
            lastName: "Test",
            dateOfBirth: "1988-06-30T00:00:00Z",
            gender: "Female",
            height: 170,
            weight: 65,
            fitnessLevel: "Beginner",
            fitnessGoal: "Weight_Loss"
        );

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        await Task.Delay(500); // Wait for initial sync

        // Act - Update physical measurements
        var updateRequestJson = ApiJsonTemplates.UpdatePhysicalMeasurements(
            height: 172,
            weight: 63,
            heightUnit: "cm",
            weightUnit: "kg"
        );

        var updateResponse = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        // Assert - Update successful
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Wait for async sync to complete
        await Task.Delay(1000);

        // Verify updated metrics in Tracking module
        var metricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        metricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await metricsResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var metrics = doc.RootElement.EnumerateArray().ToList();

        // Should have multiple entries for each metric type (initial + updated)
        var heightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Height").ToList();
        var weightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Weight").ToList();

        heightMetrics.Should().HaveCountGreaterOrEqualTo(2);
        weightMetrics.Should().HaveCountGreaterOrEqualTo(2);

        // Verify latest values
        var latestHeight = heightMetrics.OrderByDescending(m => 
            DateTime.Parse(m.GetProperty("recordedAt").GetString()!)).First();
        var latestWeight = weightMetrics.OrderByDescending(m => 
            DateTime.Parse(m.GetProperty("recordedAt").GetString()!)).First();

        latestHeight.GetProperty("value").GetDouble().Should().Be(172.0);
        latestHeight.GetProperty("unit").GetString().Should().Be("cm");
        
        latestWeight.GetProperty("value").GetDouble().Should().Be(63.0);
        latestWeight.GetProperty("unit").GetString().Should().Be("kg");
    }

    [Fact]
    public async Task UpdatePhysicalMeasurements_OnlyHeight_ShouldSyncOnlyHeight()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = ApiJsonTemplates.CreateUserProfile(
            firstName: "Height",
            lastName: "Only",
            dateOfBirth: "1990-01-01T00:00:00Z",
            gender: "Male",
            height: 175,
            weight: 70,
            fitnessLevel: "Beginner",
            fitnessGoal: "Muscle_Gain"
        );

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        await Task.Delay(500);

        // Clear existing metrics for clean test
        var initialMetricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var initialContent = await initialMetricsResponse.Content.ReadAsStringAsync();
        var initialDoc = JsonDocument.Parse(initialContent);
        var initialMetrics = initialDoc.RootElement.EnumerateArray().ToList();
        var initialWeightCount = initialMetrics.Count(m => 
            m.GetProperty("metricType").GetString() == "Weight");

        // Act - Update only height
        var updateRequestJson = ApiJsonTemplates.UpdatePhysicalMeasurementsPartial(
            height: 177,
            heightUnit: "cm",
            weightUnit: "kg"
        );

        var updateResponse = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Wait for async sync
        await Task.Delay(1000);

        // Assert - Only height should be synced
        var metricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var content = await metricsResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var metrics = doc.RootElement.EnumerateArray().ToList();

        var heightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Height").ToList();
        var weightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Weight").ToList();

        // Height should have new entry (initial + updated)
        heightMetrics.Should().HaveCountGreaterOrEqualTo(2);
        var latestHeight = heightMetrics.OrderByDescending(m => 
            DateTime.Parse(m.GetProperty("recordedAt").GetString()!)).First();
        latestHeight.GetProperty("value").GetDouble().Should().Be(177.0);

        // Weight should only have initial entry (no new entry)
        weightMetrics.Should().HaveCount(initialWeightCount);
    }

    [Fact]
    public async Task UpdatePhysicalMeasurements_OnlyWeight_ShouldSyncOnlyWeight()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Weight",
            "lastName": "Only",
            "dateOfBirth": "1990-01-01T00:00:00Z",
            "gender": "Female",
            "height": 168,
            "weight": 60,
            "fitnessLevel": "Enthousiast",
            "fitnessGoal": "Weight_Loss"
        }
        """;

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        await Task.Delay(500);

        // Get initial height count
        var initialMetricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var initialContent = await initialMetricsResponse.Content.ReadAsStringAsync();
        var initialDoc = JsonDocument.Parse(initialContent);
        var initialMetrics = initialDoc.RootElement.EnumerateArray().ToList();
        var initialHeightCount = initialMetrics.Count(m => 
            m.GetProperty("metricType").GetString() == "Height");

        // Act - Update only weight
        var updateRequestJson = """
        {
            "weight": 58,
            "units": {
                "heightUnit": "cm",
                "weightUnit": "kg"
            }
        }
        """;

        var updateResponse = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Wait for async sync
        await Task.Delay(1000);

        // Assert - Only weight should be synced
        var metricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var content = await metricsResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var metrics = doc.RootElement.EnumerateArray().ToList();

        var heightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Height").ToList();
        var weightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Weight").ToList();

        // Weight should have new entry
        weightMetrics.Should().HaveCountGreaterOrEqualTo(2);
        var latestWeight = weightMetrics.OrderByDescending(m => 
            DateTime.Parse(m.GetProperty("recordedAt").GetString()!)).First();
        latestWeight.GetProperty("value").GetDouble().Should().Be(58.0);

        // Height should only have initial entry
        heightMetrics.Should().HaveCount(initialHeightCount);
    }

    [Fact]
    public async Task MultipleUpdates_ShouldCreateSeparateTrackingEntries()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Multiple",
            "lastName": "Updates",
            "dateOfBirth": "1985-12-25T00:00:00Z",
            "gender": "Male",
            "height": 175,
            "weight": 80,
            "fitnessLevel": "Advanced",
            "fitnessGoal": "Endurance"
        }
        """;

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        await Task.Delay(500);

        // Act - Multiple updates
        var update1Json = """
        {
            "height": 176,
            "weight": 79,
            "units": {
                "heightUnit": "cm",
                "weightUnit": "kg"
            }
        }
        """;

        await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(update1Json, Encoding.UTF8, "application/json"));

        await Task.Delay(700); // Wait between updates

        var update2Json = """
        {
            "height": 177,
            "weight": 78,
            "units": {
                "heightUnit": "cm",
                "weightUnit": "kg"
            }
        }
        """;

        await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(update2Json, Encoding.UTF8, "application/json"));

        await Task.Delay(1000); // Final wait for sync

        // Assert - Should have multiple entries
        var metricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var content = await metricsResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var metrics = doc.RootElement.EnumerateArray().ToList();

        var heightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Height")
            .OrderBy(m => DateTime.Parse(m.GetProperty("recordedAt").GetString()!))
            .ToList();
        var weightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Weight")
            .OrderBy(m => DateTime.Parse(m.GetProperty("recordedAt").GetString()!))
            .ToList();

        // Should have initial + 2 updates = 3 entries each
        heightMetrics.Should().HaveCountGreaterOrEqualTo(3);
        weightMetrics.Should().HaveCountGreaterOrEqualTo(3);

        // Verify progression
        heightMetrics.Should().Contain(m => m.GetProperty("value").GetDouble() == 175.0);
        heightMetrics.Should().Contain(m => m.GetProperty("value").GetDouble() == 176.0);
        heightMetrics.Should().Contain(m => m.GetProperty("value").GetDouble() == 177.0);

        weightMetrics.Should().Contain(m => m.GetProperty("value").GetDouble() == 80.0);
        weightMetrics.Should().Contain(m => m.GetProperty("value").GetDouble() == 79.0);
        weightMetrics.Should().Contain(m => m.GetProperty("value").GetDouble() == 78.0);
    }

    #endregion

    #region Unit Conversion Cross-Module Tests

    [Fact]
    public async Task UpdateMeasurements_WithImperialUnits_ShouldSyncConvertedValues()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Imperial",
            "lastName": "User",
            "dateOfBirth": "1992-03-15T00:00:00Z",
            "gender": "Female",
            "height": 165,
            "weight": 60,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Muscle_Gain"
        }
        """;

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        await Task.Delay(500);

        // Act - Update with imperial units (5.8 ft ≈ 176.78 cm, 140 lbs ≈ 63.5 kg)
        var updateRequestJson = """
        {
            "height": 5.8,
            "weight": 140,
            "units": {
                "heightUnit": "ft",
                "weightUnit": "lbs"
            }
        }
        """;

        var updateResponse = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await Task.Delay(1000);

        // Assert - Tracking should store in metric units
        var metricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var content = await metricsResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var metrics = doc.RootElement.EnumerateArray().ToList();

        var latestHeight = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Height")
            .OrderByDescending(m => DateTime.Parse(m.GetProperty("recordedAt").GetString()!))
            .First();
        
        var latestWeight = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Weight")
            .OrderByDescending(m => DateTime.Parse(m.GetProperty("recordedAt").GetString()!))
            .First();

        // Values should be converted to metric
        latestHeight.GetProperty("value").GetDouble().Should().BeApproximately(177, 1); // 5.8 ft ≈ 176.78 cm
        latestHeight.GetProperty("unit").GetString().Should().Be("cm");

        latestWeight.GetProperty("value").GetDouble().Should().BeApproximately(63.5, 1); // 140 lbs ≈ 63.5 kg
        latestWeight.GetProperty("unit").GetString().Should().Be("kg");
    }

    [Fact]
    public async Task CreateProfile_WithMixedUnits_ShouldSyncStandardizedValues()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Note: Profile creation always expects metric units in the current API
        var createRequestJson = """
        {
            "firstName": "Standard",
            "lastName": "Units",
            "dateOfBirth": "1990-07-20T00:00:00Z",
            "gender": "Male",
            "height": 183,
            "weight": 77,
            "fitnessLevel": "Enthousiast",
            "fitnessGoal": "Muscle_Gain"
        }
        """;

        // Act
        var createResponse = await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await Task.Delay(1000);

        // Assert - Tracking should have standardized metric values
        var metricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var content = await metricsResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var metrics = doc.RootElement.EnumerateArray().ToList();

        var heightMetric = metrics.FirstOrDefault(m => 
            m.GetProperty("metricType").GetString() == "Height");
        var weightMetric = metrics.FirstOrDefault(m => 
            m.GetProperty("metricType").GetString() == "Weight");

        heightMetric.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        heightMetric.GetProperty("value").GetDouble().Should().Be(183.0);
        heightMetric.GetProperty("unit").GetString().Should().Be("cm");

        weightMetric.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        weightMetric.GetProperty("value").GetDouble().Should().Be(77.0);
        weightMetric.GetProperty("unit").GetString().Should().Be("kg");
    }

    #endregion

    #region Error Handling and Edge Cases

    [Fact]
    public async Task UpdateMeasurements_WithInvalidData_ShouldNotSyncToTracking()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Error",
            "lastName": "Test",
            "dateOfBirth": "1990-01-01T00:00:00Z",
            "gender": "Male",
            "height": 175,
            "weight": 70,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Weight_Loss"
        }
        """;

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        await Task.Delay(500);

        // Get initial metrics count
        var initialResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var initialContent = await initialResponse.Content.ReadAsStringAsync();
        var initialDoc = JsonDocument.Parse(initialContent);
        var initialCount = initialDoc.RootElement.GetArrayLength();

        // Act - Try to update with invalid data
        var invalidUpdateJson = """
        {
            "height": -180,
            "weight": -70,
            "units": {
                "heightUnit": "cm",
                "weightUnit": "kg"
            }
        }
        """;

        var updateResponse = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(invalidUpdateJson, Encoding.UTF8, "application/json"));

        // Assert - Update should fail
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await Task.Delay(500);

        // Tracking metrics should not have changed
        var finalResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var finalContent = await finalResponse.Content.ReadAsStringAsync();
        var finalDoc = JsonDocument.Parse(finalContent);
        var finalCount = finalDoc.RootElement.GetArrayLength();

        finalCount.Should().Be(initialCount);
    }

    [Fact]
    public async Task UpdateMeasurements_WithNonExistentProfile_ShouldNotAffectTracking()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Get initial metrics count (should be 0 for fresh user)
        var initialResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var initialContent = await initialResponse.Content.ReadAsStringAsync();
        var initialDoc = JsonDocument.Parse(initialContent);
        var initialCount = initialDoc.RootElement.GetArrayLength();

        // Act - Try to update measurements without profile
        var updateRequestJson = """
        {
            "height": 175,
            "weight": 70,
            "units": {
                "heightUnit": "cm",
                "weightUnit": "kg"
            }
        }
        """;

        var updateResponse = await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(updateRequestJson, Encoding.UTF8, "application/json"));

        // Assert - Should fail
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        await Task.Delay(500);

        // Tracking should remain unchanged
        var finalResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var finalContent = await finalResponse.Content.ReadAsStringAsync();
        var finalDoc = JsonDocument.Parse(finalContent);
        var finalCount = finalDoc.RootElement.GetArrayLength();

        finalCount.Should().Be(initialCount);
    }

    #endregion

    #region Performance and Consistency Tests

    [Fact]
    public async Task ConcurrentProfileUpdates_ShouldSyncConsistently()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        var createRequestJson = """
        {
            "firstName": "Concurrent",
            "lastName": "Test",
            "dateOfBirth": "1990-01-01T00:00:00Z",
            "gender": "Male",
            "height": 175,
            "weight": 70,
            "fitnessLevel": "Beginner",
            "fitnessGoal": "Muscle_Gain"
        }
        """;

        await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));

        await Task.Delay(500);

        // Act - Rapid successive updates
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 1; i <= 3; i++)
        {
            var updateJson = $$"""
            {
                "height": {{175 + i}},
                "weight": {{70 + i}},
                "units": {
                    "heightUnit": "cm",
                    "weightUnit": "kg"
                }
            }
            """;

            tasks.Add(Client.PatchAsync("/api/v1/users/profile/measurements",
                new StringContent(updateJson, Encoding.UTF8, "application/json")));
        }

        var responses = await Task.WhenAll(tasks);

        // At least one should succeed (last-write-wins scenario)
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.OK);

        await Task.Delay(2000); // Wait for all sync operations

        // Assert - All metrics should be consistent
        var metricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var content = await metricsResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var metrics = doc.RootElement.EnumerateArray().ToList();

        var heightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Height").ToList();
        var weightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Weight").ToList();

        // Should have at least initial metrics + some updates
        heightMetrics.Should().HaveCountGreaterOrEqualTo(2);
        weightMetrics.Should().HaveCountGreaterOrEqualTo(2);

        // All entries should be valid
        foreach (var metric in heightMetrics.Concat(weightMetrics))
        {
            metric.GetProperty("value").GetDouble().Should().BeGreaterThan(0);
            metric.GetProperty("notes").GetString().Should().Contain("Auto-sync");
        }
    }

    [Fact]
    public async Task CompleteProfileWorkflow_ShouldMaintainSynchronization()
    {
        // Arrange
        await AuthenticateAsUserAsync();

        // Step 1: Create profile
        var createRequestJson = """
        {
            "firstName": "Complete",
            "lastName": "Workflow",
            "dateOfBirth": "1987-04-12T00:00:00Z",
            "gender": "Female",
            "height": 165,
            "weight": 58,
            "fitnessLevel": "Enthousiast",
            "fitnessGoal": "Muscle_Gain"
        }
        """;

        var createResponse = await Client.PostAsync(ApiEndpoints.Users.Profile,
            new StringContent(createRequestJson, Encoding.UTF8, "application/json"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await Task.Delay(500);

        // Step 2: Update personal info (should not trigger sync)
        var updatePersonalJson = """
        {
            "firstName": "Updated",
            "lastName": "Workflow"
        }
        """;

        await Client.PatchAsync("/api/v1/users/profile/personal",
            new StringContent(updatePersonalJson, Encoding.UTF8, "application/json"));

        await Task.Delay(500);

        // Step 3: Update fitness profile (should not trigger sync)
        var updateFitnessJson = """
        {
            "fitnessLevel": "Advanced",
            "fitnessGoal": "Endurance"
        }
        """;

        await Client.PatchAsync("/api/v1/users/profile/fitness",
            new StringContent(updateFitnessJson, Encoding.UTF8, "application/json"));

        await Task.Delay(500);

        // Step 4: Update measurements (should trigger sync)
        var updateMeasurementsJson = """
        {
            "height": 167,
            "weight": 60,
            "units": {
                "heightUnit": "cm",
                "weightUnit": "kg"
            }
        }
        """;

        await Client.PatchAsync(ApiEndpoints.Users.ProfileMeasurements,
            new StringContent(updateMeasurementsJson, Encoding.UTF8, "application/json"));

        await Task.Delay(1000);

        // Assert - Should have initial + updated measurements only
        var metricsResponse = await Client.GetAsync(ApiEndpoints.Tracking.Metrics);
        var content = await metricsResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var metrics = doc.RootElement.EnumerateArray().ToList();

        var heightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Height")
            .OrderBy(m => DateTime.Parse(m.GetProperty("recordedAt").GetString()!))
            .ToList();
        var weightMetrics = metrics.Where(m => 
            m.GetProperty("metricType").GetString() == "Weight")
            .OrderBy(m => DateTime.Parse(m.GetProperty("recordedAt").GetString()!))
            .ToList();

        // Should have initial + updated = 2 entries each
        heightMetrics.Should().HaveCount(2);
        weightMetrics.Should().HaveCount(2);

        // Initial values
        heightMetrics[0].GetProperty("value").GetDouble().Should().Be(165.0);
        weightMetrics[0].GetProperty("value").GetDouble().Should().Be(58.0);

        // Updated values
        heightMetrics[1].GetProperty("value").GetDouble().Should().Be(167.0);
        weightMetrics[1].GetProperty("value").GetDouble().Should().Be(60.0);
    }

    #endregion
}

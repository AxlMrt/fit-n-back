using FluentAssertions.Execution;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Tracking.Domain.Entities;
using System.Net;

namespace FitnessApp.IntegrationTests.Helpers;

public static class CustomAssertions
{
    #region HTTP Response Assertions

    /// <summary>
    /// Asserts that an HTTP response has the expected status and non-empty content, and deserializes the content.
    /// </summary>
    public static async Task<T> ShouldHaveStatusAndContentAsync<T>(
        this HttpResponseMessage response, 
        HttpStatusCode expectedStatus)
    {
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(expectedStatus, 
                "Response should have status {0} but got {1}", 
                expectedStatus, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty("Response content should not be empty");

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                })!;
            }
            catch (Exception ex)
            {
                throw new AssertionException($"Failed to deserialize content to {typeof(T).Name}: {ex.Message}. Content: {content}");
            }
        }
    }

    /// <summary>
    /// Asserts that an HTTP response has the expected error status and optionally contains an error message.
    /// </summary>
    public static async Task ShouldHaveErrorStatusAsync(
        this HttpResponseMessage response, 
        HttpStatusCode expectedStatus,
        string? expectedErrorMessage = null)
    {
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(expectedStatus);

            if (expectedErrorMessage != null)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain(expectedErrorMessage);
            }
        }
    }

    /// <summary>
    /// Asserts that an HTTP response is successful (2xx).
    /// </summary>
    public static void ShouldBeSuccessful(this HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue(
            "Response should be successful but got status {0}: {1}", 
            (int)response.StatusCode, response.StatusCode);
    }

    #endregion

    #region Entity Assertions

    /// <summary>
    /// Asserts that a UserProfile has the expected properties.
    /// </summary>
    public static void ShouldHaveCorrectUserProfileData(
        this UserProfile profile,
        Guid expectedUserId,
        string expectedFirstName,
        string expectedLastName)
    {
        using (new AssertionScope())
        {
            profile.UserId.Should().Be(expectedUserId);
            profile.Name.FirstName.Should().Be(expectedFirstName);
            profile.Name.LastName.Should().Be(expectedLastName);
            profile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }
    }

    /// <summary>
    /// Asserts that a UserMetric has the expected properties.
    /// </summary>
    public static void ShouldHaveCorrectMetricData(
        this UserMetric metric,
        Guid expectedUserId,
        double expectedValue,
        string? expectedUnit = null)
    {
        using (new AssertionScope())
        {
            metric.UserId.Should().Be(expectedUserId);
            metric.Value.Should().Be(expectedValue);
            
            if (expectedUnit != null)
            {
                metric.Unit.Should().Be(expectedUnit);
            }
            
            metric.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }
    }

    #endregion

    #region Collection Assertions

    /// <summary>
    /// Asserts that a collection contains exactly the expected items in order.
    /// </summary>
    public static void ShouldContainInOrder<T>(this IEnumerable<T> collection, params T[] expectedItems)
    {
        collection.Should().ContainInOrder(expectedItems);
        collection.Should().HaveCount(expectedItems.Length);
    }

    /// <summary>
    /// Asserts that a collection of metrics is sorted by recorded date.
    /// </summary>
    public static void ShouldBeSortedByRecordedDate(this IEnumerable<UserMetric> metrics, bool ascending = true)
    {
        var metricsList = metrics.ToList();
        
        if (metricsList.Count <= 1) return;

        for (int i = 1; i < metricsList.Count; i++)
        {
            var comparison = metricsList[i].RecordedAt.CompareTo(metricsList[i - 1].RecordedAt);
            
            if (ascending)
            {
                comparison.Should().BeGreaterOrEqualTo(0, 
                    "Metrics should be sorted in ascending order by date");
            }
            else
            {
                comparison.Should().BeLessOrEqualTo(0, 
                    "Metrics should be sorted in descending order by date");
            }
        }
    }

    #endregion

    #region Time Assertions

    /// <summary>
    /// Asserts that a DateTime is recent (within X minutes).
    /// </summary>
    public static void ShouldBeRecent(this DateTime dateTime, int maxMinutesAgo = 2)
    {
        dateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(maxMinutesAgo));
    }

    /// <summary>
    /// Asserts that a DateTime is before another.
    /// </summary>
    public static void ShouldBeBefore(this DateTime dateTime, DateTime otherDateTime)
    {
        dateTime.Should().BeBefore(otherDateTime);
    }

    /// <summary>
    /// Asserts that a DateTime is after another.
    /// </summary>
    public static void ShouldBeAfter(this DateTime dateTime, DateTime otherDateTime)
    {
        dateTime.Should().BeAfter(otherDateTime);
    }

    #endregion

    #region Business Logic Assertions

    /// <summary>
    /// Asserts that a UserProfile has a correctly calculated BMI.
    /// </summary>
    public static void ShouldHaveCorrectBMI(this UserProfile profile, decimal expectedBMI, decimal tolerance = 0.1m)
    {
        var actualBMI = profile.GetBMI();
        actualBMI.Should().NotBeNull("BMI should be calculated");
        actualBMI!.Value.Should().BeApproximately(expectedBMI, tolerance, 
            "BMI should be calculated correctly");
    }

    /// <summary>
    /// Asserts that a series of metrics shows a trend (increasing/decreasing).
    /// </summary>
    public static void ShouldShowTrend(this IEnumerable<UserMetric> metrics, bool isIncreasing)
    {
        var sortedMetrics = metrics.OrderBy(m => m.RecordedAt).ToList();
        
        if (sortedMetrics.Count < 2)
        {
            throw new AssertionException("At least 2 metrics are required to analyze a trend");
        }

        var firstValue = sortedMetrics.First().Value;
        var lastValue = sortedMetrics.Last().Value;

        if (isIncreasing)
        {
            lastValue.Should().BeGreaterThan(firstValue, "Trend should be increasing");
        }
        else
        {
            lastValue.Should().BeLessThan(firstValue, "Trend should be decreasing");
        }
    }

    #endregion

    #region Performance Assertions

    /// <summary>
    /// Asserts that a task completes within a given duration.
    /// </summary>
    public static async Task<T> ShouldCompleteWithin<T>(this Task<T> task, TimeSpan maxDuration)
    {
        var startTime = DateTime.UtcNow;
        var result = await task;
        var duration = DateTime.UtcNow - startTime;

        duration.Should().BeLessOrEqualTo(maxDuration, 
            "Operation should complete within {0} but took {1}", 
            maxDuration, duration);

        return result;
    }

    /// <summary>
    /// Asserts that a task completes within a given duration.
    /// </summary>
    public static async Task ShouldCompleteWithin(this Task task, TimeSpan maxDuration)
    {
        var startTime = DateTime.UtcNow;
        await task;
        var duration = DateTime.UtcNow - startTime;

        duration.Should().BeLessOrEqualTo(maxDuration, 
            "Operation should complete within {0} but took {1}", 
            maxDuration, duration);
    }

    #endregion
}

public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
    public AssertionException(string message, Exception innerException) : base(message, innerException) { }
}

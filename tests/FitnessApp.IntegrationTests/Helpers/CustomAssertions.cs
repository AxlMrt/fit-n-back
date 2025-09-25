using FluentAssertions;
using FluentAssertions.Execution;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Tracking.Domain.Entities;
using System.Net;

namespace FitnessApp.IntegrationTests.Helpers;

/// <summary>
/// Extensions d'assertions personnalisées pour les tests d'intégration FitnessApp
/// </summary>
public static class CustomAssertions
{
    #region HTTP Response Assertions

    /// <summary>
    /// Vérifie qu'une réponse HTTP a le statut attendu et un contenu non vide
    /// </summary>
    public static async Task<T> ShouldHaveStatusAndContentAsync<T>(
        this HttpResponseMessage response, 
        HttpStatusCode expectedStatus)
    {
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(expectedStatus, 
                "la réponse devrait avoir le statut {0} mais a reçu {1}", 
                expectedStatus, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty("le contenu de la réponse ne devrait pas être vide");

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
                throw new AssertionException($"Impossible de désérialiser le contenu en {typeof(T).Name}: {ex.Message}. Contenu: {content}");
            }
        }
    }

    /// <summary>
    /// Vérifie qu'une réponse HTTP a le statut d'erreur attendu
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
    /// Vérifie qu'une réponse est un succès (2xx)
    /// </summary>
    public static void ShouldBeSuccessful(this HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue(
            "la réponse devrait être un succès mais a reçu le statut {0}: {1}", 
            (int)response.StatusCode, response.StatusCode);
    }

    #endregion

    #region Entity Assertions

    /// <summary>
    /// Vérifie qu'un UserProfile a les propriétés attendues
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
    /// Vérifie qu'un UserMetric a les propriétés attendues
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
    /// Vérifie qu'une collection contient exactement les éléments attendus dans l'ordre
    /// </summary>
    public static void ShouldContainInOrder<T>(this IEnumerable<T> collection, params T[] expectedItems)
    {
        collection.Should().ContainInOrder(expectedItems);
        collection.Should().HaveCount(expectedItems.Length);
    }

    /// <summary>
    /// Vérifie qu'une collection de métriques est triée par date
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
                    "les métriques devraient être triées par date croissante");
            }
            else
            {
                comparison.Should().BeLessOrEqualTo(0, 
                    "les métriques devraient être triées par date décroissante");
            }
        }
    }

    #endregion

    #region Time Assertions

    /// <summary>
    /// Vérifie qu'une date est récente (dans les X minutes)
    /// </summary>
    public static void ShouldBeRecent(this DateTime dateTime, int maxMinutesAgo = 2)
    {
        dateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(maxMinutesAgo));
    }

    /// <summary>
    /// Vérifie qu'une date est antérieure à une autre
    /// </summary>
    public static void ShouldBeBefore(this DateTime dateTime, DateTime otherDateTime)
    {
        dateTime.Should().BeBefore(otherDateTime);
    }

    /// <summary>
    /// Vérifie qu'une date est postérieure à une autre
    /// </summary>
    public static void ShouldBeAfter(this DateTime dateTime, DateTime otherDateTime)
    {
        dateTime.Should().BeAfter(otherDateTime);
    }

    #endregion

    #region Business Logic Assertions

    /// <summary>
    /// Vérifie qu'un UserProfile a un BMI calculé correctement
    /// </summary>
    public static void ShouldHaveCorrectBMI(this UserProfile profile, decimal expectedBMI, decimal tolerance = 0.1m)
    {
        var actualBMI = profile.GetBMI();
        actualBMI.Should().NotBeNull("le BMI devrait être calculé");
        actualBMI!.Value.Should().BeApproximately(expectedBMI, tolerance, 
            "le BMI devrait être calculé correctement");
    }

    /// <summary>
    /// Vérifie qu'une série de métriques montre une tendance (croissante/décroissante)
    /// </summary>
    public static void ShouldShowTrend(this IEnumerable<UserMetric> metrics, bool isIncreasing)
    {
        var sortedMetrics = metrics.OrderBy(m => m.RecordedAt).ToList();
        
        if (sortedMetrics.Count < 2)
        {
            throw new AssertionException("Il faut au moins 2 métriques pour analyser une tendance");
        }

        var firstValue = sortedMetrics.First().Value;
        var lastValue = sortedMetrics.Last().Value;

        if (isIncreasing)
        {
            lastValue.Should().BeGreaterThan(firstValue, "la tendance devrait être croissante");
        }
        else
        {
            lastValue.Should().BeLessThan(firstValue, "la tendance devrait être décroissante");
        }
    }

    #endregion

    #region Performance Assertions

    /// <summary>
    /// Vérifie qu'une opération s'exécute dans un délai acceptable
    /// </summary>
    public static async Task<T> ShouldCompleteWithin<T>(this Task<T> task, TimeSpan maxDuration)
    {
        var startTime = DateTime.UtcNow;
        var result = await task;
        var duration = DateTime.UtcNow - startTime;

        duration.Should().BeLessOrEqualTo(maxDuration, 
            "l'opération devrait se terminer dans les {0} mais a pris {1}", 
            maxDuration, duration);

        return result;
    }

    /// <summary>
    /// Vérifie qu'une action s'exécute dans un délai acceptable
    /// </summary>
    public static async Task ShouldCompleteWithin(this Task task, TimeSpan maxDuration)
    {
        var startTime = DateTime.UtcNow;
        await task;
        var duration = DateTime.UtcNow - startTime;

        duration.Should().BeLessOrEqualTo(maxDuration, 
            "l'opération devrait se terminer dans les {0} mais a pris {1}", 
            maxDuration, duration);
    }

    #endregion
}

/// <summary>
/// Exception d'assertion personnalisée
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
    public AssertionException(string message, Exception innerException) : base(message, innerException) { }
}
